using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Data.LuminaExtensions;
using Dalamud.Game.Internal.Gui.Addon;

namespace KingdomHeartsPlugin.HealthBar
{
    class HealthFrame : IDisposable
    {
        private float _verticalAnimationTicks;
        private IntPtr _expAddonPtr;

        private enum ParameterType
        {
            Mp,
            Gp,
            Cp
        }

        public HealthFrame(DalamudPluginInterface pi, PluginUI ui)
        {
            Ui = ui;
            Pi = pi;
            HealthY = 0;
            _verticalAnimationTicks = 0;
            HealthVerticalSpeed = 0f;
            Timer = Stopwatch.StartNew();
            _expAddonPtr = Pi.Framework.Gui.GetUiObjectByName("_Exp", 1);
            var imagesPath = KingdomHeartsPlugin.TemplateLocation;
            HealthRingSegmentTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_health_segment.png"));
            RingValueSegmentTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_value_segment.png"));
            RingOutlineTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_outline_segment.png"));
            RingTrackTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_track.png"));
            RingBaseTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_base_edge.png"));
            RingEndTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_end_edge.png"));
            RingExperienceTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_experience_segment.png"));
            RingExperienceBgTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_experience_outline.png"));
            BarTextures = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\bar_textures.png"));
            HealthRingBg = new Ring(RingValueSegmentTexture, 0.07843f, 0.07843f, 0.0745f);
            HealthLostRing = new Ring(RingValueSegmentTexture, 1, 0, 0);
            RingOutline = new Ring(RingOutlineTexture);
            ExperienceRing = new Ring(RingExperienceTexture);
            ExperienceRingRest = new Ring(RingExperienceTexture, alpha: 0.25f);
            ExperienceRingGain = new Ring(RingExperienceTexture, 0.65f, 0.92f, 1.00f) { Flip = true };
            ExperienceRingBg = new Ring(RingExperienceTexture, 0.07843f, 0.07843f, 0.0745f) {Flip = true};
            HealthRing = new Ring(HealthRingSegmentTexture);
            HealthRestoredRing = new Ring(HealthRingSegmentTexture) {Flip = true};
        }

        public void Draw(PlayerCharacter player)
        {
            Addon _parameterWidget = Pi.Framework.Gui.GetAddonByName("_ParameterWidget", 1);

            if (_parameterWidget != null)
            {
                // Do not do or draw anything if the parameter widget is not visible
                if (!_parameterWidget.Visible)
                {
                    Timer.Restart();
                    return;
                }
            }

            // Do not do or draw anything if player is null or game ui is hidden
            if (player is null || Pi.Framework.Gui.GameUiHidden)
            {
                Timer.Restart();
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            UiSpeed = Timer.ElapsedMilliseconds / 1000f;

            UpdateHealth(player);
            
            ImGui.Dummy(new Vector2(220, 256));

            try
            {
                var current = Marshal.ReadInt32(_expAddonPtr + 0x278);
                var max = Marshal.ReadInt32(_expAddonPtr + 0x27C);
                UpdateExperience(current, max, player.ClassJob.Id, player.Level);
                DrawExperience(drawList, current, max);
            }
            catch
            {
                try
                {
                    _expAddonPtr = Pi.Framework.Gui.GetUiObjectByName("_Exp", 1);
                }
                catch
                {
                    // ignored
                }
            }

            DrawHealth(drawList, player.CurrentHp, player.MaxHp);

            if (player.MaxMp > 0)
            {
                DrawParameterResourceBar(drawList, ParameterType.Mp, player.CurrentMp, player.MaxMp);
            }
            else if (player.MaxCp > 0)
            {
                DrawParameterResourceBar(drawList, ParameterType.Cp, player.CurrentCp, player.MaxCp);
            }
            else if (player.MaxGp > 0)
            {
                DrawParameterResourceBar(drawList, ParameterType.Gp, player.CurrentGp, player.MaxGp);
            }

            // Draw HP Value
            ImGui.SameLine(0, 0);
            float hp = (Ui.Configuration.TruncateHp && player.CurrentHp >= 10000) ? player.CurrentHp / 1000f : player.CurrentHp;
            string hpVal = (Ui.Configuration.TruncateHp && player.CurrentHp >= 10000)
                ? player.CurrentHp >= 100000 ? $"{hp:0}K" : $"{hp:0.#}K"
                : $"{hp}";
            ImGuiAdditions.TextCenteredShadowed(hpVal, 1.25f * Ui.Configuration.Scale, new Vector2(-196 + 22 * (Ui.Configuration.Scale - 1), 128 * Ui.Configuration.Scale), new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);
            
            Timer.Restart();
        }

        private void UpdateExperience(int exp, int maxExp, uint job, byte level)
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
                ExpGainTime -= 1 * UiSpeed;
            }
            else if (ExpTemp < exp)
            {
                ExpTemp += (exp - ExpBeforeGain) * UiSpeed;
            }

            if (ExpTemp > exp)
                ExpTemp = exp;
        }

        private void UpdateHealth(PlayerCharacter player)
        {
            if (LastHp > player.CurrentHp)
                DamagedHealth(LastHp);
            if (LastHp < player.CurrentHp)
                RestoredHealth(LastHp);
            
            UpdateDamagedHealth();

            UpdateRestoredHealth(player.CurrentHp);

            LastHp = player.CurrentHp;
        }

        private void DamagedHealth(int health)
        {
            DamagedHealthAlpha = 1f;
            HealthY = 0;
            HealthVerticalSpeed = -3;
            HpBeforeDamaged = health;
        }

        private void RestoredHealth(int health)
        {
            if (HealthRestoreTime <= 0)
            {
                HpTemp = health;
                HpBeforeRestored = health;
            }

            HealthRestoreTime = 1f;
        }

        private void UpdateRestoredHealth(int currentHp)
        {
            if (HealthRestoreTime > 0)
            {
                HealthRestoreTime -= 1 * UiSpeed;
            }
            else if (HpTemp < currentHp)
            {
                HpTemp += (currentHp - HpBeforeRestored) * UiSpeed;
                if (HpBeforeRestored > currentHp)
                    HpBeforeRestored = currentHp;
            }

            if (HpTemp > currentHp)
                HpTemp = currentHp;
        }

        private void UpdateDamagedHealth()
        {
            if (DamagedHealthAlpha > 0.97f)
            {
                DamagedHealthAlpha -= 0.09f * UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.6f)
            {
                DamagedHealthAlpha -= 0.8f * UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.59f)
            {
                DamagedHealthAlpha -= 0.005f * UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.0f)
            {
                DamagedHealthAlpha -= 1f * UiSpeed;
            }

            // Vertical wobble
            _verticalAnimationTicks += 240 * UiSpeed;

            while (_verticalAnimationTicks > 1)
            {
                _verticalAnimationTicks--;
                HealthY += HealthVerticalSpeed;

                if (HealthY > 3)
                {
                    HealthVerticalSpeed -= 0.2f;
                }

                else if (HealthY < -3)
                {
                    HealthVerticalSpeed += 0.2f;
                }
                else if (HealthY > -3 && HealthY < 3 && HealthVerticalSpeed > -0.33f && HealthVerticalSpeed < 0.33f)
                {
                    HealthVerticalSpeed = 0;
                    HealthY = 0;
                }
                else if (HealthVerticalSpeed != 0)
                {
                    HealthVerticalSpeed *= 0.94f;
                }
            }
        }

        private void DrawExperience(ImDrawListPtr drawList, int experience, int maxExp)
        {
            int size = (int)Math.Ceiling(256 * Ui.Configuration.Scale);
            int rest = Marshal.ReadInt32(_expAddonPtr + 0x280);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale));

            ExperienceRingBg.Draw(drawList, 1, drawPosition, 4, Ui.Configuration.Scale);

            ExperienceRingRest.Draw(drawList, (experience + rest) / (float)maxExp, drawPosition, 4, Ui.Configuration.Scale);

            ExperienceRingGain.Draw(drawList, experience / (float)maxExp, drawPosition, 4, Ui.Configuration.Scale);

            ExperienceRing.Draw(drawList, ExpTemp / maxExp, drawPosition, 4, Ui.Configuration.Scale);
            
            drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
            drawList.AddImage(RingExperienceBgTexture.ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
            drawList.PopClipRect();


            float iconSize = 3f * Ui.Configuration.Scale;
            ImageDrawing.DrawIcon(Pi, drawList, (ushort)(62000 + Pi.ClientState.LocalPlayer.ClassJob.Id), new Vector2(iconSize, iconSize), new Vector2((int)(size / 2f), (int)(size / 2f + 18 * Ui.Configuration.Scale)) + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)));

            ImGuiAdditions.TextShadowedDrawList(drawList, 32f * Ui.Configuration.Scale, $"Lv{Pi.ClientState.LocalPlayer.Level}", drawPosition + new Vector2(size / 2f - 26 * Ui.Configuration.Scale, size / 2f - 52 * Ui.Configuration.Scale), new Vector4(249 / 255f, 247 / 255f, 232 / 255f, 0.9f), new Vector4(96 / 255f, 78 / 255f, 23 / 255f, 0.25f), 3);
        }

        private void DrawHealth(ImDrawListPtr drawList, int hp, int maxHp)
        {
            HpLengthMultiplier = maxHp < Ui.Configuration.MinimumHpForLength
                ?
                Ui.Configuration.MinimumHpForLength / (float) maxHp
                : maxHp > Ui.Configuration.MaximumHpForMaximumLength
                    ? maxHp / (float) Ui.Configuration.MaximumHpForMaximumLength
                    : 1f;
            var drawPosition = ImGui.GetItemRectMin();
            var maxHealthPercent = maxHp / (float)Ui.Configuration.HpForFullRing * HpLengthMultiplier;

            DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)));

            HealthRingBg.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)), 3, Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(drawList, HpBeforeDamaged / (float) Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)), 3, Ui.Configuration.Scale);
            }

            if (HpTemp < hp)
                HealthRestoredRing.Draw(drawList, hp / (float)Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)), 3, Ui.Configuration.Scale);

            HealthRing.Draw(drawList, HpTemp / Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)), 3, Ui.Configuration.Scale);

            RingOutline.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale)), 3, Ui.Configuration.Scale);

            DrawLongHealthBar(drawList, drawPosition, hp, maxHp);

            //ImGuiAdditions.TextShadowedDrawList(drawList, 24f, 1, $"{hp}", drawPosition + new Vector2(0, 128), new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);
        }

        private void DrawParameterResourceBar(ImDrawListPtr drawList, ParameterType type, int val, int valMax)
        {
            var minLength = 1;
            var maxLength = 1;
            var lengthRate = 1f;

            switch (type)
            {
                case ParameterType.Mp:
                    if (Ui.Configuration.TruncateMp)
                    {
                        val /= 100;
                        valMax /= 100;
                    }
                    minLength = Ui.Configuration.MinimumMpLength;
                    maxLength = Ui.Configuration.MaximumMpLength;
                    lengthRate = Ui.Configuration.MpPerPixelLength;
                    break;
                case ParameterType.Gp:
                    minLength = Ui.Configuration.MinimumGpLength;
                    maxLength = Ui.Configuration.MaximumGpLength;
                    lengthRate = Ui.Configuration.GpPerPixelLength;
                    break;
                case ParameterType.Cp:
                    minLength = Ui.Configuration.MinimumCpLength;
                    maxLength = Ui.Configuration.MaximumCpLength;
                    lengthRate = Ui.Configuration.CpPerPixelLength;
                    break;
            }

            var lengthMultiplier = valMax < minLength ? minLength / (float)valMax : valMax > maxLength ? (float)maxLength / valMax : 1f;
            var drawPosition = ImGui.GetItemRectMin();
            var barHeight = 20f * Ui.Configuration.Scale;
            var outlineHeight = 30 * Ui.Configuration.Scale;
            var barMaxLength = (int) Math.Ceiling(valMax / lengthRate * lengthMultiplier * Ui.Configuration.Scale);
            var barLength = (int) Math.Ceiling(val / lengthRate * lengthMultiplier * Ui.Configuration.Scale);

            var outlineSize = new Vector2(barMaxLength, outlineHeight);
            var edgeSize = new Vector2((int) Math.Ceiling(5 * Ui.Configuration.Scale), (int) Math.Ceiling(30 * Ui.Configuration.Scale));
            var edgeOffset = new Vector2((int) Math.Ceiling(-barMaxLength - 5 * Ui.Configuration.Scale), 0);
            var barOffset = new Vector2((int) Math.Ceiling(40 * Ui.Configuration.Scale), (int) Math.Ceiling(205 * Ui.Configuration.Scale));
            var outlineOffset = new Vector2((int) Math.Ceiling(40 * Ui.Configuration.Scale), (int) Math.Ceiling(200 * Ui.Configuration.Scale));
            var baseSize = new Vector2((int) Math.Ceiling(73 * Ui.Configuration.Scale), (int) Math.Ceiling(30 * Ui.Configuration.Scale));

            ImageDrawing.DrawImage(drawList, BarTextures, baseSize, outlineOffset, new Vector4(1, 44, 73, 30));
            ImageDrawing.DrawImage(drawList, BarTextures, -edgeSize, outlineOffset + baseSize + new Vector2((int) Math.Ceiling(5 * Ui.Configuration.Scale), 0), new Vector4(23, 1, 5, 30f));

            drawList.PushClipRect(drawPosition + outlineOffset + edgeOffset, drawPosition + outlineOffset + edgeOffset + edgeSize);
            ImageDrawing.DrawImage(drawList, BarTextures, edgeSize, outlineOffset + edgeOffset, new Vector4(23, 1, 5, 30f));
            drawList.PopClipRect();

            if (barMaxLength > 0)
            {
                DrawBar(drawList, drawPosition, outlineOffset, outlineSize, new Vector4(30, 1, 1, 30f));
            }
            else
            {
                return;
            }

            if (barLength > 0)
            {
                DrawBar(drawList, drawPosition, barOffset, new Vector2(barLength, barHeight), new Vector4(34, 6, 1, 20f));                
            }

            ImGuiAdditions.TextShadowedDrawList(drawList, 24f * Ui.Configuration.Scale, $"{val}", drawPosition + outlineOffset - new Vector2(32 * Ui.Configuration.Scale, 16 * Ui.Configuration.Scale), new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);
        }

        private void DrawLongHealthBar(ImDrawListPtr drawList, Vector2 position, int hp, int maxHp)
        {
            var barHeight = (int)Math.Ceiling(37f * Ui.Configuration.Scale);
            var outlineHeight = (int)Math.Ceiling(42f * Ui.Configuration.Scale);
            var healthLength = (int)Math.Ceiling((HpTemp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar * Ui.Configuration.Scale);
            var damagedHealthLength = (int)Math.Ceiling((HpBeforeDamaged * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar * Ui.Configuration.Scale);
            var restoredHealthLength = (int)Math.Ceiling((hp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar * Ui.Configuration.Scale);
            var maxHealthLength = (int)Math.Ceiling((maxHp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar * Ui.Configuration.Scale);
            var outlineSize = new Vector2(maxHealthLength, outlineHeight);
            var edgeSize = new Vector2((int)Math.Ceiling(5 * Ui.Configuration.Scale), (int)Math.Ceiling(42 * Ui.Configuration.Scale));
            var maxHealthSize = new Vector2(maxHealthLength, barHeight);
            var barOffset = new Vector2((int)Math.Ceiling(128 * Ui.Configuration.Scale), (int)Math.Ceiling(216 * Ui.Configuration.Scale)) + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale));
            var outlineOffset = new Vector2((int)Math.Ceiling(128 * Ui.Configuration.Scale), (int)Math.Ceiling(213 * Ui.Configuration.Scale)) + new Vector2(0, (int)(HealthY * Ui.Configuration.Scale));


            if (maxHealthLength > 0)
            {
                DrawBar(drawList, position, barOffset, maxHealthSize, new Vector4(10, 1, 1, 37f), ImGui.GetColorU32(new Vector4(0.07843f, 0.07843f, 0.0745f, 1)));
            }

            if (damagedHealthLength > 0)
            {
                DrawBar(drawList, position, barOffset, new Vector2(damagedHealthLength, barHeight), new Vector4(10, 1, 1, 37f), ImGui.GetColorU32(new Vector4(1f, 0f, 0f, DamagedHealthAlpha)));
            }

            if (restoredHealthLength > 0)
            {
                DrawBar(drawList, position, barOffset, new Vector2(restoredHealthLength, barHeight), new Vector4(14, 1, 1, 37f));
            }

            if (healthLength > 0)
            {
                DrawBar(drawList, position, barOffset, new Vector2(healthLength, barHeight), new Vector4(6, 1, 1, 37f));
            }

            if (maxHealthLength > 0)
            {
                DrawBar(drawList, position, outlineOffset, outlineSize, new Vector4(2, 1, 1, 42f));
                var edgeOffset = new Vector2(-maxHealthLength - (int)Math.Ceiling(5 * Ui.Configuration.Scale), 0);
                drawList.PushClipRect(position + outlineOffset + edgeOffset, position + outlineOffset + edgeOffset + edgeSize);
                ImageDrawing.DrawImage(drawList, BarTextures, edgeSize, outlineOffset + edgeOffset, new Vector4(17, 1, 5, 42f));
                drawList.PopClipRect();
            }
        }

        private void DrawBar(ImDrawListPtr drawList, Vector2 position, Vector2 offSet, Vector2 size, Vector4 imageArea, uint color = UInt32.MaxValue)
        {
            drawList.PushClipRect(position + offSet - new Vector2(size.X, 0), position + offSet + new Vector2(0, size.Y));
            ImageDrawing.DrawImage(drawList, BarTextures, size, offSet + new Vector2(-size.X, 0), imageArea, color);
            drawList.PopClipRect();
        }

        private void DrawRingEdgesAndTrack(ImDrawListPtr drawList, float percent, Vector2 position)
        {
            var size = 256 * Ui.Configuration.Scale;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.ImGuiHandle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.ImGuiHandle, position, position + new Vector2(size, size));
            ImageDrawing.ImageRotated(drawList, RingEndTexture.ImGuiHandle, new Vector2(position.X + size / 2f, position.Y + size / 2f), new Vector2(RingEndTexture.Width * Ui.Configuration.Scale, RingEndTexture.Height * Ui.Configuration.Scale), Math.Min(percent, 1) * 0.75f * (float)Math.PI * 2);
            drawList.PopClipRect();
        }

        public void Dispose()
        {
            RingOutlineTexture?.Dispose();
            HealthRingSegmentTexture?.Dispose();
            RingValueSegmentTexture?.Dispose();
            RingTrackTexture?.Dispose();
            RingBaseTexture?.Dispose();
            RingEndTexture?.Dispose();
            BarTextures?.Dispose();
            RingExperienceBgTexture?.Dispose();
            RingExperienceTexture?.Dispose();
            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
            ExperienceRing = null;
            ExperienceRingGain = null;
            ExperienceRingBg = null;
            ExperienceRingRest = null;
            Timer = null;
            _expAddonPtr = IntPtr.Zero;
        }

        // Temp Health Values
        private int LastHp { get; set; }
        private int HpBeforeDamaged { get; set; }
        private int HpBeforeRestored { get; set; }
        private float HpTemp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Temp Exp values
        private int LastExp { get; set; }
        private uint LastJob { get; set; }
        private byte LastLevel { get; set; }
        private int ExpBeforeGain { get; set; }
        private float ExpTemp { get; set; }

        // Alpha Channels
        private float DamagedHealthAlpha { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }
        private float ExpGainTime { get; set; }
        private Stopwatch Timer { get; set; }
        private float UiSpeed { get; set; }

        // Positioning
        private float HealthY { get; set; }
        private float HealthVerticalSpeed { get; set; }

        // Textures
        private TextureWrap HealthRingSegmentTexture { get; }
        private TextureWrap RingValueSegmentTexture { get; }
        private TextureWrap RingOutlineTexture { get; }
        private TextureWrap RingTrackTexture { get; }
        private TextureWrap RingBaseTexture { get; }
        private TextureWrap RingEndTexture { get; }
        private TextureWrap RingExperienceTexture { get; }
        private TextureWrap RingExperienceBgTexture { get; }
        private TextureWrap BarTextures { get; }
        // Rings
        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring HealthRestoredRing { get; set; }
        private Ring HealthLostRing { get; set; }
        private Ring ExperienceRing { get; set; }
        private Ring ExperienceRingRest { get; set; }
        private Ring ExperienceRingGain { get; set; }
        private Ring ExperienceRingBg { get; set; }
        private DalamudPluginInterface Pi { get; }
        private PluginUI Ui { get; }
    }
}
