#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Lighting.hlsl"
#include "Data.hlsl"

[shader("closesthit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    GetInstanceData(attr, barycentrics, posX, posY, posZ, uv);

    int mip = GetMip();

    $$(LIGHTING_FUNCTION)
    (
        payload, barycentrics,
        posX, posY, posZ

#if HAS_BASE_COLOR
        ,GetBaseColor(mip, uv, g_SamPointWrap)
#endif

#if HAS_NORMAL_MAP
        ,GetSampledNormal(mip, uv)
#endif

#if HAS_PHYSICAL_MAP
        ,GetPhysical(mip, uv)
#endif
    );

#if HAS_EMISSIVE_MAP
    payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
#endif
}
