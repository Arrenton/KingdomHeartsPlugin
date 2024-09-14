using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Configuration
{
    [Serializable]
    public partial class Settings : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        #region General
        public bool Locked { get; set; } = Defaults.Locked;
        public bool Enabled { get; set; } = Defaults.Enabled;
        public bool HideWhenNpcTalking { get; set; } = Defaults.HideWhenNpcTalking;
        public float Scale { get; set; } = Defaults.Scale;
        public string TextFormatCulture = Defaults.TextFormatCulture;
        #endregion

        #region HP
        public bool HpBarEnabled { get; set; } = Defaults.HpBarEnabled;
        public float HpValueTextPositionX { get; set; } = Defaults.HpValueTextPositionX;
        public float HpValueTextPositionY { get; set; } = Defaults.HpValueTextPositionY;
        public float HpValueTextSize { get; set; } = Defaults.HpValueTextSize;
        public int HpValueTextAlignment { get; set; } = Defaults.HpValueTextAlignment;
        public NumberFormatStyle HpValueTextStyle { get; set; } = Defaults.HpValueTextStyle;
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
        public bool ShowHpVal { get; set; } = Defaults.ShowHpVal;
        public bool ShieldBarEnabled { get; set; } = Defaults.ShieldBarEnabled;
        #endregion

        #region Resource
        public bool ResourceBarEnabled { get; set; } = Defaults.ResourceBarEnabled;
        public bool LowMpEnabled { get; set; } = Defaults.LowMpEnabled;
        public float LowMpPercent { get; set; } = Defaults.LowMpPercent;
        public float ResourceBarPositionX { get; set; } = Defaults.ResourceBarPositionX;
        public float ResourceBarPositionY { get; set; } = Defaults.ResourceBarPositionY;
        public float ResourceTextPositionX { get; set; } = Defaults.ResourceTextPositionX;
        public float ResourceTextPositionY { get; set; } = Defaults.ResourceTextPositionY;
        public float ResourceTextSize { get; set; } = Defaults.ResourceTextSize;
        public NumberFormatStyle ResourceTextStyle { get; set; } = Defaults.ResourceTextStyle;
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
        public bool ExpValueTextEnabled { get; set; } = Defaults.ExpValueTextEnabled;
        public float ExpValueTextSize { get; set; } = Defaults.ExpValueTextSize;
        public int ExpValueTextAlignment { get; set; } = Defaults.ExpValueTextAlignment;
        public NumberFormatStyle ExpValueTextFormatStyle { get; set; } = Defaults.ExpValueTextFormatStyle;
        public float ExpValueTextPositionX { get; set; } = Defaults.ExpValueTextPositionX;
        public float ExpValueTextPositionY { get; set; } = Defaults.ExpValueTextPositionY;

        #endregion

        #region ClassInfo

        public bool LevelEnabled { get; set; } = Defaults.LevelEnabled;
        public bool ClassIconEnabled { get; set; } = Defaults.ClassIconEnabled;

        #endregion
        
        #region Party
        public bool PartyEnabled { get; set; } = Defaults.PartyEnabled;
        #endregion

        // the below exist just to make saving less cumbersome

        [NonSerialized]
        private IDalamudPluginInterface _pluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this._pluginInterface.SavePluginConfig(this);
        }
    }
}
