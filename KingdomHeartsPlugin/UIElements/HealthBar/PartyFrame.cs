using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.LimitBreak;
using KingdomHeartsPlugin.UIElements.ParameterResource;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Data;
using System.IO;
using System.Numerics;


namespace KingdomHeartsPlugin.UIElements.PartyFrame
{
    public class PartyFrame : IDisposable
    {
        private readonly Vector3 _bgColor;
        private readonly Vector2[] defaultPartyLocations;

        public PartyFrame()
        {
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);
            defaultPartyLocations =[
                new Vector2(-200f,100f),
                new Vector2(-200f, -100f),
                new Vector2(0f, -200f),
                new Vector2(-200f, -300f),
                new Vector2(0f, -400f),
                new Vector2(-200f, -500f),
                new Vector2(0f, -600f),
                new Vector2(-200f, -700f)
            ];
            
            HealthRingBg = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), _bgColor.X, _bgColor.Y, _bgColor.Z);
            RingOutline = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            HealthRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            MpRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
            ShieldRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_shield_segment.png"));
        }

        public unsafe void Draw()
        {
            var drawList = ImGui.GetWindowDrawList();
            var drawPosition = ImGui.GetItemRectMin();

            
            
            int i = 0;
            foreach (var member in Service.PartyList){
                // skips first player's portrait, comment out to test
                 if (i==0 || member == null){
                     i++;
                     continue;
                 }

                //todo: add control for party spacing
                // drawPosition = Vector2.Multiply(new Vector2(xmod, ymod), defaultPartyLocations[i]);

                drawPosition = Vector2.Add(new Vector2(KingdomHeartsPlugin.Ui.Configuration.PartyXModifier, KingdomHeartsPlugin.Ui.Configuration.PartyYModifier),Vector2.Add(ImGui.GetItemRectMin(),defaultPartyLocations[i]));
                DrawPartyFrame(drawList, member, drawPosition);
                i++;
            }
            
        }
        
        private void DrawPartyFrame(ImDrawListPtr drawList, IPartyMember member,Vector2 drawPosition)
        {
            HealthRingBg.Draw(drawList, 1, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale/2);
            MpRing.DrawRightHalf(drawList, (float)member.CurrentMP/member.MaxMP, drawPosition, KingdomHeartsPlugin.Ui.Configuration.Scale/2);
            HealthRing.DrawLeftHalf(drawList, (float)member.CurrentHP/member.MaxHP, drawPosition,  KingdomHeartsPlugin.Ui.Configuration.Scale/2);
            RingOutline.Draw(drawList, 1, drawPosition, 4, KingdomHeartsPlugin.Ui.Configuration.Scale/2);
        }
        
            
        public void Dispose()
        {   
            HealthRing.Dispose();
            RingOutline.Dispose();
            HealthRingBg.Dispose();
            MpRing.Dispose();
            ShieldRing.Dispose();
            
            HealthRing = null;
            RingOutline = null;
            HealthRingBg = null;
            MpRing = null;
            ShieldRing = null;
        }

        private Ring HealthRing { get; set; }
        private Ring RingOutline { get; set; }
        private Ring HealthRingBg { get; set; }
        private Ring MpRing { get; set; }
        private Ring ShieldRing { get; set; }
    }
}
