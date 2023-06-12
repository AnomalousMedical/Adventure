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
        public float u1;//also base color extras in mesh
        public float v1;
        public float u2;//also normal extras in mesh
        public float v2;
        public float u3;//also physical extras in mesh
        public float v3;
        public float u4;//also emissive extras in mesh
        public float v4;

        public int tex0;
        public int tex1;
        public int tex2;
        public int tex3;
        public uint indexOffset;
        public uint vertexOffset;
        public uint dispatchType;
        public uint padding;
        public uint extra0;
        public uint extra1;
        public uint extra2;
        public uint extra3;
        public uint extra4;
        public uint extra5;
        public uint extra6;
        public uint extra7;
    };

    public static class GlassInstanceDataCreator
    {
        public static BlasInstanceData Create(in float3 GlassReflectionColorMask, float GlassAbsorption, in float2 GlassIndexOfRefraction, in Engine.Color GlassMaterialColor)
        {
            return new BlasInstanceData
            {
                u1 = GlassReflectionColorMask.x,
                v1 = GlassReflectionColorMask.y,
                u2 = GlassReflectionColorMask.z,
                v2 = GlassAbsorption,
                u3 = GlassIndexOfRefraction.x,
                v3 = GlassIndexOfRefraction.y,
                padding = (uint)GlassMaterialColor.toRGB(),
            };
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct TextureSet
    {
        public int baseTexture;
        public int normalTexture;
        public int physicalTexture;
        public int emissiveTexture;
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
        public const uint Glass = 512;
        public const uint Water = 1024;

        public static uint GetShaderForDescription(bool hasNormal, bool hasPhysical, bool reflective, bool emissive, bool isSprite, bool isGlass = false, bool isWater = false)
        {
            if (isWater)
            {
                return Water;
            }

            if (isGlass)
            {
                return Glass;
            }

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
