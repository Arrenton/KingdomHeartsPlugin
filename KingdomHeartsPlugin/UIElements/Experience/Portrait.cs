using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.Configuration
{
    public partial class Defaults
    {
        public const float PortraitX = 0;
        public const float PortraitY = 0;
        public const float PortraitScale = 1f;
        public const string PortraitNormalImage = "";
        public const string PortraitHurtImage = "";
        public const string PortraitDangerImage = "";
        public const string PortraitCombatImage = "";
    }

    public partial class Settings
    {
        public float PortraitX { get; set; } = Defaults.PortraitX;
        public float PortraitY { get; set; } = Defaults.PortraitY;
        public float PortraitScale { get; set; } = Defaults.PortraitScale;
        public string PortraitNormalImage { get; set; } = Defaults.PortraitNormalImage;
        public string PortraitHurtImage { get; set; } = Defaults.PortraitHurtImage;
        public string PortraitDangerImage { get; set; } = Defaults.PortraitDangerImage;
        public string PortraitCombatImage { get; set; } = Defaults.PortraitCombatImage;
    }
}

namespace KingdomHeartsPlugin.UIElements.Experience
{
    public static class Portrait
    {
        public static void SetAllPortraits()
        {
            SetPortraitNormal(KingdomHeartsPlugin.Ui.Configuration.PortraitNormalImage);
            SetPortraitHurt(KingdomHeartsPlugin.Ui.Configuration.PortraitHurtImage);
            SetPortraitDanger(KingdomHeartsPlugin.Ui.Configuration.PortraitDangerImage);
            SetPortraitCombat(KingdomHeartsPlugin.Ui.Configuration.PortraitCombatImage);
        }

        public static void SetPortraitNormal(string path)
        {
            PortraitNormal?.Dispose();
            PortraitNormal = GetTexture(path);
        }

        public static void SetPortraitHurt(string path)
        {
            PortraitHurt?.Dispose();
            PortraitHurt = GetTexture(path);
        }
        public static void SetPortraitDanger(string path)
        {
            PortraitDanger?.Dispose();
            PortraitDanger = GetTexture(path);
        }
        public static void SetPortraitCombat(string path)
        {
            PortraitCombat?.Dispose();
            PortraitCombat = GetTexture(path);
        }

        private static TextureWrap GetTexture(string path)
        {
            if (path.IsNullOrEmpty() || !File.Exists(path)) return null;

            try
            {
                return KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(path);
            }
            catch
            {
                Dalamud.Logging.PluginLog.Warning($"Could not load image for portrait at: \"{path}\"");
                return null;
            }
        }

        public static void Draw(float healthY)
        {
            if (KingdomHeartsPlugin.Cs.LocalPlayer == null) return;

            var drawList = ImGui.GetWindowDrawList();
            var drawPosition = new Vector2(KingdomHeartsPlugin.Ui.Configuration.PortraitX, KingdomHeartsPlugin.Ui.Configuration.PortraitY + healthY * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var damagedAlpha = KingdomHeartsPlugin.Ui.HealthFrame.DamagedHealthAlpha;
            var lowHealthAlpha = KingdomHeartsPlugin.Ui.HealthFrame.LowHealthAlpha;
            var dangerStatus = KingdomHeartsPlugin.Cs.LocalPlayer.CurrentHp <= KingdomHeartsPlugin.Cs.LocalPlayer.MaxHp * (KingdomHeartsPlugin.Ui.Configuration.LowHpPercent / 100f);
            var portraitDangerAlpha = dangerStatus ? 1 : 0;
            var inCombat = (KingdomHeartsPlugin.Cs.LocalPlayer.StatusFlags & StatusFlags.InCombat) == StatusFlags.InCombat;

            //ImGuiAdditions.TextShadowedDrawList(drawList,24, $"{KingdomHeartsPlugin.Cs.LocalPlayer.StatusFlags}", ImGui.GetItemRectMin() + new Vector2(0,0), new Vector4(1, 1, 1, 1), new Vector4(0,0,0,1));

            if (damagedAlpha > 0.595f && PortraitHurt != null)
            {
                ImageDrawing.DrawImage(drawList, PortraitHurt, KingdomHeartsPlugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
            else if (dangerStatus && PortraitDanger != null)
            {
                ImageDrawing.DrawImage(drawList, PortraitDanger, KingdomHeartsPlugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 0.2f, 0.2f, 1)));
            }
            else if (inCombat && PortraitCombat != null)
            {
                ImageDrawing.DrawImage(drawList, PortraitCombat, KingdomHeartsPlugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
            else if (PortraitNormal != null)
            {
                ImageDrawing.DrawImage(drawList, PortraitNormal, KingdomHeartsPlugin.Ui.Configuration.PortraitScale, drawPosition, ImGui.GetColorU32(new Vector4(1 - lowHealthAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1 - damagedAlpha - portraitDangerAlpha, 1)));
            }
        }

        public static void Dispose()
        {
            PortraitNormal?.Dispose();
            PortraitHurt?.Dispose();
            PortraitDanger?.Dispose();
            PortraitCombat?.Dispose();
        }


        private static TextureWrap PortraitNormal { get; set; }
        private static TextureWrap PortraitHurt { get; set; }
        private static TextureWrap PortraitDanger { get; set; }
        private static TextureWrap PortraitCombat { get; set; }
    }
}
