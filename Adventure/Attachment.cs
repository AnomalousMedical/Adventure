﻿using Adventure.Services;
using DiligentEngine;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;

namespace Adventure
{
    class Attachment<T> : IDisposable
    {
        public class Description
        {
            public Quaternion Orientation { get; set; } = Quaternion.Identity;

            public ISprite Sprite { get; set; }

            public SpriteMaterialDescription SpriteMaterial { get; set; }

            public bool RenderShadow { get; set; } = true;

            public Light Light { get; set; }

            public Vector3 LightOffset { get; set; }
        }

        
        private const int PrimaryAttachment = 0;

        private readonly IDestructionRequest destructionRequest;
        private readonly RTInstances<T> rtInstances;
        private readonly SpriteInstanceFactory spriteInstanceFactory;
        private readonly LightManager lightManager;
        private readonly ISprite sprite;
        private readonly TLASInstanceData tlasData;
        private readonly Light light;
        private readonly Vector3 lightOffset;
        private bool dummyLight;

        private SpriteInstance spriteInstance;
        private bool disposed;

        private Quaternion orientation;
        private Quaternion additionalRotation = Quaternion.Identity;
        private bool graphicsActive;
        private bool makeGraphicsActive = true;
        private bool graphicsReady = false;

        public Attachment
        (
            IDestructionRequest destructionRequest,
            RTInstances<T> rtInstances,
            SpriteInstanceFactory spriteInstanceFactory,
            IScopedCoroutine coroutine,
            Description attachmentDescription,
            LightManager lightManager
        )
        {
            this.orientation = attachmentDescription.Orientation;
            this.sprite = attachmentDescription.Sprite;
            this.destructionRequest = destructionRequest;
            this.rtInstances = rtInstances;
            this.spriteInstanceFactory = spriteInstanceFactory;
            this.lightManager = lightManager;
            this.lightOffset = attachmentDescription.LightOffset;

            this.tlasData = new TLASInstanceData()
            {
                InstanceName = RTId.CreateId("Attachment"),
                Mask = attachmentDescription.RenderShadow ? RtStructures.OPAQUE_GEOM_MASK : RtStructures.TRANSPARENT_GEOM_MASK,
                Transform = new InstanceMatrix(Vector3.Zero, attachmentDescription.Orientation, sprite.BaseScale) //It might be worth it to skip this line
            };

            if (attachmentDescription.Light != null)
            {
                light = attachmentDescription.Light;
                lightManager.AddLight(light);
                dummyLight = false;
            }
            else
            {
                light = new Light();
                dummyLight = true;
            }

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until task is finished and this is disposed.

                this.spriteInstance = await spriteInstanceFactory.Checkout(attachmentDescription.SpriteMaterial, sprite);
                graphicsReady = true;
                if (this.disposed)
                {
                    this.spriteInstanceFactory.TryReturn(spriteInstance);
                    return; //Stop loading
                }
                else
                {
                    SetGraphicsActive(makeGraphicsActive);
                }
            });
        }

        public void Dispose()
        {
            disposed = true;
            this.spriteInstanceFactory.TryReturn(spriteInstance);
            SetGraphicsActive(false);
            if (!dummyLight)
            {
                lightManager.RemoveLight(light);
            }
        }

        public void SetGraphicsActive(bool active)
        {
            makeGraphicsActive = active;
            if (graphicsActive != active && graphicsReady)
            {
                if (active)
                {
                    rtInstances.AddTlasBuild(tlasData);
                    rtInstances.AddShaderTableBinder(Bind);
                    rtInstances.AddSprite(sprite, tlasData, spriteInstance);
                    graphicsActive = true;
                }
                else
                {
                    rtInstances.RemoveSprite(sprite);
                    rtInstances.RemoveShaderTableBinder(Bind);
                    rtInstances.RemoveTlasBuild(tlasData);
                    graphicsActive = false;
                }
            }
        }

        public void RequestDestruction()
        {
            destructionRequest.RequestDestruction();
        }

        public void SetAdditionalRotation(in Quaternion additionalRotation)
        {
            this.additionalRotation = additionalRotation;
        }

        public void SetAnimation(String name)
        {
            sprite.SetAnimation(name);
        }

        public Vector3 BaseScale => sprite.BaseScale;

        public void SetPosition(in Vector3 parentPosition, in Quaternion parentRotation, in Vector3 parentScale)
        {
            var frame = sprite.GetCurrentFrame();
            var primaryAttach = frame.Attachments[PrimaryAttachment];

            //Get the primary attachment out of sprite space into world space
            var scale = sprite.BaseScale * parentScale;
            var translate = scale * primaryAttach.translate;
            var fullRot = parentRotation * this.orientation * additionalRotation;
            translate = Quaternion.quatRotate(fullRot, translate);

            var finalPosition = parentPosition - translate; //The attachment point on the sprite is an offset to where that sprite attaches, subtract it
            var finalOrientation = fullRot;
            var finalScale = scale;

            SetWorldPosition(finalPosition, finalOrientation, finalScale);
        }

        public void SetWorldPosition(in Vector3 finalPosition, in Quaternion finalOrientation, in Vector3 finalScale)
        {
            this.tlasData.Transform = new InstanceMatrix(finalPosition, finalOrientation, finalScale);

            light.Position = (finalPosition + lightOffset).ToVector4();
        }

        private void Bind(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            spriteInstance.Bind(this.tlasData.InstanceName, sbt, tlas, sprite);
        }
    }
}
