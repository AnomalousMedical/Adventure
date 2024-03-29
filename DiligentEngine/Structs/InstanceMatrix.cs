﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine
{

    /// <summary>
    ///        rotation        translation
    /// ([0,0]  [0,1]  [0,2])   ([0,3])
    /// ([1,0]  [1,1]  [1,2])   ([1,3])
    /// ([2,0]  [2,1]  [2,2])   ([2,3])
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct InstanceMatrix
    {
        [FieldOffset(0)]
        public float m00;
        [FieldOffset(4)]
        public float m01;
        [FieldOffset(8)]
        public float m02;
        [FieldOffset(12)]
        public float m03;
        [FieldOffset(16)]
        public float m10;
        [FieldOffset(20)]
        public float m11;
        [FieldOffset(24)]
        public float m12;
        [FieldOffset(28)]
        public float m13;
        [FieldOffset(32)]
        public float m20;
        [FieldOffset(36)]
        public float m21;
        [FieldOffset(40)]
        public float m22;
        [FieldOffset(44)]
        public float m23;

        /// <summary>
        /// Set using a vector3 and quaternion. Will convert to camera space.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="rot"></param>
        public InstanceMatrix(in Vector3 trans, in Quaternion rot)
        {
            var rotMat = rot.toRotationMatrix3x3();

            this.m00 = rotMat.m00;
            this.m01 = rotMat.m01;
            this.m02 = rotMat.m02;
            this.m10 = rotMat.m10;
            this.m11 = rotMat.m11;
            this.m12 = rotMat.m12;
            this.m20 = rotMat.m20;
            this.m21 = rotMat.m21;
            this.m22 = rotMat.m22;

            this.m03 = -trans.x;
            this.m13 = -trans.y;
            this.m23 = -trans.z;
        }

        /// <summary>
        /// Set using a vector3 and quaternion. Will convert to camera space.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="rot"></param>
        public InstanceMatrix(in Vector3 trans, in Quaternion rot, in Vector3 scale)
        {
            //TODO: Don't use mat4x4 here
            var rotMat = rot.toRotationMatrix4x4();
            var scaleMat = Matrix4x4.Scale(scale.x, scale.y, scale.z);

            var finalMat = rotMat * scaleMat;

            this.m00 = finalMat.m00;
            this.m01 = finalMat.m01;
            this.m02 = finalMat.m02;
            this.m10 = finalMat.m10;
            this.m11 = finalMat.m11;
            this.m12 = finalMat.m12;
            this.m20 = finalMat.m20;
            this.m21 = finalMat.m21;
            this.m22 = finalMat.m22;

            this.m03 = -trans.x;
            this.m13 = -trans.y;
            this.m23 = -trans.z;
        }

        /// <summary>
        /// Set using a vector3 and quaternion. Will convert to camera space.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="rot"></param>
        public InstanceMatrix(in Vector3 trans, in Vector3 scale)
        {
            var scaleMat = Matrix3x3.Scale(scale.x, scale.y, scale.z);

            this.m00 = scaleMat.m00;
            this.m01 = scaleMat.m01;
            this.m02 = scaleMat.m02;
            this.m10 = scaleMat.m10;
            this.m11 = scaleMat.m11;
            this.m12 = scaleMat.m12;
            this.m20 = scaleMat.m20;
            this.m21 = scaleMat.m21;
            this.m22 = scaleMat.m22;

            this.m03 = -trans.x;
            this.m13 = -trans.y;
            this.m23 = -trans.z;
        }

        public static readonly InstanceMatrix Identity = new InstanceMatrix(new Vector3(0, 0, 0), Quaternion.Identity);
    }
}
