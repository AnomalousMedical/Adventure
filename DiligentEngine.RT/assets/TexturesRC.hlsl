float GetTexLOD(in float2 uvAreaFromCone, Texture2D texture)
{
	uint2 vTexSize;
	texture.GetDimensions(vTexSize.x,vTexSize.y);
    return UVAreaToTexLOD(vTexSize,uvAreaFromCone);
}

float3 GetSampledNormalRC(in float2 uvAreaFromCone, in float2 uv)
{
	int tex = instanceData.tex0;
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].normalTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float3 GetSampledNormalRC(in float2 uvAreaFromCone, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].normalTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float4 GetPhysicalRC(in float2 uvAreaFromCone, in float2 uv)
{
	int tex = instanceData.tex0;
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].physicalTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(g_SamLinearWrap, uv, mip);
}

float4 GetPhysicalRC(in float2 uvAreaFromCone, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].physicalTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(g_SamLinearWrap, uv, mip);
}

float3 GetBaseColorRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).rgb;
}

float3 GetBaseColorRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).rgb;
}

float GetOpacityRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).a;
}

float GetOpacityRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).a;
}

float3 GetEmissiveRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].emissiveTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).rgb;
}

float3 GetEmissiveRC(in float2 uvAreaFromCone, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	Texture2D resolvedTex = $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].emissiveTexture];
	float mip = GetTexLOD(uvAreaFromCone, resolvedTex);
	return resolvedTex.SampleLevel(sState, uv, mip).rgb;
}