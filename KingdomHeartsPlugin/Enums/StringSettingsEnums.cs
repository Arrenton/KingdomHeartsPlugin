using System.ComponentModel;

namespace KingdomHeartsPlugin.Enums
{
    public enum NumberFormatStyle
    {
        [Description("No Formatting")]
        NoFormatting,
        [Description("Large Number Separators")]
        ThousandsSeparator,
        [Description("Small Numbers")]
        SmallNumber,
        [Description("Small Numbers One Decimal")]
        SmallNumberOneDecimalPrecision,
        [Description("Small Numbers Two Decimal")]
        SmallNumberTwoDecimalPrecision
    }

    public enum TextAlignment
    {
        Center,
        Left,
        Right
    }
}
