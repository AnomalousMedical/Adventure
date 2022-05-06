Texture2D    $$(G_TEXTURES)[$$(NUM_TEXTURES)];
SamplerState g_SamLinearWrap;

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

float4 GetPhysical(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.physicalTexture].SampleLevel(g_SamLinearWrap, uv, mip);
}