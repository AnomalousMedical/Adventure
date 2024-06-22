#include "Structures.hlsl"

#define PI 3.14159

// User zoran404 https://gamedev.stackexchange.com/questions/114412/how-to-get-uv-coordinates-for-sphere-cylindrical-projection
// Assumes p is a point on unit sphere (normalize non-unit vectors).
// For a skybox it is the ray direction.
// https://en.wikipedia.org/wiki/File:Equirectangular_projection_SW.jpg
float2 sphere2mapUV_Equirectangular(float3 p)
{
    return float2(
        atan2(p.x, -p.z) / (2 * PI) + .5,
        p.y * .5 + .5
    );
}

// https://en.wikipedia.org/wiki/File:Lambert_cylindrical_equal-area_projection_SW.jpg
float2 sphere2mapUV_EqualArea(float3 p)
{
    return float2(
        (atan2(p.x, -p.z) / PI + 1) / 2,
        asin(p.y) / PI + .5
    );
}

ConstantBuffer<Constants> g_ConstantsCB;

Texture2D    $$(G_TEXTURES)[$$(NUM_TEXTURES)];
StructuredBuffer<TextureSet>   $$(G_TEXTURESETS);
SamplerState g_SamLinearWrap;

[shader("miss")]
void main(inout PrimaryRayPayload payload)
{
    float3 color;
    if(g_ConstantsCB.missTextureSet > -1)
    {
        float2 uv = sphere2mapUV_EqualArea(WorldRayDirection());
        color = $$(G_TEXTURES)[$$(G_TEXTURESETS)[g_ConstantsCB.missTextureSet].baseTexture].SampleLevel(g_SamLinearWrap, uv, 0).rgb;
        //color = float3(uv.x, uv.y, 0);
    }
    else
    {
        // Generate sky color.
        float factor = clamp((-WorldRayDirection().y + 0.5) / 1.5 * 4.0, 0.0, 4.0);
        int   idx = floor(factor);
        factor -= float(idx);
        color = lerp(g_ConstantsCB.Pallete[idx].xyz, g_ConstantsCB.Pallete[idx + 1].xyz, factor);
    }

    payload.Color = color;
    //payload.Depth = RayTCurrent(); // bug in DXC for SPIRV
    payload.Depth = g_ConstantsCB.ClipPlanes.y;
}
