using System;
using System.IO;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Interface;
using Dalamud.Plugin;
using ImGuiNET;
using KingdomHeartsPlugin.HealthBar;
using KingdomHeartsPlugin.Utilities;

namespace KingdomHeartsPlugin
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    public class PluginUI : IDisposable
    {
        internal Configuration Configuration;
        private readonly HealthFrame _healthFrame;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = true;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, DalamudPluginInterface pluginInterface)
        {
            this.Configuration = configuration;
            _healthFrame = new HealthFrame(pluginInterface, this);
        }

        public void Dispose()
        {
            _healthFrame?.Dispose();
            ImageDrawing.Dispose();
        }

        public void OnUpdate(DalamudPluginInterface pi)
        {
        }

        public void Draw(PlayerCharacter player)
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow(player);
            DrawSettingsWindow();
        }

        public void DrawMainWindow(PlayerCharacter player)
        {
            if (!Visible)
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

            var size = new Vector2(320, 180);
            ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGui.SetNextWindowSizeConstraints(size, new Vector2(float.MaxValue, float.MaxValue));
            
            if (ImGui.Begin("KH Frame", ref this.visible, window_flags))
            {
                _healthFrame.Draw(player);
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Kingdom Hearts Bars: Configuration", ref this.settingsVisible,
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                ImGui.BeginTabBar("KhTabBar");
                if (ImGui.BeginTabItem("General"))
                {
                    // can't ref a property, so use a local copy
                    var enabled = this.Configuration.Locked;
                    if (ImGui.Checkbox("Locked", ref enabled))
                    {
                        this.Configuration.Locked = enabled;
                        // can save immediately on change, if you don't want to provide a "Save and Close" button
                    }

                    var scale = this.Configuration.Scale;
                    if (ImGui.InputFloat("Scale", ref scale, 0.025f, 0.1f))
                    {
                        Configuration.Scale = scale;
                        if (Configuration.Scale < 0.25f)
                            Configuration.Scale = 0.25f;
                        if (Configuration.Scale > 3)
                            Configuration.Scale = 3;
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Health"))
                {
                    var fullRing = Configuration.HpForFullRing;
                    if (ImGui.InputInt("HP for full ring", ref fullRing, 5, 50))
                    {
                        Configuration.HpForFullRing = fullRing;
                        if (Configuration.HpForFullRing < 1)
                            Configuration.HpForFullRing = 1;
                    }

                    var hpPerPixel = Configuration.HpPerPixelLongBar;
                    if (ImGui.InputFloat("HP per pixel for long bar", ref hpPerPixel, 5, 50))
                    {
                        Configuration.HpPerPixelLongBar = hpPerPixel;
                        if (Configuration.HpPerPixelLongBar < 0.0001f)
                            Configuration.HpPerPixelLongBar = 0.0001f;
                    }

                    var maxLength = Configuration.MaximumHpForMaximumLength;
                    if (ImGui.InputInt("HP for maximum total length", ref maxLength, 5, 50))
                    {
                        Configuration.MaximumHpForMaximumLength = maxLength;
                        if (Configuration.MaximumHpForMaximumLength < 1)
                            Configuration.MaximumHpForMaximumLength = 1;
                    }

                    var minLength = Configuration.MinimumHpForLength;
                    if (ImGui.InputInt("HP for minimum length", ref minLength, 5, 50))
                    {
                        Configuration.MinimumHpForLength = minLength;
                        if (Configuration.MinimumHpForLength < 1)
                            Configuration.MinimumHpForLength = 1;
                    }

                    var truncate = Configuration.TruncateHp;
                    if (ImGui.Checkbox("Truncate HP Text Value", ref truncate))
                    {
                        Configuration.TruncateHp = truncate;
                    }
                    if (ImGui.IsItemHovered())
                    {
                        Vector2 m = ImGui.GetIO().MousePos;
                        ImGui.SetNextWindowPos(new Vector2(m.X + 20, m.Y + 20));
                        ImGui.Begin("TT1", ImGuiWindowFlags.Tooltip | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
                        ImGui.Text("Truncate HP value over 10000 to 10.0K and 100000 to 100K");
                        ImGui.End();
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("MP/GP/CP"))
                {
                    ImGui.Text("MP");
                    ImGui.Separator();
                    var mpPerPixel = Configuration.MpPerPixelLength;
                    if (ImGui.InputFloat("MP per pixel for bar length", ref mpPerPixel, 0.1f, 0.5f, "%f"))
                    {
                        Configuration.MpPerPixelLength = mpPerPixel;
                        if (Configuration.MpPerPixelLength < 0.0001f)
                            Configuration.MpPerPixelLength = 0.0001f;
                    }

                    var maximumMpLength = Configuration.MaximumMpLength;
                    if (ImGui.InputInt("MP for maximum length", ref maximumMpLength, 1, 25))
                    {
                        Configuration.MaximumMpLength = maximumMpLength;
                        if (Configuration.MaximumMpLength < 1)
                            Configuration.MaximumMpLength = 1;
                    }

                    var minimumMpLength = Configuration.MinimumMpLength;
                    if (ImGui.InputInt("MP for minimum length", ref minimumMpLength, 1, 25))
                    {
                        Configuration.MinimumMpLength = minimumMpLength;
                        if (Configuration.MinimumMpLength < 1)
                            Configuration.MinimumMpLength = 1;
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

                    var maximumGpLength = Configuration.MaximumGpLength;
                    if (ImGui.InputInt("GP for maximum length", ref maximumGpLength, 1, 25))
                    {
                        Configuration.MaximumGpLength = maximumGpLength;
                        if (Configuration.MaximumGpLength < 1)
                            Configuration.MaximumGpLength = 1;
                    }

                    var minimumGpLength = Configuration.MinimumGpLength;
                    if (ImGui.InputInt("GP for minimum length", ref minimumGpLength, 1, 25))
                    {
                        Configuration.MinimumGpLength = minimumGpLength;
                        if (Configuration.MinimumGpLength < 1)
                            Configuration.MinimumGpLength = 1;
                    }

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

                    var maximumCpLength = Configuration.MaximumCpLength;
                    if (ImGui.InputInt("CP for maximum length", ref maximumCpLength, 1, 25))
                    {
                        Configuration.MaximumCpLength = maximumCpLength;
                        if (Configuration.MaximumCpLength < 1)
                            Configuration.MaximumCpLength = 1;
                    }

                    var minimumCpLength = Configuration.MinimumCpLength;
                    if (ImGui.InputInt("CP for minimum length", ref minimumCpLength, 1, 25))
                    {
                        Configuration.MinimumCpLength = minimumCpLength;
                        if (Configuration.MinimumCpLength < 1)
                            Configuration.MinimumCpLength = 1;
                    }

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
                ImGui.Separator();
                if (ImGui.Button("Save"))
                {
                    this.Configuration.Save();
                }
            }
            ImGui.End();
        }
    }
}
