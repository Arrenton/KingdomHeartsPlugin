using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Storage.Assets;

namespace KingdomHeartsPlugin;


public sealed class Service
{
    [PluginService] public static IPartyList PartyList { get; private set; }
    
    internal static void Initialize(IDalamudPluginInterface iface)
    {
        iface.Create<Service>();
    }
}