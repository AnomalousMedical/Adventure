#pragma once
#include "Primitives/interface/BasicTypes.h"
#include "Graphics/GraphicsEngine/interface/GraphicsTypes.h"

namespace Diligent 
{
struct TextureSubResDataPassStruct
{
        void* pData;
        IBuffer* pSrcBuffer;
        Uint64 SrcOffset;
        Uint64 Stride;
        Uint64 DepthStride;
};
}