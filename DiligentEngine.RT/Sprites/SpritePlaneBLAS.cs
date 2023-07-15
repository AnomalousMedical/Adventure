﻿using DiligentEngine;
using DiligentEngine.RT.ShaderSets;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class SpritePlaneBLAS : IDisposable
    {
        private BLASInstance instance;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();

        public BLASInstance Instance => instance;

        public SpritePlaneBLAS
        (
            BLASBuilder blasBuilder,
            IScopedCoroutine coroutineRunner
        )
        {
            coroutineRunner.RunTask(async () =>
            {
                try
                {
                    var blasDesc = new BLASDesc(RTId.CreateId("SpritePlaneBLAS"))
                    {
                        Flags = RAYTRACING_GEOMETRY_FLAGS.RAYTRACING_GEOMETRY_FLAG_NONE
                    };

                    blasDesc.CubePos = new Vector3[]
                    {
                        new Vector3(-0.5f,-0.5f,+0.0f), new Vector3(+0.5f,-0.5f,+0.0f), new Vector3(+0.5f,+0.5f,+0.0f), new Vector3(-0.5f,+0.5f,+0.0f), //Front +z
                    };

                    blasDesc.CubeUV = new Vector2[]
                    {
                        new Vector2(1,0), new Vector2(0,0), new Vector2(0,1), new Vector2(1,1)  //Front +z
                    };

                    blasDesc.GlobalCubeUV = new Vector2[]
                    {
                        new Vector2(1,0), new Vector2(0,0), new Vector2(0,1), new Vector2(1,1)  //Front +z
                    };

                    blasDesc.CubeNormals = new Vector4[]
                    {
                        new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0)  //Front +z
                    };

                    blasDesc.Textures = new float[4]; //Sprites cannot use these textures, since they override the uvs
                    blasDesc.Texture2 = new float[4]; //Sprites cannot use these textures, since they override the uvs

                    // 3   2
                    //
                    // 0   1
                    blasDesc.Indices = new uint[]
                    {
                        0,1,2, 2,3,0
                    };

                    instance = await blasBuilder.CreateBLAS(blasDesc);

                    loadingTask.SetResult();
                }
                catch (Exception ex)
                {
                    loadingTask.TrySetException(ex);
                }
            });
        }

        public void Dispose()
        {
            instance?.Dispose();
        }

        public Task WaitForLoad()
        {
            //This should be called by anything using this class, but it will have already started its setup in its constructor.
            return loadingTask.Task;
        }
    }
}
