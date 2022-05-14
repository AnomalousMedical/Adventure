using DiligentEngine;
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
        public class Desc
        {
            public float Left { get; set; } = 0.0f;

            public float Top { get; set; } = 0.0f;

            public float Right { get; set; } = 1.0f;

            public float Bottom { get; set; } = 1.0f;

            public override bool Equals(object obj)
            {
                return obj is Desc description &&
                       Left == description.Left &&
                       Top == description.Top &&
                       Right == description.Right &&
                       Bottom == description.Bottom;
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(Left);
                hashCode.Add(Top);
                hashCode.Add(Right);
                hashCode.Add(Bottom);
                return hashCode.ToHashCode();
            }
        }

        public class Factory
        {
            private readonly PooledResourceManager<Desc, SpritePlaneBLAS> pooledResources
                = new PooledResourceManager<Desc, SpritePlaneBLAS>();

            private readonly BLASBuilder blasBuilder;

            public Factory
            (
                BLASBuilder blasBuilder
            )
            {
                this.blasBuilder = blasBuilder;
            }

            /// <summary>
            /// Create a shader. The caller is responsible for calling return.
            /// </summary>
            /// <param name="baseName"></param>
            /// <param name="numTextures"></param>
            /// <returns></returns>
            public Task<SpritePlaneBLAS> Checkout(Desc desc)
            {
                return pooledResources.Checkout(desc, async () =>
                {
                    var blas = new SpritePlaneBLAS();
                    await blas.Setup(desc, blasBuilder);
                    return pooledResources.CreateResult(blas);
                });
            }

            public void TryReturn(SpritePlaneBLAS item)
            {
                if (item != null)
                {
                    pooledResources.Return(item);
                }
            }
        }

        private BLASInstance instance;
        private TaskCompletionSource loadingTask = new TaskCompletionSource();

        public BLASInstance Instance => instance;

        private async Task Setup(Desc desc, BLASBuilder blasBuilder)
        {
            try
            {
                var blasDesc = new BLASDesc(RTId.CreateId("SpritePlaneBLAS"))
                {
                    Flags = RAYTRACING_GEOMETRY_FLAGS.RAYTRACING_GEOMETRY_FLAG_NONE
                };

                //Top Right
                //Top Left
                //Bottom Left
                //Bottom Right

                blasDesc.CubePos = new Vector3[]
                {
                        new Vector3(-0.5f,-0.5f,+0.0f), new Vector3(+0.5f,-0.5f,+0.0f), new Vector3(+0.5f,+0.5f,+0.0f), new Vector3(-0.5f,+0.5f,+0.0f), //Front +z
                };

                blasDesc.CubeUV = new Vector4[]
                {
                    new Vector4(desc.Right,desc.Top,0,0), 
                    new Vector4(desc.Left,desc.Top,0,0), 
                    new Vector4(desc.Left,desc.Bottom,0,0), 
                    new Vector4(desc.Right,desc.Bottom,0,0)  //Front +z
                };

                blasDesc.CubeNormals = new Vector4[]
                {
                        new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0), new Vector4(0, 0, +1, 0)  //Front +z
                };

                blasDesc.Indices = new uint[]
                {
                        0,1,2, 0,3,2
                };

                instance = await blasBuilder.CreateBLAS(blasDesc);

                loadingTask.SetResult();
            }
            catch (Exception ex)
            {
                loadingTask.TrySetException(ex);
            }
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
