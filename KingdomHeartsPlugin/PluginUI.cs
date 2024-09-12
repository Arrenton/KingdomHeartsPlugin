using Dalamud.Interface;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using KingdomHeartsPlugin.Configuration;
using KingdomHeartsPlugin.Enums;
using KingdomHeartsPlugin.UIElements.Experience;
using KingdomHeartsPlugin.UIElements.HealthBar;
using KingdomHeartsPlugin.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility;

namespace KingdomHeartsPlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        internal Settings Configuration;
        public readonly HealthFrame HealthFrame;
        private readonly FileDialogManager _dialogManager;
        /*private TextureWrap _testTextureWrap;
        private float _width;
        private float _height;
        private float[] pos;
        private float[] pos2;
        private float[] uv;
        private float[] uv2;*/

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = true;
        public bool Visible
        {
            get => visible;
            set => visible = value;
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get => settingsVisible;
            set => settingsVisible = value;
        }

        // passing in the image here just for simplicity
        public PluginUI(Settings configuration)
        {
            Configuration = configuration;
            HealthFrame = new HealthFrame();
            _dialogManager = SetupDialogManager();

            /*_testTextureWrap = KingdomHeartsPlugin.Pi.UiBuilder.LoadImage(Path.Combine(KingdomHeartsPlugin.TemplateLocation, @"Textures\LimitGauge\number_2.png"));
            pos = new float[4];
            pos2 = new float[4];
            uv = new float[4];
            uv2 = new float[4];
            _width = 256;
            _height = 256;*/
        }

        public void Dispose()
        {
            HealthFrame?.Dispose();
            Portrait.Dispose();
            ImageDrawing.Dispose();
            //_testTextureWrap?.Dispose();
        }

        public void OnUpdate()
        {
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
            DrawSettingsWindow();
        }

        public void DrawMainWindow()
        {
            Visible = true;

            CheckNpcTalkingVisibility();

            if (!Visible || !KingdomHeartsPlugin.Ui.Configuration.Enabled)
            {
                return;
            }


            ImGuiWindowFlags window_flags = 0;
            window_flags |= ImGuiWindowFlags.NoTitleBar;
            window_flags |= ImGuiWindowFlags.NoScrollbar;
            if (Configuration.Locked)
            {
                window_flags |= ImGuiWindowFlags.NoMove;
                window_flags |= ImGuiWindowFlags.NoMouseInputs;
                window_flags |= ImGuiWindowFlags.NoNav;
            }
            window_flags |= ImGuiWindowFlags.AlwaysAutoResize;
            window_flags |= ImGuiWindowFlags.NoBackground;

            var size = new Vector2(320, 320);
            ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
            
            if (ImGui.Begin("KH Frame", ref visible, window_flags))
            {
                HealthFrame.Draw();
            }
            ImGui.End();
        }

        private unsafe void CheckNpcTalkingVisibility()
        {
            var actionBarWidget = (AtkUnitBase*)KingdomHeartsPlugin.Gui.GetAddonByName("_ActionBar", 1);
            var actionCrossWidget = (AtkUnitBase*)KingdomHeartsPlugin.Gui.GetAddonByName("_ActionCross", 1);

            if (actionBarWidget == null || actionCrossWidget == null || !KingdomHeartsPlugin.Ui.Configuration.HideWhenNpcTalking) return;

            if (!actionBarWidget->IsVisible && !actionCrossWidget->IsVisible)
                Visible = false;
        }

        private void GeneralSettings()
        {
            if (!ImGui.BeginTabItem("General")) return;

            var enabled = Configuration.Enabled;
            if (ImGui.Checkbox("Visible", ref enabled))
            {
                Configuration.Enabled = enabled;
            }
            var hideWhenNpcTalking = Configuration.HideWhenNpcTalking;
            if (ImGui.Checkbox("Hide when dialogue box is shown", ref hideWhenNpcTalking))
            {
                Configuration.HideWhenNpcTalking = hideWhenNpcTalking;
            }

            var locked = Configuration.Locked;
            if (ImGui.Checkbox("Locked", ref locked))
            {
                Configuration.Locked = locked;
            }

            var scale = Configuration.Scale;
            if (ImGui.InputFloat("Scale", ref scale, 0.025f, 0.1f))
            {
                Configuration.Scale = scale;
                if (Configuration.Scale < 0.25f)
                    Configuration.Scale = 0.25f;
                if (Configuration.Scale > 3)
                    Configuration.Scale = 3;
            }

            /*ImGui.NewLine();
                ImGui.Separator();

                ImGui.SliderFloat("Width", ref _width, 0, 512);
                ImGui.SliderFloat("Height", ref _height, 0, 512);
                ImGui.SliderFloat("Pos[0]", ref pos[0], 0, 256);
                ImGui.SliderFloat("Pos[1]", ref pos[1], 0, 256);
                ImGui.SliderFloat("Pos[2]", ref pos[2], 0, 256);
                ImGui.SliderFloat("Pos[3]", ref pos[3], 0, 256);
                ImGui.SliderFloat("Pos2[0]", ref pos2[0], 0, 256);
                ImGui.SliderFloat("Pos2[1]", ref pos2[1], 0, 256);
                ImGui.SliderFloat("Pos2[2]", ref pos2[2], 0, 256);
                ImGui.SliderFloat("Pos2[3]", ref pos2[3], 0, 256);
                ImGui.SliderFloat("UV[0]", ref uv[0], 0, 1);
                ImGui.SliderFloat("UV[1]", ref uv[1], 0, 1);
                ImGui.SliderFloat("UV[2]", ref uv[2], 0, 1);
                ImGui.SliderFloat("UV[3]", ref uv[3], 0, 1);
                ImGui.SliderFloat("UV2[0]", ref uv2[0], 0, 1);
                ImGui.SliderFloat("UV2[1]", ref uv2[1], 0, 1);
                ImGui.SliderFloat("UV2[2]", ref uv2[2], 0, 1);
                ImGui.SliderFloat("UV2[3]", ref uv2[3], 0, 1);

                ImGui.NewLine();

                //ImGui.Image(_testTextureWrap.ImGuiHandle, new Vector2(pos[0], pos[1]), new Vector2(uv[0], uv[1]), new Vector2(uv[2], uv[3]));

                var dl = ImGui.GetWindowDrawList();
                ImGui.Dummy(new Vector2(_width, _height));
                double width = _testTextureWrap.Width;
                double height = _testTextureWrap.Height;
                Vector2 position = ImGui.GetItemRectMin();

                dl.PushClipRect(position - new Vector2(0, 0), position + new Vector2(_width, _height));
                dl.AddImageQuad(_testTextureWrap.ImGuiHandle, 
                    position + new Vector2((pos[0]), (pos[1])), 
                    position + new Vector2((pos[2]), (pos[3])),
                    position + new Vector2((pos2[0]), (pos2[1])),
                    position + new Vector2((pos2[2]), (pos2[3]))/*,
                    position + new Vector2((uv[0]), (uv[1])), 
                    position + new Vector2((uv[2]), (uv[3])),
                    position + new Vector2((uv2[0]), (uv2[1])),
                    position + new Vector2((uv2[2]), (uv2[3]))
                    );
                dl.PopClipRect();*/

            ImGui.EndTabItem();
        }

        private void HealthSettings()
        {
            if (!ImGui.BeginTabItem("Health")) return;

            var enabled = Configuration.HpBarEnabled;
            if (ImGui.Checkbox("Enabled", ref enabled))
            {
                Configuration.HpBarEnabled = enabled;
            }
            var shieldEnabled = Configuration.ShieldBarEnabled;
            if (ImGui.Checkbox("Shield Overlay Enabled", ref shieldEnabled))
            {
                Configuration.ShieldBarEnabled = shieldEnabled;
            }
            ImGui.NewLine();
            ImGui.Separator();
            ImGui.Text("Length");
            ImGui.Separator();
            if (ImGui.TreeNode("Standard"))
            {
                ImGui.BeginGroup();

                var fullRing = Configuration.HpForFullRing;
                if (ImGui.InputInt("HP for full ring", ref fullRing, 5, 50))
                {
                    Configuration.HpForFullRing = fullRing;
                    if (Configuration.HpForFullRing < 1)
                        Configuration.HpForFullRing = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text($"How much HP will make the ring max out, then goes long bar.\n\nDefault: {Defaults.HpForFullRing}");
                    ImGui.End();
                }

                var hpPerPixel = Configuration.HpPerPixelLongBar;
                if (ImGui.InputFloat("HP per pixel for long bar", ref hpPerPixel, 5, 50))
                {
                    Configuration.HpPerPixelLongBar = hpPerPixel;
                    if (Configuration.HpPerPixelLongBar < 0.0001f)
                        Configuration.HpPerPixelLongBar = 0.0001f;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text($"Defines the width of the long bar.\n100 would mean that every 100 HP over the value set for HP for full ring, the bar is 1 pixel wider.\n\nDefault: {Defaults.HpPerPixelLongBar}");
                    ImGui.End();
                }

                var maxLength = Configuration.MaximumHpForMaximumLength;
                if (ImGui.InputInt("Max HP for maximum total length", ref maxLength, 5, 50))
                {
                    Configuration.MaximumHpForMaximumLength = maxLength;
                    if (Configuration.MaximumHpForMaximumLength < 1)
                        Configuration.MaximumHpForMaximumLength = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text(
                        $"Defines when the total bar size, including the ring, will stop getting larger.\n10000 would make the bar stop getting longer at 10000 MaxHP. Prevents an HP bar that's too big.\n\nDefault: {Defaults.MaximumHpForMaximumLength}");
                    ImGui.End();
                }

                var minLength = Configuration.MinimumHpForLength;
                if (ImGui.InputInt("Max HP for minimum length", ref minLength, 5, 50))
                {
                    Configuration.MinimumHpForLength = minLength;
                    if (Configuration.MinimumHpForLength < 1)
                        Configuration.MinimumHpForLength = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text(
                        $"Defines when the total bar size, including the ring, will stop getting smaller.\n1000 would make the bar stop getting smaller at 1000 MaxHP. Prevents an HP bar that's too small.\n\nDefault: {Defaults.MinimumHpForLength}");
                    ImGui.End();
                }
                ImGui.EndGroup();
                ImGui.TreePop();
            }


            if (ImGui.TreeNode("PvP"))
            {
                ImGui.BeginGroup();

                var fullRing = Configuration.PvpHpForFullRing;
                if (ImGui.InputInt("HP for full ring", ref fullRing, 5, 50))
                {
                    Configuration.PvpHpForFullRing = fullRing;
                    if (Configuration.PvpHpForFullRing < 1)
                        Configuration.PvpHpForFullRing = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text($"How much HP will make the ring max out, then goes long bar.\n\nDefault: {Defaults.PvpHpForFullRing}");
                    ImGui.End();
                }

                var hpPerPixel = Configuration.PvpHpPerPixelLongBar;
                if (ImGui.InputFloat("HP per pixel for long bar", ref hpPerPixel, 5, 50))
                {
                    Configuration.PvpHpPerPixelLongBar = hpPerPixel;
                    if (Configuration.PvpHpPerPixelLongBar < 0.0001f)
                        Configuration.PvpHpPerPixelLongBar = 0.0001f;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text($"Defines the width of the long bar.\n100 would mean that every 100 HP over the value set for HP for full ring, the bar is 1 pixel wider.\n\nDefault: {Defaults.PvpHpPerPixelLongBar}");
                    ImGui.End();
                }

                var maxLength = Configuration.PvpMaximumHpForMaximumLength;
                if (ImGui.InputInt("Max HP for maximum total length", ref maxLength, 5, 50))
                {
                    Configuration.PvpMaximumHpForMaximumLength = maxLength;
                    if (Configuration.PvpMaximumHpForMaximumLength < 1)
                        Configuration.PvpMaximumHpForMaximumLength = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text(
                        $"Defines when the total bar size, including the ring, will stop getting larger.\n10000 would make the bar stop getting longer at 10000 MaxHP. Prevents an HP bar that's too big.\n\nDefault: {Defaults.PvpMaximumHpForMaximumLength}");
                    ImGui.End();
                }

                var minLength = Configuration.PvpMinimumHpForLength;
                if (ImGui.InputInt("Max HP for minimum length", ref minLength, 5, 50))
                {
                    Configuration.PvpMinimumHpForLength = minLength;
                    if (Configuration.PvpMinimumHpForLength < 1)
                        Configuration.PvpMinimumHpForLength = 1;
                }

                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text(
                        $"Defines when the total bar size, including the ring, will stop getting smaller.\n1000 would make the bar stop getting smaller at 1000 MaxHP. Prevents an HP bar that's too small.\n\nDefault: {Defaults.PvpMinimumHpForLength}");
                    ImGui.End();
                }

                ImGui.EndGroup();
                ImGui.TreePop();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Value Text");
            ImGui.Separator();

            var hpTextPos = new Vector2(Configuration.HpValueTextPositionX, Configuration.HpValueTextPositionY);
            if (ImGui.DragFloat2("Text Position (X, Y)", ref hpTextPos))
            {
                Configuration.HpValueTextPositionX = hpTextPos.X;
                Configuration.HpValueTextPositionY = hpTextPos.Y;
            }

            var hpTextSize = Configuration.HpValueTextSize;
            if (ImGui.InputFloat("Text Size", ref hpTextSize))
            {
                Configuration.HpValueTextSize = hpTextSize;
            }

            if (ImGui.BeginCombo("Text Alignment", Enum.GetName((TextAlignment)Configuration.HpValueTextAlignment)))
            {
                var alignments = Enum.GetNames(typeof(TextAlignment));
                for (int i = 0; i < alignments.Length; i++)
                {
                    if (ImGui.Selectable(alignments[i]))
                    {
                        Configuration.HpValueTextAlignment = i;
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("TT1", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text("Please note that center and right alignments are not perfect and may not hold the same position.");
                ImGui.End();
            }
            
            if (ImGui.BeginCombo("Text Formatting", Configuration.HpValueTextStyle.GetDescription()))
            {
                var styles = (NumberFormatStyle[])Enum.GetValues(typeof(NumberFormatStyle));
                for (int i = 0; i < styles.Length; i++)
                {
                    if (ImGui.Selectable($"{styles[i].GetDescription()} ({StringFormatting.FormatDigits(1234567, (NumberFormatStyle)i)}) ({StringFormatting.FormatDigits(54321, (NumberFormatStyle)i)})"))
                    {
                        Configuration.HpValueTextStyle = (NumberFormatStyle)i;
                    }
                }
                ImGui.EndCombo();
            }

            var showHpVal = Configuration.ShowHpVal;
            if (ImGui.Checkbox("Show HP Value", ref showHpVal))
            {
                Configuration.ShowHpVal = showHpVal;
            }


            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Miscellaneous");
            ImGui.Separator();

            var lowHpPercent = Configuration.LowHpPercent;
            if (ImGui.SliderFloat("Percent To Trigger Low HP", ref lowHpPercent, 0, 100))
            {
                Configuration.LowHpPercent = lowHpPercent;
            }

            var hpDamageWobbleIntensity = Configuration.HpDamageWobbleIntensity;
            if (ImGui.SliderFloat("Damage wobble intensity %", ref hpDamageWobbleIntensity, 0, 200))
            {
                Configuration.HpDamageWobbleIntensity = hpDamageWobbleIntensity;
            }

            var showHpRecovery = Configuration.ShowHpRecovery;
            if (ImGui.Checkbox("Show HP Recovery", ref showHpRecovery))
            {
                Configuration.ShowHpRecovery = showHpRecovery;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("TT2", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text("Shows a blue bar for when HP is recovered then gradually fills the green bar.");
                ImGui.End();
            }

            ImGui.EndTabItem();
        }

        private void ResourceSettings()
        {
            if (!ImGui.BeginTabItem("MP/GP/CP")) return;

            var enabled = Configuration.ResourceBarEnabled;
            if (ImGui.Checkbox("Enabled", ref enabled))
            {
                Configuration.ResourceBarEnabled = enabled;
            }
            ImGui.NewLine();
            ImGui.Separator();
            ImGui.Text("Position");
            var resourcePos = new Vector2(Configuration.ResourceBarPositionX, Configuration.ResourceBarPositionY);
            if (ImGui.DragFloat2("Position (X, Y)", ref resourcePos))
            {
                Configuration.ResourceBarPositionX = resourcePos.X;
                Configuration.ResourceBarPositionY = resourcePos.Y;
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Value Text");
            ImGui.Separator();

            var showVal = Configuration.ShowResourceVal;
            if (ImGui.Checkbox("Show Resource Value", ref showVal))
            {
                Configuration.ShowResourceVal = showVal;
            }
            var resourceTextPos = new Vector2(Configuration.ResourceTextPositionX, Configuration.ResourceTextPositionY);
            if (ImGui.DragFloat2("Text Position (X, Y)", ref resourceTextPos))
            {
                Configuration.ResourceTextPositionX = resourceTextPos.X;
                Configuration.ResourceTextPositionY = resourceTextPos.Y;
            }

            var resourceTextSize = Configuration.ResourceTextSize;
            if (ImGui.InputFloat("Text Size", ref resourceTextSize))
            {
                Configuration.ResourceTextSize = resourceTextSize;
            }

            if (ImGui.BeginCombo("Text Alignment", Enum.GetName((TextAlignment)Configuration.ResourceTextAlignment)))
            {
                var alignments = Enum.GetNames(typeof(TextAlignment));
                for (int i = 0; i < alignments.Length; i++)
                {
                    if (ImGui.Selectable(alignments[i]))
                    {
                        Configuration.ResourceTextAlignment = i;
                    }
                }
                ImGui.EndCombo();
            }

            if (ImGui.BeginCombo("Text Formatting", Configuration.ResourceTextStyle.GetDescription()))
            {
                var styles = (NumberFormatStyle[])Enum.GetValues(typeof(NumberFormatStyle));
                for (int i = 0; i < styles.Length; i++)
                {
                    if (ImGui.Selectable($"{styles[i].GetDescription()} ({StringFormatting.FormatDigits(10000, (NumberFormatStyle)i)})"))
                    {
                        Configuration.ResourceTextStyle = (NumberFormatStyle)i;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("Length");
            ImGui.Separator();
            ImGui.Text("MP");
            ImGui.Separator();

            var mpPerPixel = Configuration.MpPerPixelLength;
            if (ImGui.InputFloat("MP per pixel for bar length", ref mpPerPixel, 0.1f, 0.5f, "%f"))
            {
                Configuration.MpPerPixelLength = mpPerPixel;
                if (Configuration.MpPerPixelLength < 0.0001f)
                    Configuration.MpPerPixelLength = 0.0001f;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines how long the MP bar is.\nFor example: If set to 20, every 20 MP would increase the width by 1 pixel.\n\nDefault: {Defaults.MpPerPixelLength}");
                ImGui.End();
            }

            var maximumMpLength = Configuration.MaximumMpLength;
            if (ImGui.InputInt("MP for maximum length", ref maximumMpLength, 1, 25))
            {
                Configuration.MaximumMpLength = maximumMpLength;
                if (Configuration.MaximumMpLength < 1)
                    Configuration.MaximumMpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxMP on how long the bar can get.\nSetting to 10000 would prevent the bar from getting longer after 10000 MaxMP.\n\nDefault: {Defaults.MaximumMpLength}");
                ImGui.End();
            }

            var minimumMpLength = Configuration.MinimumMpLength;
            if (ImGui.InputInt("MP for minimum length", ref minimumMpLength, 1, 25))
            {
                Configuration.MinimumMpLength = minimumMpLength;
                if (Configuration.MinimumMpLength < 1)
                    Configuration.MinimumMpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxMP on how small the bar can get.\nSetting to 100 would prevent the bar from getting smaller lower than 100 MaxMP.\n\nDefault: {Defaults.MinimumMpLength}");
                ImGui.End();
            }

            var truncate = Configuration.TruncateMp;
            if (ImGui.Checkbox("Truncate MP Value", ref truncate))
            {
                Configuration.TruncateMp = truncate;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("TT1", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text("Truncate MP from 10000 to 100.");
                ImGui.End();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("GP");
            ImGui.Separator();

            var gpPerPixel = Configuration.GpPerPixelLength;
            if (ImGui.InputFloat("GP per pixel for bar length", ref gpPerPixel, 0.1f, 0.5f, "%f"))
            {
                Configuration.GpPerPixelLength = gpPerPixel;
                if (Configuration.GpPerPixelLength < 0.0001f)
                    Configuration.GpPerPixelLength = 0.0001f;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines how long the GP bar is.\nFor example: If set to 20, every 20 GP would increase the width by 1 pixel.\n\nDefault: {Defaults.GpPerPixelLength}");
                ImGui.End();
            }

            var maximumGpLength = Configuration.MaximumGpLength;
            if (ImGui.InputInt("GP for maximum length", ref maximumGpLength, 1, 25))
            {
                Configuration.MaximumGpLength = maximumGpLength;
                if (Configuration.MaximumGpLength < 1)
                    Configuration.MaximumGpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxGP on how long the bar can get.\nSetting to 500 would prevent the bar from getting longer after 500 MaxGP.\n\nDefault: {Defaults.MaximumGpLength}");
                ImGui.End();
            }

            var minimumGpLength = Configuration.MinimumGpLength;
            if (ImGui.InputInt("GP for minimum length", ref minimumGpLength, 1, 25))
            {
                Configuration.MinimumGpLength = minimumGpLength;
                if (Configuration.MinimumGpLength < 1)
                    Configuration.MinimumGpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxGP on how small the bar can get.\nSetting to 100 would prevent the bar from getting smaller lower than 100 MaxGP.\n\nDefault: {Defaults.MinimumGpLength}");
                ImGui.End();
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Text("CP");
            ImGui.Separator();

            var cpPerPixel = Configuration.CpPerPixelLength;
            if (ImGui.InputFloat("CP per pixel for bar length", ref cpPerPixel, 0.1f, 0.5f, "%f"))
            {
                Configuration.CpPerPixelLength = cpPerPixel;
                if (Configuration.CpPerPixelLength < 0.0001f)
                    Configuration.CpPerPixelLength = 0.0001f;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines how long the CP bar is.\nFor example: If set to 20, every 20 CP would increase the width by 1 pixel.\n\nDefault: {Defaults.CpPerPixelLength}");
                ImGui.End();
            }

            var maximumCpLength = Configuration.MaximumCpLength;
            if (ImGui.InputInt("CP for maximum length", ref maximumCpLength, 1, 25))
            {
                Configuration.MaximumCpLength = maximumCpLength;
                if (Configuration.MaximumCpLength < 1)
                    Configuration.MaximumCpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxCP on how long the bar can get.\nSetting to 400 would prevent the bar from getting longer after 400 MaxCP.\n\nDefault: {Defaults.MaximumCpLength}");
                ImGui.End();
            }

            var minimumCpLength = Configuration.MinimumCpLength;
            if (ImGui.InputInt("CP for minimum length", ref minimumCpLength, 1, 25))
            {
                Configuration.MinimumCpLength = minimumCpLength;
                if (Configuration.MinimumCpLength < 1)
                    Configuration.MinimumCpLength = 1;
            }
            if (ImGui.IsItemHovered())
            {
                Vector2 m = ImGui.GetIO().MousePos;
                ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text($"Defines the limit of MaxCP on how small the bar can get.\nSetting to 100 would prevent the bar from getting smaller lower than 100 MaxCP.\n\nDefault: {Defaults.MinimumCpLength}");
                ImGui.End();
            }

            ImGui.EndTabItem();
        }

        private void LimitSettings()
        {
            if (!ImGui.BeginTabItem("Limit Gauge")) return;

            var enabled = Configuration.LimitBarEnabled;
            if (ImGui.Checkbox("Enabled", ref enabled))
            {
                Configuration.LimitBarEnabled = enabled;
            }
            var limitAlwaysShow = Configuration.LimitGaugeAlwaysShow;
            if (ImGui.Checkbox("Always Show", ref limitAlwaysShow))
            {
                Configuration.LimitGaugeAlwaysShow = limitAlwaysShow;
            }
            var limitDiadem = Configuration.LimitGaugeDiadem;
            if (ImGui.Checkbox("Show for Diadem Compressed Aether", ref limitDiadem))
            {
                Configuration.LimitGaugeDiadem = limitDiadem;
            }
            var limitPosX = Configuration.LimitGaugePositionX;
            if (ImGui.InputFloat("X Position", ref limitPosX, 1, 25))
            {
                Configuration.LimitGaugePositionX = limitPosX;
            }
            var limitPosY = Configuration.LimitGaugePositionY;
            if (ImGui.InputFloat("Y Position", ref limitPosY, 1, 25))
            {
                Configuration.LimitGaugePositionY = limitPosY;
            }

            ImGui.EndTabItem();
        }

        private void ClassSettings()
        {
            if (!ImGui.BeginTabItem("Class Info")) return;

            ImGui.Text("Exp Info");
            ImGui.Separator();

            var expBarEnabled = Configuration.ExpBarEnabled;
            if (ImGui.Checkbox("EXP Bar Enabled", ref expBarEnabled))
            {
                Configuration.ExpBarEnabled = expBarEnabled;
            }

            if (ImGui.TreeNode("Exp Text"))
            {
                //ImGui.Indent(20);
                ImGui.BeginGroup();

                var expTextEnabled = Configuration.ExpValueTextEnabled;
                if (ImGui.Checkbox("Enabled", ref expTextEnabled))
                {
                    Configuration.ExpValueTextEnabled = expTextEnabled;
                }

                var expTextPos = new Vector2(Configuration.ExpValueTextPositionX, Configuration.ExpValueTextPositionY);
                if (ImGui.DragFloat2("Position (X, Y)", ref expTextPos))
                {
                    Configuration.ExpValueTextPositionX = expTextPos.X;
                    Configuration.ExpValueTextPositionY = expTextPos.Y;
                }

                var expTextSize = Configuration.ExpValueTextSize;
                if (ImGui.InputFloat("Size", ref expTextSize))
                {
                    Configuration.ExpValueTextSize = expTextSize;
                }

                if (ImGui.BeginCombo("Alignment", Enum.GetName((TextAlignment)Configuration.ExpValueTextAlignment)))
                {
                    var alignments = Enum.GetNames(typeof(TextAlignment));
                    for (int i = 0; i < alignments.Length; i++)
                    {
                        if (ImGui.Selectable(alignments[i]))
                        {
                            Configuration.ExpValueTextAlignment = i;
                        }
                    }
                    ImGui.EndCombo();
                }
                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("TT1", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text("Please note that center and right alignments are not perfect and may not hold the same position.");
                    ImGui.End();
                }

                if (ImGui.BeginCombo("Formatting", Configuration.ExpValueTextFormatStyle.GetDescription()))
                {
                    var styles = (NumberFormatStyle[])Enum.GetValues(typeof(NumberFormatStyle));
                    for (int i = 0; i < styles.Length; i++)
                    {
                        if (ImGui.Selectable($"{styles[i].GetDescription()} ({StringFormatting.FormatDigits(12345, (NumberFormatStyle)i)}/{StringFormatting.FormatDigits(9999999, (NumberFormatStyle)i)})"))
                        {
                            Configuration.ExpValueTextFormatStyle = (NumberFormatStyle)i;
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.EndGroup();
                ImGui.TreePop();
                //ImGui.Indent(-20);
            }

            ImGui.Separator();


            if (ImGui.TreeNode("Level Text"))
            {
                //ImGui.Indent(20);
                ImGui.BeginGroup();

                var levelTextEnabled = Configuration.LevelEnabled;
                if (ImGui.Checkbox("Enabled", ref levelTextEnabled))
                {
                    Configuration.LevelEnabled = levelTextEnabled;
                }

                var levelTextPos = new Vector2(Configuration.LevelTextX, Configuration.LevelTextY);
                if (ImGui.DragFloat2("Position (X, Y)", ref levelTextPos))
                {
                    Configuration.LevelTextX = levelTextPos.X;
                    Configuration.LevelTextY = levelTextPos.Y;
                }
                
                var levelTextSize = Configuration.LevelTextSize;
                if (ImGui.InputFloat("Size", ref levelTextSize))
                {
                    Configuration.LevelTextSize = levelTextSize;
                }

                if (ImGui.BeginCombo("Alignment", Enum.GetName(Configuration.LevelTextAlignment)))
                {
                    var alignments = Enum.GetNames(typeof(TextAlignment));
                    for (int i = 0; i < alignments.Length; i++)
                    {
                        if (ImGui.Selectable(alignments[i]))
                        {
                            Configuration.LevelTextAlignment = (TextAlignment)i;
                        }
                    }
                    ImGui.EndCombo();
                }
                if (ImGui.IsItemHovered())
                {
                    Vector2 m = ImGui.GetIO().MousePos;
                    ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                    ImGui.Begin("TT1", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                    ImGui.Text("Please note that center and right alignments are not perfect and may not hold the same position.");
                    ImGui.End();
                }

                ImGui.EndGroup();
                ImGui.TreePop();
                //ImGui.Indent(-20);
            }

            ImGui.Text("Class Icon");
            ImGui.Separator();

            var classIconEnabled = Configuration.ClassIconEnabled;
            if (ImGui.Checkbox("Class Icon Enabled", ref classIconEnabled))
            {
                Configuration.ClassIconEnabled = classIconEnabled;
            }


            var classIconPos = new Vector2(Configuration.ClassIconX, Configuration.ClassIconY);
            if (ImGui.DragFloat2("Position (X, Y)", ref classIconPos))
            {
                Configuration.ClassIconX = classIconPos.X;
                Configuration.ClassIconY = classIconPos.Y;
            }

            var scale = Configuration.ClassIconScale;
            if (ImGui.InputFloat("Scale", ref scale, 0.025f, 0.1f))
            {
                Configuration.ClassIconScale = scale;
                if (Configuration.ClassIconScale < 0.1f)
                    Configuration.ClassIconScale = 0.1f;
                if (Configuration.ClassIconScale > 3)
                    Configuration.ClassIconScale = 3;
            }

            ImGui.EndTabItem();
        }

        private void PortraitSettings()
        {
            string supportedImages = "Image Files{.png,.jpg,.jpeg,.bmp}";

            if (!ImGui.BeginTabItem("Portrait")) return;

            var portraitPos = new Vector2(Configuration.PortraitX, Configuration.PortraitY);
            if (ImGui.DragFloat2("Position (X, Y)", ref portraitPos))
            {
                Configuration.PortraitX = portraitPos.X;
                Configuration.PortraitY = portraitPos.Y;
            }

            var portraitScale = Configuration.PortraitScale;
            if (ImGui.DragFloat("Scale##Portrait", ref portraitScale, 0.001f, 0, 10f))
            {
                Configuration.PortraitScale = portraitScale;
            }

            var redWhenDamaged = Configuration.PortraitRedWhenDamaged;
            if (ImGui.Checkbox("Red Color When Damaged##Portrait", ref redWhenDamaged))
            {
                Configuration.PortraitRedWhenDamaged = redWhenDamaged;
            }

            var redWhenDanger = Configuration.PortraitRedWhenDanger;
            if (ImGui.Checkbox("Red Color When Danger Status##Portrait", ref redWhenDanger))
            {
                Configuration.PortraitRedWhenDanger = redWhenDanger;
            }


            ImGui.NewLine();
            ImGui.Text("Portrait image paths");
            ImGui.Separator();
            ImGui.NewLine();
            
            var normalPortraitPath = Configuration.PortraitNormalImage;

            ImGui.Text("Normal Portrait");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1,0,0,1),FindImageMessage(normalPortraitPath));

            ImGui.InputText("##Normal", ref normalPortraitPath, 512, ImGuiInputTextFlags.ReadOnly);

            ImGui.SameLine();
            if (ImGui.Button("Browse...##NormalPortrait"))
            {
                var startDir = Path.GetDirectoryName(Configuration.PortraitNormalImage);

                void UpdatePath(bool success, List<string> paths)
                {
                    if (success && paths.Count > 0)
                    {
                        Configuration.PortraitNormalImage = paths[0];
                        Portrait.SetPortraitNormal(Configuration.PortraitNormalImage);
                    }
                }

                _dialogManager.OpenFileDialog("Choose an image file for Normal Portrait", supportedImages, UpdatePath, 1, startDir);
            }

            var hurtPortraitPath = Configuration.PortraitHurtImage;
            ImGui.Text("Hurt Portrait");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), FindImageMessage(hurtPortraitPath));
            
            ImGui.InputText("##HurtPortrait", ref hurtPortraitPath, 512, ImGuiInputTextFlags.ReadOnly);

            ImGui.SameLine();
            if (ImGui.Button("Browse...##HurtPortrait"))
            {
                var startDir = Path.GetDirectoryName(Configuration.PortraitHurtImage);

                void UpdatePath(bool success, List<string> paths)
                {
                    if (success && paths.Count > 0)
                    {
                        Configuration.PortraitHurtImage = paths[0];
                        Portrait.SetPortraitHurt(Configuration.PortraitHurtImage);
                    }
                }

                _dialogManager.OpenFileDialog("Choose an image file for Hurt Portrait", supportedImages, UpdatePath, 1, startDir);
            }

            var dangerPortraitPath = Configuration.PortraitDangerImage;
            ImGui.Text("Danger Portrait");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), FindImageMessage(dangerPortraitPath));

            ImGui.InputText("##Danger", ref dangerPortraitPath, 512, ImGuiInputTextFlags.ReadOnly);

            ImGui.SameLine();
            if (ImGui.Button("Browse...##DangerPortrait"))
            {
                var startDir = Path.GetDirectoryName(Configuration.PortraitDangerImage);

                void UpdatePath(bool success, List<string> paths)
                {
                    if (success && paths.Count > 0)
                    {
                        Configuration.PortraitDangerImage = paths[0];
                        Portrait.SetPortraitDanger(Configuration.PortraitDangerImage);
                    }
                }

                _dialogManager.OpenFileDialog("Choose an image file for Danger Portrait", supportedImages, UpdatePath, 1, startDir);
            }

            var combatPortraitPath = Configuration.PortraitCombatImage;
            ImGui.Text("Combat Portrait");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), FindImageMessage(combatPortraitPath));

            ImGui.InputText("##Combat", ref combatPortraitPath, 512, ImGuiInputTextFlags.ReadOnly);

            ImGui.SameLine();
            if (ImGui.Button("Browse...##CombatPortrait"))
            {
                var startDir = Path.GetDirectoryName(Configuration.PortraitCombatImage);

                void UpdatePath(bool success, List<string> paths)
                {
                    if (success && paths.Count > 0)
                    {
                        Configuration.PortraitCombatImage = paths[0];
                        Portrait.SetPortraitCombat(Configuration.PortraitCombatImage);
                    }
                }

                _dialogManager.OpenFileDialog("Choose an image file for Combat Portrait", supportedImages, UpdatePath, 1, startDir);
            }

            ImGui.EndTabItem();
        }

        private void SoundSettings()
        {
            if (!ImGui.BeginTabItem("Sound")) return;

            ImGui.NewLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "This feature has been made in to a new plugin called Audible Character Status.");
            ImGui.NewLine();
            ImGui.EndTabItem();
        }

        private void Tooltip(string message)
        {
            Vector2 m = ImGui.GetIO().MousePos;
            ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
            ImGui.Begin("KHTT", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGui.Text(message);
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(600, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Kingdom Hearts Bars: Settings", ref settingsVisible,
               ImGuiWindowFlags.NoCollapse))
            {
                ImGui.BeginTabBar("KhTabBar");
                
                GeneralSettings();
                HealthSettings();
                ResourceSettings();
                LimitSettings();
                ClassSettings();
                PortraitSettings();
                SoundSettings();
                _dialogManager.Draw();

                ImGui.EndTabBar();
                ImGui.Separator();
                if (ImGui.Button("Save"))
                {
                    Configuration.Save();
                }
            }
            ImGui.End();
        }

        /// <summary>
        /// Returns a message depending on if an image is found or not, and if it is not a supported format.
        /// Supported formats are png, jpg, jpeg, and bmp.
        /// </summary>
        /// <param name="path">Path to image</param>
        /// <returns>string</returns>
        private string FindImageMessage(string path)
        {
            if (path.IsNullOrEmpty()) return "";

            var fileFound = File.Exists(path);

            if (!fileFound) return "File not found.";

            string[] supportedImages = { ".png", ".jpg", ".jpeg", ".bmp" };

            var isImage = supportedImages.Any(ext => Path.GetExtension(path) == ext);

            return isImage ? "" : "File is not an image. png, jpg, jpeg, bmp are supported.";
        }
        private FileDialogManager SetupDialogManager()
        {
            var fileManager = new FileDialogManager { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking };

            // Remove Videos and Music.
            fileManager.CustomSideBarItems.Add(("Videos", string.Empty, 0, -1));

            return fileManager;
        }
    }
}
