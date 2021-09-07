using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Utility;

namespace KingdomHeartsPlugin.Utilities
{
    internal static class ImageDrawing
    {

        private static readonly Dictionary<ushort, TextureWrap> IconTextures = new Dictionary<ushort, TextureWrap>();
        private static Vector2 ImRotate(Vector2 v, float cos_a, float sin_a)
        {
            return new Vector2(v.X * cos_a - v.Y * sin_a, v.X * sin_a + v.Y * cos_a);
        }
        public static void ImageRotated(ImDrawListPtr d, IntPtr tex_id, Vector2 position, Vector2 size, float angle, uint col = UInt32.MaxValue)
        {
            float cos_a = (float)Math.Cos(angle);
            float sin_a = (float)Math.Sin(angle);
            Vector2[] pos =
            {
                position + ImRotate(new Vector2(-size.X * 0.5f, -size.Y * 0.5f), cos_a, sin_a),
                position + ImRotate(new Vector2(+size.X * 0.5f, -size.Y * 0.5f), cos_a, sin_a),
                position + ImRotate(new Vector2(+size.X * 0.5f, +size.Y * 0.5f), cos_a, sin_a),
                position + ImRotate(new Vector2(-size.X * 0.5f, +size.Y * 0.5f), cos_a, sin_a)
            };
            Vector2[] uvs = {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            d.AddImageQuad(tex_id, pos[0], pos[1], pos[2], pos[3], uvs[0], uvs[1], uvs[2], uvs[3], col);
        }


        /// <summary>
        /// Places an image at position relative to the base position of the attached interface object.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="image"></param>
        /// <param name="size">(width, height)</param>
        /// <param name="position">(top, left)</param>
        /// <param name="imageArea">(left, top, width, height)</param>
        public static void DrawImage(ImDrawListPtr d, TextureWrap image, Vector2 size, Vector2 position, Vector4 imageArea, uint color = UInt32.MaxValue)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            var finalPosition = basePosition + position;

            d.AddImage(image.ImGuiHandle, finalPosition, finalPosition + size, finalPosition + new Vector2(imageArea.X / imageWidth, imageArea.Y / imageHeight), finalPosition + new Vector2((imageArea.X + imageArea.Z) / imageWidth,
                (imageArea.Y + imageArea.W) / imageHeight), color);
        }



        internal static void DrawIcon(ImDrawListPtr d, ushort icon, Vector2 size, Vector2 position)
        {
            if (icon is >= 65000 or <= 62000) return;

            if (IconTextures.ContainsKey(icon))
            {
                var tex = IconTextures[icon];
                if (tex != null && tex.ImGuiHandle != IntPtr.Zero)
                {
                    var iconSize = new Vector2(IconTextures[icon].Width, IconTextures[icon].Height) * size;
                    DrawImage(d, IconTextures[icon], iconSize, position - new Vector2((int)Math.Floor(iconSize.X / 2f), (int)Math.Floor(iconSize.Y / 2f)), new Vector4(0, 0, IconTextures[icon].Width, IconTextures[icon].Height));
                }
            }
            else
            {

                IconTextures[icon] = null;

                Task.Run(() => {
                    try
                    {
                        var iconTex = KingdomHeartsPlugin.Dm.GetIcon(icon);
                        var tex = KingdomHeartsPlugin.Pi.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);
                        if (tex != null && tex.ImGuiHandle != IntPtr.Zero)
                        {
                            IconTextures[icon] = tex;
                        }
                    }
                    catch
                    {
                        // Ignore
                    }
                });
            }
        }

        public static void Dispose()
        {
            foreach (var tex in IconTextures)
            {
                tex.Value?.Dispose();
            }

            IconTextures.Clear();
        }
    }
}
