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
    public partial class BlendStateDesc
    {

        public BlendStateDesc()
        {
            
        }
        public Bool AlphaToCoverageEnable { get; set; } = false;
        public Bool IndependentBlendEnable { get; set; } = false;
        public RenderTargetBlendDesc RenderTargets_0 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_1 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_2 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_3 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_4 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_5 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_6 { get; set; } = new RenderTargetBlendDesc();
        public RenderTargetBlendDesc RenderTargets_7 { get; set; } = new RenderTargetBlendDesc();


    }
}
