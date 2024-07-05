using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace KingdomHeartsPlugin.Utilities
{
    internal static class ImageDrawing
    {

        private static readonly Dictionary<uint, ISharedImmediateTexture> _iconTextures = new();
        internal static ISharedImmediateTexture GetSharedTexture(string path)
        {
            return KingdomHeartsPlugin.Tp.GetFromFile(path);
        }
        private static Vector2 ImRotate(Vector2 v, float cosA, float sinA)
        {
            return new Vector2(v.X * cosA - v.Y * sinA, v.X * sinA + v.Y * cosA);
        }
        public static void ImageRotated(ImDrawListPtr d, IntPtr texId, Vector2 position, Vector2 size, float angle, uint col = UInt32.MaxValue)
        {
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);
            Vector2[] pos =
            {
                position + ImRotate(new Vector2(-size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
                position + ImRotate(new Vector2(+size.X * 0.5f, -size.Y * 0.5f), cosA, sinA),
                position + ImRotate(new Vector2(+size.X * 0.5f, +size.Y * 0.5f), cosA, sinA),
                position + ImRotate(new Vector2(-size.X * 0.5f, +size.Y * 0.5f), cosA, sinA)
            };
            Vector2[] uvs = {
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            };

            d.AddImageQuad(texId, pos[0], pos[1], pos[2], pos[3], uvs[0], uvs[1], uvs[2], uvs[3], col);
        }
        public static void DrawImageRotated(ImDrawListPtr d, ISharedImmediateTexture texture, Vector2 position, Vector2 size, float angle, uint col = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var scaledSize = size * KingdomHeartsPlugin.Ui.Configuration.Scale;


            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);
            Vector2[] pos =
            {
                finalPosition + ImRotate(new Vector2(-scaledSize.X * 0.5f, -scaledSize.Y * 0.5f), cosA, sinA),
                finalPosition + ImRotate(new Vector2(+scaledSize.X * 0.5f, -scaledSize.Y * 0.5f), cosA, sinA),
                finalPosition + ImRotate(new Vector2(+scaledSize.X * 0.5f, +scaledSize.Y * 0.5f), cosA, sinA),
                finalPosition + ImRotate(new Vector2(-scaledSize.X * 0.5f, +scaledSize.Y * 0.5f), cosA, sinA)
            };
            Vector2[] uvs = {
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            };

            d.PushClipRect(finalPosition - scaledSize * 2, finalPosition + scaledSize * 2);
            d.AddImageQuad(texture.GetWrapOrEmpty().ImGuiHandle, pos[0], pos[1], pos[2], pos[3], uvs[0], uvs[1], uvs[2], uvs[3], col);
            d.PopClipRect();
        }


        /// <summary>
        /// Places a partial image at position relative to the base position of the attached interface object.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="size">(width, height)</param>
        /// <param name="position">(top, left)</param>
        /// <param name="imageArea">(left, top, width, height)</param>
        /// <param name="color"></param>
        public static void DrawImageArea(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 size, Vector2 position, Vector4 imageArea, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageWidth = image.GetWrapOrEmpty().Width;
            var imageHeight = image.GetWrapOrEmpty().Height;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;

            d.AddImage(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + size, finalPosition + new Vector2(imageArea.X / imageWidth, imageArea.Y / imageHeight), finalPosition + new Vector2((imageArea.X + imageArea.Z) / imageWidth,
                (imageArea.Y + imageArea.W) / imageHeight), color);
        }

        /// <summary>
        /// Places an image quad
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="ULPos">Upper Left Corner</param>
        /// <param name="URPos">Upper Right Corner</param>
        /// <param name="LRPos">Lower Right Corner</param>
        /// <param name="LLPos">Lower Left Corner</param>
        /// <param name="color"></param>
        public static void DrawImageQuad(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 position, Vector2 ULPos, Vector2 URPos, Vector2 LRPos, Vector2 LLPos, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale; 
            
            Vector2[] uvs = {
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            };

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImageQuad(image.GetWrapOrEmpty().ImGuiHandle, finalPosition + ULPos * KingdomHeartsPlugin.Ui.Configuration.Scale, finalPosition + new Vector2(imageSize.X, 0) + URPos * KingdomHeartsPlugin.Ui.Configuration.Scale, finalPosition + imageSize + LRPos * KingdomHeartsPlugin.Ui.Configuration.Scale, finalPosition + new Vector2(0, imageSize.Y) + LLPos * KingdomHeartsPlugin.Ui.Configuration.Scale, uvs[0], uvs[1], uvs[2], uvs[3], color);
            d.PopClipRect();
        }

        /// <summary>
        /// Places an image with specified scale multiplier
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="scale"></param
        /// <param name="color"></param>
        public static void DrawImageScaled(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 position, Vector2 scale, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height) * KingdomHeartsPlugin.Ui.Configuration.Scale * scale;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;

            Vector2[] uvs = {
                new(0.0f, 0.0f),
                new(1.0f, 0.0f),
                new(1.0f, 1.0f),
                new(0.0f, 1.0f)
            };

            d.PushClipRect(finalPosition - Vector2.One * 3, finalPosition + imageSize + Vector2.One * 3);
            d.AddImageQuad(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + new Vector2(imageSize.X, 0), finalPosition + imageSize, finalPosition + new Vector2(0, imageSize.Y), uvs[0], uvs[1], uvs[2], uvs[3], color);
            d.PopClipRect();
        }
        public static void DrawImageQuad(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 position, Vector2 ULPos, Vector2 URPos, Vector2 LRPos, Vector2 LLPos, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height);
            var finalPosition = basePosition + position;

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImageQuad(image.GetWrapOrEmpty().ImGuiHandle, finalPosition + ULPos, finalPosition + new Vector2(imageSize.X, 0) + URPos, finalPosition + imageSize + LRPos, finalPosition + new Vector2(0, imageSize.Y) + LLPos, uv1, uv2, uv3, uv4, color);
            d.PopClipRect();
        }

        /// <summary>
        /// Places an image relative to current draw list
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="position">additive position</param>
        /// <param name="color"></param>
        public static void DrawImage(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 position, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImage(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + imageSize, new Vector2(0,0), new Vector2(1,1), color);
            d.PopClipRect();
        }

        /// <summary>
        /// Places an image relative to current draw list
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="position">additive position, width and height</param>
        /// <param name="color"></param>
        public static void DrawImage(ImDrawListPtr d, ISharedImmediateTexture image, Vector4 position, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(position.Z, position.W) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var finalPosition = basePosition + new Vector2(position.X, position.Y) * KingdomHeartsPlugin.Ui.Configuration.Scale;

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImage(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + imageSize, new Vector2(0, 0), new Vector2(1, 1), color);
            d.PopClipRect();
        }

        /// <summary>
        /// Places an image relative to current draw list
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="scale">image scale</param>
        /// <param name="position">additive position</param>
        /// <param name="color">image color</param>
        public static void DrawImage(ImDrawListPtr d, ISharedImmediateTexture image, float scale, Vector2 position, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height) * KingdomHeartsPlugin.Ui.Configuration.Scale * scale;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImage(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + imageSize, new Vector2(0, 0), new Vector2(1, 1), color);
            d.PopClipRect();
        }

        /// <summary>
        /// Places an image relative to current draw list
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="position">additive position</param>
        /// <param name="imagePortion">Area which to draw in the image (0 - 1) (Left, Top, Right, Bottom)</param>
        /// <param name="color"></param>
        public static void DrawImage(ImDrawListPtr d, ISharedImmediateTexture image, Vector2 position, Vector4 imagePortion, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageSize = new Vector2(image.GetWrapOrEmpty().Width, image.GetWrapOrEmpty().Height) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var finalPosition = basePosition + position * KingdomHeartsPlugin.Ui.Configuration.Scale;

            d.PushClipRect(finalPosition - imageSize * 2, finalPosition + imageSize * 2);
            d.AddImage(image.GetWrapOrEmpty().ImGuiHandle, finalPosition, finalPosition + imageSize * new Vector2(1 - imagePortion.X, 1 - imagePortion.Y) * new Vector2(imagePortion.Z, imagePortion.W),
                new Vector2(imagePortion.X, imagePortion.Y), new Vector2(imagePortion.Z, imagePortion.W), color);
            d.PopClipRect();
        }

        internal static void DrawIcon(ImDrawListPtr d, uint icon, Vector2 size, Vector2 position)
        {
            var tex = GetIconImage(icon);
            if (tex != null && tex.GetWrapOrEmpty().ImGuiHandle != IntPtr.Zero)
            {
                var iconSize = new Vector2(tex.GetWrapOrEmpty().Width, tex.GetWrapOrEmpty().Height) * size;
                var imagePosition = position - new Vector2((int)Math.Floor(iconSize.X / 2f), (int)Math.Floor(iconSize.Y / 2f));
                DrawImage(d, tex, new Vector4(imagePosition.X, imagePosition.Y, iconSize.X, iconSize.Y));
            }
        }

        private static ISharedImmediateTexture? GetIconImage(uint icon, uint stackCount = 0)
        {
            GameIconLookup lookup = new GameIconLookup(icon, false, true);
            return KingdomHeartsPlugin.Tp.GetFromGameIcon(lookup);
        }

        public static void Dispose()
        {
        }
    }
}
