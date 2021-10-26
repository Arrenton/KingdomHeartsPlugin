using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
