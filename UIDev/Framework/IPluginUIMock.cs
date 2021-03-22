using System;
using ImGuiScene;

namespace UIDev.Framework
{
    interface IPluginUIMock : IDisposable
    {
        void Initialize(SimpleImGuiScene scene);
    }
}
