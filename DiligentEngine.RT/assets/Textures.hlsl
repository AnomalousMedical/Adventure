int GetMip()
{
	//Lame mip calculation, but looks tons better than just mip0.
	//Need to add screen size and some more info
	float depth = RayTCurrent();
	int mip = min(depth / 4, 4);
	return mip;
}

int GetTextureSet(in int texIdx) 
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.tex0;
		break;
	case 1:
		tex = instanceData.tex1;
		break;
	case 2:
		tex = instanceData.tex2;
		break;
	case 3:
		tex = instanceData.tex3;
		break;
	case 4:
		tex = instanceData.uv0.x;
		break;
	case 5:
		tex = instanceData.uv0.y;
		break;
	case 6:
		tex = instanceData.uv1.x;
		break;
	case 7:
		tex = instanceData.uv1.y;
		break;
	case 8:
		tex = instanceData.uv2.x;
		break;
	case 9:
		tex = instanceData.uv2.y;
		break;
	case 10:
		tex = instanceData.uv3.x;
		break;
	case 11:
		tex = instanceData.uv3.y;
		break;
	}
	return tex;
}

float3 GetSampledNormal(in int mip, in float2 uv)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].normalTexture].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float3 GetSampledNormal(in int mip, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].normalTexture].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float4 GetPhysical(in int mip, in float2 uv)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].physicalTexture].SampleLevel(g_SamLinearWrap, uv, mip);
}

float4 GetPhysical(in int mip, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].physicalTexture].SampleLevel(g_SamLinearWrap, uv, mip);
}

float3 GetBaseColor(in int mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture].SampleLevel(sState, uv, mip).rgb;
}

float3 GetBaseColor(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture].SampleLevel(sState, uv, mip).rgb;
}

float GetOpacity(in int mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture].SampleLevel(sState, uv, mip).a;
}

float GetOpacity(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].baseTexture].SampleLevel(sState, uv, mip).a;
}

float3 GetEmissive(in int mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].emissiveTexture].SampleLevel(sState, uv, mip).rgb;
}

float3 GetEmissive(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[$$(G_TEXTURESETS)[tex].emissiveTexture].SampleLevel(sState, uv, mip).rgb;
}