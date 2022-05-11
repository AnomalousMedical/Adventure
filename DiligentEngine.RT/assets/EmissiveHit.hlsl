#include "Structures.hlsl"
#include "Data.hlsl"

[shader("closesthit")]
void main(inout EmissiveRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    GetInstanceData(attr, barycentrics, posX, posY, posZ, uv);

    int mip = GetMip();

    payload.Color = GetEmissive(mip, uv, g_SamLinearWrap);
}
