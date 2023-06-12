struct BlasInstanceData
{
    float2 uv0; //also tex4 (x) and tex5 (y) in mesh, GlassReflectionColorMask.xy for glass
    float2 uv1; //also tex6 (x) and tex7 (y) in mesh, GlassReflectionColorMask.z and GlassAbsorption
    float2 uv2; //also tex8 (x) and tex9 (y) in mesh, GlassIndexOfRefraction
    float2 uv3; //also tex10 (x) and tex11 (y) in mesh

    int tex0;
    int tex1;
    int tex2;
    int tex3;
    uint indexOffset;
    uint vertexOffset;
    uint dispatchType;
    uint padding;   //also GlassMaterialColor
};

struct TextureSet
{
    int baseTexture;
    int normalTexture;
    int physicalTexture;
    int emissiveTexture;
};

struct CubeAttribVertex {
  float2 uv;
  float2 globalUv;
  float4 tangent;
  float4 binormal;
  float4 normal;
  float tex;
  int pad1;
  int pad2;
  int pad3;
};

struct PrimaryRayPayload
{
    float3 Color;
    float  Depth;
    uint   Recursion;
};

struct EmissiveRayPayload
{
    float3 Color;
    uint   Recursion;
};

struct ShadowRayPayload
{
    float  Shading;   // 0 - fully shadowed, 1 - fully in light, 0..1 - for semi-transparent objects
    uint   Recursion; // Current recusrsion depth
};

struct Constants
{
    // Camera world position
    float4   CameraPos;

    // Near and far clip plane distances
    float2   ClipPlanes;
    float2   Padding0;

    // Camera view frustum corner rays
    float4   FrustumRayLT;
    float4   FrustumRayLB;
    float4   FrustumRayRT;
    float4   FrustumRayRB;


    // The number of shadow PCF samples
    int      ShadowPCF; 
    // Maximum ray recursion depth
    int      MaxRecursion;
    float    Darkness;
    int   NumActiveLights;

    // Light properties
    float4  AmbientColor;
    float4  LightPos[NUM_LIGHTS];
    float4  LightColor[NUM_LIGHTS]; //Light color stores length of the light in a/w

    //Sky properties
    float4 Pallete[6];
};

struct SurfaceReflectanceInfo
{
    float  PerceptualRoughness;
    float3 Reflectance0;
    float3 Reflectance90;
    float3 DiffuseColor;
};

struct AngularInfo
{
    float NdotL;   // cos angle between normal and light direction
    float NdotV;   // cos angle between normal and view direction
    float NdotH;   // cos angle between normal and half vector
    float LdotH;   // cos angle between light direction and half vector
    float VdotH;   // cos angle between view direction and half vector
};

// Instance mask.
#define OPAQUE_GEOM_MASK      0x01
#define TRANSPARENT_GEOM_MASK 0x02

// Ray types
#define HIT_GROUP_STRIDE  2
#define PRIMARY_RAY_INDEX 0
#define SHADOW_RAY_INDEX  1


// Small offset between ray intersection and new ray origin to avoid self-intersections.
//TODO: IT might be ideal to move this epsilon to config struct.
//This value is fairly large, but it fixes the triangles that appear in the world mesh
# define SMALL_OFFSET 0.1

