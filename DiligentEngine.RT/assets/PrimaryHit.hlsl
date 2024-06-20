#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"
#include "Lighting.hlsl"
#include "GlassPrimaryHit.hlsl"
#include "TexturesRC.hlsl"
#include "MultiTexture.hlsl"

[shader("closesthit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    float2 globalUv;
    float2 uvAreaFromCone;
    float3 baseColor;
    float3 normalColor;
    float4 physicalColor;
    float3 emissiveColor;
    float2 currentRayCone = payload.RayConeAtOrigin;

    [forcecase] switch (instanceData.dispatchType) 
    {
        case $$(LIGHTANDSHADEBASE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap)
            );
            break;

        case $$(LIGHTANDSHADEBASE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBase(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor
            );
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASEEMISSIVE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseEmissive(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, emissiveColor);
            LightAndShadeBase
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor
            );
            payload.Color += emissiveColor;
            break;

        case $$(LIGHTANDSHADEBASENORMAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMAL) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMAL) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormal(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALEMISSIVE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormalEmissive(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor, emissiveColor);
            LightAndShadeBaseNormal
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor
            );
            payload.Color += emissiveColor;
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex),
                GetPhysicalRC(uvAreaFromCone, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv),
                GetPhysicalRC(uvAreaFromCone, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICAL) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormalPhysical(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor, physicalColor);
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
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex),
                GetPhysicalRC(uvAreaFromCone, uv, posX.tex)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv),
                GetPhysicalRC(uvAreaFromCone, uv)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALEMISSIVE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormalPhysicalEmissive(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor, physicalColor, emissiveColor);
            LightAndShadeBaseNormalPhysical
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor,
                physicalColor
            );
            payload.Color += emissiveColor;
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex),
                GetPhysicalRC(uvAreaFromCone, uv, posX.tex)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv),
                GetPhysicalRC(uvAreaFromCone, uv)
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormalPhysical(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor, physicalColor);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor,
                physicalColor
            );
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex),
                GetSampledNormalRC(uvAreaFromCone, uv, posX.tex),
                GetPhysicalRC(uvAreaFromCone, uv, posX.tex)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE) + 1:
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv, uvAreaFromCone, currentRayCone);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                GetBaseColorRC(uvAreaFromCone, uv, g_SamPointWrap),
                GetSampledNormalRC(uvAreaFromCone, uv),
                GetPhysicalRC(uvAreaFromCone, uv)
            );
            payload.Color += GetEmissiveRC(uvAreaFromCone, uv, g_SamPointWrap);
            break;

        case $$(LIGHTANDSHADEBASENORMALPHYSICALREFLECTIVEEMISSIVE) + 2:
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            GetMultiBaseNormalPhysicalEmissive(uv, globalUv, uvAreaFromCone, posX, posY, posZ, baseColor, normalColor, physicalColor, emissiveColor);
            LightAndShadeBaseNormalPhysicalReflective
            (
                payload, barycentrics,
                posX, posY, posZ,
                baseColor,
                normalColor,
                physicalColor
            );
            payload.Color += emissiveColor;
            break;

        case $$(GLASSMATERIAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            Glass
            (
                payload, barycentrics,
                posX, posY, posZ,
                float3(instanceData.uv0.x, instanceData.uv0.y, instanceData.uv1.x), //GlassReflectionColorMask
                instanceData.uv1.y, //GlassAbsorption
                instanceData.uv2, //GlassIndexOfRefraction
                instanceData.padding, //GlassMaterialColorRgb
                currentRayCone
            );
            break;

        case $$(WATERMATERIAL):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv, globalUv, uvAreaFromCone, currentRayCone);
            Water
            (
                payload, barycentrics,
                posX, posY, posZ,
                float3(instanceData.uv0.x, instanceData.uv0.y, instanceData.uv1.x), //GlassReflectionColorMask
                instanceData.uv1.y, //GlassAbsorption
                instanceData.uv2, //GlassIndexOfRefraction
                instanceData.padding, //GlassMaterialColorRgb
                currentRayCone
            );
            break;
    }
}
