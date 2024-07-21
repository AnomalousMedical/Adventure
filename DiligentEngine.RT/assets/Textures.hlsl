float GetMip()
{
	//Lame mip calculation, but looks tons better than just mip0.
	//Need to add screen size and some more info
	float depth = RayTCurrent();
	float mip = min(depth / 3 / 4, 4.0);
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

float3 GetSampledNormal(in float mip, in float2 uv)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].normalTexture)].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float3 GetSampledNormal(in float mip, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].normalTexture)].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float4 GetPhysical(in float mip, in float2 uv)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].physicalTexture)].SampleLevel(g_SamLinearWrap, uv, mip);
}

float4 GetPhysical(in float mip, in float2 uv, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].physicalTexture)].SampleLevel(g_SamLinearWrap, uv, mip);
}

float3 GetBaseColor(in float mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].baseTexture)].SampleLevel(sState, uv, mip).rgb;
}

float3 GetBaseColor(in float mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].baseTexture)].SampleLevel(sState, uv, mip).rgb;
}

float GetOpacity(in float mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].baseTexture)].SampleLevel(sState, uv, mip).a;
}

float GetOpacity(in float mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].baseTexture)].SampleLevel(sState, uv, mip).a;
}

float3 GetEmissive(in float mip, in float2 uv, SamplerState sState)
{
	int tex = instanceData.tex0;
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].emissiveTexture)].SampleLevel(sState, uv, mip).rgb;
}

float3 GetEmissive(in float mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = GetTextureSet(texIdx);
	return $$(G_TEXTURES)[NonUniformResourceIndex($$(G_TEXTURESETS)[NonUniformResourceIndex(tex)].emissiveTexture)].SampleLevel(sState, uv, mip).rgb;
}