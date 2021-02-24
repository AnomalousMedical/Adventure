﻿using DiligentEngine;
using DiligentEngine.GltfPbr;
using DiligentEngine.GltfPbr.Shapes;
using Engine;
using Engine.Platform;
using FreeImageAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTest
{
    class Sword : IDisposable
    {
        private AutoPtr<IShaderResourceBinding> pboMatBindingSprite;
        private SceneObjectManager sceneObjectManager;
        private SpriteManager sprites;
        private IDestructionRequest destructionRequest;
        private SceneObject sceneObject;
        private Sprite sprite = new Sprite();

        public Sword(
            SceneObjectManager sceneObjectManager,
            SpriteManager sprites,
            Plane plane,
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IMaterialSpriteBuilder materialSpriteBuilder)
        {
            this.sceneObjectManager = sceneObjectManager;
            this.sprites = sprites;
            this.destructionRequest = destructionRequest;

            IEnumerator<YieldAction> co()
            {
                yield return coroutine.Await(async () =>
                {
                    pboMatBindingSprite = await materialSpriteBuilder.CreateSpriteAsync(new MaterialSpriteBindingDescription()
                    {
                        ColorMap = "original/Sword.png",
                        Materials = new Dictionary<uint, (String, String)>()
                        {
                            { 0xff6c351c, ( "cc0Textures/Wood049_1K", "jpg" ) }, //Hilt (brown)
                            { 0xffadadad, ( "cc0Textures/Metal032_1K", "jpg" ) }, //Blade (grey)
                        }
                    });
                });

                sceneObject = new SceneObject()
                {
                    vertexBuffer = plane.VertexBuffer,
                    skinVertexBuffer = plane.SkinVertexBuffer,
                    indexBuffer = plane.IndexBuffer,
                    numIndices = plane.NumIndices,
                    pbrAlphaMode = PbrAlphaMode.ALPHA_MODE_MASK,
                    position = new Vector3(-1, 0, 0),
                    orientation = Quaternion.Identity,
                    scale = new Vector3(1, 1.714285714285714f, 1) * 0.5f,
                    shaderResourceBinding = pboMatBindingSprite.Obj,
                    RenderShadow = true,
                    Sprite = sprite,
                };
                sprites.Add(sprite);
                sceneObjectManager.Add(sceneObject);
            }
            coroutine.Run(co());
        }

        public void Dispose()
        {
            sprites.Remove(sprite);
            sceneObjectManager.Remove(sceneObject);
            pboMatBindingSprite.Dispose();
        }
    }
}
