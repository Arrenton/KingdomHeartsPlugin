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
using System.Threading.Tasks;
using Dalamud.Data.LuminaExtensions;

namespace KingdomHeartsPlugin.HealthBar
{
    class HealthFrame : IDisposable
    {

        public HealthFrame(DalamudPluginInterface pi, PluginUI ui)
        {
            Ui = ui;
            Pi = pi;
            HealthY = 0;
            HealthVerticalSpeed = 0f;
            Timer = Stopwatch.StartNew();
            var imagesPath = "" + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
            ExperienceRingBg = new Ring(RingExperienceTexture) {Flip = true};
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

            DrawExperience(drawList, 0, 1);

            DrawHealth(drawList, player.CurrentHp, player.MaxHp);

            if (player.MaxMp > 0)
            {
                DrawMana(drawList, player.CurrentMp, player.MaxMp);
            }
            else if (player.MaxCp > 0)
            {
                DrawMana(drawList, player.CurrentCp, player.MaxCp);
            }
            else if (player.MaxGp > 0)
            {
                DrawMana(drawList, player.CurrentGp, player.MaxGp);
            }

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
            HealthY = 0;
            HealthVerticalSpeed = -4;
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

            // Vertical wobble
            HealthY += HealthVerticalSpeed;

            if (HealthY > 3)
            {
                HealthVerticalSpeed -= 0.3f;
            }

            else if (HealthY < -3)
            {
                HealthVerticalSpeed += 0.3f;
            }
            else if (HealthY > -3 && HealthY < 3 && HealthVerticalSpeed > -0.33 && HealthVerticalSpeed < 0.33)
            {
                HealthVerticalSpeed = 0;
                HealthY = 0;
            }
            else if (HealthVerticalSpeed != 0)
            {
                HealthVerticalSpeed *= 0.94f;
            }
        }

        private void DrawExperience(ImDrawListPtr drawList, int experience, int maxExp)
        {
            int size = (int)Math.Ceiling(256 * Ui.Configuration.Scale);
            var drawPosition = ImGui.GetItemRectMin() + new Vector2(0, (int)HealthY);

            ExperienceRingBg.Draw(drawList, 1, drawPosition, 4, Ui.Configuration.Scale);

            ExperienceRing.Draw(drawList, experience / (float)maxExp, drawPosition, 4, Ui.Configuration.Scale);

            drawList.PushClipRect(drawPosition, drawPosition + new Vector2(size, size));
            drawList.AddImage(RingExperienceBgTexture.ImGuiHandle, drawPosition, drawPosition + new Vector2(size, size));
            drawList.PopClipRect();

            int iconSize = 64;
            ImageDrawing.DrawIcon(Pi, drawList, (ushort)(62000 + Pi.ClientState.LocalPlayer.ClassJob.Id), new Vector2(iconSize, iconSize), new Vector2((int)(size / 2f - iconSize / 2f), (int)(size / 2f - iconSize / 2f)) + new Vector2(0, (int)HealthY));
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

            DrawRingEdgesAndTrack(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)HealthY));

            HealthRingBg.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)HealthY), 3, Ui.Configuration.Scale);

            if (DamagedHealthAlpha > 0)
            {
                HealthLostRing.Alpha = DamagedHealthAlpha;
                HealthLostRing.Draw(drawList, HpBeforeDamaged / (float) Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)HealthY), 3, Ui.Configuration.Scale);
            }

            if (HpTemp < hp)
                HealthRestoredRing.Draw(drawList, hp / (float)Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)HealthY), 3, Ui.Configuration.Scale);

            HealthRing.Draw(drawList, HpTemp / Ui.Configuration.HpForFullRing * HpLengthMultiplier, drawPosition + new Vector2(0, (int)HealthY), 3, Ui.Configuration.Scale);

            RingOutline.Draw(drawList, maxHealthPercent, drawPosition + new Vector2(0, (int)HealthY), 3, Ui.Configuration.Scale);

            DrawLongHealthBar(drawList, drawPosition, hp, maxHp);
        }

        private void DrawMana(ImDrawListPtr drawList, int mp, int maxMp)
        {

            var drawPosition = ImGui.GetItemRectMin();
            var barHeight = 20f;
            var outlineHeight = 30f;
            var maxManaLength = (int) Math.Ceiling(maxMp / 25f);
            var manaLength = (int)Math.Ceiling(mp / 25f);

            var outlineSize = new Vector2(maxManaLength, outlineHeight);
            var edgeSize = new Vector2(5, 30);
            var edgeOffset = new Vector2(-maxManaLength - 5, 0);
            var barOffset = new Vector2(40, 205);
            var outlineOffset = new Vector2(40, 200);
            var baseSize = new Vector2(73, 30);

            ImageDrawing.DrawImage(drawList, BarTextures, baseSize, outlineOffset, new Vector4(1, 44, 73, 30));
            ImageDrawing.DrawImage(drawList, BarTextures, -edgeSize, outlineOffset + baseSize + new Vector2(5, 0), new Vector4(23, 1, 5, 30f));

            drawList.PushClipRect(drawPosition + outlineOffset + edgeOffset, drawPosition + outlineOffset + edgeOffset + edgeSize);
            ImageDrawing.DrawImage(drawList, BarTextures, edgeSize, outlineOffset + edgeOffset, new Vector4(23, 1, 5, 30f));
            drawList.PopClipRect();

            if (maxManaLength > 0)
            {
                DrawBar(drawList, drawPosition, outlineOffset, outlineSize, new Vector4(30, 1, 1, 30f));
            }

            if (manaLength > 0)
            {
                DrawBar(drawList, drawPosition, barOffset, new Vector2(manaLength, barHeight), new Vector4(34, 6, 1, 20f));
            }
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
            var barOffset = new Vector2(128, 216) + new Vector2(0, (int)HealthY);
            var outlineOffset = new Vector2(128, 213) + new Vector2(0, (int)HealthY);


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
            RingExperienceBgTexture?.Dispose();
            RingExperienceTexture?.Dispose();
            HealthRing = null;
            HealthRingBg = null;
            RingOutline = null;
            HealthRestoredRing = null;
            HealthLostRing = null;
            ExperienceRing = null;
            ExperienceRingBg = null;
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
        private Ring ExperienceRingBg { get; set; }
        private DalamudPluginInterface Pi { get; }
        private PluginUI Ui { get; }
    }
}
