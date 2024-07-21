using Engine;
using System;
using System.Runtime.InteropServices;

using Uint8 = System.Byte;
using Int8 = System.SByte;
using Bool = System.Boolean;
using Uint32 = System.UInt32;
using Uint64 = System.UInt64;
using Float32 = System.Single;
using Uint16 = System.UInt16;
using System.Text.Json.Serialization;

namespace DiligentEngine
{
    public class GraphicsEngine : IDisposable
    {
        public enum FeatureFlags : Uint32
        {
            None = 0,
            RayTracing = 1 << 1,
        };

        [JsonConverter(typeof(JsonStringEnumConverter<GraphicsEngine.RenderApi>))]
        public enum RenderApi : Uint32
        {
            Vulkan = 0,
            D3D12 = 1
        }

        AutoPtr<IRenderDevice> RenderDevicePtr;
        AutoPtr<IDeviceContext> ImmediateContextPtr;
        AutoPtr<ISwapChain> SwapChainPtr;

        [StructLayout(LayoutKind.Sequential)]
        struct CreateDeviceAndSwapChainResult
        {
            public IntPtr m_pDevice;
            public IntPtr m_pImmediateContext;
            public IntPtr m_pSwapChain;
        }

        public void Dispose()
        {
            this.ImmediateContext.Flush(); //The sample app flushes this out when it shuts down

            this.RenderDevicePtr.Dispose();
            this.ImmediateContextPtr.Dispose();
            this.SwapChainPtr.Dispose();
        }

        internal void CreateDeviceAndSwapChain(IntPtr hwnd, SwapChainDesc swapChainDesc, FeatureFlags features, RenderApi renderApi, UInt32? deviceId)
        {
            var result = GenericEngineFactory_CreateDeviceAndSwapChain(
            hwnd
            , features
            , renderApi
            , swapChainDesc.Width
            , swapChainDesc.Height
            , swapChainDesc.ColorBufferFormat
            , swapChainDesc.DepthBufferFormat
            , swapChainDesc.Usage
            , swapChainDesc.PreTransform
            , swapChainDesc.BufferCount
            , swapChainDesc.DefaultDepthValue
            , swapChainDesc.DefaultStencilValue
            , swapChainDesc.IsPrimary
            , deviceId.HasValue ? deviceId.Value + 1 : 0
            );

            if(result.m_pDevice == IntPtr.Zero)
            {
                var message = "Cannot create device.";
                if (features.HasFlag(FeatureFlags.RayTracing))
                {
                    message += " This application requires hardware with support for ray tracing.";
                }
                throw new InvalidOperationException(message);
            }

            this.RenderDevicePtr = new AutoPtr<IRenderDevice>(new IRenderDevice(result.m_pDevice), false);
            this.ImmediateContextPtr = new AutoPtr<IDeviceContext>(new IDeviceContext(result.m_pImmediateContext), false);
            this.SwapChainPtr = new AutoPtr<ISwapChain>(new ISwapChain(result.m_pSwapChain), false);
        }

        public IRenderDevice RenderDevice => this.RenderDevicePtr.Obj;

        public IDeviceContext ImmediateContext => this.ImmediateContextPtr.Obj;

        public ISwapChain SwapChain => this.SwapChainPtr.Obj;

        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern CreateDeviceAndSwapChainResult GenericEngineFactory_CreateDeviceAndSwapChain(
            IntPtr hWnd
            , FeatureFlags features
            , RenderApi renderApi
            , Uint32 Width                       
            , Uint32 Height                      
            , TEXTURE_FORMAT ColorBufferFormat   
            , TEXTURE_FORMAT DepthBufferFormat   
            , SWAP_CHAIN_USAGE_FLAGS Usage       
            , SURFACE_TRANSFORM PreTransform     
            , Uint32 BufferCount                 
            , Float32 DefaultDepthValue          
            , Uint8 DefaultStencilValue          
            , [MarshalAs(UnmanagedType.I1)] bool IsPrimary
            , UInt32 deviceId
            );
    }
}
