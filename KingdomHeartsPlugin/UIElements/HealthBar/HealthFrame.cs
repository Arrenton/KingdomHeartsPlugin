using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.UIElements.ParameterResource;
using KingdomHeartsPlugin.Utilities;
using System;
using System.IO;
using System.Numerics;

namespace KingdomHeartsPlugin.UIElements.HealthBar
{
    public class HealthFrame : IDisposable
    {
        private float _verticalAnimationTicks;
        private readonly Vector3 _bgColor;
        private LimitGauge? _limitGauge;
        private ResourceBar? _resourceBar;
        private ClassBar? _expBar;


        public HealthFrame()
        {
            HealthY = 0;
            _verticalAnimationTicks = 0;
            HealthVerticalSpeed = 0f;
            LowHealthAlpha = 0;
            LowHealthAlphaDirection = 0;
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);

            HealthRingBg = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), _bgColor.X, _bgColor.Y, _bgColor.Z);
            HealthLostRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), 1, 0, 0);
            RingOutline = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            HealthRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            HealthRestoredRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
            _limitGauge = new LimitGauge();
            _resourceBar = new ResourceBar();
            _expBar = new ClassBar();
        }

        public unsafe void Draw()
        {
            var player = KingdomHeartsPlugin.Cs.LocalPlayer;
            var parameterWidget = (AtkUnitBase*) KingdomHeartsPlugin.Gui.GetAddonByName("_ParameterWidget", 1);

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

            ImGui.Dummy(new Vector2(220, 256));

            if (KingdomHeartsPlugin.Ui.Configuration.HpBarEnabled)
            {
                UpdateHealth(player);
                DrawHealth(drawList, player.CurrentHp, player.MaxHp);
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ResourceBarEnabled) _resourceBar?.Draw(player);
            if (KingdomHeartsPlugin.Ui.Configuration.LimitBarEnabled) _limitGauge?.Draw();
            _expBar?.Draw(player, HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpVal && KingdomHeartsPlugin.Ui.Configuration.HpBarEnabled)
            {
                // Draw HP Value
                var basePosition = ImGui.GetItemRectMin() + new Vector2(KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionX, KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;
                /*float hp = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp / 1000f
                    : player.CurrentHp;

                string hpVal = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp >= 100000 ? $"{hp:0}K" : $"{hp:0.#}K" : $"{hp}";*/

                ImGuiAdditions.TextShadowedDrawList(drawList,
                    KingdomHeartsPlugin.Ui.Configuration.HpValueTextSize,
                    $"{StringFormatting.FormatDigits(player.CurrentHp, (NumberFormatStyle)KingdomHeartsPlugin.Ui.Configuration.HpValueTextStyle)}",
                    basePosition,
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (TextAlignment)KingdomHeartsPlugin.Ui.Configuration.HpValueTextAlignment);
            }
        }
        
        private void UpdateHealth(IPlayerCharacter player)
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
            switch (DamagedHealthAlpha)
            {
                case > 0.97f:
                    DamagedHealthAlpha -= 0.09f * KingdomHeartsPlugin.UiSpeed;
                    break;
                case > 0.6f:
                    DamagedHealthAlpha -= 0.8f * KingdomHeartsPlugin.UiSpeed;
                    break;
                case > 0.59f:
                    DamagedHealthAlpha -= 0.005f * KingdomHeartsPlugin.UiSpeed;
                    break;
                case > 0.0f:
                    DamagedHealthAlpha -= 1f * KingdomHeartsPlugin.UiSpeed;
                    break;
            }

            // Vertical wobble
            _verticalAnimationTicks += 240 * KingdomHeartsPlugin.UiSpeed;

            while (_verticalAnimationTicks > 1)
            {
                float intensity = KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f;
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

            if (HealthRingBg is not null)
                HealthRingBg.Color = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);

        }

        private void DrawHealth(ImDrawListPtr drawList, uint hp, uint maxHp)
        {
            var fullRing = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpForFullRing : KingdomHeartsPlugin.Ui.Configuration.HpForFullRing;
            var minimumMaxHpSize = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpMinimumHpForLength : KingdomHeartsPlugin.Ui.Configuration.MinimumHpForLength;
            var maximumMaxHpSize = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpMaximumHpForMaximumLength : KingdomHeartsPlugin.Ui.Configuration.MaximumHpForMaximumLength;
            HpLengthMultiplier = maxHp < minimumMaxHpSize
                ?
                minimumMaxHpSize / (float) maxHp
                : maxHp > maximumMaxHpSize
                    ? (float)maximumMaxHpSize / maxHp 
                    : 1f;
            var drawPosition = ImGui.GetItemRectMin();
            var maxHealthPercent = maxHp / (float)fullRing * HpLengthMultiplier;

            try
            {
                DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)));
            }
            catch
            {
                // Will sometimes error when hot reloading and I have no idea what is causing it. So exit.
                return;
            }

            HealthRingBg?.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                if (HealthLostRing is not null)
                {
                    HealthLostRing.Alpha = DamagedHealthAlpha;
                    HealthLostRing.Draw(drawList, HpBeforeDamaged / (float)fullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
                }
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery)
            {
                if (HpTemp < hp)
                    HealthRestoredRing?.Draw(drawList, hp / (float)fullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

                HealthRing?.Draw(drawList, HpTemp / fullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }
            else
            {
                HealthRing?.Draw(drawList, hp / (float)fullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            RingOutline?.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            DrawLongHealthBar(drawList, hp, maxHp);
        }
        
        private void DrawLongHealthBar(ImDrawListPtr drawList, uint hp, uint maxHp)
        {
            var fullRing = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpForFullRing : KingdomHeartsPlugin.Ui.Configuration.HpForFullRing;
            var HpPerWidth = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpPerPixelLongBar : KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            var basePosition = new Vector2(129, 212 + HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);
            var healthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : hp) * HpLengthMultiplier - fullRing) / HpPerWidth;
            var damagedHealthLength = (HpBeforeDamaged * HpLengthMultiplier - fullRing) / HpPerWidth;
            var restoredHealthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? hp : 0) * HpLengthMultiplier - fullRing) / HpPerWidth;
            var maxHealthLength = (maxHp * HpLengthMultiplier - fullRing) / HpPerWidth;
            
            if (maxHealthLength > 0)
            {
                Vector3 lowHealthColor = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
                ImageDrawing.DrawImage(drawList, BarEdgeTexture, new Vector2(basePosition.X - 5.4f - maxHealthLength, basePosition.Y));
                ImageDrawing.DrawImageScaled(drawList, BarColorlessTexture, new Vector2(basePosition.X - maxHealthLength, basePosition.Y + 4), new Vector2(maxHealthLength, 1), ImGui.GetColorU32(new Vector4(lowHealthColor.X, lowHealthColor.Y, lowHealthColor.Z, 1)));
            }

            if (damagedHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarColorlessTexture, new Vector2(basePosition.X - damagedHealthLength, basePosition.Y + 4), new Vector2(damagedHealthLength, 1), ImGui.GetColorU32(new Vector4(1f, 0f, 0f, DamagedHealthAlpha)));
            }

            if (restoredHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarRecoveryTexture, new Vector2(basePosition.X - restoredHealthLength, basePosition.Y + 4), new Vector2(restoredHealthLength, 1));
            }

            if (healthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarForegroundTexture, new Vector2(basePosition.X - healthLength, basePosition.Y + 4), new Vector2(healthLength, 1));
            }

            if (maxHealthLength > 0)
            {
                ImageDrawing.DrawImageScaled(drawList, BarOutlineTexture, new Vector2(basePosition.X - maxHealthLength, basePosition.Y), new Vector2(maxHealthLength, 1));
            }
        }

        private void DrawRingEdgesAndTrack(ImDrawListPtr drawList, float percent, Vector2 position)
        {
            var size = 256 * KingdomHeartsPlugin.Ui.Configuration.Scale;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.GetWrapOrEmpty().ImGuiHandle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.GetWrapOrEmpty().ImGuiHandle, position, position + new Vector2(size, size));
            ImageDrawing.ImageRotated(drawList, RingEndTexture.GetWrapOrEmpty().ImGuiHandle, new Vector2(position.X + size / 2f, position.Y + size / 2f), new Vector2(RingEndTexture.GetWrapOrEmpty().Width * KingdomHeartsPlugin.Ui.Configuration.Scale, RingEndTexture.GetWrapOrEmpty().Height * KingdomHeartsPlugin.Ui.Configuration.Scale), Math.Min(percent, 1) * 0.75f * (float)Math.PI * 2);
            drawList.PopClipRect();
        }

        public void Dispose()
        {
            _limitGauge?.Dispose();
            _resourceBar?.Dispose();
            _expBar?.Dispose();

            _limitGauge = null;
            _resourceBar = null;
            _expBar = null;
            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
        }

        // Temp Health Values
        private uint LastHp { get; set; }
        private uint HpBeforeDamaged { get; set; }
        private uint HpBeforeRestored { get; set; }
        private float HpTemp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Alpha Channels
        public float DamagedHealthAlpha { get; private set; }
        public float LowHealthAlpha { get; private set; }
        private int LowHealthAlphaDirection { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }

        // Positioning
        private float HealthY { get; set; }
        private float HealthVerticalSpeed { get; set; }

        // Textures
        private ISharedImmediateTexture HealthRingSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
        }
        private ISharedImmediateTexture HealthRestoredRingSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
        }
        private ISharedImmediateTexture BarOutlineTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_outline.png"));
        }
        private ISharedImmediateTexture BarColorlessTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_colorless.png"));
        }
        private ISharedImmediateTexture BarForegroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_foreground.png"));
        }
        private ISharedImmediateTexture BarRecoveryTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_recovery.png"));
        }
        private ISharedImmediateTexture BarEdgeTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_edge.png"));
        }
        private ISharedImmediateTexture RingValueSegmentTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"));
        }
        private ISharedImmediateTexture RingOutlineTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
        }
        private ISharedImmediateTexture RingTrackTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_track.png"));
        }
        private ISharedImmediateTexture RingBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_base_edge.png"));
        }
        private ISharedImmediateTexture RingEndTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_end_edge.png"));
        }

        // Rings
        private Ring? HealthRing { get; set; }
        private Ring? RingOutline { get; set; }
        private Ring? HealthRingBg { get; set; }
        private Ring? HealthRestoredRing { get; set; }
        private Ring? HealthLostRing { get; set; }
    }
}
