using System.Numerics;
using Dalamud.Bindings.ImGui;
using KingdomHeartsPlugin.Enums;

namespace KingdomHeartsPlugin.Utilities
{
    public static class ImGuiAdditions
    {

        public static void TextShadowed(string text, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            var x = ImGui.GetCursorPosX();
            var y = ImGui.GetCursorPosY();

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    ImGui.SetCursorPosX(x + i);
                    ImGui.SetCursorPosY(y + j);
                    ImGui.TextColored(shadowColor, text);
                }
            }
            ImGui.SetCursorPosX(x);
            ImGui.SetCursorPosY(y);
            ImGui.TextColored(foregroundColor, text);
        }
        public static void TextShadowed(string text, float size, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            ImGui.SetWindowFontScale(size);
            var x = ImGui.GetCursorPosX() + position.X;
            var y = ImGui.GetCursorPosY() + position.Y;

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    ImGui.SetCursorPosX(x + i);
                    ImGui.SetCursorPosY(y + j);
                    ImGui.TextColored(shadowColor, text);
                }
            }
            ImGui.SetCursorPosX(x);
            ImGui.SetCursorPosY(y);
            ImGui.TextColored(foregroundColor, text);
            ImGui.SetWindowFontScale(1);
        }

        public static void TextCenteredShadowed(string text, float size, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            ImGui.SetWindowFontScale(size);
            float fontSize = ImGui.GetFontSize() * text.Length * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var y = position.Y;
            var x = position.X - fontSize;


            TextShadowedDrawList(ImGui.GetWindowDrawList(),
                KingdomHeartsPlugin.Ui.Configuration.ResourceTextSize * KingdomHeartsPlugin.Ui.Configuration.Scale,
                $"{fontSize} Pos: {x},{y}",
                ImGui.GetItemRectMin(),
                new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);

            ImGui.PushClipRect(
                ImGui.GetItemRectMin() - new Vector2(x + shadowWidth + fontSize, y + shadowWidth + ImGui.GetFontSize()),
                ImGui.GetItemRectMin() + new Vector2(x + shadowWidth + fontSize, y + shadowWidth + ImGui.GetFontSize()), 
                true);

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    ImGui.SetCursorPosX(i);
                    ImGui.SetCursorPosY(position.Y + j);
                    ImGui.TextColored(shadowColor, text);
                }
            }

            ImGui.SetCursorPosX(x);
            ImGui.SetCursorPosY(y);
            ImGui.TextColored(foregroundColor, text);
            ImGui.PopClipRect();
            ImGui.SetWindowFontScale(1);
        }

        public static void TextShadowedDrawList(ImDrawListPtr drawList, float size, string text, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1, TextAlignment alignment = TextAlignment.Left)
        {
            var sizeVector = new Vector2(size * text.Length, size * text.Length) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var x = alignment switch
            {
                TextAlignment.Center => position.X - sizeVector.X / 4.55f,
                TextAlignment.Right => position.X - sizeVector.X / 2.6f,
                TextAlignment.Left => position.X,
                _ => position.X
            };

            var y = position.Y;
            var font = ImGui.GetIO().FontDefault;

            drawList.PushClipRect(new Vector2(x, y) - sizeVector * text.Length, new Vector2(x, y) + sizeVector * text.Length);

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    if (i == 0 && j == 0) continue;
                    drawList.AddText(font, size * KingdomHeartsPlugin.Ui.Configuration.Scale, new Vector2(x + i * KingdomHeartsPlugin.Ui.Configuration.Scale, y + j * KingdomHeartsPlugin.Ui.Configuration.Scale), ImGui.GetColorU32(shadowColor), text);
                }
            }
            drawList.AddText(font, size * KingdomHeartsPlugin.Ui.Configuration.Scale, new Vector2(x, y), ImGui.GetColorU32(foregroundColor), text);
            drawList.PopClipRect();
        }
    }
}
