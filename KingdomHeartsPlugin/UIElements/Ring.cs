using System;
using System.IO;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements
{
    internal class Ring : IDisposable
    {
        public Ring(string image, float colorR = 1f, float colorG = 1f, float colorB = 1f, float alpha = 1f)
        {
            ImagePath = image;
            Color = new Vector3(colorR, colorG, colorB);
            Alpha = alpha;
        }

        public void Draw(ImDrawListPtr drawList, float percent, Vector2 position, int segments, float scale = 1f)
        {
            if (segments < 1) segments = 1;
            if (segments > 4) segments = 4;

            float size = (256 * scale);
            float sizeHalf = (size / 2f);
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));
            drawList.PushClipRect(position, position + new Vector2(sizeHalf, sizeHalf + 1));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + Math.Min(percent * 0.25f * segments, 0.25f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (segments < 2) return;
            if (percent * 0.25f * segments < 0.25f) return;

            drawList.PushClipRect(position + new Vector2(sizeHalf - 1, 0), position + new Vector2(sizeHalf * 2 + 2, sizeHalf));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + Math.Min(Math.Max(percent * 0.25f * segments, 0.25f), 0.5f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (segments < 3) return;
            if (percent * 0.25f * segments < 0.5f) return;

            drawList.PushClipRect(position + new Vector2(sizeHalf - 1, sizeHalf - 1), position + new Vector2(sizeHalf * 2 + 2, sizeHalf * 2  + 2));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + Math.Min(Math.Max(percent * 0.25f * segments, 0.5f), 0.75f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();
            
            if (segments < 4) return;
            if (percent * 0.25f * segments < 0.75f) return;

            drawList.PushClipRect(position + new Vector2(-1, sizeHalf - 1), position + new Vector2(sizeHalf + 2, sizeHalf * 2 + 2));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f + Math.Min(Math.Max(percent * 0.25f * segments, 0.75f), 1f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();
        }
        public void DrawLeftHalf(ImDrawListPtr drawList, float percent, Vector2 position, float scale = 1f)
        {
            //to say this is a messy calculation is an understatement, but it works
            var segments = 2;
            float size = (256 * scale);
            float sizeHalf = (size / 2f);
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));
            
            //upper
            drawList.PushClipRect(position + new Vector2(-1, sizeHalf - 1), position + new Vector2(sizeHalf + 2, sizeHalf * 2 + 2));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (0.5f * Math.Min(percent, 0.5f) - 0.5f) * (float)Math.PI * 2, color);

            drawList.PopClipRect();

            if (percent >= .5f){
                drawList.PushClipRect(position, position + new Vector2(sizeHalf, sizeHalf + 1));

                ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (0.5f * Math.Min(Math.Max(percent, 0.5f), 1f) - 0.5f) * (float)Math.PI * 2, color);

                drawList.PopClipRect();
            }
        }
        
        public void DrawRightHalf(ImDrawListPtr drawList, float percent, Vector2 position, float scale = 1f)
        {
            //to say this is a messy calculation is an understatement, but it works
            var segments = 2;
            float size = (256 * scale);
            float sizeHalf = (size / 2f);
            percent = Math.Max(percent > 0 ? 0.002f : 0, percent);
            var color = ImGui.GetColorU32(new Vector4(Color.X, Color.Y, Color.Z, Alpha));
            drawList.PushClipRect(position + new Vector2(sizeHalf - 1, sizeHalf - 1), position + new Vector2(sizeHalf * 2 + 2, sizeHalf * 2  + 2));

            ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), -(-0.75f + Math.Min(percent * 0.25f * segments, 0.25f)) * (float)Math.PI * 2, color);

            drawList.PopClipRect();
            
            if (percent > .5f){
                drawList.PushClipRect(position + new Vector2(sizeHalf - 1, 0), position + new Vector2(sizeHalf * 2 + 2, sizeHalf));

                ImageDrawing.ImageRotated(drawList, Image.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + sizeHalf, position.Y + sizeHalf), new Vector2(size, size), (-0.25f - Math.Min(Math.Max(percent * 0.5f, 0.25f), 0.5f)) * (float)Math.PI*2 , color);

                drawList.PopClipRect();
            }
        }

        public void Dispose()
        {
        }

        private ISharedImmediateTexture Image
        {
            get => ImageDrawing.GetSharedTexture(this.ImagePath);
        }
        private string ImagePath { get; }
        internal Vector3 Color { get; set; }
        internal float Alpha { get; set; }
    }
}
