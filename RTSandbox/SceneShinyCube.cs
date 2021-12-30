﻿using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSandbox
{
    internal class SceneShinyCube : IDisposable
    {
        public class Desc
        {
            public string InstanceName { get; set; } = RTId.CreateId("SceneCube");

            public uint TextureIndex { get; set; } = 0;

            public byte Mask { get; set; } = RtStructures.OPAQUE_GEOM_MASK;

            public RAYTRACING_INSTANCE_FLAGS Flags { get; set; } = RAYTRACING_INSTANCE_FLAGS.RAYTRACING_INSTANCE_NONE;

            public InstanceMatrix Transform = InstanceMatrix.Identity;
        }

        private readonly TLASBuildInstanceData instanceData;
        private readonly ShinyCubeBLAS cubeBLAS;
        private readonly RTInstances rtInstances;

        public SceneShinyCube
        (
            Desc description,
            ShinyCubeBLAS cubeBLAS,
            IScopedCoroutine coroutine,
            RTInstances rtInstances
        )
        {
            this.cubeBLAS = cubeBLAS;
            this.rtInstances = rtInstances;
            this.instanceData = new TLASBuildInstanceData()
            {
                InstanceName = description.InstanceName,
                CustomId = description.TextureIndex, // texture index
                Mask = description.Mask,
                Transform = description.Transform,
                Flags = description.Flags,
            };

            coroutine.RunTask(async () =>
            {
                await this.cubeBLAS.WaitForLoad();
                this.instanceData.pBLAS = cubeBLAS.Instance.BLAS.Obj;

                rtInstances.AddTlasBuild(instanceData);
                rtInstances.AddShaderTableBinder(Bind);
            });
        }

        public void Dispose()
        {
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(instanceData);
        }

        public void SetTransform(in Vector3 trans, in Quaternion rot)
        {
            this.instanceData.Transform = new InstanceMatrix(trans, rot);
        }

        public void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            cubeBLAS.PrimaryHitShader.BindSbt(instanceData.InstanceName, sbt, tlas, IntPtr.Zero, 0);
        }
    }
}
