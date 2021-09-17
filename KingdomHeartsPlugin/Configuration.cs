using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace KingdomHeartsPlugin
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        #region General
        public bool Locked { get; set; } = false;
        public float Scale { get; set; } = 1f;
        #endregion

        #region HP
        public int HpForFullRing { get; set; } = 6000;
        public int MaximumHpForMaximumLength { get; set; } = 12000;
        public int MinimumHpForLength { get; set; } = 100;
        public float HpPerPixelLongBar { get; set; } = 10;
        public float LowHpPercent { get; set; } = 25f;
        public bool ShowHpRecovery { get; set; } = true;
        public bool TruncateHp { get; set; } = true;
        public bool ShowHpVal { get; set; } = true;
        #endregion

        #region Resource
        public int MaximumMpLength { get; set; } = 11500;
        public int MinimumMpLength { get; set; } = 500;
        public float MpPerPixelLength { get; set; } = 22.5f;
        public float GpPerPixelLength { get; set; } = 1.76f;
        public int MaximumGpLength { get; set; } = 900;
        public int MinimumGpLength { get; set; } = 1;
        public float CpPerPixelLength { get; set; } = 1.27f;
        public int MaximumCpLength { get; set; } = 650;
        public int MinimumCpLength { get; set; } = 1;
        public bool TruncateMp { get; set; } = false;
        public bool ShowResourceVal { get; set; } = true;
        #endregion

        #region Limit Break

        public bool LimitGaugeAlwaysShow { get; set; } = false;
        public float LimitGaugePositionX { get; set; } = -180;
        public float LimitGaugePositionY { get; set; } = 149;

        #endregion

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
