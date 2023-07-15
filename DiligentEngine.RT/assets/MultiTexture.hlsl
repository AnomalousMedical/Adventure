void GetMultiBase
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor
)
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseEmissive
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);
    emissiveColor = GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissive(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormal
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor
)
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormal(mip, uv, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormal(mip, uv, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalEmissive
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormal(mip, uv, posX.tex);
    emissiveColor = GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormal(mip, uv, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissive(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalPhysical
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float4 physicalColor
) 
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormal(mip, uv, posX.tex);
    physicalColor = GetPhysical(mip, uv, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormal(mip, uv, posX.tex2) * tex2Blend;
    physicalColor = physicalColor * tex1Blend + GetPhysical(mip, uv, posX.tex2) * tex2Blend;
}

void GetMultiBaseNormalPhysicalEmissive
(
    in float2 uv,
    in float2 globalUv,
    in int mip,

    in CubeAttribVertex posX,
    in CubeAttribVertex posY,
    in CubeAttribVertex posZ,

    out float3 baseColor,
    out float3 normalColor,
    out float4 physicalColor,
    out float3 emissiveColor
)
{
    baseColor = GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex);
    normalColor = GetSampledNormal(mip, uv, posX.tex);
    physicalColor = GetPhysical(mip, uv, posX.tex);
    emissiveColor = GetEmissive(mip, uv, g_SamLinearWrap, posX.tex);

    float tex1Blend = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).r;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormal(mip, uv, posX.tex2) * tex2Blend;
    physicalColor = physicalColor * tex1Blend + GetPhysical(mip, uv, posX.tex2) * tex2Blend;
    emissiveColor = emissiveColor * tex1Blend + GetEmissive(mip, uv, g_SamLinearWrap, posX.tex2) * tex2Blend;
}