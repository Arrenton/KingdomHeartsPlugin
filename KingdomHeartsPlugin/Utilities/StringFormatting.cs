using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Utilities
{
    public static class StringFormatting
    {
        public static string FormatDigits(uint val, NumberFormatStyle formatStyle = NumberFormatStyle.NoFormatting)
        {
            if (formatStyle == NumberFormatStyle.NoFormatting) return $"{val}";

            if (formatStyle == NumberFormatStyle.ThousandsSeparator) return val.ToString("N0", KingdomHeartsPlugin.GetCulture());

            var stringFormat = formatStyle switch
            {
                NumberFormatStyle.SmallNumberOneDecimalPrecision => "F1",
                NumberFormatStyle.SmallNumberTwoDecimalPrecision => "F2",
                _ => "F0"
            };

            return val switch
            {
                >= 1000000 => $"{(val / 1000000f).ToString(stringFormat, KingdomHeartsPlugin.GetCulture())}M",
                >= 10000 => $"{(val / 1000f).ToString(stringFormat, KingdomHeartsPlugin.GetCulture())}K",
                _ => $"{val}"
            };
        }
        public static string FormatDigits(int val, NumberFormatStyle formatStyle = NumberFormatStyle.NoFormatting)
        {
            if (formatStyle == NumberFormatStyle.NoFormatting) return $"{val}";

            if (formatStyle == NumberFormatStyle.ThousandsSeparator) return val.ToString("N0", KingdomHeartsPlugin.GetCulture());

            var stringFormat = formatStyle switch
            {
                NumberFormatStyle.SmallNumberOneDecimalPrecision => "F1",
                NumberFormatStyle.SmallNumberTwoDecimalPrecision => "F2",
                _ => "F0"
            };

            return val switch
            {
                >= 1000000 => $"{(val / 1000000f).ToString(stringFormat, KingdomHeartsPlugin.GetCulture())}M",
                >= 10000 => $"{(val / 1000f).ToString(stringFormat, KingdomHeartsPlugin.GetCulture())}K",
                _ => $"{val}"
            };
        }
    }
}
