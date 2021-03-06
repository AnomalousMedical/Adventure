
#include "Structures.hlsl"
#include "RayUtils.hlsl"

RWTexture2D<float4> g_ColorBuffer;

[shader("raygeneration")]
void main()
{
    // Calculate ray direction by interpolating frustum corner rays.
    float3  rayOrigin = g_ConstantsCB.CameraPos.xyz;
    float2  uv        = (float2(DispatchRaysIndex().xy) + float2(0.5, 0.5)) / float2(DispatchRaysDimensions().xy);
    float3  rayDir    = normalize(lerp(lerp(g_ConstantsCB.FrustumRayLB.xyz, g_ConstantsCB.FrustumRayRB.xyz, uv.x),
                                       lerp(g_ConstantsCB.FrustumRayLT.xyz, g_ConstantsCB.FrustumRayRT.xyz, uv.x), uv.y));

    RayDesc ray;
    ray.Origin    = rayOrigin;
    ray.Direction = rayDir;
    ray.TMin      = g_ConstantsCB.ClipPlanes.x;
    ray.TMax      = g_ConstantsCB.ClipPlanes.y;

    PrimaryRayPayload payload = CastPrimaryRay(ray, /*recursion*/0);

    g_ColorBuffer[DispatchRaysIndex().xy] = float4(payload.Color, 1.0);
}
