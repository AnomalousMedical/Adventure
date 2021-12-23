﻿using DiligentEngine;
using DiligentEngine.RT;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTBepuDemo
{
    internal class SceneCube : IDisposable
    {
        public class Desc
        {
            public string InstanceName { get; set; } = Guid.NewGuid().ToString();

            public uint TextureIndex { get; set; } = 0;

            public byte Mask { get; set; } = RtStructures.OPAQUE_GEOM_MASK;

            public InstanceMatrix Transform = InstanceMatrix.Identity;
        }

        private readonly TLASBuildInstanceData instanceData;
        private readonly CubeBLAS cubeBLAS;
        private readonly RayTracingRenderer renderer;

        public SceneCube
        (
            Desc description,
            CubeBLAS cubeBLAS,
            RayTracingRenderer renderer,
            IScopedCoroutine coroutine
        )
        {
            this.cubeBLAS = cubeBLAS;
            this.renderer = renderer;
            this.instanceData = new TLASBuildInstanceData()
            {
                InstanceName = description.InstanceName,
                CustomId = description.TextureIndex, // texture index
                Mask = description.Mask,
                Transform = description.Transform
            };

            coroutine.RunTask(async () =>
            {
                await cubeBLAS.WaitForLoad();

                instanceData.pBLAS = cubeBLAS.Instance.BLAS.Obj;

                renderer.AddTlasBuild(instanceData);
                renderer.AddShaderTableBinder(Bind);
            });
        }

        public void Dispose()
        {
            renderer.RemoveShaderTableBinder(Bind);
            renderer.RemoveTlasBuild(instanceData);
        }

        public void SetTransform(InstanceMatrix matrix)
        {
            this.instanceData.Transform = matrix;
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            sbt.BindHitGroupForInstance(tlas, instanceData.InstanceName, RtStructures.PRIMARY_RAY_INDEX, cubeBLAS.PrimaryHitShader.ShaderGroupName, IntPtr.Zero);
        }
    }
}