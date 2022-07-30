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

float2 GetTriUvs(uint vertId)
{
    switch ($$(G_INDICES)[vertId])
    {
    case 0:
        return instanceData.uv0;
    case 1:
        return instanceData.uv1;
    case 2:
        return instanceData.uv2;
    case 3:
        return instanceData.uv3;
    }

    //Should not happen
    return instanceData.uv0;
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

    posX = $$(G_VERTICES)[$$(G_INDICES)[vertId    ] + instanceData.vertexOffset];
    posY = $$(G_VERTICES)[$$(G_INDICES)[vertId + 1] + instanceData.vertexOffset];
    posZ = $$(G_VERTICES)[$$(G_INDICES)[vertId + 2] + instanceData.vertexOffset];

    //float2 frameVertX = instanceData.uv[$$(G_INDICES)[vertId + 0]];
    //float2 frameVertY = instanceData.uv[$$(G_INDICES)[vertId + 1]];
    //float2 frameVertZ = instanceData.uv[$$(G_INDICES)[vertId + 2]];

    float2 frameVertX = GetTriUvs(vertId    );
    float2 frameVertY = GetTriUvs(vertId + 1);
    float2 frameVertZ = GetTriUvs(vertId + 2);

    uv = frameVertX.xy * barycentrics.x +
        frameVertY.xy * barycentrics.y +
        frameVertZ.xy * barycentrics.z;
}