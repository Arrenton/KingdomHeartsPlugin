using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using KingdomHeartsPlugin.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;

namespace KingdomHeartsPlugin.Configuration
{
    public partial class Defaults
    {
        public const float LevelTextX = 132;
        public const float LevelTextY = 81;
        public const float LevelTextSize = 32;
        public const TextAlignment LevelTextAlignment = TextAlignment.Center;

        public const float ClassIconX = 128;
        public const float ClassIconY = 150;
        public const float ClassIconScale = 1.0f;
    }

    public partial class Settings
    {
        public float LevelTextX { get; set; } = Defaults.LevelTextX;
        public float LevelTextY { get; set; } = Defaults.LevelTextY;
        public float LevelTextSize { get; set; } = Defaults.LevelTextSize;
        public TextAlignment LevelTextAlignment { get; set; } = Defaults.LevelTextAlignment;
        public float ClassIconX { get; set; } = Defaults.ClassIconX;
        public float ClassIconY { get; set; } = Defaults.ClassIconY;
        public float ClassIconScale { get; set; } = Defaults.ClassIconScale;
    }
}

namespace KingdomHeartsPlugin.UIElements.Experience
{
    public class ClassBar
    {
        private ISharedImmediateTexture _expBarSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"));
        }
        private ISharedImmediateTexture _expColorlessBarSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"));
        }
        private ISharedImmediateTexture _expBarBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_outline.png"));
        }
        private IntPtr _expAddonPtr;

        public ClassBar()
        {
            ExperienceRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"));
            ExperienceRingRest = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_segment.png"), alpha: 0.25f);
            ExperienceRingGain = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"), 0.65f, 0.92f, 1.00f);
            ExperienceRingBg = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\Experience\ring_experience_colorless_segment.png"), 0.07843f, 0.07843f, 0.0745f);
        }

        private void Update(IPlayerCharacter player)
        {
            _expAddonPtr = KingdomHeartsPlugin.Gui.GetAddonByName("_Exp", 1);

            try
            {
                var current = Marshal.ReadInt32(_expAddonPtr + 0x278);
                var max = Marshal.ReadInt32(_expAddonPtr + 0x27C);
                int rest = Marshal.ReadInt32(_expAddonPtr + 0x280);
                UpdateExperience(current, max, rest, player.ClassJob.Id, player.Level);
            }
            catch
            {
                try
                {
                    _expAddonPtr = KingdomHeartsPlugin.Gui.GetAddonByName("_Exp", 1);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private void UpdateExperience(int exp, int maxExp, int rest, uint job, byte level)
        {
            if (LastLevel < level)
            {
                ExpTemp = 0;
                LastExp = 0;
            }

            if (LastJob != job)
            {
                ExpTemp = exp;
                LastExp = exp;
                ExpGainTime = 0;
            }

            if (LastExp > exp) ExpTemp = exp;

            if (LastExp < exp) GainExperience(LastExp);

            UpdateGainedExperience(exp);

            LastExp = exp;
            LastJob = job;
            LastLevel = level;
            Experience = exp;
            MaxExperience = maxExp;
            RestedBonusExperience = rest;
        }

        private void GainExperience(int exp)
        {
            if (ExpGainTime <= 0)
            {
                ExpBeforeGain = exp;
                ExpTemp = exp;
            }

            ExpGainTime = 3f;
        }

        private void UpdateGainedExperience(int exp)
        {
            if (ExpGainTime > 0)
            {
                ExpGainTime -= 1 * KingdomHeartsPlugin.UiSpeed;
            }
            else if (ExpTemp < exp)
            {
                ExpTemp += (exp - ExpBeforeGain) * KingdomHeartsPlugin.UiSpeed;
            }

            if (ExpTemp > exp)
                ExpTemp = exp;
        }

        public void Draw(IPlayerCharacter player, float healthY)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();

            int size = (int)Math.Ceiling(256 * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)(healthY * KingdomHeartsPlugin.Ui.Configuration.Scale));

            if (KingdomHeartsPlugin.Ui.Configuration.ExpBarEnabled)
            {

                ExperienceRingBg.Draw(drawList, 1, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

                ExperienceRingRest.Draw(drawList, (Experience + RestedBonusExperience) / (float)MaxExperience, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

                ExperienceRingGain.Draw(drawList, Experience / (float)MaxExperience, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

                ExperienceRing.Draw(drawList, ExpTemp / MaxExperience, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

                drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
                drawList.AddImage(_expBarBaseTexture.GetWrapOrEmpty().ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
                drawList.PopClipRect();
            }

            Portrait.Draw(healthY);

            if (KingdomHeartsPlugin.Ui.Configuration.ClassIconEnabled)
            {
                float iconSize = KingdomHeartsPlugin.Ui.Configuration.ClassIconScale;

                if (KingdomHeartsPlugin.Cs.LocalPlayer is null) return;

                
                ImageDrawing.DrawIcon(drawList, (ushort)(62000 + KingdomHeartsPlugin.Cs.LocalPlayer.ClassJob.Id),
                    new Vector2(iconSize, iconSize),
                    //new Vector2((int)(size / 2f), (int)(size / 2f + 18 * KingdomHeartsPlugin.Ui.Configuration.Scale)) +
                    new Vector2((int)(KingdomHeartsPlugin.Ui.Configuration.ClassIconX), (int)(KingdomHeartsPlugin.Ui.Configuration.ClassIconY)) +
                    new Vector2(0, (int)(healthY * KingdomHeartsPlugin.Ui.Configuration.ClassIconScale * KingdomHeartsPlugin.Ui.Configuration.Scale)));
            }

            if (KingdomHeartsPlugin.Ui.Configuration.LevelEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPlugin.Ui.Configuration.LevelTextSize,
                    $"Lv{KingdomHeartsPlugin.Cs.LocalPlayer.Level}",
                    drawPosition + new Vector2(KingdomHeartsPlugin.Ui.Configuration.LevelTextX, KingdomHeartsPlugin.Ui.Configuration.LevelTextY) * KingdomHeartsPlugin.Ui.Configuration.Scale,
                    new Vector4(249 / 255f, 247 / 255f, 232 / 255f, 0.9f),
                    new Vector4(96 / 255f, 78 / 255f, 23 / 255f, 0.25f), 3,
                    KingdomHeartsPlugin.Ui.Configuration.LevelTextAlignment);

            if (KingdomHeartsPlugin.Ui.Configuration.ExpValueTextEnabled)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPlugin.Ui.Configuration.ExpValueTextSize,
                    $"{StringFormatting.FormatDigits(Experience, KingdomHeartsPlugin.Ui.Configuration.ExpValueTextFormatStyle)} / {StringFormatting.FormatDigits(MaxExperience, KingdomHeartsPlugin.Ui.Configuration.ExpValueTextFormatStyle)}",
                    drawPosition + new Vector2(KingdomHeartsPlugin.Ui.Configuration.ExpValueTextPositionX, KingdomHeartsPlugin.Ui.Configuration.ExpValueTextPositionY),
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f),
                    3,
                    (TextAlignment)KingdomHeartsPlugin.Ui.Configuration.ExpValueTextAlignment);
        }

        public void Dispose()
        {
            ExperienceRing.Dispose();
            ExperienceRingRest.Dispose();
            ExperienceRingGain.Dispose();
            ExperienceRingBg.Dispose();

            ExperienceRing = null;
            ExperienceRingRest = null;
            ExperienceRingGain = null;
            ExperienceRingBg = null;
            _expAddonPtr = IntPtr.Zero;
        }

        private int Experience { get; set; }
        private int RestedBonusExperience { get; set; }
        private int MaxExperience { get; set; }
        private int LastExp { get; set; }
        private uint LastJob { get; set; }
        private byte LastLevel { get; set; }
        private int ExpBeforeGain { get; set; }
        private float ExpTemp { get; set; }
        private float ExpGainTime { get; set; }
        private Ring ExperienceRing { get; set; }
        private Ring ExperienceRingRest { get; set; }
        private Ring ExperienceRingGain { get; set; }
        private Ring ExperienceRingBg { get; set; }
    }
}
