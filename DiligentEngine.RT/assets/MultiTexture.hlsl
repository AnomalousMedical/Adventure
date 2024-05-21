void GetMultiBase
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor
)
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseEmissive
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
    emissiveColor = GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormal
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor
)
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormalRC(uvAreaFromCone, uv, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalEmissive
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormalRC(uvAreaFromCone, uv, posX.tex);
    emissiveColor = GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalPhysical
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float4 physicalColor
) 
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormalRC(uvAreaFromCone, uv, posX.tex);
    physicalColor = GetPhysicalRC(uvAreaFromCone, uv, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
    physicalColor = physicalColor * tex1Blend + GetPhysicalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalPhysicalEmissive
(
    in float2 uv,
    in float2 globalUv,
    in float2 uvAreaFromCone,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float4 physicalColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormalRC(uvAreaFromCone, uv, posX.tex);
    physicalColor = GetPhysicalRC(uvAreaFromCone, uv, posX.tex);
    emissiveColor = GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColorRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
    physicalColor = physicalColor * tex1Blend + GetPhysicalRC(uvAreaFromCone, uv, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissiveRC(uvAreaFromCone, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}