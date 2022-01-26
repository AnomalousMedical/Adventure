﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class CameraMover
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Orientation = Quaternion.Identity;
        public Vector3 SceneCenter;
    }
}
