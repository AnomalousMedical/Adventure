#pragma once
#include "Primitives/interface/BasicTypes.h"
#include "Graphics/GraphicsEngine/interface/GraphicsTypes.h"

namespace Diligent 
{
struct BLASBuildTriangleDataPassStruct
{
        char* GeometryName;
        IBuffer* pVertexBuffer;
        Uint64 VertexOffset;
        Uint32 VertexStride;
        Uint32 VertexCount;
        VALUE_TYPE VertexValueType;
        Uint8 VertexComponentCount;
        Uint32 PrimitiveCount;
        IBuffer* pIndexBuffer;
        Uint64 IndexOffset;
        VALUE_TYPE IndexType;
        IBuffer* pTransformBuffer;
        Uint64 TransformBufferOffset;
        RAYTRACING_GEOMETRY_FLAGS Flags;
};
}