using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace KingdomHeartsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public int HpForFullRing { get; set; } = 7500;

        public int MaximumHpForMaximumLength { get; set; } = 20000;

        public int MinimumHpForLength { get; set; } = 1250;

        public float HpPerPixelLongBar { get; set; } = 16;

        public bool Locked { get; set; } = false;

        public float Scale { get; set; } = 1f;

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
