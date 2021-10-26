using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomHeartsPlugin
{
    public static class Extensions
    {
        public static string GetDescription(this Enum @enum)
        {
            var description = @enum.ToString();
            var fieldInfo = @enum.GetType().GetField(@enum.ToString());

            if (fieldInfo == null) return description;

            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs is { Length: > 0 })
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }

            return description;
        }
    }
}
