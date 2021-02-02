#include "StdAfx.h"

#include "Graphics/GraphicsEngine/interface/SwapChain.h"
#include "Graphics/GraphicsEngine/interface/BlendState.h"
#include "Graphics/GraphicsEngine/interface/RenderDevice.h"

using namespace Diligent;

extern "C" _AnomalousExport IPipelineState * IRenderDevice_CreateGraphicsPipelineState(IRenderDevice * objPtr, GraphicsPipelineStateCreateInfo * PSOCreateInfo)
{
	IPipelineState* ret = nullptr;
	objPtr->CreateGraphicsPipelineState(*PSOCreateInfo, &ret);
	return ret;
}

extern "C" _AnomalousExport IShader * IRenderDevice_CreateShader(IRenderDevice * objPtr, ShaderCreateInfo * ShaderCI)
{
	IShader* ret = nullptr;
	objPtr->CreateShader(*ShaderCI, &ret);
	return ret;
}