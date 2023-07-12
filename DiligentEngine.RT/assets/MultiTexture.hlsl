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

    //You could check the depth to see if this blend is worth it, otherwise just use the mixed up uvs (texture2 here)
    //It would save lookups in reflections, maybe a precompiler option?
    float2 noise = GetBaseColor(0, globalUv, g_SamPointWrap, instanceData.padding).rg;

    uv += noise.r;

    float tex1Blend = noise.g;
    float tex2Blend = 1.0f - tex1Blend;

    baseColor = baseColor * tex1Blend + GetBaseColor(mip, uv, g_SamLinearWrap, posX.tex) * tex2Blend;
    normalColor = normalColor * tex1Blend + GetSampledNormal(mip, uv, posX.tex) * tex2Blend;
    physicalColor = physicalColor * tex1Blend + GetPhysical(mip, uv, posX.tex) * tex2Blend;
}