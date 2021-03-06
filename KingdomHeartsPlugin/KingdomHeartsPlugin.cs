using System.IO;
using System.Reflection;
using Dalamud.Game.Command;
using Dalamud.Game.Internal;
using Dalamud.Plugin;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin
{
    public class KingdomHeartsPlugin : IDalamudPlugin
    {
        public string Name => "Kingdom Hearts UI Plugin";

        private const string commandName = "/khb";
        
        // When loaded by LivePluginLoader, the executing assembly will be wrong.
        // Supplying this property allows LivePluginLoader to supply the correct location, so that
        // you have full compatibility when loaded normally and through LPL.
        public string AssemblyLocation { get => assemblyLocation; set => assemblyLocation = value; }
        private string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;

        public DalamudPluginInterface Pi;
        private Configuration _configuration;
        private PluginUI _ui;
        public static string TemplateLocation;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Pi = pluginInterface;

            TemplateLocation = Path.GetDirectoryName(assemblyLocation);
            
            _configuration = Pi.GetPluginConfig() as Configuration ?? new Configuration();
            _configuration.Initialize(Pi);

            Pi.Framework.OnUpdateEvent += OnUpdate;

            _ui = new PluginUI(_configuration, pluginInterface);

            Pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens configuration for Kingdom Hearts UI Bars."
            });

            Pi.UiBuilder.OnBuildUi += DrawUI;
            Pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUi();
        }

        public void Dispose()
        {
            _ui?.Dispose();

            Pi.CommandManager.RemoveHandler(commandName);

            Pi.Framework.OnUpdateEvent -= OnUpdate;
            
            Pi.UiBuilder.OnBuildUi -= DrawUI;

            Pi?.Dispose();
        }

        private void OnUpdate(Framework framework)
        {
            _ui.OnUpdate(Pi);
        }

        private void OnCommand(string command, string args)
        {
            DrawConfigUi();
        }

        private void DrawUI()
        {
            _ui.Draw(Pi.ClientState.LocalPlayer);
        }

        private void DrawConfigUi()
        {
            _ui.SettingsVisible = true;
        }
    }
}
