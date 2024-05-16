#include "StdAfx.h"

//#include "Graphics/GraphicsEngineD3D11/interface/EngineFactoryD3D11.h"
#include "Graphics/GraphicsEngineD3D12/interface/EngineFactoryD3D12.h"
//#include "Graphics/GraphicsEngineOpenGL/interface/EngineFactoryOpenGL.h"
#include "Graphics/GraphicsEngineVulkan/interface/EngineFactoryVk.h"
#include <vector>

#include "Graphics/GraphicsEngine/interface/RenderDevice.h"
#include "Graphics/GraphicsEngine/interface/DeviceContext.h"
#include "Graphics/GraphicsEngine/interface/SwapChain.h"
#include "Color.h"

using namespace Diligent;

enum FeatureFlags : Uint32 
{
	FeatureFlags_NONE = 0,
	FeatureFlags_RAY_TRACING = 1 << 1,
};

enum RenderApi : Uint32
{
    RenderApi_Vulkan = 0,
    RenderApi_D3D12 = 1
};

struct CreateDeviceAndSwapChainResult
{
	IRenderDevice* m_pDevice;
	IDeviceContext* m_pImmediateContext;
	ISwapChain* m_pSwapChain;
};

extern "C" _AnomalousExport CreateDeviceAndSwapChainResult GenericEngineFactory_CreateDeviceAndSwapChain(
	void* hWnd
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
	, bool IsPrimary
    , Uint32 deviceId
)
{
	CreateDeviceAndSwapChainResult result;
	SwapChainDesc SCDesc;
	SCDesc.Width = Width;
	SCDesc.Height = Height;
	SCDesc.ColorBufferFormat = ColorBufferFormat;
	SCDesc.DepthBufferFormat = DepthBufferFormat;
	SCDesc.Usage = Usage;
	SCDesc.PreTransform = PreTransform;
	SCDesc.BufferCount = BufferCount;
	SCDesc.DefaultDepthValue = DefaultDepthValue;
	SCDesc.DefaultStencilValue = DefaultStencilValue;
	SCDesc.IsPrimary = IsPrimary;

    switch (renderApi) {
        case RenderApi_Vulkan:
        {
#   if EXPLICITLY_LOAD_ENGINE_VK_DLL
            // Load the dll and import GetEngineFactoryVk() function
            auto GetEngineFactoryVk = LoadGraphicsEngineVk();
#   endif

            EngineVkCreateInfo EngineCI;
            EngineCI.Features.RayTracing = (features & FeatureFlags_RAY_TRACING) == FeatureFlags_RAY_TRACING ? DEVICE_FEATURE_STATE_ENABLED : DEVICE_FEATURE_STATE_DISABLED;

#   ifdef DILIGENT_DEBUG
            EngineCI.EnableValidation = true;
#   endif

            auto* pFactoryVk = GetEngineFactoryVk();
            pFactoryVk->CreateDeviceAndContextsVk(EngineCI, &(result.m_pDevice), &(result.m_pImmediateContext));

            if (result.m_pDevice == NULL)
            {
                //Fix for amd optimus switchable graphics
                //https://github.com/KhronosGroup/Vulkan-Loader/issues/552
                SetEnvironmentVariable(L"DISABLE_LAYER_AMD_SWITCHABLE_GRAPHICS_1", L"1");
                pFactoryVk->CreateDeviceAndContextsVk(EngineCI, &(result.m_pDevice), &(result.m_pImmediateContext));
            }

            if (result.m_pDevice != NULL)
            {
                Win32NativeWindow Window{ hWnd };
                pFactoryVk->CreateSwapChainVk(result.m_pDevice, result.m_pImmediateContext, SCDesc, Window, &(result.m_pSwapChain));
            }
            break;
        }


        case RenderApi_D3D12:
        {
#    if ENGINE_DLL
            // Load the dll and import GetEngineFactoryD3D12() function
            auto GetEngineFactoryD3D12 = LoadGraphicsEngineD3D12();
#    endif
            auto* pFactoryD3D12 = GetEngineFactoryD3D12();
            if (!pFactoryD3D12->LoadD3D12())
            {
                //This is an error
                return result;
            }

            EngineD3D12CreateInfo EngineCI;
            EngineCI.GraphicsAPIVersion = { 11, 0 };
            EngineCI.Features.RayTracing = (features & FeatureFlags_RAY_TRACING) == FeatureFlags_RAY_TRACING ? DEVICE_FEATURE_STATE_ENABLED : DEVICE_FEATURE_STATE_DISABLED;
            //if (m_ValidationLevel >= 0)
            //    EngineCI.SetValidationLevel(static_cast<VALIDATION_LEVEL>(m_ValidationLevel));

            Uint32 NumAdapters = 0;
            pFactoryD3D12->EnumerateAdapters(EngineCI.GraphicsAPIVersion, NumAdapters, nullptr);
            std::vector<GraphicsAdapterInfo> Adapters(NumAdapters);
            if (NumAdapters > 0)
            {
                pFactoryD3D12->EnumerateAdapters(EngineCI.GraphicsAPIVersion, NumAdapters, Adapters.data());
            }
            else
            {
                //This is an error
                return result;
            }

            //Use first discrete adapter, or the only adapter if there is only 1
            //Will fallback to device 0 if no discrete adapter is found
            Uint32 m_AdapterId = 0;
            if (deviceId == 0) {
                if (NumAdapters > 1)
                {
                    Uint32 discreteAdapterId = 0;
                    bool foundDiscreteAdapter = false;

                    for (const Diligent::GraphicsAdapterInfo& i : Adapters) {
                        if (i.Type == ADAPTER_TYPE_DISCRETE) {
                            foundDiscreteAdapter = true;
                            break;
                        }
                        else {
                            ++discreteAdapterId;
                        }
                    }

                    if (foundDiscreteAdapter) {
                        m_AdapterId = discreteAdapterId;
                    }
                }
            }
            //User provided id
            else {
                m_AdapterId = deviceId - 1;
            }

            EngineCI.AdapterId = m_AdapterId;
            EngineCI.GPUDescriptorHeapDynamicSize[0] = 32768;
            EngineCI.GPUDescriptorHeapSize[1] = 128;
            EngineCI.GPUDescriptorHeapDynamicSize[1] = 2048 - 128;
            EngineCI.DynamicDescriptorAllocationChunkSize[0] = 32;
            EngineCI.DynamicDescriptorAllocationChunkSize[1] = 8; // D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER

            //ADAPTER_TYPE m_AdapterType = ADAPTER_TYPE_UNKNOWN;
            //m_AdapterAttribs = Adapters[EngineCI.AdapterId];
            //if (m_AdapterType != ADAPTER_TYPE_SOFTWARE)
            //{
            //    Uint32 NumDisplayModes = 0;
            //    pFactoryD3D12->EnumerateDisplayModes(EngineCI.GraphicsAPIVersion, EngineCI.AdapterId, 0, TEX_FORMAT_RGBA8_UNORM_SRGB, NumDisplayModes, nullptr);
            //    m_DisplayModes.resize(NumDisplayModes);
            //    pFactoryD3D12->EnumerateDisplayModes(EngineCI.GraphicsAPIVersion, EngineCI.AdapterId, 0, TEX_FORMAT_RGBA8_UNORM_SRGB, NumDisplayModes, m_DisplayModes.data());
            //}

            pFactoryD3D12->CreateDeviceAndContextsD3D12(EngineCI, &(result.m_pDevice), &(result.m_pImmediateContext));

            if (result.m_pDevice != NULL)
            {
                Win32NativeWindow Window{ hWnd };
                pFactoryD3D12->CreateSwapChainD3D12(result.m_pDevice, result.m_pImmediateContext, SCDesc, FullScreenModeDesc{}, Window, &(result.m_pSwapChain));
            }
            break;
        }
    }
    
    return result;
}