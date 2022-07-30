int GetMip()
{
	//Lame mip calculation, but looks tons better than just mip0.
	//Need to add screen size and some more info
	float depth = RayTCurrent();
	int mip = min(depth / 4, 4);
	return mip;
}

float3 GetSampledNormal(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.normalTexture].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float3 GetSampledNormal(in int mip, in float2 uv, in int texIdx)
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.normalTexture;
		break;
	case 1:
		tex = instanceData.uv1.x;
		break;
	case 2:
		tex = instanceData.uv1.y;
		break;
	}

	return $$(G_TEXTURES)[tex].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float4 GetPhysical(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.physicalTexture].SampleLevel(g_SamLinearWrap, uv, mip);
}

float4 GetPhysical(in int mip, in float2 uv, in int texIdx)
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.physicalTexture;
		break;
	case 1:
		tex = instanceData.uv2.x;
		break;
	case 2:
		tex = instanceData.uv2.y;
		break;
	}

	return $$(G_TEXTURES)[tex].SampleLevel(g_SamLinearWrap, uv, mip);
}

float3 GetBaseColor(in int mip, in float2 uv, SamplerState sState)
{
	return $$(G_TEXTURES)[instanceData.baseTexture].SampleLevel(sState, uv, mip).rgb;
}

float3 GetBaseColor(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.baseTexture;
		break;
	case 1:
		tex = instanceData.uv0.x;
		break;
	case 2:
		tex = instanceData.uv0.y;
		break;
	}

	return $$(G_TEXTURES)[tex].SampleLevel(sState, uv, mip).rgb;
}

float GetOpacity(in int mip, in float2 uv, SamplerState sState)
{
	return $$(G_TEXTURES)[instanceData.baseTexture].SampleLevel(sState, uv, mip).a;
}

float GetOpacity(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.baseTexture;
		break;
	case 1:
		tex = instanceData.uv0.x;
		break;
	case 2:
		tex = instanceData.uv0.y;
		break;
	}

	return $$(G_TEXTURES)[tex].SampleLevel(sState, uv, mip).a;
}

float3 GetEmissive(in int mip, in float2 uv, SamplerState sState)
{
	return $$(G_TEXTURES)[instanceData.emissiveTexture].SampleLevel(sState, uv, mip).rgb;
}

float3 GetEmissive(in int mip, in float2 uv, SamplerState sState, in int texIdx)
{
	int tex = 0;
	switch (texIdx) {
	case 0:
		tex = instanceData.emissiveTexture;
		break;
	case 1:
		tex = instanceData.uv3.x;
		break;
	case 2:
		tex = instanceData.uv3.y;
		break;
	}

	return $$(G_TEXTURES)[tex].SampleLevel(sState, uv, mip).rgb;
}