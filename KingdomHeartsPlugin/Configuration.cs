﻿using System;
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
        public bool Enabled { get; set; } = true;
        public bool HideWhenNpcTalking { get; set; } = false;
        public float Scale { get; set; } = 1f;
        #endregion

        #region HP
        public float HpValueTextPositionX { get; set; } = 26;
        public float HpValueTextPositionY { get; set; } = 130;
        public float HpValueTextSize { get; set; } = 21;
        public int HpValueTextAlignment { get; set; } = 0;
        public float HpDamageWobbleIntensity { get; set; } = 100f;
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
        public float ResourceBarPositionX { get; set; } = 0;
        public float ResourceBarPositionY { get; set; } = 200;
        public float ResourceTextPositionX { get; set; } = -10;
        public float ResourceTextPositionY { get; set; } = -7;
        public float ResourceTextSize { get; set; } = 24;
        public int ResourceTextAlignment { get; set; } = 2;
        public int MaximumMpLength { get; set; } = 11500;
        public int MinimumMpLength { get; set; } = 500;
        public float MpPerPixelLength { get; set; } = 24.4f;
        public float GpPerPixelLength { get; set; } = 1.913f;
        public int MaximumGpLength { get; set; } = 900;
        public int MinimumGpLength { get; set; } = 1;
        public float CpPerPixelLength { get; set; } = 1.382f;
        public int MaximumCpLength { get; set; } = 650;
        public int MinimumCpLength { get; set; } = 1;
        public bool TruncateMp { get; set; } = false;
        public bool ShowResourceVal { get; set; } = true;
        #endregion

        #region Limit Break

        public bool LimitGaugeAlwaysShow { get; set; } = false;
        public bool LimitGaugeDiadem { get; set; } = true;
        public float LimitGaugePositionX { get; set; } = -180;
        public float LimitGaugePositionY { get; set; } = 149;

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
