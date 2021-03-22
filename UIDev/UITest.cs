using ImGuiNET;
using ImGuiScene;
using System;
using System.Numerics;
using UIDev.Framework;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private TextureWrap goatImage;
        private TextureWrap barImage;
        private SimpleImGuiScene scene;

        public void Initialize(SimpleImGuiScene scene)
        {
            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            // eg, to load an image resource for use with ImGui 
            this.goatImage = scene.LoadImage(@"goat.png");
            this.barImage = scene.LoadImage(@"bar_textures.png");

            scene.OnBuildUI += Draw;

            this.Visible = true;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            this.scene = scene;
        }

        public void Dispose()
        {
            this.goatImage.Dispose();
            this.barImage.Dispose();
        }

        // You COULD go all out here and make your UI generic and work on interfaces etc, and then
        // mock dependencies and conceivably use exactly the same class in this testbed and the actual plugin
        // That is, however, a bit excessive in general - it could easily be done for this sample, but I
        // don't want to imply that is easy or the best way to go usually, so it's not done here either
        private void Draw()
        {
            DrawMainWindow();
            DrawSettingsWindow();

            if (!Visible)
            {
                this.scene.ShouldQuit = true;
            }
        }

        #region Nearly a copy/paste of PluginUI
        private bool visible = false;
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

        // this is where you'd have to start mocking objects if you really want to match
        // but for simple UI creation purposes, just hardcoding values works
        private int fakeBarLength = 20;
        private int val1 = 0;
        private int val2 = 0;
        private int val3 = 0;
        private int val4 = 0;
        private int val5 = 0;
        private int val6 = 0;
        private int val7 = 0;
        private int val8 = 0;

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("My Amazing Window", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {

                if (ImGui.Button("Show Settings"))
                {
                    SettingsVisible = true;
                }

                ImGui.Spacing();

                ImGui.Unindent(55);

                ImGui.Dummy(new Vector2(256, 256));

                var drawList = ImGui.GetWindowDrawList();
                var p0 = ImGui.GetItemRectMin();

                drawList.PushClipRect(p0 + new Vector2(val1, val2), p0 + new Vector2(val1, val2) + new Vector2(val5, val6));
                drawList.AddRectFilled(p0 + new Vector2(val1, val2), p0 + new Vector2(val1, val2) + new Vector2(val5, val6), UInt32.MaxValue);
                DrawImage(drawList, barImage, new Vector2(val5, val6), new Vector2(val1, val2), new Vector4(6, 1, 1, 32));
                drawList.PopClipRect();
            }
            ImGui.End();
        }

        public void DrawSettingsWindow()
        {
            if (!SettingsVisible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(350, 300), ImGuiCond.Always);
            if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse))
            {
                ImGui.DragInt("Bar Length", ref this.fakeBarLength, 1, 1, 100);
                ImGui.DragInt("val1", ref this.val1, 1, 0, 100);
                ImGui.DragInt("val2", ref this.val2, 1, 0, 100);
                ImGui.DragInt("val3", ref this.val3, 1, 0, 64);
                ImGui.DragInt("val4", ref this.val4, 1, 0, 64);
                ImGui.DragInt("val5", ref this.val5, 1, -100, 100);
                ImGui.DragInt("val6", ref this.val6, 1, 0, 100);
                ImGui.DragInt("val7", ref this.val7, 1, 0, 64);
                ImGui.DragInt("val8", ref this.val8, 1, 0, 64);
            }
            ImGui.End();
        }

        /// <summary>
        /// Places an image at position relative to the base position of the attached interface object, while being able to define what part of the image to draw.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size">(width, height)</param>
        /// <param name="position">(top, left)</param>
        /// <param name="imageArea">(left, top, width, height)</param>
        public void DrawImage(ImDrawListPtr d, TextureWrap image, Vector2 size, Vector2 position, Vector4 imageArea)
        {
            var basePosition = ImGui.GetItemRectMin();
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            var finalPosition = basePosition + position;

            d.AddImage(image.ImGuiHandle, finalPosition, finalPosition + size, finalPosition + new Vector2(imageArea.X / imageWidth, imageArea.Y / imageHeight), finalPosition + new Vector2((imageArea.X + imageArea.Z) / imageWidth,
                (imageArea.Y + imageArea.W) / imageHeight));
        }
        #endregion
    }
}
