using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

namespace DiligentEngine
{
    public partial class ShaderCreateInfo
    {

        public ShaderCreateInfo()
        {
            
        }
        public String FilePath { get; set; }
        public String Source { get; set; }
        public String EntryPoint { get; set; } = "main";
        public bool UseCombinedTextureSamplers { get; set; } = false;
        public String CombinedSamplerSuffix { get; set; } = "_sampler";
        public ShaderDesc Desc { get; set; } = new ShaderDesc();
        public SHADER_SOURCE_LANGUAGE SourceLanguage { get; set; } = SHADER_SOURCE_LANGUAGE.SHADER_SOURCE_LANGUAGE_DEFAULT;
        public SHADER_COMPILER ShaderCompiler { get; set; } = SHADER_COMPILER.SHADER_COMPILER_DEFAULT;
        public ShaderVersion HLSLVersion { get; set; } = new ShaderVersion();
        public ShaderVersion MSLVersion { get; set; } = new ShaderVersion();
        public SHADER_COMPILE_FLAGS CompileFlags { get; set; } = SHADER_COMPILE_FLAGS.SHADER_COMPILE_FLAG_NONE;


    }
}
