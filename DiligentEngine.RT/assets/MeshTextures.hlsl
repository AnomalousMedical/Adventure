#include "Textures.hlsl"

float3 GetBaseColor(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.baseTexture].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}

float GetOpacity(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.baseTexture].SampleLevel(g_SamLinearWrap, uv, mip).a;
}

float3 GetEmissive(in int mip, in float2 uv)
{
	return $$(G_TEXTURES)[instanceData.emissiveTexture].SampleLevel(g_SamLinearWrap, uv, mip).rgb;
}