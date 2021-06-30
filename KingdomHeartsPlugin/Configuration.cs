using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace KingdomHeartsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public int HpForFullRing { get; set; } = 100000;

        public int MaximumHpForMaximumLength { get; set; } = 250000;

        public int MinimumHpForLength { get; set; } = 33333;

        public float HpPerPixelLongBar { get; set; } = 160;

        public float MpPerPixelLength { get; set; } = 0.2355f;

        public int MaximumMpLength { get; set; } = 200;

        public int MinimumMpLength { get; set; } = 50;

        public float GpPerPixelLength { get; set; } = 1.06f;

        public int MaximumGpLength { get; set; } = 900;

        public int MinimumGpLength { get; set; } = 1;

        public float CpPerPixelLength { get; set; } = 0.765f;

        public int MaximumCpLength { get; set; } = 650;

        public int MinimumCpLength { get; set; } = 1;

        public bool TruncateMp { get; set; } = true;

        public bool TruncateHp { get; set; } = true;

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
