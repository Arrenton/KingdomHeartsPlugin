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
        public static string FormatDigits(uint val, NumberFormatStyle formatStyle)
        {
            switch (formatStyle)
            {
                case NumberFormatStyle.ThousandsSeparator:
                    return $"{val:#,##0}";
                case NumberFormatStyle.TruncateTenThousands:
                {
                    float floatVal = val >= 10000 ? val / 1000f : val;

                    return val >= 10000 ? val >= 100000 ? $"{floatVal:0}K" : $"{floatVal:0.#}K" : $"{floatVal}";
                }
                case NumberFormatStyle.TruncateTenThousandsAndSeparator:
                {
                    float floatVal = val >= 10000 ? val / 1000f : val;

                    return val >= 10000 ? val >= 100000 ? $"{floatVal:#,##0}K" : $"{floatVal:#,##0.#}K" : $"{floatVal}";
                }
                case NumberFormatStyle.NoFormatting:
                    return $"{val}";
                default:
                    return $"{val}";
            }
        }
    }
}
