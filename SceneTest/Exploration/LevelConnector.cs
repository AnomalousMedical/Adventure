﻿using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using DiligentEngine.GltfPbr;
using DiligentEngine.GltfPbr.Shapes;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest
{
    class LevelConnector : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            public String Texture { get; set; } = "cc0Textures/Bricks045_1K";

            public bool RenderShadow { get; set; } = false;

            public bool GetShadow { get; set; } = true;

            /// <summary>
            /// Set this to true to go to the previous level. False to go to the next.
            /// </summary>
            public bool GoPrevious { get; set; }
        }

        private readonly SceneObjectManager<ILevelManager> sceneObjectManager;
        private readonly IBepuScene bepuScene;
        private readonly ICC0TextureManager textureManager;
        private readonly ICollidableTypeIdentifier collidableIdentifier;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ILevelManager levelManager;
        private IShaderResourceBinding matBinding;
        private SceneObject sceneObject;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool disposed;
        private bool goPrevious;

        public LevelConnector(
            SceneObjectManager<ILevelManager> sceneObjectManager,
            Cube cube,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene bepuScene,
            ICC0TextureManager textureManager,
            Description description,
            ICollidableTypeIdentifier collidableIdentifier,
            ICoroutineRunner coroutineRunner,
            ILevelManager levelManager)
        {
            this.goPrevious = description.GoPrevious;
            this.sceneObjectManager = sceneObjectManager;
            this.bepuScene = bepuScene;
            this.textureManager = textureManager;
            this.collidableIdentifier = collidableIdentifier;
            this.coroutineRunner = coroutineRunner;
            this.levelManager = levelManager;
            sceneObject = new SceneObject()
            {
                vertexBuffer = cube.VertexBuffer,
                skinVertexBuffer = cube.SkinVertexBuffer,
                indexBuffer = cube.IndexBuffer,
                numIndices = cube.NumIndices,
                pbrAlphaMode = PbrAlphaMode.ALPHA_MODE_OPAQUE,
                position = description.Translation,
                orientation = description.Orientation,
                scale = description.Scale,
                RenderShadow = description.RenderShadow,
                GetShadows = description.GetShadow
            };

            var shape = new Box(description.Scale.x, description.Scale.y, description.Scale.z); //Each one creates its own, try to load from resources
            shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

            staticHandle = bepuScene.Simulation.Statics.Add(
                new StaticDescription(
                    new System.Numerics.Vector3(description.Translation.x, description.Translation.y, description.Translation.z),
                    new System.Numerics.Quaternion(description.Orientation.x, description.Orientation.y, description.Orientation.z, description.Orientation.w),
                    new CollidableDescription(shapeIndex, 0.1f)));

            bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), HandleCollision);

            coroutine.RunTask(async () =>
            {
                using var destructionBlock = destructionRequest.BlockDestruction(); //Block destruction until coroutine is finished and this is disposed.

                matBinding = await textureManager.Checkout(new CCOTextureBindingDescription(description.Texture, getShadow: description.GetShadow));
                if (disposed)
                {
                    textureManager.Return(matBinding);
                    return; //Stop loading
                }

                if (!destructionRequest.DestructionRequested)
                {
                    sceneObject.shaderResourceBinding = matBinding;
                    this.sceneObjectManager.Add(sceneObject);
                }
            });
        }

        public void Dispose()
        {
            disposed = true;
            bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
            bepuScene.Simulation.Shapes.Remove(shapeIndex);
            bepuScene.Simulation.Statics.Remove(staticHandle);
            sceneObjectManager.Remove(sceneObject);
            textureManager.TryReturn(matBinding);
        }

        internal void SetPosition(in Vector3 position)
        {
            this.sceneObject.position = position;

            bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
            bepuScene.Simulation.Statics.Remove(this.staticHandle);

            staticHandle = bepuScene.Simulation.Statics.Add(
            new StaticDescription(
                position.ToSystemNumerics(),
                this.sceneObject.orientation.ToSystemNumerics(),
                new CollidableDescription(shapeIndex, 0.1f)));
            bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), HandleCollision);
        }

        private void HandleCollision(CollisionEvent evt)
        {
            Console.WriteLine(evt.Pair);
            Console.WriteLine(evt.EventSource);
            //Don't want to do this during the physics update. Trigger to run later.

            if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var _)
                || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out var _))
            {
                coroutineRunner.RunTask(async () =>
                {
                    if (this.goPrevious)
                    {
                        await levelManager.GoPreviousLevel();
                    }
                    else
                    {
                        await levelManager.GoNextLevel();
                    }
                });
            }            
        }
    }
}