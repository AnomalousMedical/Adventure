﻿using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Resources;
using Engine;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSandbox
{
    internal class SandboxScene : IDisposable
    {
        private readonly IObjectResolver objectResolver;
        private readonly RayTracingRenderer renderer;
        private readonly RTCameraAndLight cameraAndLight;
        private SceneCube rotateCube;
        private Quaternion currentRot = Quaternion.Identity;

        public SandboxScene
        (
            IObjectResolverFactory objectResolverFactory, 
            RayTracingRenderer renderer,
            RTCameraAndLight cameraAndLight
        )
        {
            this.renderer = renderer;
            this.cameraAndLight = cameraAndLight;
            objectResolver = objectResolverFactory.Create();

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-3, 0, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/Lava004_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-3, -3, 0), Quaternion.Identity);
                o.Flags = RAYTRACING_INSTANCE_FLAGS.RAYTRACING_INSTANCE_FORCE_NO_OPAQUE;
                o.Texture = new CCOTextureBindingDescription("cc0Textures/SheetMetal002_1K", Reflective: true);
            });

            rotateCube = objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(0, 0, 0), Quaternion.Identity);
                o.IsGlass = true;
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(0, -3, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/Ice004_1K");
            });

            objectResolver.Resolve<SceneSprite, SceneSprite.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(0, -3, -10), Quaternion.Identity, new Vector3(5, 5, 1));
            });

            objectResolver.Resolve<SceneSprite, SceneSprite.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(3, -3, 0), Quaternion.Identity);
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-3, 0, 10), Quaternion.Identity, new Vector3(1, 1, 1));
                o.Texture = new CCOTextureBindingDescription("cc0Textures/MetalPlates006_1K", Reflective: true);
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-6, -3, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/Rock034_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-9, -3, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/Fabric021_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(-12, -3, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/SolarPanel003_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(3, 0, 0), Quaternion.Identity);
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(3, 3, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/Chip005_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(3, 6, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/ChristmasTreeOrnament007_1K");
            });

            objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
            {
                o.Transform = new InstanceMatrix(new Vector3(3, 9, 0), Quaternion.Identity);
                o.Texture = new CCOTextureBindingDescription("cc0Textures/AcousticFoam003_1K");
            });

            for (var x = -20; x < 20; ++x)
            {
                for (var z = -20; z < 20; ++z)
                {
                    objectResolver.Resolve<SceneCube, SceneCube.Desc>(o =>
                    {
                        o.Transform = new InstanceMatrix(new Vector3(x, -6.0f, z), Quaternion.Identity);
                        o.TextureIndex = 4;
                        o.Texture = new CCOTextureBindingDescription("cc0Textures/Ground037_1K");
                    });
                }
            }
        }

        public void Update(Clock clock)
        {
            currentRot *= new Quaternion(new Vector3(0, 1, 0), 0.35f * clock.DeltaSeconds);
            rotateCube?.SetTransform(Vector3.Zero, currentRot);

            float x = -20f;
            while(cameraAndLight.CheckoutLight(out var index))
            {
                cameraAndLight.LightPos[index] = new Vector4(x, -5, 0, 0);
                cameraAndLight.LightColor[index] = new Color(1.00f, +0.8f, +0.80f);
                cameraAndLight.LightLength[index] = 5.0f;
                x += 3f;
            }
        }

        public void Dispose()
        {
            objectResolver.Dispose();
        }
    }
}
