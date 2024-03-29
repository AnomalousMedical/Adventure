﻿using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSandbox
{
    internal class SceneSprite : IDisposable
    {
        public class Desc
        {
            public string InstanceName { get; set; } = RTId.CreateId("SceneSprite");

            public InstanceMatrix Transform = InstanceMatrix.Identity;
        }

        private readonly TLASInstanceData instanceData;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly RTInstances rtInstances;
        private SpriteInstance spriteInstance;
        private Sprite sprite = new Sprite();

        public SceneSprite
        (
            Desc description,
            SpriteInstanceFactory spriteInstanceFactory,
            IScopedCoroutine coroutine,
            RTInstances rtInstances
        )
        {
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.rtInstances = rtInstances;

            this.instanceData = new TLASInstanceData()
            {
                InstanceName = description.InstanceName,
                Mask = RtStructures.OPAQUE_GEOM_MASK,
                Transform = description.Transform,
            };

            coroutine.RunTask(async () =>
            {
                this.spriteInstance = await spriteInstanceFactory.Checkout(new SpriteMaterialDescription
                (
                    colorMap: "original/amg1_full4.png",
                    materials: new HashSet<SpriteMaterialTextureItem>
                    {
                        new SpriteMaterialTextureItem(0xffa854ff, "cc0Textures/Fabric012_1K", "jpg"),
                        new SpriteMaterialTextureItem(0xff909090, "cc0Textures/Fabric020_1K", "jpg"),
                        new SpriteMaterialTextureItem(0xff8c4800, "cc0Textures/Leather026_1K", "jpg"),
                        new SpriteMaterialTextureItem(0xffffe254, "cc0Textures/Metal038_1K", "jpg"),
                    }
                ), sprite);

                rtInstances.AddTlasBuild(instanceData);
                rtInstances.AddShaderTableBinder(Bind);
                rtInstances.AddSprite(sprite, instanceData, spriteInstance);
            });
        }

        public void Dispose()
        {
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            rtInstances.RemoveSprite(sprite);
            rtInstances.RemoveShaderTableBinder(Bind);
            rtInstances.RemoveTlasBuild(instanceData);
        }

        public void SetTransform(in Vector3 trans, in Quaternion rot)
        {
            this.instanceData.Transform = new InstanceMatrix(trans, rot);
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.instanceData.InstanceName, sbt, tlas, sprite);
        }
    }
}
