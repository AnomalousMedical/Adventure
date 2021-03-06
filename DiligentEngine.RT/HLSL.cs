using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uint8 = System.Byte;
using Int8 = System.SByte;
using Bool = System.Boolean;
using Uint32 = System.UInt32;
using Uint64 = System.UInt64;
using Float32 = System.Single;
using Uint16 = System.UInt16;
using PVoid = System.IntPtr;
using float4 = Engine.Vector4;
using float3 = Engine.Vector3;
using float2 = Engine.Vector2;
using float4x4 = Engine.Matrix4x4;
using BOOL = System.Boolean;
using System.Runtime.InteropServices;

namespace DiligentEngine.RT.HLSL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimaryRayPayload
    {
        float3 Color;
        float Depth;
        uint Recursion;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct EmissiveRayPayload
    {
        float3 Color;
        uint Recursion;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowRayPayload
    {
        float Shading;   // 0 - fully shadowed, 1 - fully in light, 0..1 - for semi-transparent objects
        uint Recursion; // Current recusrsion depth
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct BlasInstanceData
    {
        public int baseTexture;
        public int normalTexture;
        public int physicalTexture;
        public int emissiveTexture;
        public uint indexOffset;
        public uint vertexOffset;
        public uint dispatchType;
        public uint padding;
    };

    public static class BlasInstanceDataConstants
    {
        public const uint MeshData = 0;
        public const uint SpriteData = 1;

        public const uint LightAndShadeBase = 2;
        public const uint LightAndShadeBaseEmissive = 4;
        public const uint LightAndShadeBaseNormal = 8;
        public const uint LightAndShadeBaseNormalEmissive = 16;
        public const uint LightAndShadeBaseNormalPhysical = 32;
        public const uint LightAndShadeBaseNormalPhysicalEmissive = 64;
        public const uint LightAndShadeBaseNormalPhysicalReflective = 128;
        public const uint LightAndShadeBaseNormalPhysicalReflectiveEmissive = 256;

        public static uint GetShaderForDescription(bool hasNormal, bool hasPhysical, bool reflective, bool emissive, bool isSprite)
        {
            if (hasNormal && hasPhysical && reflective)
            {
                if (emissive)
                {
                    return GetSpriteFlag(LightAndShadeBaseNormalPhysicalReflectiveEmissive, isSprite);
                }
                return GetSpriteFlag(LightAndShadeBaseNormalPhysicalReflective, isSprite);
            }

            if(hasNormal && hasPhysical)
            {
                if (emissive)
                {
                    return GetSpriteFlag(LightAndShadeBaseNormalPhysicalEmissive, isSprite);
                }
                return GetSpriteFlag(LightAndShadeBaseNormalPhysical, isSprite);
            }

            if (hasNormal)
            {
                if (emissive)
                {
                    return GetSpriteFlag(LightAndShadeBaseNormalEmissive, isSprite);
                }
                return GetSpriteFlag(LightAndShadeBaseNormal, isSprite);
            }

            if (emissive)
            {
                return GetSpriteFlag(LightAndShadeBaseEmissive, isSprite);
            }
            return GetSpriteFlag(LightAndShadeBase, isSprite);
        }

        private static uint GetSpriteFlag(uint original, bool isSprite)
        {
            if (isSprite)
            {
                original |= SpriteData;
            }
            return original;
        }
    }
}
