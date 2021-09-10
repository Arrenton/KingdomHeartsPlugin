using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KingdomHeartsPlugin.Utilities
{
    class ColorAddons
    {
        public static Vector3 Interpolate(Vector3 source, Vector3 target, float percent)
        {
            float r = source.X + (target.X - source.X) * percent;
            float g = source.Y + (target.Y - source.Y) * percent;
            float b = source.Z + (target.Z - source.Z) * percent;

            return new Vector3(r, g, b);
        }
    }
}
