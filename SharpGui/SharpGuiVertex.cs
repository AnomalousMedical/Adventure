﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{

    [StructLayout(LayoutKind.Sequential)]
    struct SharpGuiVertex
    {
        public Vector3 pos;
        public Color color;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct SharpGuiTextVertex
    {
        public Vector3 pos;
        public Color color;
        public Vector2 uv;
        public uint textureIndex;
    };
}
