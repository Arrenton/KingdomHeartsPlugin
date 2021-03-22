using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;

namespace KingdomHeartsPlugin.HealthBar
{
    class HealthFrame : IDisposable
    {
        public HealthFrame(DalamudPluginInterface pi, PluginUI ui)
        {
            Ui = ui;
            Pi = pi;
            Timer = Stopwatch.StartNew();
            var imagesPath = "" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            HealthRingSegmentTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_health_segment.png"));
            RingValueSegmentTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_value_segment.png"));
            RingOutlineTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_outline_segment.png"));
            RingTrackTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_track.png"));
            RingOutlineTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_outline_segment.png"));
            RingBaseTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_base_edge.png"));
            RingEndTexture = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\ring_end_edge.png"));
            BarTextures = pi.UiBuilder.LoadImage(Path.Combine(imagesPath, @"Textures\bar_textures.png"));
            HealthRingBg = new Ring(RingValueSegmentTexture, 0.07843f, 0.07843f, 0.0745f);
            HealthLostRing = new Ring(RingValueSegmentTexture, 1, 0, 0);
            RingOutline = new Ring(RingOutlineTexture);
            HealthRing = new Ring(HealthRingSegmentTexture);
            HealthRestoredRing = new Ring(HealthRingSegmentTexture) {Flip = true};
        }

        public void Draw(PlayerCharacter player)
        {
            if (player is null || Pi.Framework.Gui.GameUiHidden) return;

            var drawList = ImGui.GetWindowDrawList();

            UiSpeed = Timer.ElapsedMilliseconds / 1000f;

            UpdateHealth(player);

            ImGui.Dummy(new Vector2(220, 256));

            DrawHealth(drawList, player.CurrentHp, player.MaxHp);

            Timer.Restart();
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
            HpBeforeDamaged = health;
        }

        private void RestoredHealth(int health)
        {
            HpBeforeRestored = health;
            if (HealthRestoreTime <= 0)
                HpTemp = health;
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

            DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition);

            HealthRingBg.Draw(drawList, maxHealthPercent, drawPosition);

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(drawList, HpBeforeDamaged / (float) Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition);
            }

            if (HpTemp < hp)
                HealthRestoredRing.Draw(drawList, hp / (float)Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition);

            HealthRing.Draw(drawList, HpTemp / Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition);

            RingOutline.Draw(drawList, maxHealthPercent, drawPosition);

            DrawLongHealthBar(drawList, drawPosition, hp, maxHp);
        }

        private void DrawLongHealthBar(ImDrawListPtr drawList, Vector2 position, int hp, int maxHp)
        {
            var barHeight = 37f;
            var outlineHeight = 42f;
            var healthLength = (int)Math.Ceiling((HpTemp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar);
            var damagedHealthLength = (int)Math.Ceiling((HpBeforeDamaged * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar);
            var restoredHealthLength = (int)Math.Ceiling((hp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar);
            var maxHealthLength = (int)Math.Ceiling((maxHp * HpLengthMultiplier - Ui.Configuration.HpForFullRing) / Ui.Configuration.HpPerPixelLongBar);
            var outlineSize = new Vector2(maxHealthLength, outlineHeight);
            var edgeSize = new Vector2(5, 42);
            var maxHealthSize = new Vector2(maxHealthLength, barHeight);
            var barOffset = new Vector2(128, 216);
            var outlineOffset = new Vector2(128, 213);


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
                var edgeOffset = new Vector2(-maxHealthLength - 5, 0);
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
            var size = 256;

            drawList.PushClipRect(position, position + new Vector2(size, size));
            drawList.AddImage(RingTrackTexture.ImGuiHandle, position, position + new Vector2(size, size));
            drawList.AddImage(RingBaseTexture.ImGuiHandle, position, position + new Vector2(size, size));
            ImageDrawing.ImageRotated(drawList, RingEndTexture.ImGuiHandle, new Vector2(position.X + size / 2f, position.Y + size / 2f), new Vector2(RingEndTexture.Width, RingEndTexture.Height), Math.Min(percent, 1) * 0.75f * (float)Math.PI * 2);
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
            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
            Timer = null;
        }

        // Temp Health Values
        private int LastHp { get; set; }
        private int HpBeforeDamaged { get; set; }
        private int HpBeforeRestored { get; set; }
        private float HpTemp { get; set; }
        private float HpLengthMultiplier { get; set; }

        // Alpha Channels
        private float DamagedHealthAlpha { get; set; }

        // Timers
        private float HealthRestoreTime { get; set; }
        private Stopwatch Timer { get; set; }
        private float UiSpeed { get; set; }

        // Textures
        private TextureWrap HealthRingSegmentTexture { get; }
        private TextureWrap RingValueSegmentTexture { get; }
        private TextureWrap RingOutlineTexture { get; }
        private TextureWrap RingTrackTexture { get; }
        private TextureWrap RingBaseTexture { get; }
        private TextureWrap RingEndTexture { get; }
        private TextureWrap BarTextures { get; }
        // Rings
        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring HealthRestoredRing { get; set; }
        private Ring HealthLostRing { get; set; }
        private DalamudPluginInterface Pi { get; }
        private PluginUI Ui { get; }
    }
}
