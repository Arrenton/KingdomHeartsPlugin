using Dalamud.Game.ClientState.Party;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Data;
using System.IO;
using System.Numerics;
using Lumina.Excel.GeneratedSheets;

namespace KingdomHeartsPlugin.UIElements.PartyFrame
{
    public class PartyFrame : IDisposable
    {
        private readonly Vector3 _bgColor;
        private readonly Vector2[] defaultPartyLocations;
        private readonly Vector2[] defaultIconLocations;

        public PartyFrame()
        {
            _bgColor = new Vector3(0.07843f, 0.07843f, 0.0745f);
            defaultPartyLocations =[
                new Vector2(-200f, -100f),
                new Vector2(0f, -200f),
                new Vector2(-200f, -300f),
                new Vector2(0f, -400f),
                new Vector2(-200f, -500f),
                new Vector2(0f, -600f),
                new Vector2(-200f, -700f)
            ];
            defaultIconLocations =[
                new Vector2(-70f, 0f),
                new Vector2(65f, -65f),
                new Vector2(-70f, -130f),
                new Vector2(65f, -195f),
                new Vector2(-70f, -265f),
                new Vector2(65f, -330f),
                new Vector2(-70f, -400f),
            ];
            
            HealthRingBg = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_value_segment.png"), _bgColor.X, _bgColor.Y, _bgColor.Z);
            RingOutline = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_outline_segment.png"));
            HealthRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_segment.png"));
            MpRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_health_restored_segment.png"));
            ShieldRing = new Ring(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\HealthBar\ring_shield_segment.png"));
        }

        public unsafe void Draw(IPlayerCharacter player)
        {
            var drawList = ImGui.GetWindowDrawList();
            var drawPosition = ImGui.GetItemRectMin();
            var drawIconPosition = ImGui.GetItemRectMin();
            var partyList = Service.PartyList;
            
            int i = 0;
            bool skipped = false;
            foreach (var member in Service.PartyList){
                if (player.Name.ToString().Equals(member.Name.ToString()) ) { continue; }
                if (i == KingdomHeartsPlugin.Ui.Configuration.PartyDisplayNumber) { break; }

                // todo: add control for party spacing
                // drawPosition = Vector2.Multiply(new Vector2(xmod, ymod), defaultPartyLocations[i]);
                DrawPartyFrame(drawList, member, i);
                i++;
            }
        }
        
        private void DrawPartyFrame(ImDrawListPtr drawList, IPartyMember member, int i)
        {
            var drawPosition = Vector2.Add(ImGui.GetItemRectMin(),defaultPartyLocations[i]);
            var drawIconPosition = defaultIconLocations[i];
            
            ImageDrawing.DrawIcon(drawList, (ushort)(62000 + member.ClassJob.Id), new Vector2(KingdomHeartsPlugin.Ui.Configuration.ClassIconScale, KingdomHeartsPlugin.Ui.Configuration.ClassIconScale), drawIconPosition);
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
