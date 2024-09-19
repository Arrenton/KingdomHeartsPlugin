using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Configuration
{
    public static partial class Defaults
    {
        #region General
        public const bool Locked = false;
        public const bool Enabled  = true;
        public const bool HideWhenNpcTalking  = false;
        public const float Scale  = 1f;
        public const string TextFormatCulture = "en-US";
        #endregion

        #region HP
        public const bool HpBarEnabled  = true;
        public const float HpValueTextPositionX  = 26;
        public const float HpValueTextPositionY  = 130;
        public const float HpValueTextSize  = 21;
        public const int HpValueTextAlignment  = 0;
        public const NumberFormatStyle HpValueTextStyle = NumberFormatStyle.NoFormatting;
        public const float HpDamageWobbleIntensity  = 100f;
        public const int HpForFullRing  = 60000;
        public const int MaximumHpForMaximumLength  = 120000;
        public const int MinimumHpForLength  = 1000;
        public const float HpPerPixelLongBar  = 100;
        public const int PvpHpForFullRing  = 20000;
        public const int PvpMaximumHpForMaximumLength  = 40000;
        public const int PvpMinimumHpForLength  = 6666;
        public const float PvpHpPerPixelLongBar  = 33.33f;
        public const float LowHpPercent  = 25f;
        public const bool ShowHpRecovery  = true;
        public const bool ShowHpVal  = true;
        public const bool ShieldBarEnabled = false;
        public const float ShieldBarTransparency = 1f;
        #endregion

        #region Resource
        public const bool ResourceBarEnabled  = true;
        public const bool LowMpEnabled  = false;
        public const float LowMpPercent = 33;
        public const float ResourceBarPositionX  = 0;
        public const float ResourceBarPositionY  = 200;
        public const float ResourceTextPositionX  = -10;
        public const float ResourceTextPositionY  = -7;
        public const float ResourceTextSize  = 24;
        public const NumberFormatStyle ResourceTextStyle = NumberFormatStyle.NoFormatting;
        public const int ResourceTextAlignment  = 2;
        public const int MaximumMpLength  = 11500;
        public const int MinimumMpLength  = 500;
        public const float MpPerPixelLength  = 24.45f;
        public const float GpPerPixelLength  = 1.913f;
        public const int MaximumGpLength  = 900;
        public const int MinimumGpLength  = 1;
        public const float CpPerPixelLength  = 1.382f;
        public const int MaximumCpLength  = 650;
        public const int MinimumCpLength  = 1;
        public const bool TruncateMp  = false;
        public const bool ShowResourceVal  = true;
        #endregion

        #region Limit Break

        public const bool LimitBarEnabled  = true;
        public const bool LimitGaugeAlwaysShow  = false;
        public const bool LimitGaugeDiadem  = true;
        public const float LimitGaugePositionX  = -180;
        public const float LimitGaugePositionY  = 149;

        #endregion

        #region Experience

        public const bool ExpBarEnabled  = true;
        public const bool ExpValueTextEnabled = false;
        public const float ExpValueTextSize = 24;
        public const int ExpValueTextAlignment = 0;
        public const NumberFormatStyle ExpValueTextFormatStyle = NumberFormatStyle.NoFormatting;
        public const float ExpValueTextPositionX = 0;
        public const float ExpValueTextPositionY = 0;

        #endregion

        #region ClassInfo
        public const bool LevelEnabled  = true;
        public const bool ClassIconEnabled  = true;

        #endregion
        
        #region PartyInfo

        public const bool PartyEnabled = false;
        public const int PartyDisplayNumber = 2;
        public const int PartyXModifier = 0;
        public const int PartyYModifier = 0;
        public const float PartyXDistance = 0;
        public const float PartyYDistance = 0;

        #endregion
    }
}
