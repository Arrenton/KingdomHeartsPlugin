using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace KingdomHeartsPlugin.Configuration
{
    [Serializable]
    public class Settings : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        #region General
        public bool Locked { get; set; } = Defaults.Locked;
        public bool Enabled { get; set; } = Defaults.Enabled;
        public bool HideWhenNpcTalking { get; set; } = Defaults.HideWhenNpcTalking;
        public float Scale { get; set; } = Defaults.Scale;
        #endregion

        #region HP
        public bool HpBarEnabled { get; set; } = Defaults.HpBarEnabled;
        public float HpValueTextPositionX { get; set; } = Defaults.HpValueTextPositionX;
        public float HpValueTextPositionY { get; set; } = Defaults.HpValueTextPositionY;
        public float HpValueTextSize { get; set; } = Defaults.HpValueTextSize;
        public int HpValueTextAlignment { get; set; } = Defaults.HpValueTextAlignment;
        public float HpDamageWobbleIntensity { get; set; } = Defaults.HpDamageWobbleIntensity;
        public int HpForFullRing { get; set; } = Defaults.HpForFullRing;
        public int MaximumHpForMaximumLength { get; set; } = Defaults.MaximumHpForMaximumLength;
        public int MinimumHpForLength { get; set; } = Defaults.MinimumHpForLength;
        public float HpPerPixelLongBar { get; set; } = Defaults.HpPerPixelLongBar;
        public int PvpHpForFullRing { get; set; } = Defaults.PvpHpForFullRing;
        public int PvpMaximumHpForMaximumLength { get; set; } = Defaults.PvpMaximumHpForMaximumLength;
        public int PvpMinimumHpForLength { get; set; } = Defaults.PvpMinimumHpForLength;
        public float PvpHpPerPixelLongBar { get; set; } = Defaults.PvpHpPerPixelLongBar;
        public float LowHpPercent { get; set; } = Defaults.LowHpPercent;
        public bool ShowHpRecovery { get; set; } = Defaults.ShowHpRecovery;
        public bool TruncateHp { get; set; } = Defaults.TruncateHp;
        public bool ShowHpVal { get; set; } = Defaults.ShowHpVal;
        #endregion

        #region Resource
        public bool ResourceBarEnabled { get; set; } = Defaults.ResourceBarEnabled;
        public float ResourceBarPositionX { get; set; } = Defaults.ResourceBarPositionX;
        public float ResourceBarPositionY { get; set; } = Defaults.ResourceBarPositionY;
        public float ResourceTextPositionX { get; set; } = Defaults.ResourceTextPositionX;
        public float ResourceTextPositionY { get; set; } = Defaults.ResourceTextPositionY;
        public float ResourceTextSize { get; set; } = Defaults.ResourceTextSize;
        public int ResourceTextAlignment { get; set; } = Defaults.ResourceTextAlignment;
        public int MaximumMpLength { get; set; } = Defaults.MaximumMpLength;
        public int MinimumMpLength { get; set; } = Defaults.MinimumMpLength;
        public float MpPerPixelLength { get; set; } = Defaults.MpPerPixelLength;
        public float GpPerPixelLength { get; set; } = Defaults.GpPerPixelLength;
        public int MaximumGpLength { get; set; } = Defaults.MaximumGpLength;
        public int MinimumGpLength { get; set; } = Defaults.MinimumGpLength;
        public float CpPerPixelLength { get; set; } = Defaults.CpPerPixelLength;
        public int MaximumCpLength { get; set; } = Defaults.MaximumCpLength;
        public int MinimumCpLength { get; set; } = Defaults.MinimumCpLength;
        public bool TruncateMp { get; set; } = Defaults.TruncateMp;
        public bool ShowResourceVal { get; set; } = Defaults.ShowResourceVal;
        #endregion

        #region Limit Break

        public bool LimitBarEnabled { get; set; } = Defaults.LimitBarEnabled;
        public bool LimitGaugeAlwaysShow { get; set; } = Defaults.LimitGaugeAlwaysShow;
        public bool LimitGaugeDiadem { get; set; } = Defaults.LimitGaugeDiadem;
        public float LimitGaugePositionX { get; set; } = Defaults.LimitGaugePositionX;
        public float LimitGaugePositionY { get; set; } = Defaults.LimitGaugePositionY;

        #endregion

        #region Experience
        
        public bool ExpBarEnabled { get; set; } = Defaults.ExpBarEnabled;

        #endregion

        #region ClassInfo
        
        public bool LevelEnabled { get; set; } = Defaults.LevelEnabled;
        public bool ClassIconEnabled { get; set; } = Defaults.ClassIconEnabled;

        #endregion

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private DalamudPluginInterface _pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this._pluginInterface.SavePluginConfig(this);
        }
    }
}
