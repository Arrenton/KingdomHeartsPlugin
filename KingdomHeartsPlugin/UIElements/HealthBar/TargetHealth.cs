using Dalamud.Interface.Textures;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using KingdomHeartsPlugin.Utilities;
using System;
using System.IO;
using System.Numerics;




namespace KingdomHeartsPlugin.UIElements.TargetHealth
{
    public class TargetHealth : IDisposable
    {
        private readonly Vector3 _bgColor;

        public TargetHealth()
        {
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);
        }

        public unsafe void Draw(IPlayerCharacter player)
        {
            // var drawList = ImGui.GetWindowDrawList();
            // var textPosition = new Vector2(KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionX, KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;
            //
            // var fullRing = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpForFullRing : KingdomHeartsPlugin.Ui.Configuration.HpForFullRing;
            // var HpPerWidth = KingdomHeartsPlugin.IsInPvp ? KingdomHeartsPlugin.Ui.Configuration.PvpHpPerPixelLongBar : KingdomHeartsPlugin.Ui.Configuration.HpPerPixelLongBar;
            // var basePosition = new Vector2(129, 212 + HealthY * KingdomHeartsPlugin.Ui.Configuration.HpDamageWobbleIntensity / 100f);
            // var healthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? HpTemp : hp) * HpLengthMultiplier - fullRing) / HpPerWidth;
            // var damagedHealthLength = (HpBeforeDamaged * HpLengthMultiplier - fullRing) / HpPerWidth;
            // var restoredHealthLength = ((KingdomHeartsPlugin.Ui.Configuration.ShowHpRecovery ? hp : 0) * HpLengthMultiplier - fullRing) / HpPerWidth;
            // var maxHealthLength = (maxHp * HpLengthMultiplier - fullRing) / HpPerWidth;
            //
            // //outline
            // ImageDrawing.DrawImageScaled(drawList, BarOutlineTexture, new Vector2(basePosition.X - maxHealthLength, basePosition.Y), new Vector2(maxHealthLength, 1));
            
        }
        
        
            
        public void Dispose()
        {   
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
        private ISharedImmediateTexture _hpBase
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\HP_base.png"));
        }
        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring MpRing { get; set; }
        private Ring ShieldRing { get; set; }
    }
}