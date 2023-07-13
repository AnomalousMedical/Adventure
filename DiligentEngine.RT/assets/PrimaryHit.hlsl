#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"
#include "MultiTexture.hlsl"
#include "Lighting.hlsl"
#include "GlassPrimaryHit.hlsl"

[shader("closesthit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    float2 globalUv;
    int mip = GetMip();
    float3 baseColor;
    float3 normalColor;
    float4 physicalColor;

    [forcecase] switch (instanceData.dispatchType) 
    {
        case $$(LIGHTANDSHADEBASE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap)
            );
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap)
            );
            payload.Color += GetEmissive(mip, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMAL) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
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
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex),
                GetPhysical(mip, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamPointWrap),
                GetSampledNormal(mip, uv),
                GetPhysical(mip, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            GetMultiBaseNormalPhysical(uv, globalUv, mip, posX, posY, posZ, baseColor, normalColor, physicalColor);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor,
                physicalColor
            );

            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex),
                GetPhysical(mip, uv, posX.tex)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
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
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex),
                GetPhysical(mip, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
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
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormal(mip, uv, posX.tex),
                GetPhysical(mip, uv, posX.tex)
            );
            payload.Color += GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
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

        case $$(GLASSMATERIAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            Glass
            (
                payload, barycentrics,
                posX, posY, posZ,
                float3(instanceData.uv0.x, instanceData.uv0.y, instanceData.uv1.x), //GlassReflectionColorMask
                instanceData.uv1.y, //GlassAbsorption
                instanceData.uv2, //GlassIndexOfRefraction
                instanceData.padding //GlassMaterialColorRgb
            );
            break;

        case $$(WATERMATERIAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv);
            Water
            (
                payload, barycentrics,
                posX, posY, posZ,
                float3(instanceData.uv0.x, instanceData.uv0.y, instanceData.uv1.x), //GlassReflectionColorMask
                instanceData.uv1.y, //GlassAbsorption
                instanceData.uv2, //GlassIndexOfRefraction
                instanceData.padding //GlassMaterialColorRgb
            );
            break;
    }
}
