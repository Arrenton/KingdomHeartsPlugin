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

namespace KingdomHeartsPlugin
{
    public sealed class KingdomHeartsPlugin : IDalamudPlugin
    {
        public string Name => "Kingdom Hearts UI Plugin";

        private const string SettingsCommand = "/khp";
        private const string HideCommand = "/khphide";
        private const string ShowCommand = "/khpshow";

        public static string TemplateLocation;

        public KingdomHeartsPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] Framework framework,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] GameGui gameGui,
            [RequiredVersion("1.0")] DataManager dataManager)
        {
            Pi = pluginInterface;
            Fw = framework;
            Cm = commandManager;
            Cs = clientState;
            Gui = gameGui;
            Dm = dataManager;
            
            
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

            Cm.AddHandler(HideCommand, new CommandInfo(OnHideCommand)
            {
                HelpMessage = "Disables the KH UI."
            });

            Cm.AddHandler(ShowCommand, new CommandInfo(OnShowCommand)
            {
                HelpMessage = "Enables the KH UI."
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
            Cm.RemoveHandler(HideCommand);
            Cm.RemoveHandler(ShowCommand);

            Fw.Update -= OnUpdate;
            
            Pi.UiBuilder.Draw -= DrawUi;
            Cs.TerritoryChanged -= OnTerritoryChange;

            Pi?.Dispose();

            Timer = null;
        }

        private void OnUpdate(Framework framework)
        {
            UiSpeed = Timer.ElapsedMilliseconds / 1000f;
            Timer.Restart();
            Ui.OnUpdate();
        }

        private void OnSettingsCommand(string command, string args)
        {
            DrawConfigUi();
        }
        private void OnHideCommand(string command, string args)
        {
            Ui.Configuration.Enabled = false;
            Ui.Configuration.Save();
        }
        private void OnShowCommand(string command, string args)
        {
            Ui.Configuration.Enabled = true;
            Ui.Configuration.Save();
        }

        private void OnTerritoryChange(object sender, ushort e)
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
        public static Framework Fw { get; private set; }
        public static CommandManager Cm { get; private set; }
        public static ClientState Cs { get; private set; }
        public static GameGui Gui { get; private set; }
        public static DataManager Dm { get; private set; }
        public static PluginUI Ui { get; private set; }

        public static Stopwatch Timer { get; private set; }
        public static float UiSpeed { get; set; }
        public static bool IsInPvp { get; private set; }

    }
}
