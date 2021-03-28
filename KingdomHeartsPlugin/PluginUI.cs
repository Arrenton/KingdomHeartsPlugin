using System;
using System.Numerics;
using Dalamud.Game.ClientState.Actors.Types;
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
                    if (ImGui.InputInt("HP for minimum total length", ref minLength, 5, 50))
                    {
                        Configuration.MinimumHpForLength = minLength;
                        if (Configuration.MinimumHpForLength < 1)
                            Configuration.MinimumHpForLength = 1;
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
