using System;
using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using ImGuiNET;
using ImGuiScene;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin.UIElements.ParameterResource
{
    public class ResourceBar
    {
        private ISharedImmediateTexture _barBackgroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\background.png"));
        }
        private ISharedImmediateTexture _barForegroundTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\foreground.png"));
        }
        private ISharedImmediateTexture _mpBaseTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\MP_base.png"));
        }
        private ISharedImmediateTexture _barEdgeTexture
        {
            get => ImageDrawing.GetSharedTexture(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\ResourceBar\edge.png"));
        }

        private enum Resource
        {
            Mp,
            Cp,
            Gp
        }

        public ResourceBar()
        {
        }

        public void Update(IPlayerCharacter player)
        {
            var minLength = 1;
            var maxLength = 1;
            var lengthRate = 1f;

            if (player.MaxMp > 0)
            {
                ResourceValue = player.CurrentMp;
                ResourceMax = player.MaxMp;
                ResourceType = Resource.Mp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumMpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumMpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.MpPerPixelLength;
            }
            else if (player.MaxCp > 0)
            {
                ResourceValue = player.CurrentCp;
                ResourceMax = player.MaxCp;
                ResourceType = Resource.Cp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumCpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumCpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.CpPerPixelLength;
            }
            else if (player.MaxGp > 0)
            {
                ResourceValue = player.CurrentGp;
                ResourceMax = player.MaxGp;
                ResourceType = Resource.Gp;

                minLength = KingdomHeartsPlugin.Ui.Configuration.MinimumGpLength;
                maxLength = KingdomHeartsPlugin.Ui.Configuration.MaximumGpLength;
                lengthRate = KingdomHeartsPlugin.Ui.Configuration.GpPerPixelLength;
            }

            var lengthMultiplier = ResourceMax < minLength ? minLength / (float)ResourceMax : ResourceMax > maxLength ? (float)maxLength / ResourceMax : 1f;
            MaxResourceLength = (int)Math.Ceiling(ResourceMax / lengthRate * lengthMultiplier);
            ResourceLength = (int)Math.Ceiling(ResourceValue / lengthRate * lengthMultiplier);
        }

        public void Draw(IPlayerCharacter player)
        {
            Update(player);
            var drawList = ImGui.GetWindowDrawList();
            var basePosition =  new Vector2(KingdomHeartsPlugin.Ui.Configuration.ResourceBarPositionX, KingdomHeartsPlugin.Ui.Configuration.ResourceBarPositionY);
            var textPosition = new Vector2(KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionX, KingdomHeartsPlugin.Ui.Configuration.ResourceTextPositionY) * KingdomHeartsPlugin.Ui.Configuration.Scale;

            // Base
            ImageDrawing.DrawImage(drawList, _mpBaseTexture, new Vector2(basePosition.X - 1, basePosition.Y), new Vector4(0, 0, 74 / 80f, 1));

            // BG
            ImageDrawing.DrawImageScaled(drawList, _barBackgroundTexture, new Vector2(basePosition.X + 0.33f - MaxResourceLength, basePosition.Y), new Vector2(MaxResourceLength, 1f));

            // FG
            ImageDrawing.DrawImageScaled(drawList, _barForegroundTexture, new Vector2(basePosition.X + 0.33f - ResourceLength, basePosition.Y + 5), new Vector2(ResourceLength, 1f));

            // Edge
            ImageDrawing.DrawImage(drawList, _barEdgeTexture, new Vector2(basePosition.X + 0.65f - MaxResourceLength - 6, basePosition.Y));
            // Base Edge
            ImageDrawing.DrawImageRotated(drawList, _barEdgeTexture, new Vector2(basePosition.X + 74, basePosition.Y + 16), new Vector2(_barEdgeTexture.GetWrapOrEmpty().Width, _barEdgeTexture.GetWrapOrEmpty().Height), (float)Math.PI);

            if (KingdomHeartsPlugin.Ui.Configuration.ShowResourceVal)
                ImGuiAdditions.TextShadowedDrawList(drawList, KingdomHeartsPlugin.Ui.Configuration.ResourceTextSize, $"{StringFormatting.FormatDigits(KingdomHeartsPlugin.Ui.Configuration.TruncateMp && ResourceType == Resource.Mp ? ResourceValue / 100 : ResourceValue, KingdomHeartsPlugin.Ui.Configuration.ResourceTextStyle)}", ImGui.GetItemRectMin() + basePosition * KingdomHeartsPlugin.Ui.Configuration.Scale + textPosition, new Vector4(255 / 255f, 255 / 255f, 255 / 255f, 1f), new Vector4(0 / 255f, 0 / 255f, 0 / 255f, 0.25f), 3, (TextAlignment)KingdomHeartsPlugin.Ui.Configuration.ResourceTextAlignment);
         }

        public void Dispose()
        {
        }

        private uint ResourceValue { get; set; }
        private Resource ResourceType { get; set; }
        private uint ResourceMax { get; set; }
        private float ResourceLength { get; set; }
        private float MaxResourceLength { get; set; }
    }
}
