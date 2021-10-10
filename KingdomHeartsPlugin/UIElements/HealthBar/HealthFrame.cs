using System;
using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.UIElements.ParameterResource;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.HealthBar
{
    class HealthFrame : IDisposable
    {
        private float _verticalAnimationTicks;
        private readonly Vector3 _bgColor;
        private LimitGauge _limitGauge;
        private ResourceBar _resourceBar;
        private ClassBar _expBar;


        public HealthFrame()
        {
            HealthY = 0;
            _verticalAnimationTicks = 0;
            HealthVerticalSpeed = 0f;
            LowHealthAlpha = 0;
            LowHealthAlphaDirection = 0;
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);

            BarOutlineTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_outline.png"));
            BarColorlessTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_colorless.png"));
            BarForegroundTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_foreground.png"));
            BarRecoveryTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_recovery.png"));
            BarEdgeTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\bar_edge.png"));
            HealthRingSegmentTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            RingValueSegmentTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"));
            RingOutlineTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            RingTrackTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_track.png"));
            RingBaseTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_base_edge.png"));
            RingEndTexture = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_end_edge.png"));

            HealthRingBg = new Ring(RingValueSegmentTexture, _bgColor.X, _bgColor.Y, _bgColor.Z);
            HealthLostRing = new Ring(RingValueSegmentTexture, 1, 0, 0);
            RingOutline = new Ring(RingOutlineTexture);
            HealthRing = new Ring(HealthRingSegmentTexture);
            HealthRestoredRing = new Ring(HealthRingSegmentTexture) {Flip = true};
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


            UpdateHealth(player);
            
            ImGui.Dummy(new Vector2(220, 256));

            DrawHealth(drawList, player.CurrentHp, player.MaxHp);
            
            _resourceBar.Draw(player);
            _limitGauge.Draw();
            _expBar.Draw(player, HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpVal)
            {
                // Draw HP Value
                var basePosition = ImGui.GetItemRectMin() + new Vector2(KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionX, KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;
                float hp = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp / 1000f
                    : player.CurrentHp;
                string hpVal = KingdomHeartsPlugin.Ui.Configuration.TruncateHp && player.CurrentHp >= 10000
                    ? player.CurrentHp >= 100000 ? $"{hp:0}K" : $"{hp:0.#}K"
                    : $"{hp}";
                /*ImGuiAdditions.TextCenteredShadowed(hpVal, 1.25f,
                    new Vector2(KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionX, KingdomHeartsPlugin.Ui.Configuration.HpValueTextPositionY),
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3);*/
                ImGuiAdditions.TextShadowedDrawList(drawList,
                    KingdomHeartsPlugin.Ui.Configuration.HpValueTextSize,
                    $"{hpVal}",
                    basePosition,
                    new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f),
                    new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (ImGuiAdditions.TextAlignment)KingdomHeartsPlugin.Ui.Configuration.HpValueTextAlignment);
            }
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

            HealthRingBg.Color = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
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
                DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)));
            }
            catch
            {
                // Will sometimes error when hot reloading and I have no idea what is causing it. So exit.
                return;
            }

            HealthRingBg.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(drawList, HpBeforeDamaged / (float) KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            if (KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery)
            {
                if (HpTemp < hp)
                    HealthRestoredRing.Draw(drawList, hp / (float) KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

                HealthRing.Draw(drawList, HpTemp / KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }
            else
            {
                HealthRing.Draw(drawList, hp / (float)KingdomHeartsPlugin.Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int) (HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);
            }

            RingOutline.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)(HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f * KingdomHeartsPlugin.Ui.Configuration.Scale)), 3, KingdomHeartsPlugin.Ui.Configuration.Scale);

            DrawLongHealthBar(drawList, hp, maxHp);
        }
        
        private void DrawLongHealthBar(ImDrawListPtr drawList, uint hp, uint maxHp)
        {
            var basePosition = new Vector2(129, 212 + HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);
            var healthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : hp) * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            var damagedHealthLength = (HpBeforeDamaged * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            var restoredHealthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? hp : 0) * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            var maxHealthLength = (maxHp * HpLengthMultiplier - KingdomHeartsPlugin.Ui.Configuration.HpForFullRing) / KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            
            if (maxHealthLength > 0)
            {
                Vector3 lowHealthColor = ColorAddons.Interpolate(_bgColor, new Vector3(1, 0, 0), LowHealthAlpha);
                ImageDrawing.DrawImage(drawList, BarEdgeTexture, new Vector2(basePosition.X- 6 - maxHealthLength, basePosition.Y));
                ImageDrawing.DrawImage(drawList, BarColorlessTexture, new Vector4(basePosition.X - maxHealthLength, basePosition.Y + 4, maxHealthLength, BarColorlessTexture.Height), ImGui.GetColorU32(new Vector4(lowHealthColor.X, lowHealthColor.Y, lowHealthColor.Z, 1)));
            }

            if (damagedHealthLength > 0)
            {
                ImageDrawing.DrawImage(drawList, BarColorlessTexture, new Vector4(basePosition.X - damagedHealthLength, basePosition.Y + 4, damagedHealthLength, BarColorlessTexture.Height), ImGui.GetColorU32(new Vector4(1f, 0f, 0f, DamagedHealthAlpha)));
            }

            if (restoredHealthLength > 0)
            {
                ImageDrawing.DrawImage(drawList, BarRecoveryTexture, new Vector4(basePosition.X - restoredHealthLength, basePosition.Y + 4, restoredHealthLength, BarRecoveryTexture.Height));
            }

            if (healthLength > 0)
            {
                ImageDrawing.DrawImage(drawList, BarForegroundTexture, new Vector4(basePosition.X - healthLength, basePosition.Y + 4, healthLength, BarForegroundTexture.Height));
            }

            if (maxHealthLength > 0)
            {
                ImageDrawing.DrawImage(drawList, BarOutlineTexture, new Vector4(basePosition.X - maxHealthLength, basePosition.Y, maxHealthLength, BarOutlineTexture.Height));
            }
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
            BarColorlessTexture?.Dispose();
            BarEdgeTexture?.Dispose();
            BarForegroundTexture?.Dispose();
            BarOutlineTexture?.Dispose();
            BarRecoveryTexture?.Dispose();
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
            RingOutlineTexture = null;
            HealthRingSegmentTexture = null;
            RingValueSegmentTexture = null;
            RingTrackTexture = null;
            RingBaseTexture = null;
            RingEndTexture = null;
            BarColorlessTexture = null;
            BarEdgeTexture = null;
            BarForegroundTexture = null;
            BarOutlineTexture = null;
            BarRecoveryTexture = null;
        }

        // Temp Health Values
        private uint LastHp { get; set; }
        private uint HpBeforeDamaged { get; set; }
        private uint HpBeforeRestored { get; set; }
        private float HpTemp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Alpha Channels
        private float DamagedHealthAlpha { get; set; }
        private float LowHealthAlpha { get; set; }
        private int LowHealthAlphaDirection { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }

        // Positioning
        private float HealthY { get; set; }
        private float HealthVerticalSpeed { get; set; }

        // Textures
        private TextureWrap HealthRingSegmentTexture { get; set; }
        private TextureWrap BarOutlineTexture { get; set; }
        private TextureWrap BarColorlessTexture { get; set; }
        private TextureWrap BarForegroundTexture { get; set; }
        private TextureWrap BarRecoveryTexture { get; set; }
        private TextureWrap BarEdgeTexture { get; set; }
        private TextureWrap RingValueSegmentTexture { get; set; }
        private TextureWrap RingOutlineTexture { get; set; }
        private TextureWrap RingTrackTexture { get; set; }
        private TextureWrap RingBaseTexture { get; set; }

        private TextureWrap RingEndTexture { get; set; }

        // Rings
        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring HealthRestoredRing { get; set; }
        private Ring HealthLostRing { get; set; }
    }
}
