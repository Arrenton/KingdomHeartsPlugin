namespace KingdomHeartsPlugin.Configuration
{
    public static class Defaults
    {
        #region General
        public static bool Locked { get; set; } = false;
        public static bool Enabled { get; set; } = true;
        public static bool HideWhenNpcTalking { get; set; } = false;
        public static float Scale { get; set; } = 1f;
        #endregion

        #region HP
        public static bool HpBarEnabled { get; set; } = true;
        public static float HpValueTextPositionX { get; set; } = 26;
        public static float HpValueTextPositionY { get; set; } = 130;
        public static float HpValueTextSize { get; set; } = 21;
        public static int HpValueTextAlignment { get; set; } = 0;
        public static float HpDamageWobbleIntensity { get; set; } = 100f;
        public static int HpForFullRing { get; set; } = 60000;
        public static int MaximumHpForMaximumLength { get; set; } = 120000;
        public static int MinimumHpForLength { get; set; } = 1000;
        public static float HpPerPixelLongBar { get; set; } = 100;
        public static int PvpHpForFullRing { get; set; } = 20000;
        public static int PvpMaximumHpForMaximumLength { get; set; } = 40000;
        public static int PvpMinimumHpForLength { get; set; } = 6666;
        public static float PvpHpPerPixelLongBar { get; set; } = 33.33f;
        public static float LowHpPercent { get; set; } = 25f;
        public static bool ShowHpRecovery { get; set; } = true;
        public static bool TruncateHp { get; set; } = true;
        public static bool ShowHpVal { get; set; } = true;
        #endregion

        #region Resource
        public static bool ResourceBarEnabled { get; set; } = true;
        public static float ResourceBarPositionX { get; set; } = 0;
        public static float ResourceBarPositionY { get; set; } = 200;
        public static float ResourceTextPositionX { get; set; } = -10;
        public static float ResourceTextPositionY { get; set; } = -7;
        public static float ResourceTextSize { get; set; } = 24;
        public static int ResourceTextAlignment { get; set; } = 2;
        public static int MaximumMpLength { get; set; } = 11500;
        public static int MinimumMpLength { get; set; } = 500;
        public static float MpPerPixelLength { get; set; } = 24.45f;
        public static float GpPerPixelLength { get; set; } = 1.913f;
        public static int MaximumGpLength { get; set; } = 900;
        public static int MinimumGpLength { get; set; } = 1;
        public static float CpPerPixelLength { get; set; } = 1.382f;
        public static int MaximumCpLength { get; set; } = 650;
        public static int MinimumCpLength { get; set; } = 1;
        public static bool TruncateMp { get; set; } = false;
        public static bool ShowResourceVal { get; set; } = true;
        #endregion

        #region Limit Break

        public static bool LimitBarEnabled { get; set; } = true;
        public static bool LimitGaugeAlwaysShow { get; set; } = false;
        public static bool LimitGaugeDiadem { get; set; } = true;
        public static float LimitGaugePositionX { get; set; } = -180;
        public static float LimitGaugePositionY { get; set; } = 149;

        #endregion

        #region Experience

        public static bool ExpBarEnabled { get; set; } = true;

        #endregion

        #region ClassInfo

        public static bool LevelEnabled { get; set; } = true;
        public static bool ClassIconEnabled { get; set; } = true;

        #endregion
    }
}
