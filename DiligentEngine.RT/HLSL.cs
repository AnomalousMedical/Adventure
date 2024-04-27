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
        public BlasInstanceData()
        {
            raycastSmallOffset = 0.0001f;
        }

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
        public float raycastSmallOffset; //A small offset to apply to rays cast in the shader. This is a cheap / ok fix, but there is a better way: https://developer.nvidia.com/blog/solving-self-intersection-artifacts-in-directx-raytracing/
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

    public enum BlasSpecialMaterial
    {
        None,
        Sprite,
        MultiTexture,
        Glass,
        Water
    }

    public static class BlasInstanceDataConstants
    {
        //These offsets determine what data type is being handled
        public const uint MeshData = 0;
        public const uint SpriteData = 1;
        public const uint MultiTextureMeshData = 2;

        //These are the base flags for the different lighting types
        public const uint LightAndShadeBase = 8;
        public const uint LightAndShadeBaseEmissive = 16;
        public const uint LightAndShadeBaseNormal = 32;
        public const uint LightAndShadeBaseNormalEmissive = 64;
        public const uint LightAndShadeBaseNormalPhysical = 128;
        public const uint LightAndShadeBaseNormalPhysicalEmissive = 256;
        public const uint LightAndShadeBaseNormalPhysicalReflective = 512;
        public const uint LightAndShadeBaseNormalPhysicalReflectiveEmissive = 1024;
        public const uint Glass = 2048;
        public const uint Water = 4096;

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

        public static uint GetShaderForDescription(bool hasNormal, bool hasPhysical, bool reflective, bool emissive, BlasSpecialMaterial specialMaterial = BlasSpecialMaterial.None)
        {
            switch (specialMaterial)
            {
                case BlasSpecialMaterial.Water:
                    return Water;
                case BlasSpecialMaterial.Glass:
                    return Glass;
            }

            if (hasNormal && hasPhysical && reflective)
            {
                if (emissive)
                {
                    return GetTypeFlag(LightAndShadeBaseNormalPhysicalReflectiveEmissive, specialMaterial);
                }
                return GetTypeFlag(LightAndShadeBaseNormalPhysicalReflective, specialMaterial);
            }

            if (hasNormal && hasPhysical)
            {
                if (emissive)
                {
                    return GetTypeFlag(LightAndShadeBaseNormalPhysicalEmissive, specialMaterial);
                }
                return GetTypeFlag(LightAndShadeBaseNormalPhysical, specialMaterial);
            }

            if (hasNormal)
            {
                if (emissive)
                {
                    return GetTypeFlag(LightAndShadeBaseNormalEmissive, specialMaterial);
                }
                return GetTypeFlag(LightAndShadeBaseNormal, specialMaterial);
            }

            if (emissive)
            {
                return GetTypeFlag(LightAndShadeBaseEmissive, specialMaterial);
            }
            return GetTypeFlag(LightAndShadeBase, specialMaterial);
        }

        private static uint GetSpriteFlag(uint original, bool isSprite)
        {
            if (isSprite)
            {
                original |= SpriteData;
            }
            return original;
        }

        private static uint GetTypeFlag(uint original, BlasSpecialMaterial specialMaterial)
        {
            switch (specialMaterial)
            {
                case BlasSpecialMaterial.Sprite:
                    original |= SpriteData;
                    break;
                case BlasSpecialMaterial.MultiTexture:
                    original |= MultiTextureMeshData;
                    break;
            }

            return original;
        }
    }
}
