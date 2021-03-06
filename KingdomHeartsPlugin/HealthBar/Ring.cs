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

        public void Draw(ImDrawListPtr drawList, float percent, Vector2 position, int segments, float scale = 1f)
        {
            if (segments < 1) segments = 1;
            if (segments > 4) segments = 4;

            int size = (int)Math.Ceiling(256 * scale);
            int sizeHalf = (int) Math.Floor(size / 2f);
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));
            drawList.PushClipRect(position, position + new Vector2(sizeHalf, sizeHalf));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(percent * 0.25f * segments, 0.25f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (segments < 2) return;

            drawList.PushClipRect(position + new Vector2(sizeHalf, 0), position + new Vector2(sizeHalf * 2, sizeHalf));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.25f), 0.5f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (segments < 3) return;

            drawList.PushClipRect(position + new Vector2(sizeHalf, sizeHalf), position + new Vector2(sizeHalf * 2, sizeHalf * 2));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.5f), 0.75f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (segments < 4) return;

            drawList.PushClipRect(position + new Vector2(0, sizeHalf), position + new Vector2(sizeHalf, sizeHalf * 2));

            ImageDrawing.ImageRotated(drawList, Image.ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + (Flip ? 0.5f : 0) + Math.Min(Math.Max(percent * 0.25f * segments, 0.75f), 1f)) * (float)Math.PI * 2, color);

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
