#include "Structures.hlsl"
#include "RayUtils.hlsl"
#include "Data.hlsl"
#include "Lighting.hlsl"

[shader("closesthit")]
void main(inout PrimaryRayPayload payload, in BuiltInTriangleIntersectionAttributes attr)
{
    float3 barycentrics;
    CubeAttribVertex posX, posY, posZ;
    float2 uv;
    int mip = GetMip();

    [forcecase] switch (instanceData.dataType) 
    {
        case $$(MESH_DATA_TYPE):
            GetInstanceDataMesh(attr, barycentrics, posX, posY, posZ, uv);
            LightMesh(payload, barycentrics, posX, posY, posZ, mip, uv);
            break;


        case $$(SPRITE_DATA_TYPE):
            GetInstanceDataSprite(attr, barycentrics, posX, posY, posZ, uv);
            LightSprite(payload, barycentrics, posX, posY, posZ, mip, uv);
            break;
    }
}
