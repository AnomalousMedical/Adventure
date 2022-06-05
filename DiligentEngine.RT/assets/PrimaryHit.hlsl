#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"
#include "Lighting.hlsl"

[shader("closesthit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    int mip = GetMip();
    GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv);

    [forcecase] switch (instanceData.dispatchType) 
    {
        case $$(LIGHTANDSHADEBASE):
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap)
            );
            break;

        case $$(LIGHTANDSHADEBASE) + 1:
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap)
            );
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE):
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap);
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE) + 1:
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap)
            );
            payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMAL):
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMAL) + 1:
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE):
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE) + 1:
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL):
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL) + 1:
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE):
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE) + 1:
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE):
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE) + 1:
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE):
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE) + 1:
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
            break;
    }
}
