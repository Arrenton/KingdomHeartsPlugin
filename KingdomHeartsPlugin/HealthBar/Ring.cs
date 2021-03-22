using System;
using System.Numerics;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.HealthBar
{
    internal class Ring : IDisposable
    {
        public Ring(TextureWrap image, float colorR = 1f, float colorG = 1f, float colorB = 1f, float alpha = 1f)
        {
            Image = image;
            Color = new Vector3(colorR, colorG, colorB);
            Alpha = alpha;
            Flip = false;
        }

        public void Draw(ImDrawListPtr drawList, float percent, Vector2 position)
        {
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));
            drawList.PushClipRect(position, position + new Vector2(128, 128));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + 128, position.Y + 128), new Vector2(Image.Width, Image.Height), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(percent * 0.75f, 0.25f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            drawList.PushClipRect(position + new Vector2(128, 0), position + new Vector2(128 * 2, 128));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + 128, position.Y + 128), new Vector2(Image.Width, Image.Height), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.75f, 0.25f), 0.5f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            drawList.PushClipRect(position + new Vector2(128, 128), position + new Vector2(128 * 2, 128 * 2));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + 128, position.Y + 128), new Vector2(Image.Width, Image.Height), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.75f, 0.5f), 0.75f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();
        }
        public void Dispose()
        {
        }

        private TextureWrap Image { get; }
        internal Vector3 Color { get; set; }
        internal float Alpha { get; set; }
        internal bool Flip { get; set; }
    }
}
