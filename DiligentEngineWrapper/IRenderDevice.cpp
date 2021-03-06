#include "StdAfx.h"
#include "Graphics/GraphicsEngine/interface/RenderDevice.h"
#include "Color.h"
#include "LayoutElement.PassStruct.h"
#include "ShaderResourceVariableDesc.PassStruct.h"
#include "ImmutableSamplerDesc.PassStruct.h"
#include "TextureSubResData.PassStruct.h"
#include "RenderTargetBlendDesc.PassStruct.h"
#include "RayTracingGeneralShaderGroup.PassStruct.h"
#include "RayTracingProceduralHitShaderGroup.PassStruct.h"
#include "RayTracingTriangleHitShaderGroup.PassStruct.h"
#include "BLASTriangleDesc.PassStruct.h"
#include "BLASBoundingBoxDesc.PassStruct.h"
using namespace Diligent;
extern "C" _AnomalousExport IBuffer* IRenderDevice_CreateBuffer(
	IRenderDevice* objPtr
	, Uint64 BuffDesc_Size
	, BIND_FLAGS BuffDesc_BindFlags
	, USAGE BuffDesc_Usage
	, CPU_ACCESS_FLAGS BuffDesc_CPUAccessFlags
	, BUFFER_MODE BuffDesc_Mode
	, MISC_BUFFER_FLAGS BuffDesc_MiscFlags
	, Uint32 BuffDesc_ElementByteStride
	, Uint64 BuffDesc_ImmediateContextMask
	, Char* BuffDesc_Name
	, void* pBuffData_pData
	, Uint64 pBuffData_DataSize
	, IDeviceContext* pBuffData_pContext
)
{
	BufferDesc BuffDesc;
	BuffDesc.Size = BuffDesc_Size;
	BuffDesc.BindFlags = BuffDesc_BindFlags;
	BuffDesc.Usage = BuffDesc_Usage;
	BuffDesc.CPUAccessFlags = BuffDesc_CPUAccessFlags;
	BuffDesc.Mode = BuffDesc_Mode;
	BuffDesc.MiscFlags = BuffDesc_MiscFlags;
	BuffDesc.ElementByteStride = BuffDesc_ElementByteStride;
	BuffDesc.ImmediateContextMask = BuffDesc_ImmediateContextMask;
	BuffDesc.Name = BuffDesc_Name;
	BufferData pBuffData;
	pBuffData.pData = pBuffData_pData;
	pBuffData.DataSize = pBuffData_DataSize;
	pBuffData.pContext = pBuffData_pContext;
	IBuffer* theReturnValue = nullptr;
	objPtr->CreateBuffer(
		BuffDesc
		, &pBuffData
		, &theReturnValue
	);
	return theReturnValue;
}
extern "C" _AnomalousExport IShader* IRenderDevice_CreateShader(
	IRenderDevice* objPtr
	, Char* ShaderCI_FilePath
	, Char* ShaderCI_Source
	, Char* ShaderCI_EntryPoint
	, bool ShaderCI_UseCombinedTextureSamplers
	, Char* ShaderCI_CombinedSamplerSuffix
	, SHADER_TYPE ShaderCI_Desc_ShaderType
	, Char* ShaderCI_Desc_Name
	, SHADER_SOURCE_LANGUAGE ShaderCI_SourceLanguage
	, SHADER_COMPILER ShaderCI_ShaderCompiler
	, Uint32 ShaderCI_HLSLVersion_Major
	, Uint32 ShaderCI_HLSLVersion_Minor
	, Uint32 ShaderCI_MSLVersion_Major
	, Uint32 ShaderCI_MSLVersion_Minor
	, SHADER_COMPILE_FLAGS ShaderCI_CompileFlags
)
{
	ShaderCreateInfo ShaderCI;
	ShaderCI.FilePath = ShaderCI_FilePath;
	ShaderCI.Source = ShaderCI_Source;
	ShaderCI.EntryPoint = ShaderCI_EntryPoint;
	ShaderCI.UseCombinedTextureSamplers = ShaderCI_UseCombinedTextureSamplers;
	ShaderCI.CombinedSamplerSuffix = ShaderCI_CombinedSamplerSuffix;
	ShaderCI.Desc.ShaderType = ShaderCI_Desc_ShaderType;
	ShaderCI.Desc.Name = ShaderCI_Desc_Name;
	ShaderCI.SourceLanguage = ShaderCI_SourceLanguage;
	ShaderCI.ShaderCompiler = ShaderCI_ShaderCompiler;
	ShaderCI.HLSLVersion.Major = ShaderCI_HLSLVersion_Major;
	ShaderCI.HLSLVersion.Minor = ShaderCI_HLSLVersion_Minor;
	ShaderCI.MSLVersion.Major = ShaderCI_MSLVersion_Major;
	ShaderCI.MSLVersion.Minor = ShaderCI_MSLVersion_Minor;
	ShaderCI.CompileFlags = ShaderCI_CompileFlags;
	IShader* theReturnValue = nullptr;
	objPtr->CreateShader(
		ShaderCI
		, &theReturnValue
	);
	return theReturnValue;
}
extern "C" _AnomalousExport ITexture* IRenderDevice_CreateTexture(
	IRenderDevice* objPtr
	, RESOURCE_DIMENSION TexDesc_Type
	, Uint32 TexDesc_Width
	, Uint32 TexDesc_Height
	, Uint32 TexDesc_ArraySize
	, TEXTURE_FORMAT TexDesc_Format
	, Uint32 TexDesc_MipLevels
	, Uint32 TexDesc_SampleCount
	, BIND_FLAGS TexDesc_BindFlags
	, USAGE TexDesc_Usage
	, CPU_ACCESS_FLAGS TexDesc_CPUAccessFlags
	, MISC_TEXTURE_FLAGS TexDesc_MiscFlags
	, TEXTURE_FORMAT TexDesc_ClearValue_Format
	, Float32 TexDesc_ClearValue_Color_0
	, Float32 TexDesc_ClearValue_Color_1
	, Float32 TexDesc_ClearValue_Color_2
	, Float32 TexDesc_ClearValue_Color_3
	, Float32 TexDesc_ClearValue_DepthStencil_Depth
	, Uint8 TexDesc_ClearValue_DepthStencil_Stencil
	, Uint64 TexDesc_ImmediateContextMask
	, Char* TexDesc_Name
	, TextureSubResDataPassStruct* pData_pSubResources
	, Uint32 pData_NumSubresources
)
{
	TextureDesc TexDesc;
	TexDesc.Type = TexDesc_Type;
	TexDesc.Width = TexDesc_Width;
	TexDesc.Height = TexDesc_Height;
	TexDesc.ArraySize = TexDesc_ArraySize;
	TexDesc.Format = TexDesc_Format;
	TexDesc.MipLevels = TexDesc_MipLevels;
	TexDesc.SampleCount = TexDesc_SampleCount;
	TexDesc.BindFlags = TexDesc_BindFlags;
	TexDesc.Usage = TexDesc_Usage;
	TexDesc.CPUAccessFlags = TexDesc_CPUAccessFlags;
	TexDesc.MiscFlags = TexDesc_MiscFlags;
	TexDesc.ClearValue.Format = TexDesc_ClearValue_Format;
	TexDesc.ClearValue.Color[0] = TexDesc_ClearValue_Color_0;
	TexDesc.ClearValue.Color[1] = TexDesc_ClearValue_Color_1;
	TexDesc.ClearValue.Color[2] = TexDesc_ClearValue_Color_2;
	TexDesc.ClearValue.Color[3] = TexDesc_ClearValue_Color_3;
	TexDesc.ClearValue.DepthStencil.Depth = TexDesc_ClearValue_DepthStencil_Depth;
	TexDesc.ClearValue.DepthStencil.Stencil = TexDesc_ClearValue_DepthStencil_Stencil;
	TexDesc.ImmediateContextMask = TexDesc_ImmediateContextMask;
	TexDesc.Name = TexDesc_Name;
	TextureData pData;
	TextureSubResData* pData_pSubResources_Native_Array = new TextureSubResData[pData_NumSubresources];
	if(pData_NumSubresources > 0)
	{
		for (Uint32 i = 0; i < pData_NumSubresources; ++i)
		{
	    pData_pSubResources_Native_Array[i].pData = pData_pSubResources[i].pData;
	    pData_pSubResources_Native_Array[i].pSrcBuffer = pData_pSubResources[i].pSrcBuffer;
	    pData_pSubResources_Native_Array[i].SrcOffset = pData_pSubResources[i].SrcOffset;
	    pData_pSubResources_Native_Array[i].Stride = pData_pSubResources[i].Stride;
	    pData_pSubResources_Native_Array[i].DepthStride = pData_pSubResources[i].DepthStride;
		}
		pData.pSubResources = pData_pSubResources_Native_Array;  
	}
	pData.NumSubresources = pData_NumSubresources;
	ITexture* theReturnValue = nullptr;
	objPtr->CreateTexture(
		TexDesc
		, &pData
		, &theReturnValue
	);
    delete[] pData_pSubResources_Native_Array;
	return theReturnValue;
}
extern "C" _AnomalousExport ISampler* IRenderDevice_CreateSampler(
	IRenderDevice* objPtr
	, FILTER_TYPE SamDesc_MinFilter
	, FILTER_TYPE SamDesc_MagFilter
	, FILTER_TYPE SamDesc_MipFilter
	, TEXTURE_ADDRESS_MODE SamDesc_AddressU
	, TEXTURE_ADDRESS_MODE SamDesc_AddressV
	, TEXTURE_ADDRESS_MODE SamDesc_AddressW
	, SAMPLER_FLAGS SamDesc_Flags
	, Bool SamDesc_UnnormalizedCoords
	, Float32 SamDesc_MipLODBias
	, Uint32 SamDesc_MaxAnisotropy
	, COMPARISON_FUNCTION SamDesc_ComparisonFunc
	, Float32 SamDesc_BorderColor_0
	, Float32 SamDesc_BorderColor_1
	, Float32 SamDesc_BorderColor_2
	, Float32 SamDesc_BorderColor_3
	, float SamDesc_MinLOD
	, float SamDesc_MaxLOD
	, Char* SamDesc_Name
)
{
	SamplerDesc SamDesc;
	SamDesc.MinFilter = SamDesc_MinFilter;
	SamDesc.MagFilter = SamDesc_MagFilter;
	SamDesc.MipFilter = SamDesc_MipFilter;
	SamDesc.AddressU = SamDesc_AddressU;
	SamDesc.AddressV = SamDesc_AddressV;
	SamDesc.AddressW = SamDesc_AddressW;
	SamDesc.Flags = SamDesc_Flags;
	SamDesc.UnnormalizedCoords = SamDesc_UnnormalizedCoords;
	SamDesc.MipLODBias = SamDesc_MipLODBias;
	SamDesc.MaxAnisotropy = SamDesc_MaxAnisotropy;
	SamDesc.ComparisonFunc = SamDesc_ComparisonFunc;
	SamDesc.BorderColor[0] = SamDesc_BorderColor_0;
	SamDesc.BorderColor[1] = SamDesc_BorderColor_1;
	SamDesc.BorderColor[2] = SamDesc_BorderColor_2;
	SamDesc.BorderColor[3] = SamDesc_BorderColor_3;
	SamDesc.MinLOD = SamDesc_MinLOD;
	SamDesc.MaxLOD = SamDesc_MaxLOD;
	SamDesc.Name = SamDesc_Name;
	ISampler* theReturnValue = nullptr;
	objPtr->CreateSampler(
		SamDesc
		, &theReturnValue
	);
	return theReturnValue;
}
extern "C" _AnomalousExport IPipelineState* IRenderDevice_CreateGraphicsPipelineState(
	IRenderDevice* objPtr
	, Bool PSOCreateInfo_GraphicsPipeline_BlendDesc_AlphaToCoverageEnable
	, Bool PSOCreateInfo_GraphicsPipeline_BlendDesc_IndependentBlendEnable
	, RenderTargetBlendDescPassStruct* PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets
	, Uint32 PSOCreateInfo_GraphicsPipeline_SampleMask
	, FILL_MODE PSOCreateInfo_GraphicsPipeline_RasterizerDesc_FillMode
	, CULL_MODE PSOCreateInfo_GraphicsPipeline_RasterizerDesc_CullMode
	, Bool PSOCreateInfo_GraphicsPipeline_RasterizerDesc_FrontCounterClockwise
	, Bool PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthClipEnable
	, Bool PSOCreateInfo_GraphicsPipeline_RasterizerDesc_ScissorEnable
	, Bool PSOCreateInfo_GraphicsPipeline_RasterizerDesc_AntialiasedLineEnable
	, Int32 PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthBias
	, Float32 PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthBiasClamp
	, Float32 PSOCreateInfo_GraphicsPipeline_RasterizerDesc_SlopeScaledDepthBias
	, Bool PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthEnable
	, Bool PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthWriteEnable
	, COMPARISON_FUNCTION PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthFunc
	, Bool PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilEnable
	, Uint8 PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilReadMask
	, Uint8 PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilWriteMask
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilFailOp
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilDepthFailOp
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilPassOp
	, COMPARISON_FUNCTION PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilFunc
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilFailOp
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilDepthFailOp
	, STENCIL_OP PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilPassOp
	, COMPARISON_FUNCTION PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilFunc
	, LayoutElementPassStruct* PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements
	, Uint32 PSOCreateInfo_GraphicsPipeline_InputLayout_NumElements
	, PRIMITIVE_TOPOLOGY PSOCreateInfo_GraphicsPipeline_PrimitiveTopology
	, Uint8 PSOCreateInfo_GraphicsPipeline_NumViewports
	, Uint8 PSOCreateInfo_GraphicsPipeline_NumRenderTargets
	, Uint8 PSOCreateInfo_GraphicsPipeline_SubpassIndex
	, PIPELINE_SHADING_RATE_FLAGS PSOCreateInfo_GraphicsPipeline_ShadingRateFlags
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_0
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_1
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_2
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_3
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_4
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_5
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_6
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_RTVFormats_7
	, TEXTURE_FORMAT PSOCreateInfo_GraphicsPipeline_DSVFormat
	, Uint8 PSOCreateInfo_GraphicsPipeline_SmplDesc_Count
	, Uint8 PSOCreateInfo_GraphicsPipeline_SmplDesc_Quality
	, Uint32 PSOCreateInfo_GraphicsPipeline_NodeMask
	, IShader* PSOCreateInfo_pVS
	, IShader* PSOCreateInfo_pPS
	, IShader* PSOCreateInfo_pDS
	, IShader* PSOCreateInfo_pHS
	, IShader* PSOCreateInfo_pGS
	, IShader* PSOCreateInfo_pAS
	, IShader* PSOCreateInfo_pMS
	, PIPELINE_TYPE PSOCreateInfo_PSODesc_PipelineType
	, Uint32 PSOCreateInfo_PSODesc_SRBAllocationGranularity
	, Uint64 PSOCreateInfo_PSODesc_ImmediateContextMask
	, SHADER_RESOURCE_VARIABLE_TYPE PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableType
	, SHADER_TYPE PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableMergeStages
	, Uint32 PSOCreateInfo_PSODesc_ResourceLayout_NumVariables
	, ShaderResourceVariableDescPassStruct* PSOCreateInfo_PSODesc_ResourceLayout_Variables
	, Uint32 PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers
	, ImmutableSamplerDescPassStruct* PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers
	, Char* PSOCreateInfo_PSODesc_Name
	, PSO_CREATE_FLAGS PSOCreateInfo_Flags
)
{
	GraphicsPipelineStateCreateInfo PSOCreateInfo;
	PSOCreateInfo.GraphicsPipeline.BlendDesc.AlphaToCoverageEnable = PSOCreateInfo_GraphicsPipeline_BlendDesc_AlphaToCoverageEnable;
	PSOCreateInfo.GraphicsPipeline.BlendDesc.IndependentBlendEnable = PSOCreateInfo_GraphicsPipeline_BlendDesc_IndependentBlendEnable;
	for (Uint32 i = 0; i < 8; ++i)
	{
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].BlendEnable = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].BlendEnable;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].LogicOperationEnable = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].LogicOperationEnable;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].SrcBlend = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].SrcBlend;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].DestBlend = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].DestBlend;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].BlendOp = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].BlendOp;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].SrcBlendAlpha = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].SrcBlendAlpha;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].DestBlendAlpha = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].DestBlendAlpha;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].BlendOpAlpha = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].BlendOpAlpha;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].LogicOp = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].LogicOp;
	    PSOCreateInfo.GraphicsPipeline.BlendDesc.RenderTargets[i].RenderTargetWriteMask = PSOCreateInfo_GraphicsPipeline_BlendDesc_RenderTargets[i].RenderTargetWriteMask;
	}
	PSOCreateInfo.GraphicsPipeline.SampleMask = PSOCreateInfo_GraphicsPipeline_SampleMask;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.FillMode = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_FillMode;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.CullMode = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_CullMode;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.FrontCounterClockwise = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_FrontCounterClockwise;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.DepthClipEnable = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthClipEnable;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.ScissorEnable = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_ScissorEnable;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.AntialiasedLineEnable = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_AntialiasedLineEnable;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.DepthBias = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthBias;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.DepthBiasClamp = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_DepthBiasClamp;
	PSOCreateInfo.GraphicsPipeline.RasterizerDesc.SlopeScaledDepthBias = PSOCreateInfo_GraphicsPipeline_RasterizerDesc_SlopeScaledDepthBias;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.DepthEnable = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthEnable;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.DepthWriteEnable = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthWriteEnable;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.DepthFunc = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_DepthFunc;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.StencilEnable = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilEnable;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.StencilReadMask = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilReadMask;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.StencilWriteMask = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_StencilWriteMask;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.FrontFace.StencilFailOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilFailOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.FrontFace.StencilDepthFailOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilDepthFailOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.FrontFace.StencilPassOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilPassOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.FrontFace.StencilFunc = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_FrontFace_StencilFunc;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.BackFace.StencilFailOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilFailOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.BackFace.StencilDepthFailOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilDepthFailOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.BackFace.StencilPassOp = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilPassOp;
	PSOCreateInfo.GraphicsPipeline.DepthStencilDesc.BackFace.StencilFunc = PSOCreateInfo_GraphicsPipeline_DepthStencilDesc_BackFace_StencilFunc;
	LayoutElement* PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array = new LayoutElement[PSOCreateInfo_GraphicsPipeline_InputLayout_NumElements];
	if(PSOCreateInfo_GraphicsPipeline_InputLayout_NumElements > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_GraphicsPipeline_InputLayout_NumElements; ++i)
		{
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].HLSLSemantic = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].HLSLSemantic;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].InputIndex = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].InputIndex;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].BufferSlot = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].BufferSlot;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].NumComponents = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].NumComponents;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].ValueType = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].ValueType;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].IsNormalized = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].IsNormalized;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].RelativeOffset = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].RelativeOffset;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].Stride = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].Stride;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].Frequency = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].Frequency;
	    PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array[i].InstanceDataStepRate = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements[i].InstanceDataStepRate;
		}
		PSOCreateInfo.GraphicsPipeline.InputLayout.LayoutElements = PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array;  
	}
	PSOCreateInfo.GraphicsPipeline.InputLayout.NumElements = PSOCreateInfo_GraphicsPipeline_InputLayout_NumElements;
	PSOCreateInfo.GraphicsPipeline.PrimitiveTopology = PSOCreateInfo_GraphicsPipeline_PrimitiveTopology;
	PSOCreateInfo.GraphicsPipeline.NumViewports = PSOCreateInfo_GraphicsPipeline_NumViewports;
	PSOCreateInfo.GraphicsPipeline.NumRenderTargets = PSOCreateInfo_GraphicsPipeline_NumRenderTargets;
	PSOCreateInfo.GraphicsPipeline.SubpassIndex = PSOCreateInfo_GraphicsPipeline_SubpassIndex;
	PSOCreateInfo.GraphicsPipeline.ShadingRateFlags = PSOCreateInfo_GraphicsPipeline_ShadingRateFlags;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[0] = PSOCreateInfo_GraphicsPipeline_RTVFormats_0;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[1] = PSOCreateInfo_GraphicsPipeline_RTVFormats_1;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[2] = PSOCreateInfo_GraphicsPipeline_RTVFormats_2;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[3] = PSOCreateInfo_GraphicsPipeline_RTVFormats_3;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[4] = PSOCreateInfo_GraphicsPipeline_RTVFormats_4;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[5] = PSOCreateInfo_GraphicsPipeline_RTVFormats_5;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[6] = PSOCreateInfo_GraphicsPipeline_RTVFormats_6;
	PSOCreateInfo.GraphicsPipeline.RTVFormats[7] = PSOCreateInfo_GraphicsPipeline_RTVFormats_7;
	PSOCreateInfo.GraphicsPipeline.DSVFormat = PSOCreateInfo_GraphicsPipeline_DSVFormat;
	PSOCreateInfo.GraphicsPipeline.SmplDesc.Count = PSOCreateInfo_GraphicsPipeline_SmplDesc_Count;
	PSOCreateInfo.GraphicsPipeline.SmplDesc.Quality = PSOCreateInfo_GraphicsPipeline_SmplDesc_Quality;
	PSOCreateInfo.GraphicsPipeline.NodeMask = PSOCreateInfo_GraphicsPipeline_NodeMask;
	PSOCreateInfo.pVS = PSOCreateInfo_pVS;
	PSOCreateInfo.pPS = PSOCreateInfo_pPS;
	PSOCreateInfo.pDS = PSOCreateInfo_pDS;
	PSOCreateInfo.pHS = PSOCreateInfo_pHS;
	PSOCreateInfo.pGS = PSOCreateInfo_pGS;
	PSOCreateInfo.pAS = PSOCreateInfo_pAS;
	PSOCreateInfo.pMS = PSOCreateInfo_pMS;
	PSOCreateInfo.PSODesc.PipelineType = PSOCreateInfo_PSODesc_PipelineType;
	PSOCreateInfo.PSODesc.SRBAllocationGranularity = PSOCreateInfo_PSODesc_SRBAllocationGranularity;
	PSOCreateInfo.PSODesc.ImmediateContextMask = PSOCreateInfo_PSODesc_ImmediateContextMask;
	PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableType;
	PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableMergeStages = PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableMergeStages;
	PSOCreateInfo.PSODesc.ResourceLayout.NumVariables = PSOCreateInfo_PSODesc_ResourceLayout_NumVariables;
	ShaderResourceVariableDesc* PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array = new ShaderResourceVariableDesc[PSOCreateInfo_PSODesc_ResourceLayout_NumVariables];
	if(PSOCreateInfo_PSODesc_ResourceLayout_NumVariables > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_PSODesc_ResourceLayout_NumVariables; ++i)
		{
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].ShaderStages = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].ShaderStages;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Name = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Name;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Type = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Type;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Flags = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Flags;
		}
		PSOCreateInfo.PSODesc.ResourceLayout.Variables = PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array;  
	}
	PSOCreateInfo.PSODesc.ResourceLayout.NumImmutableSamplers = PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers;
	ImmutableSamplerDesc* PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array = new ImmutableSamplerDesc[PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers];
	if(PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers; ++i)
		{
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].ShaderStages = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].ShaderStages;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].SamplerOrTextureName = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].SamplerOrTextureName;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MinFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MinFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MagFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MagFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MipFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MipFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressU = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressU;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressV = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressV;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressW = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressW;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.Flags = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_Flags;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.UnnormalizedCoords = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_UnnormalizedCoords;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MipLODBias = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MipLODBias;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MaxAnisotropy = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MaxAnisotropy;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.ComparisonFunc = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_ComparisonFunc;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[0] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[0];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[1] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[1];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[2] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[2];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[3] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[3];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MinLOD = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MinLOD;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MaxLOD = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MaxLOD;
		}
		PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array;  
	}
	PSOCreateInfo.PSODesc.Name = PSOCreateInfo_PSODesc_Name;
	PSOCreateInfo.Flags = PSOCreateInfo_Flags;
	IPipelineState* theReturnValue = nullptr;
	objPtr->CreateGraphicsPipelineState(
		PSOCreateInfo
		, &theReturnValue
	);
    delete[] PSOCreateInfo_GraphicsPipeline_InputLayout_LayoutElements_Native_Array;
    delete[] PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array;
    delete[] PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array;
	return theReturnValue;
}
extern "C" _AnomalousExport IPipelineState* IRenderDevice_CreateRayTracingPipelineState(
	IRenderDevice* objPtr
	, Uint16 PSOCreateInfo_RayTracingPipeline_ShaderRecordSize
	, Uint8 PSOCreateInfo_RayTracingPipeline_MaxRecursionDepth
	, RayTracingGeneralShaderGroupPassStruct* PSOCreateInfo_pGeneralShaders
	, Uint32 PSOCreateInfo_GeneralShaderCount
	, RayTracingTriangleHitShaderGroupPassStruct* PSOCreateInfo_pTriangleHitShaders
	, Uint32 PSOCreateInfo_TriangleHitShaderCount
	, RayTracingProceduralHitShaderGroupPassStruct* PSOCreateInfo_pProceduralHitShaders
	, Uint32 PSOCreateInfo_ProceduralHitShaderCount
	, Char* PSOCreateInfo_pShaderRecordName
	, Uint32 PSOCreateInfo_MaxAttributeSize
	, Uint32 PSOCreateInfo_MaxPayloadSize
	, PIPELINE_TYPE PSOCreateInfo_PSODesc_PipelineType
	, Uint32 PSOCreateInfo_PSODesc_SRBAllocationGranularity
	, Uint64 PSOCreateInfo_PSODesc_ImmediateContextMask
	, SHADER_RESOURCE_VARIABLE_TYPE PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableType
	, SHADER_TYPE PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableMergeStages
	, Uint32 PSOCreateInfo_PSODesc_ResourceLayout_NumVariables
	, ShaderResourceVariableDescPassStruct* PSOCreateInfo_PSODesc_ResourceLayout_Variables
	, Uint32 PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers
	, ImmutableSamplerDescPassStruct* PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers
	, Char* PSOCreateInfo_PSODesc_Name
	, PSO_CREATE_FLAGS PSOCreateInfo_Flags
)
{
	RayTracingPipelineStateCreateInfo PSOCreateInfo;
	PSOCreateInfo.RayTracingPipeline.ShaderRecordSize = PSOCreateInfo_RayTracingPipeline_ShaderRecordSize;
	PSOCreateInfo.RayTracingPipeline.MaxRecursionDepth = PSOCreateInfo_RayTracingPipeline_MaxRecursionDepth;
	RayTracingGeneralShaderGroup* PSOCreateInfo_pGeneralShaders_Native_Array = new RayTracingGeneralShaderGroup[PSOCreateInfo_GeneralShaderCount];
	if(PSOCreateInfo_GeneralShaderCount > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_GeneralShaderCount; ++i)
		{
	    PSOCreateInfo_pGeneralShaders_Native_Array[i].Name = PSOCreateInfo_pGeneralShaders[i].Name;
	    PSOCreateInfo_pGeneralShaders_Native_Array[i].pShader = PSOCreateInfo_pGeneralShaders[i].pShader;
		}
		PSOCreateInfo.pGeneralShaders = PSOCreateInfo_pGeneralShaders_Native_Array;  
	}
	PSOCreateInfo.GeneralShaderCount = PSOCreateInfo_GeneralShaderCount;
	RayTracingTriangleHitShaderGroup* PSOCreateInfo_pTriangleHitShaders_Native_Array = new RayTracingTriangleHitShaderGroup[PSOCreateInfo_TriangleHitShaderCount];
	if(PSOCreateInfo_TriangleHitShaderCount > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_TriangleHitShaderCount; ++i)
		{
	    PSOCreateInfo_pTriangleHitShaders_Native_Array[i].Name = PSOCreateInfo_pTriangleHitShaders[i].Name;
	    PSOCreateInfo_pTriangleHitShaders_Native_Array[i].pClosestHitShader = PSOCreateInfo_pTriangleHitShaders[i].pClosestHitShader;
	    PSOCreateInfo_pTriangleHitShaders_Native_Array[i].pAnyHitShader = PSOCreateInfo_pTriangleHitShaders[i].pAnyHitShader;
		}
		PSOCreateInfo.pTriangleHitShaders = PSOCreateInfo_pTriangleHitShaders_Native_Array;  
	}
	PSOCreateInfo.TriangleHitShaderCount = PSOCreateInfo_TriangleHitShaderCount;
	RayTracingProceduralHitShaderGroup* PSOCreateInfo_pProceduralHitShaders_Native_Array = new RayTracingProceduralHitShaderGroup[PSOCreateInfo_ProceduralHitShaderCount];
	if(PSOCreateInfo_ProceduralHitShaderCount > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_ProceduralHitShaderCount; ++i)
		{
	    PSOCreateInfo_pProceduralHitShaders_Native_Array[i].Name = PSOCreateInfo_pProceduralHitShaders[i].Name;
	    PSOCreateInfo_pProceduralHitShaders_Native_Array[i].pIntersectionShader = PSOCreateInfo_pProceduralHitShaders[i].pIntersectionShader;
	    PSOCreateInfo_pProceduralHitShaders_Native_Array[i].pClosestHitShader = PSOCreateInfo_pProceduralHitShaders[i].pClosestHitShader;
	    PSOCreateInfo_pProceduralHitShaders_Native_Array[i].pAnyHitShader = PSOCreateInfo_pProceduralHitShaders[i].pAnyHitShader;
		}
		PSOCreateInfo.pProceduralHitShaders = PSOCreateInfo_pProceduralHitShaders_Native_Array;  
	}
	PSOCreateInfo.ProceduralHitShaderCount = PSOCreateInfo_ProceduralHitShaderCount;
	PSOCreateInfo.pShaderRecordName = PSOCreateInfo_pShaderRecordName;
	PSOCreateInfo.MaxAttributeSize = PSOCreateInfo_MaxAttributeSize;
	PSOCreateInfo.MaxPayloadSize = PSOCreateInfo_MaxPayloadSize;
	PSOCreateInfo.PSODesc.PipelineType = PSOCreateInfo_PSODesc_PipelineType;
	PSOCreateInfo.PSODesc.SRBAllocationGranularity = PSOCreateInfo_PSODesc_SRBAllocationGranularity;
	PSOCreateInfo.PSODesc.ImmediateContextMask = PSOCreateInfo_PSODesc_ImmediateContextMask;
	PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableType = PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableType;
	PSOCreateInfo.PSODesc.ResourceLayout.DefaultVariableMergeStages = PSOCreateInfo_PSODesc_ResourceLayout_DefaultVariableMergeStages;
	PSOCreateInfo.PSODesc.ResourceLayout.NumVariables = PSOCreateInfo_PSODesc_ResourceLayout_NumVariables;
	ShaderResourceVariableDesc* PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array = new ShaderResourceVariableDesc[PSOCreateInfo_PSODesc_ResourceLayout_NumVariables];
	if(PSOCreateInfo_PSODesc_ResourceLayout_NumVariables > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_PSODesc_ResourceLayout_NumVariables; ++i)
		{
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].ShaderStages = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].ShaderStages;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Name = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Name;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Type = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Type;
	    PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array[i].Flags = PSOCreateInfo_PSODesc_ResourceLayout_Variables[i].Flags;
		}
		PSOCreateInfo.PSODesc.ResourceLayout.Variables = PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array;  
	}
	PSOCreateInfo.PSODesc.ResourceLayout.NumImmutableSamplers = PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers;
	ImmutableSamplerDesc* PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array = new ImmutableSamplerDesc[PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers];
	if(PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers > 0)
	{
		for (Uint32 i = 0; i < PSOCreateInfo_PSODesc_ResourceLayout_NumImmutableSamplers; ++i)
		{
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].ShaderStages = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].ShaderStages;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].SamplerOrTextureName = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].SamplerOrTextureName;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MinFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MinFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MagFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MagFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MipFilter = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MipFilter;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressU = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressU;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressV = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressV;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.AddressW = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_AddressW;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.Flags = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_Flags;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.UnnormalizedCoords = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_UnnormalizedCoords;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MipLODBias = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MipLODBias;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MaxAnisotropy = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MaxAnisotropy;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.ComparisonFunc = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_ComparisonFunc;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[0] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[0];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[1] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[1];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[2] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[2];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.BorderColor[3] = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_BorderColor[3];
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MinLOD = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MinLOD;
	    PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array[i].Desc.MaxLOD = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers[i].Desc_MaxLOD;
		}
		PSOCreateInfo.PSODesc.ResourceLayout.ImmutableSamplers = PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array;  
	}
	PSOCreateInfo.PSODesc.Name = PSOCreateInfo_PSODesc_Name;
	PSOCreateInfo.Flags = PSOCreateInfo_Flags;
	IPipelineState* theReturnValue = nullptr;
	objPtr->CreateRayTracingPipelineState(
		PSOCreateInfo
		, &theReturnValue
	);
    delete[] PSOCreateInfo_pGeneralShaders_Native_Array;
    delete[] PSOCreateInfo_pTriangleHitShaders_Native_Array;
    delete[] PSOCreateInfo_pProceduralHitShaders_Native_Array;
    delete[] PSOCreateInfo_PSODesc_ResourceLayout_Variables_Native_Array;
    delete[] PSOCreateInfo_PSODesc_ResourceLayout_ImmutableSamplers_Native_Array;
	return theReturnValue;
}
extern "C" _AnomalousExport IBottomLevelAS* IRenderDevice_CreateBLAS(
	IRenderDevice* objPtr
	, BLASTriangleDescPassStruct* Desc_pTriangles
	, Uint32 Desc_TriangleCount
	, BLASBoundingBoxDescPassStruct* Desc_pBoxes
	, Uint32 Desc_BoxCount
	, RAYTRACING_BUILD_AS_FLAGS Desc_Flags
	, Uint64 Desc_CompactedSize
	, Uint64 Desc_ImmediateContextMask
	, Char* Desc_Name
)
{
	BottomLevelASDesc Desc;
	BLASTriangleDesc* Desc_pTriangles_Native_Array = new BLASTriangleDesc[Desc_TriangleCount];
	if(Desc_TriangleCount > 0)
	{
		for (Uint32 i = 0; i < Desc_TriangleCount; ++i)
		{
	    Desc_pTriangles_Native_Array[i].GeometryName = Desc_pTriangles[i].GeometryName;
	    Desc_pTriangles_Native_Array[i].MaxVertexCount = Desc_pTriangles[i].MaxVertexCount;
	    Desc_pTriangles_Native_Array[i].VertexValueType = Desc_pTriangles[i].VertexValueType;
	    Desc_pTriangles_Native_Array[i].VertexComponentCount = Desc_pTriangles[i].VertexComponentCount;
	    Desc_pTriangles_Native_Array[i].MaxPrimitiveCount = Desc_pTriangles[i].MaxPrimitiveCount;
	    Desc_pTriangles_Native_Array[i].IndexType = Desc_pTriangles[i].IndexType;
	    Desc_pTriangles_Native_Array[i].AllowsTransforms = Desc_pTriangles[i].AllowsTransforms;
		}
		Desc.pTriangles = Desc_pTriangles_Native_Array;  
	}
	Desc.TriangleCount = Desc_TriangleCount;
	BLASBoundingBoxDesc* Desc_pBoxes_Native_Array = new BLASBoundingBoxDesc[Desc_BoxCount];
	if(Desc_BoxCount > 0)
	{
		for (Uint32 i = 0; i < Desc_BoxCount; ++i)
		{
	    Desc_pBoxes_Native_Array[i].GeometryName = Desc_pBoxes[i].GeometryName;
	    Desc_pBoxes_Native_Array[i].MaxBoxCount = Desc_pBoxes[i].MaxBoxCount;
		}
		Desc.pBoxes = Desc_pBoxes_Native_Array;  
	}
	Desc.BoxCount = Desc_BoxCount;
	Desc.Flags = Desc_Flags;
	Desc.CompactedSize = Desc_CompactedSize;
	Desc.ImmediateContextMask = Desc_ImmediateContextMask;
	Desc.Name = Desc_Name;
	IBottomLevelAS* theReturnValue = nullptr;
	objPtr->CreateBLAS(
		Desc
		, &theReturnValue
	);
    delete[] Desc_pTriangles_Native_Array;
    delete[] Desc_pBoxes_Native_Array;
	return theReturnValue;
}
extern "C" _AnomalousExport ITopLevelAS* IRenderDevice_CreateTLAS(
	IRenderDevice* objPtr
	, Uint32 Desc_MaxInstanceCount
	, RAYTRACING_BUILD_AS_FLAGS Desc_Flags
	, Uint64 Desc_CompactedSize
	, Uint64 Desc_ImmediateContextMask
	, Char* Desc_Name
)
{
	TopLevelASDesc Desc;
	Desc.MaxInstanceCount = Desc_MaxInstanceCount;
	Desc.Flags = Desc_Flags;
	Desc.CompactedSize = Desc_CompactedSize;
	Desc.ImmediateContextMask = Desc_ImmediateContextMask;
	Desc.Name = Desc_Name;
	ITopLevelAS* theReturnValue = nullptr;
	objPtr->CreateTLAS(
		Desc
		, &theReturnValue
	);
	return theReturnValue;
}
extern "C" _AnomalousExport IShaderBindingTable* IRenderDevice_CreateSBT(
	IRenderDevice* objPtr
	, IPipelineState* Desc_pPSO
	, Char* Desc_Name
)
{
	ShaderBindingTableDesc Desc;
	Desc.pPSO = Desc_pPSO;
	Desc.Name = Desc_Name;
	IShaderBindingTable* theReturnValue = nullptr;
	objPtr->CreateSBT(
		Desc
		, &theReturnValue
	);
	return theReturnValue;
}
