﻿using DiligentEngine;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSandbox
{
    internal class SceneGlassCube : IDisposable, IShaderTableBinder
    {
        public class Desc
        {
            public string InstanceName { get; set; } = Guid.NewGuid().ToString();

            public byte Mask { get; set; } = RtStructures.TRANSPARENT_GEOM_MASK;


            public InstanceMatrix Transform = InstanceMatrix.Identity;
        }

        private readonly TLASBuildInstanceData instanceData;
        private readonly RTInstances instances;

        public SceneGlassCube
        (
            Desc description,
            RTInstances instances, 
            CubeBLAS cubeBLAS
        )
        {
            this.instances = instances;
            this.instanceData = new TLASBuildInstanceData()
            {
                InstanceName = description.InstanceName,
                pBLAS = cubeBLAS.BLAS,
                Mask = description.Mask,
                Transform = description.Transform
            };

            instances.AddTlasBuild(instanceData);
            instances.AddShaderTableBinder(this);
        }

        public void Dispose()
        {
            instances.RemoveShaderTableBinder(this);
            instances.RemoveTlasBuild(instanceData);
        }

        public void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            sbt.BindHitGroupForInstance(tlas, instanceData.InstanceName, RtStructures.PRIMARY_RAY_INDEX, "GlassPrimaryHit", IntPtr.Zero);

            // We must specify the intersection shader for procedural geometry.
            sbt.BindHitGroupForInstance(tlas, instanceData.InstanceName, RtStructures.SHADOW_RAY_INDEX, "SphereShadowHit", IntPtr.Zero);
        }
    }
}