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

enum FeatureFlags : Uint32 {
	FeatureFlags_NONE = 0,
	FeatureFlags_RAY_TRACING = 1 << 1,
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

    bool useVulkan = true;

    if (useVulkan)
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
        
    }
    else 
    {
#    if ENGINE_DLL
        // Load the dll and import GetEngineFactoryD3D12() function
        auto GetEngineFactoryD3D12 = LoadGraphicsEngineD3D12();
#    endif
        auto* pFactoryD3D12 = GetEngineFactoryD3D12();
        if (!pFactoryD3D12->LoadD3D12())
        {
            //LOG_ERROR_AND_THROW("Failed to load Direct3D12");
        }
        //m_pEngineFactory = pFactoryD3D12;

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
            //#    if D3D11_SUPPORTED
            //        LOG_ERROR_MESSAGE("Failed to find Direct3D12-compatible hardware adapters. Attempting to initialize the engine in Direct3D11 mode.");
            //        m_DeviceType = RENDER_DEVICE_TYPE_D3D11;
            //        InitializeDiligentEngine(pWindow);
            //        return;
            //#    else
            //        LOG_ERROR_AND_THROW("Failed to find Direct3D12-compatible hardware adapters.");
            //#    endif
        }

        Uint32       m_AdapterId = 0;
        ADAPTER_TYPE m_AdapterType = ADAPTER_TYPE_UNKNOWN;


        EngineCI.AdapterId = m_AdapterId;
        if (m_AdapterType == ADAPTER_TYPE_SOFTWARE)
        {
            for (Uint32 i = 0; i < Adapters.size(); ++i)
            {
                if (Adapters[i].Type == m_AdapterType)
                {
                    EngineCI.AdapterId = i;
                    //LOG_INFO_MESSAGE("Found software adapter '", Adapters[i].Description, "'");
                    break;
                }
            }
        }

        /*m_TheSample->ModifyEngineInitInfo({ pFactoryD3D12, m_DeviceType, EngineCI, m_SwapChainInitDesc });*/
        EngineD3D12CreateInfo& EngineD3D12CI = static_cast<EngineD3D12CreateInfo&>(EngineCI);
        EngineD3D12CI.GPUDescriptorHeapDynamicSize[0] = 32768;
        EngineD3D12CI.GPUDescriptorHeapSize[1] = 128;
        EngineD3D12CI.GPUDescriptorHeapDynamicSize[1] = 2048 - 128;
        EngineD3D12CI.DynamicDescriptorAllocationChunkSize[0] = 32;
        EngineD3D12CI.DynamicDescriptorAllocationChunkSize[1] = 8; // D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER
        //end moidfyengineinitinfo

        //m_AdapterAttribs = Adapters[EngineCI.AdapterId];
        //if (m_AdapterType != ADAPTER_TYPE_SOFTWARE)
        //{
        //    Uint32 NumDisplayModes = 0;
        //    pFactoryD3D12->EnumerateDisplayModes(EngineCI.GraphicsAPIVersion, EngineCI.AdapterId, 0, TEX_FORMAT_RGBA8_UNORM_SRGB, NumDisplayModes, nullptr);
        //    m_DisplayModes.resize(NumDisplayModes);
        //    pFactoryD3D12->EnumerateDisplayModes(EngineCI.GraphicsAPIVersion, EngineCI.AdapterId, 0, TEX_FORMAT_RGBA8_UNORM_SRGB, NumDisplayModes, m_DisplayModes.data());
        //}

        /*NumImmediateContexts = std::max(1u, EngineCI.NumImmediateContexts);
        ppContexts.resize(NumImmediateContexts + EngineCI.NumDeferredContexts);*/
        pFactoryD3D12->CreateDeviceAndContextsD3D12(EngineCI, &(result.m_pDevice), &(result.m_pImmediateContext));
        //if (!m_pDevice)
        //{
        //    /*LOG_ERROR_AND_THROW("Unable to initialize Diligent Engine in Direct3D12 mode. The API may not be available, "
        //        "or required features may not be supported by this GPU/driver/OS version.");*/
        //}

        if (result.m_pDevice != NULL)
        {
            Win32NativeWindow Window{ hWnd };
            pFactoryD3D12->CreateSwapChainD3D12(result.m_pDevice, result.m_pImmediateContext, SCDesc, FullScreenModeDesc{}, Window, &(result.m_pSwapChain));
        }

    }
    
    return result;
}