#include "BasicStructures.fxh"
#include "GLTF_PBR_VertexProcessing.fxh"

struct GLTF_VS_Input
{
    float3 Pos     : ATTRIB0;
    float3 Normal  : ATTRIB1;
    float2 UV0     : ATTRIB2;
    float2 UV1     : ATTRIB3;
    float4 Joint0  : ATTRIB4;
    float4 Weight0 : ATTRIB5;
};

cbuffer cbCameraAttribs
{
    CameraAttribs g_CameraAttribs;
}

#if ANOMALOUS_USE_SIMPLE_SHADOW
cbuffer cbLightAttribs
{
    LightAttribs g_LightAttribs;
}
#endif

cbuffer cbTransforms
{
    GLTFNodeShaderTransforms g_Transforms;
}

#ifndef MAX_JOINT_COUNT
#   define MAX_JOINT_COUNT 64
#endif

cbuffer cbJointTransforms
{
    float4x4 g_Joints[MAX_JOINT_COUNT];
}
    
void main(in  GLTF_VS_Input  VSIn,
          out float4 ClipPos  : SV_Position,
          out float3 WorldPos : WORLD_POS,
          out float3 Normal   : NORMAL,
          out float2 UV0      : UV0,
          out float2 UV1      : UV1
#if ANOMALOUS_USE_SIMPLE_SHADOW
          ,out float3 ShadowMapPos : SHADOW_MAP_POS
#endif
)
{
    // Warning: moving this block into GLTF_TransformVertex() function causes huge
    // performance degradation on Vulkan because glslang/SPIRV-Tools are apparently not able
    // to eliminate the copy of g_Transforms structure.
    float4x4 Transform = g_Transforms.NodeMatrix;
    if (g_Transforms.JointCount > 0)
    {
        // Mesh is skinned
        float4x4 SkinMat = 
            VSIn.Weight0.x * g_Joints[int(VSIn.Joint0.x)] +
            VSIn.Weight0.y * g_Joints[int(VSIn.Joint0.y)] +
            VSIn.Weight0.z * g_Joints[int(VSIn.Joint0.z)] +
            VSIn.Weight0.w * g_Joints[int(VSIn.Joint0.w)];
        Transform = mul(Transform, SkinMat);
    }

    GLTF_TransformedVertex TransformedVert = GLTF_TransformVertex(VSIn.Pos, VSIn.Normal, Transform);

    ClipPos  = mul(float4(TransformedVert.WorldPos, 1.0), g_CameraAttribs.mViewProj);
    WorldPos = TransformedVert.WorldPos;
    Normal   = TransformedVert.Normal;
    UV0      = VSIn.UV0;
    UV1      = VSIn.UV1;

#if ANOMALOUS_USE_SIMPLE_SHADOW
    float4 locPos = mul(Transform, float4(VSIn.Pos, 1.0)); //Can shadows be done without the extra transform?
    float4 ShadowMapPos4 = mul(locPos, g_LightAttribs.ShadowAttribs.mWorldToShadowMapUVDepth[0]);
    ShadowMapPos = ShadowMapPos4.xyz / ShadowMapPos4.w;
#endif

#if ANOMALOUS_USE_SPRITE
    UV0 = g_Joints[0][int(VSIn.Joint0.x)].xy; //Cramming uv index into the Joint0 part of the struct for now and using the first joint matrix to pass the uvs
                                              //This all needs to be refactored
    UV1 = UV0;
#endif
}
