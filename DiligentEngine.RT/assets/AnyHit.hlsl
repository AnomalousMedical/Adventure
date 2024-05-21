#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"

[shader("anyhit")]
void main(
#ifdef PRIMARY_HIT
    inout PrimaryRayPayload payload,
#endif
#ifdef SHADOW_HIT
    inout ShadowRayPayload payload,
#endif
    in BuiltInTriangleIntersectionAttributes attr
)
{
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    float2 globalUv;
    float mip = GetMip();
    float opacity;

    [forcecase] switch (instanceData.dispatchType & 0x1)
    {
        case $$(MESH_DATA_TYPE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            opacity = GetOpacity(mip, uv, g_SamLinearWrap);
            break;


        case $$(SPRITE_DATA_TYPE):
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            opacity = GetOpacity(mip, uv, g_SamPointWrap);
            break;
    }

    AnyHitOpacityTest(opacity);
}