using DiligentEngine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public interface IRTImageBlitterImpl : IDisposable
    {
        uint Height { get; }
        ITexture Texture { get; }
        uint Width { get; }

        void Blit(GraphicsEngine graphicsEngine);
        void CreateBuffers(GraphicsEngine graphicsEngine, ShaderLoader<RTShaders> shaderLoader);
        void WindowResize(GraphicsEngine graphicsEngine, uint width, uint height);
    }

    public class RTImageBlitter : IDisposable
    {
        private readonly GraphicsEngine graphicsEngine;
        private readonly OSWindow window;

        IRTImageBlitterImpl blitterImpl;

        public RTImageBlitter(ShaderLoader<RTShaders> shaderLoader, GraphicsEngine graphicsEngine, OSWindow window)
        {
            this.graphicsEngine = graphicsEngine;
            this.window = window;

            //blitterImpl = new FSRImageBlitterImpl();
            blitterImpl = new DirectImageBlitter();
            blitterImpl.CreateBuffers(graphicsEngine, shaderLoader);

            WindowResize((uint)window.WindowWidth, (uint)window.WindowHeight);
            window.Resized += Window_Resized;
        }

        public void Dispose()
        {
            window.Resized -= Window_Resized;
            blitterImpl?.Dispose();
        }

        public void SetupUnorderedAccess(List<StateTransitionDesc> barriers)
        {
            barriers.Add(new StateTransitionDesc { pResource = blitterImpl.Texture, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_UNORDERED_ACCESS, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
        }

        public void Blit()
        {
            blitterImpl.Blit(graphicsEngine);
        }

        private void Window_Resized(OSWindow window)
        {
            WindowResize((uint)window.WindowWidth, (uint)window.WindowHeight);
        }

        public IDeviceObject TextureView => blitterImpl.Texture.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_UNORDERED_ACCESS);

        public uint Width => blitterImpl.Texture.GetDesc_Width;

        public uint Height => blitterImpl.Texture.GetDesc_Height;

        public void WindowResize(UInt32 width, UInt32 height)
        {
            blitterImpl.WindowResize(graphicsEngine, width, height);
        }
    }
}
