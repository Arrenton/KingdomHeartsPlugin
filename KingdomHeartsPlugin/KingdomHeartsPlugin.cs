using System.Diagnostics;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;

namespace KingdomHeartsPlugin
{
    public sealed class KingdomHeartsPlugin : IDalamudPlugin
    {
        public string Name => "Kingdom Hearts UI Plugin";

        private const string CommandName = "/khp";
        
        // When loaded by LivePluginLoader, the executing assembly will be wrong.
        // Supplying this property allows LivePluginLoader to supply the correct location, so that
        // you have full compatibility when loaded normally and through LPL.
        //public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        //private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        private string assemblyLocation = Assembly.GetExecutingAssembly().Location;

        public static DalamudPluginInterface Pi { get; private set; }
        public static Framework Fw { get; private set; }
        public static CommandManager Cm { get; private set; }
        public static ClientState Cs { get; private set; }
        public static GameGui Gui { get; private set; }
        public static DataManager Dm { get; private set; }
        public static PluginUI Ui { get; private set; }

        public static Stopwatch Timer { get; private set; }
        public static float UiSpeed { get; set; }

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

            TemplateLocation = Path.GetDirectoryName(assemblyLocation);
            
            var configuration = Pi.GetPluginConfig() as Configuration ?? new Configuration();
            configuration.Initialize(Pi);

            Ui = new PluginUI(configuration);

            Fw.Update += OnUpdate;


            Cm.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens configuration for Kingdom Hearts UI Bars."
            });

            Pi.UiBuilder.Draw += DrawUi;
            Pi.UiBuilder.OpenConfigUi += DrawConfigUi;
        }

        public void Dispose()
        {
            Ui?.Dispose();

            Cm.RemoveHandler(CommandName);

            Fw.Update -= OnUpdate;
            
            Pi.UiBuilder.Draw -= DrawUi;

            Pi?.Dispose();
        }

        private void OnUpdate(Framework framework)
        {
            UiSpeed = Timer.ElapsedMilliseconds / 1000f;
            Timer.Restart();
            Ui.OnUpdate();
        }

        private void OnCommand(string command, string args)
        {
            DrawConfigUi();
        }

        private void DrawUi()
        {
            Ui.Draw();
        }

        private void DrawConfigUi()
        {
            Ui.SettingsVisible = true;
        }
    }
}
