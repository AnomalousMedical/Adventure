StructuredBuffer<CubeAttribVertex> $$(G_VERTICES);
StructuredBuffer<uint> $$(G_INDICES);

[[vk::shader_record_ext]]
ConstantBuffer<BlasInstanceData> instanceData;

Texture2D    $$(G_TEXTURES)[$$(NUM_TEXTURES)];
StructuredBuffer<TextureSet>   $$(G_TEXTURESETS);
SamplerState g_SamLinearWrap;
SamplerState g_SamPointWrap;

#include "Textures.hlsl"
#include "RayCones.hlsl"

float2 GetUvAreaFromCone
(
    in float3 barycentrics,
    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ
)
{
    float3 hitPosition = WorldRayOrigin() + RayTCurrent() * WorldRayDirection();

    //TODO: This normal needs to be in world space. However, with the scenes in this game it is ok as is for now
    float3 normal = posX.normal.xyz * barycentrics.x +
        posY.normal.xyz * barycentrics.y +
        posZ.normal.xyz * barycentrics.z;

    //TODO: I assume these positions need to be in world space
    const float3 aPos[3] = { posX.position.xyz, posY.position.xyz, posZ.position.xyz };
	const float2 aUVs[3] = { posX.uv, posY.uv, posZ.uv };

	const float2 rayConeAtOrigin = float2(0, g_ConstantsCB.eyeToPixelConeSpreadAngle);

	// New cone width should increase by 2*RayLength*tan(SpreadAngle/2), but RayLength*SpreadAngle is a close approximation
	const float2 rayConeAtHit = float2
    (
		rayConeAtOrigin.x + rayConeAtOrigin.y * length(hitPosition - g_ConstantsCB.CameraPos.xyz),
		rayConeAtOrigin.y + g_ConstantsCB.eyeToPixelConeSpreadAngle
    );

    const matrix matWorld = matrix(float4(1,0,0,0), float4(0,1,0,0), float4(0,0,1,0), float4(0,0,0,1));

    return UVAreaFromRayCone(WorldRayDirection(), normal, rayConeAtHit.x, aUVs, aPos, (float3x3)matWorld);
}

void GetInstanceDataMesh
(
    in BuiltInTriangleIntersectionAttributes attr,
    out float3 barycentrics,
    out CubeAttribVertex posX,
    out CubeAttribVertex posY,
    out CubeAttribVertex posZ,
    out float2 uv,
    out float2 globalUv
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

    globalUv = posX.globalUv.xy * barycentrics.x +
        posY.globalUv.xy * barycentrics.y +
        posZ.globalUv.xy * barycentrics.z;
}

void GetInstanceDataMesh
(
    in BuiltInTriangleIntersectionAttributes attr,
    out float3 barycentrics,
    out CubeAttribVertex posX,
    out CubeAttribVertex posY,
    out CubeAttribVertex posZ,
    out float2 uv,
    out float2 globalUv,
    out float2 uvAreaFromCone
)
{
    GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
    uvAreaFromCone = GetUvAreaFromCone(barycentrics, posX, posY, posZ);
}

float2 GetTriUvs(uint vertId)
{
    //This is obviously not very efficient, but passing arrays in the blas
    //data only works correcly on Nvidia using Vulkan. All the other combos tested
    //don't work
    //Nvidia - D3D12, all blases are black
    //Amd - Vulkan - Works, but has corruption across the middle of the sprite where the triangle intersects
    //Amd - D3D12, crashes
    //So instead the frames are passed in as 4 float2 elements, however this changes 3 array
    //lookups into 3 switch statements, so its going to be slower, real world impact seems minimal
    //TODO: Optimize so only sprites that have animations need to call GetTriUvs
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

void GetInstanceDataSprite
(
    in BuiltInTriangleIntersectionAttributes attr,
    out float3 barycentrics,
    out CubeAttribVertex posX,
    out CubeAttribVertex posY,
    out CubeAttribVertex posZ,
    out float2 uv,
    out float2 uvAreaFromCone
)
{
    GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
    uvAreaFromCone = GetUvAreaFromCone(barycentrics, posX, posY, posZ);
}