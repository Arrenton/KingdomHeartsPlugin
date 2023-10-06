using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Dalamud.Logging;
using KingdomHeartsPlugin.Configuration;
using KingdomHeartsPlugin.UIElements.Experience;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin.Services;

namespace KingdomHeartsPlugin
{
    public sealed class KingdomHeartsPlugin : IDalamudPlugin
    {
        public string Name => "Kingdom Hearts UI Plugin";

        private const string SettingsCommand = "/khpconfig";
        private const string ToggleCommand = "/khp";

        public static string TemplateLocation;

        public KingdomHeartsPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] IFramework framework,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IClientState clientState,
            [RequiredVersion("1.0")] IGameGui gameGui,
            [RequiredVersion("1.0")] IDataManager dataManager,
            [RequiredVersion("1.0")] ITextureProvider textureProvider

            )
        {
            Pi = pluginInterface;
            Fw = framework;
            Cm = commandManager;
            Cs = clientState;
            Gui = gameGui;
            Dm = dataManager;
            Tp = textureProvider;


            Timer = Stopwatch.StartNew();

            var assemblyLocation = pluginInterface.AssemblyLocation.DirectoryName + "\\";

            TemplateLocation = Path.GetDirectoryName(assemblyLocation);

            var configuration = Pi.GetPluginConfig() as Settings ?? new Settings();
            configuration.Initialize(Pi);

            Ui = new PluginUI(configuration);

            Portrait.SetAllPortraits();

            Fw.Update += OnUpdate;


            Cm.AddHandler(SettingsCommand, new CommandInfo(OnSettingsCommand)
            {
                HelpMessage = "Opens configuration for Kingdom Hearts UI Bars."
            });

            Cm.AddHandler(ToggleCommand, new CommandInfo(OnToggleCommand)
            {
                HelpMessage = "Toggles the KH UI."
            });

            Pi.UiBuilder.Draw += DrawUi;
            Pi.UiBuilder.OpenConfigUi += DrawConfigUi;
            Cs.TerritoryChanged += OnTerritoryChange;

            if (Cs.LocalPlayer != null)
            {
                IsInPvp = GetTerritoryPvP(Cs.TerritoryType);
            }
        }

        public void Dispose()
        {
            Ui?.Dispose();

            Cm.RemoveHandler(SettingsCommand);
            Cm.RemoveHandler(ToggleCommand);

            Fw.Update -= OnUpdate;

            Pi.UiBuilder.Draw -= DrawUi;
            Cs.TerritoryChanged -= OnTerritoryChange;

            Timer = null;
        }

        private void OnUpdate(IFramework framework)
        {
            UiSpeed = Timer.ElapsedMilliseconds / 1000f;
            Timer.Restart();
            Ui.OnUpdate();
        }

        private void OnSettingsCommand(string command, string args)
        {
            DrawConfigUi();
        }
        private void OnToggleCommand(string command, string args)
        {
            Ui.Configuration.Enabled = !Ui.Configuration.Enabled;
            Ui.Configuration.Save();
        }

        private void OnTerritoryChange(ushort e)
        {
            IsInPvp = GetTerritoryPvP(e);
        }

        private void DrawUi()
        {
            Ui.Draw();
        }

        private void DrawConfigUi()
        {
            Ui.SettingsVisible = true;
        }

        private bool GetTerritoryPvP(uint territoryType)
        {
            try
            {
                var territory = Dm.GetExcelSheet<TerritoryType>().GetRow(territoryType);
                return territory.IsPvpZone;
            }
            catch (KeyNotFoundException)
            {
                PluginLog.Warning("Could not get territory for current zone");
                return false;
            }
        }
        public static CultureInfo GetCulture()
        {
            try
            {
                return CultureInfo.GetCultureInfo(Ui.Configuration.TextFormatCulture);
            }
            catch
            {
                return CultureInfo.GetCultureInfo("en-US");
            }
        }

        public static DalamudPluginInterface Pi { get; private set; }
        public static IFramework Fw { get; private set; }
        public static ICommandManager Cm { get; private set; }
        public static IClientState Cs { get; private set; }
        public static IGameGui Gui { get; private set; }
        public static IDataManager Dm { get; private set; }

        public static ITextureProvider Tp { get; private set; }

        public static PluginUI Ui { get; private set; }

        public static Stopwatch Timer { get; private set; }
        public static float UiSpeed { get; set; }
        public static bool IsInPvp { get; private set; }

    }
}
