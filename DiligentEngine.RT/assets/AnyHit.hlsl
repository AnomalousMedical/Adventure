#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"

[shader("anyhit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    int mip = GetMip();
    float opacity;
    GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv);

    [forcecase] switch (instanceData.dispatchType & 0x1)
    {
        case $$(MESH_DATA_TYPE):
            opacity = GetOpacity(mip, uv, g_SamLinearWrap);
            break;


        case $$(SPRITE_DATA_TYPE):
            opacity = GetOpacity(mip, uv, g_SamPointWrap);
            break;
    }

    AnyHitOpacityTest(opacity);
}