using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.HealthBar
{
    class HealthFrame : IDisposable
    {
        private float _verticalAnimationTicks;
        private readonly Vector3 _bgColor;
        private readonly LimitGauge _limitGauge;
        private IntPtr _expAddonPtr;

        private enum ParameterType
        {
            Mp,
            Gp,
            Cp
        }

        public HealthFrame()
        {
            HealthY = 0;
            _verticalAnimationTicks = 0;
            HealthVerticalSpeed = 0f;
            LowHealthAlpha = 0;
            LowHealthAlphaDirection = 0;
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);
            var imagesPath = KingdomHeartsPlugin.TemplateLocation;
            HealthRingSegmentTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_health_segment.png"));
            RingValueSegmentTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_value_segment.png"));
            RingOutlineTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_outline_segment.png"));
            RingTrackTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_track.png"));
            RingBaseTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_base_edge.png"));
            RingEndTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_end_edge.png"));
            RingExperienceTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_experience_segment.png"));
            RingExperienceBgTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_experience_outline.png"));
            BarTextures = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\bar_textures.png"));
            HealthRingBg = new Ring(RingValueSegmentTexture, _bgColor.X, _bgColor.Y, _bgColor.Z);
            HealthLostRing = new Ring(RingValueSegmentTexture, 1, 0, 0);
            RingOutline = new Ring(RingOutlineTexture);
            ExperienceRing = new Ring(RingExperienceTexture);
            ExperienceRingRest = new Ring(RingExperienceTexture, alpha: 0.25f);
            ExperienceRingGain = new Ring(RingExperienceTexture, 0.65f, 0.92f, 1.00f) { Flip = true };
            ExperienceRingBg = new Ring(RingExperienceTexture, 0.07843f, 0.07843f, 0.0745f) {Flip = true};
            HealthRing = new Ring(HealthRingSegmentTexture);
            HealthRestoredRing = new Ring(HealthRingSegmentTexture) {Flip = true};
            _limitGauge = new LimitGauge();
        }

        public unsafe void Draw()
        {
            var player = KingdomHeartsPlugin.Cs.LocalPlayer;
            var parameterWidget = (AtkUnitBase*) KingdomHeartsPlugin.Gui.GetAddonByName("_ParameterWidget", 1);
            _expAddonPtr = KingdomHeartsPlugin.Gui.GetAddonByName("_Exp", 1);

            if (parameterWidget != null)
            {
                // Do not do or draw anything if the parameter widget is not visible
                if (!parameterWidget->IsVisible)
                {
                    return;
                }
            }

            // Do not do or draw anything if player is null or game ui is hidden
            if (player is null || KingdomHeartsPlugin.Gui.GameUiHidden)
            {
                return;
            }

            var drawList = ImGui.GetWindowDrawList();

            if (ImGui.GetDrawListSharedData() == IntPtr.Zero) return;


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
                    _expAddonPtr = KingdomHeartsPlugin.Gui.GetAddonByName("_Exp", 1);
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

            _limitGauge.Draw();

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpVal)
            {
                // Draw HP Value
                ImGui.SameLine(0, 0);
                float hp = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp / 1000f
                    : player.CurrentHp;
                string hpVal = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp >= 100000 ? $"{hp:0}K" : $"{hp:0.#}K"
                    : $"{hp}";
                ImGuiAdditions.TextCenteredShadowed(hpVal, 1.25f * KingdomHeartsPlugin.Ui.Configuration.Scale,
                    new Vector2(-196 + 22 * (KingdomHeartsPlugin.Ui.Configuration.Scale - 1), 128 * KingdomHeartsPlugin.Ui.Configuration.Scale),
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);
            }
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
                ExpGainTime -= 1 * KingdomHeartsPlugin.UiSpeed;
            }
            else if (ExpTemp < exp)
            {
                ExpTemp += (exp - ExpBeforeGain) * KingdomHeartsPlugin.UiSpeed;
            }

            if (ExpTemp > exp)
                ExpTemp = exp;
        }

        private void UpdateHealth(PlayerCharacter player)
        {
            if (LastHp > player.CurrentHp && LastHp <= player.MaxHp)
                DamagedHealth(LastHp);
            if (LastHp < player.CurrentHp)
                RestoredHealth(LastHp);

            UpdateLowHealth(player.CurrentHp, player.MaxHp);
            
            UpdateDamagedHealth();

            UpdateRestoredHealth(player.CurrentHp);

            if (HpBeforeDamaged > player.MaxHp)
                HpBeforeDamaged = player.MaxHp;

            LastHp = player.CurrentHp;
        }

        private void DamagedHealth(uint health)
        {
            DamagedHealthAlpha = 1f;
            HealthY = 0;
            HealthVerticalSpeed = -3;
            HpBeforeDamaged = health;
        }

        private void RestoredHealth(uint health)
        {
            if (HealthRestoreTime <= 0)
            {
                HpTemp = health;
                HpBeforeRestored = health;
            }

            HealthRestoreTime = 1f;
        }

        private void UpdateRestoredHealth(uint currentHp)
        {
            if (HealthRestoreTime > 0)
            {
                HealthRestoreTime -= 1 * KingdomHeartsPlugin.UiSpeed;
            }
            else if (HpTemp < currentHp)
            {
                HpTemp += (currentHp - HpBeforeRestored) * KingdomHeartsPlugin.UiSpeed;
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
                DamagedHealthAlpha -= 0.09f * KingdomHeartsPlugin.UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.6f)
            {
                DamagedHealthAlpha -= 0.8f * KingdomHeartsPlugin.UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.59f)
            {
                DamagedHealthAlpha -= 0.005f * KingdomHeartsPlugin.UiSpeed;
            }
            else if (DamagedHealthAlpha > 0.0f)
            {
                DamagedHealthAlpha -= 1f * KingdomHeartsPlugin.UiSpeed;
            }

            // Vertical wobble
            _verticalAnimationTicks += 240 * KingdomHeartsPlugin.UiSpeed;

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
                else if (HealthY is > -3 and < 3 && HealthVerticalSpeed is > -0.33f and < 0.33f)
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

        private void UpdateLowHealth(uint health, uint maxHealth)
        {
            if ((health > maxHealth * (KingdomHeartsPlugin.Ui.Configuration.LowHpPercent / 100f) || health <= 0) && LowHealthAlpha <= 0) return;

            if (LowHealthAlphaDirection == 0)
            {
                LowHealthAlpha += 1.6f * KingdomHeartsPlugin.UiSpeed;

                if (LowHealthAlpha >= .4)
                    LowHealthAlphaDirection = 1;
            }
            else
            {
                LowHealthAlpha -= 1.6f * KingdomHeartsPlugin.UiSpeed;

                if (LowHealthAlpha <= 0)
                    LowHealthAlphaDirection = 0;
            }

            HealthRingBg.Color = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
        }

        private void DrawExperience(ImDrawListPtr drawList, int experience, int maxExp)
        {
            int size = (int)Math.Ceiling(256 * KingdomHeartsPlugin.Ui.Configuration.Scale);
            int rest = Marshal.ReadInt32(_expAddonPtr + 0x280);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale));

            ExperienceRingBg.Draw(drawList, 1, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

            ExperienceRingRest.Draw(drawList, (experience + rest) / (float)maxExp, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

            ExperienceRingGain.Draw(drawList, experience / (float)maxExp, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);

            ExperienceRing.Draw(drawList, ExpTemp / maxExp, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale);
            
            drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
            drawList.AddImage(RingExperienceBgTexture.ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
            drawList.PopClipRect();


            float iconSize = 3f * KingdomHeartsPlugin.Ui.Configuration.Scale;

            if (KingdomHeartsPlugin.Cs.LocalPlayer is null) return;

            ImageDrawing.DrawIcon(drawList, (ushort)(62000 + KingdomHeartsPlugin.Cs.LocalPlayer.ClassJob.Id),
                new Vector2(iconSize, iconSize),
                new Vector2((int)(size / 2f), (int)(size / 2f + 18 * KingdomHeartsPlugin.Ui.Configuration.Scale)) +
                new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)));

            ImGuiAdditions.TextShadowedDrawList(drawList, 32f * KingdomHeartsPlugin.Ui.Configuration.Scale,
                $"Lv{KingdomHeartsPlugin.Cs.LocalPlayer.Level}",
                drawPosition + new Vector2(size / 2f - 26 * KingdomHeartsPlugin.Ui.Configuration.Scale,
                    size / 2f - 52 * KingdomHeartsPlugin.Ui.Configuration.Scale),
                new Vector4(249 / 255f, 247 / 255f, 232 / 255f, 0.9f),
                new Vector4(96 / 255f, 78 / 255f, 23 / 255f, 0.25f), 3);
        }

        private void DrawHealth(ImDrawListPtr drawList, uint hp, uint maxHp)
        {
            HpLengthMultiplier = maxHp < KingdomHeartsPlugin.Ui.Configuration.MinimumHpForLength
                ?
                KingdomHeartsPlugin.Ui.Configuration.MinimumHpForLength / (float) maxHp
                : maxHp > KingdomHeartsPlugin.Ui.Configuration.MaximumHpForMaximumLength
                    ? (float)KingdomHeartsPlugin.Ui.Configuration.MaximumHpForMaximumLength / maxHp 
                    : 1f;
            var drawPosition = ImGui.GetItemRectMin();
            var maxHealthPercent = maxHp / (float)KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier;

            try
            {
                DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)));
            }
            catch
            {
                // Will sometimes error when hot reloading and I have no idea what is causing it. So exit.
                return;
            }

            HealthRingBg.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(drawList, HpBeforeDamaged / (float) KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery)
            {
                if (HpTemp < hp)
                    HealthRestoredRing.Draw(drawList, hp / (float) KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

                HealthRing.Draw(drawList, HpTemp / KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }
            else
            {
                HealthRing.Draw(drawList, hp / (float)KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            RingOutline.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            DrawLongHealthBar(drawList, drawPosition, hp, maxHp);
        }

        private void DrawParameterResourceBar(ImDrawListPtr drawList, ParameterType type, uint val, uint valMax)
        {
            var minLength = 1;
            var maxLength = 1;
            var lengthRate = 1f;

            switch (type)
            {
                case ParameterType.Mp:
                    if (KingdomHeartsPlugin.Ui.Configuration.TruncateMp)
                    {
                        val /= 100;
                        valMax /= 100;
                    }
                    minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumMpLength;
                    maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumMpLength;
                    lengthRate = KingdomHeartsPlugin.Ui.Configuration.MpPerPixelLength;
                    break;
                case ParameterType.Gp:
                    minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumGpLength;
                    maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumGpLength;
                    lengthRate = KingdomHeartsPlugin.Ui.Configuration.GpPerPixelLength;
                    break;
                case ParameterType.Cp:
                    minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumCpLength;
                    maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumCpLength;
                    lengthRate = KingdomHeartsPlugin.Ui.Configuration.CpPerPixelLength;
                    break;
            }

            var lengthMultiplier = valMax < minLength ? minLength / (float)valMax : valMax > maxLength ? (float)maxLength / valMax : 1f;
            var drawPosition = ImGui.GetItemRectMin();
            var barHeight = 20f * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var outlineHeight = 30 * KingdomHeartsPlugin.Ui.Configuration.Scale;
            var barMaxLength = (int) Math.Ceiling(valMax / lengthRate * lengthMultiplier * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var barLength = (int) Math.Ceiling(val / lengthRate * lengthMultiplier * KingdomHeartsPlugin.Ui.Configuration.Scale);

            var outlineSize = new Vector2(barMaxLength, outlineHeight);
            var edgeSize = new Vector2((int) Math.Ceiling(5 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int) Math.Ceiling(30 * KingdomHeartsPlugin.Ui.Configuration.Scale));
            var edgeOffset = new Vector2((int) Math.Ceiling(-barMaxLength - 5 * KingdomHeartsPlugin.Ui.Configuration.Scale), 0);
            var barOffset = new Vector2((int) Math.Ceiling(40 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int) Math.Ceiling(205 * KingdomHeartsPlugin.Ui.Configuration.Scale));
            var outlineOffset = new Vector2((int) Math.Ceiling(40 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int) Math.Ceiling(200 * KingdomHeartsPlugin.Ui.Configuration.Scale));
            var baseSize = new Vector2((int) Math.Ceiling(73 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int) Math.Ceiling(30 * KingdomHeartsPlugin.Ui.Configuration.Scale));

            ImageDrawing.DrawImageArea(drawList, BarTextures, baseSize, outlineOffset, new Vector4(1, 44, 73, 30));
            ImageDrawing.DrawImageArea(drawList, BarTextures, -edgeSize, outlineOffset + baseSize + new Vector2((int) Math.Ceiling(5 * KingdomHeartsPlugin.Ui.Configuration.Scale), 0), new Vector4(23, 1, 5, 30f));

            drawList.PushClipRect(drawPosition + outlineOffset + edgeOffset, drawPosition + outlineOffset + edgeOffset + edgeSize);
            ImageDrawing.DrawImageArea(drawList, BarTextures, edgeSize, outlineOffset + edgeOffset, new Vector4(23, 1, 5, 30f));
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

            if (KingdomHeartsPlugin.Ui.Configuration.ShowResourceVal)
                ImGuiAdditions.TextShadowedDrawList(drawList, 24f * KingdomHeartsPlugin.Ui.Configuration.Scale, $"{val}", drawPosition + outlineOffset - new Vector2(32 * KingdomHeartsPlugin.Ui.Configuration.Scale, 16 * KingdomHeartsPlugin.Ui.Configuration.Scale), new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);
        }

        private void DrawLongHealthBar(ImDrawListPtr drawList, Vector2 position, uint hp, uint maxHp)
        {
            var barHeight = (int)Math.Ceiling(37f * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var outlineHeight = (int)Math.Ceiling(42f * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var healthLength = (int)Math.Ceiling(((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : hp) * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var damagedHealthLength = (int)Math.Ceiling((HpBeforeDamaged * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var restoredHealthLength = (int)Math.Ceiling(((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? hp : 0) * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var maxHealthLength = (int)Math.Ceiling((maxHp * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar * KingdomHeartsPlugin.Ui.Configuration.Scale);
            var outlineSize = new Vector2(maxHealthLength, outlineHeight);
            var edgeSize = new Vector2((int)Math.Ceiling(5 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int)Math.Ceiling(42 * KingdomHeartsPlugin.Ui.Configuration.Scale));
            var maxHealthSize = new Vector2(maxHealthLength, barHeight);
            var barOffset = new Vector2((int)Math.Ceiling(128 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int)Math.Ceiling(216 * KingdomHeartsPlugin.Ui.Configuration.Scale)) + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale));
            var outlineOffset = new Vector2((int)Math.Ceiling(128 * KingdomHeartsPlugin.Ui.Configuration.Scale), (int)Math.Ceiling(213 * KingdomHeartsPlugin.Ui.Configuration.Scale)) + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.Scale));


            if (maxHealthLength > 0)
            {
                Vector3 lowHealthColor = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
                DrawBar(drawList, position, barOffset, maxHealthSize, new Vector4(10, 1, 1, 37f), ImGui.GetColorU32(new Vector4(lowHealthColor.X, lowHealthColor.Y, lowHealthColor.Z, 1)));
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
                var edgeOffset = new Vector2(-maxHealthLength - (int)Math.Ceiling(5 * KingdomHeartsPlugin.Ui.Configuration.Scale), 0);
                drawList.PushClipRect(position + outlineOffset + edgeOffset, position + outlineOffset + edgeOffset + edgeSize);
                ImageDrawing.DrawImageArea(drawList, BarTextures, edgeSize, outlineOffset + edgeOffset, new Vector4(17, 1, 5, 42f));
                drawList.PopClipRect();
            }
        }

        private void DrawBar(ImDrawListPtr drawList, Vector2 position, Vector2 offSet, Vector2 size, Vector4 imageArea, uint color = UInt32.MaxValue)
        {
            drawList.PushClipRect(position + offSet - new Vector2(size.X, 0), position + offSet + new Vector2(0, size.Y));
            ImageDrawing.DrawImageArea(drawList, BarTextures, size, offSet + new Vector2(-size.X, 0), imageArea, color);
            drawList.PopClipRect();
        }

        private void DrawRingEdgesAndTrack(ImDrawListPtr drawList, float percent, Vector2 position)
        {
            var size = 256 * KingdomHeartsPlugin.Ui.Configuration.Scale;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.ImGuiHandle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.ImGuiHandle, position, position + new Vector2(size, size));
            ImageDrawing.ImageRotated(drawList, RingEndTexture.ImGuiHandle, new Vector2(position.X + size / 2f, position.Y + size / 2f), new Vector2(RingEndTexture.Width * KingdomHeartsPlugin.Ui.Configuration.Scale, RingEndTexture.Height * KingdomHeartsPlugin.Ui.Configuration.Scale), Math.Min(percent, 1) * 0.75f * (float)Math.PI * 2);
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
            _expAddonPtr = IntPtr.Zero;
            _limitGauge?.Dispose();
        }

        // Temp Health Values
        private uint LastHp { get; set; }
        private uint HpBeforeDamaged { get; set; }
        private uint HpBeforeRestored { get; set; }
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
        private float LowHealthAlpha { get; set; }
        private int LowHealthAlphaDirection { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }
        private float ExpGainTime { get; set; }

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
    }
}
