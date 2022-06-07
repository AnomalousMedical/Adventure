StructuredBuffer<CubeAttribVertex> $$(G_VERTICES)[100]; //TODO: don't hardcode this and make it bigger
StructuredBuffer<uint> $$(G_INDICES)[100]; //TODO: don't hardcode this and make it bigger

[[vk::shader_record_ext]]
ConstantBuffer<BlasInstanceData> instanceData;

Texture2D    $$(G_TEXTURES)[$$(NUM_TEXTURES)];
SamplerState g_SamLinearWrap;
SamplerState g_SamPointWrap;

#include "Textures.hlsl"

void GetInstanceDataMesh
(
    in BuiltInTriangleIntersectionAttributes attr,
    out float3 barycentrics,
    out CubeAttribVertex posX,
    out CubeAttribVertex posY,
    out CubeAttribVertex posZ,
    out float2 uv
)
{
    barycentrics = float3(1.0 - attr.barycentrics.x - attr.barycentrics.y, attr.barycentrics.x, attr.barycentrics.y);

    uint vertId = 3 * PrimitiveIndex();

    //TODO: Vertex offset is a temp geometry instance since its already setup
    StructuredBuffer<CubeAttribVertex> vertices = $$(G_VERTICES)[instanceData.vertexOffset];
    StructuredBuffer<uint> indices = $$(G_INDICES)[instanceData.vertexOffset];

    posX = vertices[indices[vertId + 0]];
    posY = vertices[indices[vertId + 1]];
    posZ = vertices[indices[vertId + 2]];

    uv = posX.uv.xy * barycentrics.x +
        posY.uv.xy * barycentrics.y +
        posZ.uv.xy * barycentrics.z;
}