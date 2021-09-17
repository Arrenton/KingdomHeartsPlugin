using System.Numerics;

namespace KingdomHeartsPlugin.UIElements.LimitBreak
{
    public class Orb
    {
        public Orb()
        {
            Position = Vector2.Zero;
        }
        public Vector2 Position { get; set; }
        public float Angle { get; set; }
        public float Alpha { get; set; }
    }
}
