using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

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
            var textSize = ImGui.CalcTextSize(text);
            var x = ImGui.GetCursorPosX() + position.X - textSize.X / 2;
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
        public static void TextShadowedDrawList(ImDrawListPtr drawList, string text, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            var x = position.X;
            var y = position.Y;

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    if (i == 0 && j == 0) continue;
                    drawList.AddText(new Vector2(x + i, y + j), ImGui.GetColorU32(shadowColor), text);
                }
            }
            drawList.AddText(new Vector2(x, y), ImGui.GetColorU32(foregroundColor), text);
        }
        public static void TextShadowedDrawList(ImDrawListPtr drawList, float size, string text, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            var x = position.X;
            var y = position.Y;
            var font = ImGui.GetIO().FontDefault;

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    if (i == 0 && j == 0) continue;
                    drawList.AddText(font, size, new Vector2(x + i, y + j), ImGui.GetColorU32(shadowColor), text);
                }
            }
            drawList.AddText(font, size, new Vector2(x, y), ImGui.GetColorU32(foregroundColor), text);
        }

        public static void TextShadowedDrawList(ImDrawListPtr drawList, float size, int hAlign, string text, Vector2 position, Vector4 foregroundColor, Vector4 shadowColor, byte shadowWidth = 1)
        {
            var width = text.Length * size;
            var x = position.X - (hAlign == 2 ? width : hAlign == 1 ? width / 2f : 0);
            var y = position.Y;
            var font = ImGui.GetIO().FontDefault;

            for (var i = -shadowWidth; i < shadowWidth; i++)
            {
                for (var j = -shadowWidth; j < shadowWidth; j++)
                {
                    if (i == 0 && j == 0) continue;
                    drawList.AddText(font, size, new Vector2(x + i, y + j), ImGui.GetColorU32(shadowColor), text);
                }
            }
            drawList.AddText(font, size, new Vector2(x, y), ImGui.GetColorU32(foregroundColor), text);
        }
    }
}
