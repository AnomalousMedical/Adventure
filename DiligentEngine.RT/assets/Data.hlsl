StructuredBuffer<CubeAttribVertex> $$(G_VERTICES);
StructuredBuffer<uint> $$(G_INDICES);

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

    uint vertId = 3 * PrimitiveIndex() + instanceData.indexOffset;

    posX = $$(G_VERTICES)[$$(G_INDICES)[vertId + 0] + instanceData.vertexOffset];
    posY = $$(G_VERTICES)[$$(G_INDICES)[vertId + 1] + instanceData.vertexOffset];
    posZ = $$(G_VERTICES)[$$(G_INDICES)[vertId + 2] + instanceData.vertexOffset];

    uv = posX.uv.xy * barycentrics.x +
        posY.uv.xy * barycentrics.y +
        posZ.uv.xy * barycentrics.z;
}

void GetInstanceDataSprite
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

    uint vertId = 3 * PrimitiveIndex() + instanceData.indexOffset;

    posX = $$(G_VERTICES)[$$(G_INDICES)[vertId + 0] + instanceData.vertexOffset];
    posY = $$(G_VERTICES)[$$(G_INDICES)[vertId + 1] + instanceData.vertexOffset];
    posZ = $$(G_VERTICES)[$$(G_INDICES)[vertId + 2] + instanceData.vertexOffset];

    float2 frameVertX = instanceData.uv[$$(G_INDICES)[vertId + 0]];
    float2 frameVertY = instanceData.uv[$$(G_INDICES)[vertId + 1]];
    float2 frameVertZ = instanceData.uv[$$(G_INDICES)[vertId + 2]];

    uv = frameVertX.xy * barycentrics.x +
        frameVertY.xy * barycentrics.y +
        frameVertZ.xy * barycentrics.z;
}