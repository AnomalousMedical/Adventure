﻿using DiligentEngine;
using Engine.Platform;
using Microsoft.Extensions.Logging;
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
        ITexture RTTexture { get; }
        ITexture FullBufferTexture { get; }
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

        public RTImageBlitter(ShaderLoader<RTShaders> shaderLoader, GraphicsEngine graphicsEngine, OSWindow window, DiligentEngineOptions options, ILogger<RTImageBlitter> logger)
        {
            this.graphicsEngine = graphicsEngine;
            this.window = window;

            switch (options.UpsamplingMethod)
            {
                case UpsamplingMethod.FSR1:
                    logger.LogInformation($"Creating FSR1 blitter upsampling from {options.FSR1RenderPercentage}.");
                    blitterImpl = new FSRImageBlitterImpl(options);
                    break;
                case UpsamplingMethod.None:
                default:
                    logger.LogInformation($"Creating direct blitter.");
                    blitterImpl = new DirectImageBlitter();
                    break;
            }
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
            barriers.Add(new StateTransitionDesc { pResource = blitterImpl.RTTexture, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_UNORDERED_ACCESS, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
        }

        public void Blit()
        {
            blitterImpl.Blit(graphicsEngine);
        }

        private void Window_Resized(OSWindow window)
        {
            WindowResize((uint)window.WindowWidth, (uint)window.WindowHeight);
        }

        public IDeviceObject RTTextureView => blitterImpl.RTTexture.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_UNORDERED_ACCESS);

        public uint RTBufferWidth => blitterImpl.RTTexture.GetDesc_Width;

        public uint RTBufferHeight => blitterImpl.RTTexture.GetDesc_Height;

        public uint FullBufferWidth => blitterImpl.FullBufferTexture.GetDesc_Width;

        public uint FullBufferHeight => blitterImpl.FullBufferTexture.GetDesc_Height;

        public void WindowResize(UInt32 width, UInt32 height)
        {
            blitterImpl.WindowResize(graphicsEngine, width, height);
        }
    }
}
