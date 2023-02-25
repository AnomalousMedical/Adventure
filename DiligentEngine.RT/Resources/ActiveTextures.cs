using DiligentEngine.RT.HLSL;
using DiligentEngine.RT.Sprites;
using Engine.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT.Resources
{
    public class ActiveTextures : IDisposable
    {
        class TextureBinding
        {
            public int count;
            public int textureSetIndex;
        }

        public int MaxTextures => 100; //This can be much higher
        internal List<IDeviceObject> Textures => textures;

        private Stack<int> availableSlots;
        private Stack<int> availableSetSlots;

        private Dictionary<CC0TextureResult, TextureBinding> cc0Textures;
        private Dictionary<SpriteMaterial, TextureBinding> spriteTextures;

        private List<IDeviceObject> textures;
        private TextureSet[] textureSets;
        private AutoPtr<ITexture> placeholderTexture;
        private IDeviceObject placeholderTextureDeviceObject;
        private readonly RayTracingRenderer renderer;
        private readonly GraphicsEngine graphicsEngine;
        AutoPtr<IBuffer> texSetBuffer;
        private bool requestBufferRecreate = false;

        public IBuffer TexSetBuffer => texSetBuffer.Obj; //Need to bind this in the shader and setup variables

        public ActiveTextures(TextureLoader textureLoader, IResourceProvider<ShaderLoader<RTShaders>> resourceProvider, RayTracingRenderer renderer, GraphicsEngine graphicsEngine)
        {
            var barriers = new List<StateTransitionDesc>(1);
            using var placeholderStream = resourceProvider.openFile("assets/Placeholder.png");

            placeholderTexture = textureLoader.LoadTexture(placeholderStream, "Placeholder", RESOURCE_DIMENSION.RESOURCE_DIM_TEX_2D, false);
            barriers.Add(new StateTransitionDesc { pResource = placeholderTexture.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
            
            graphicsEngine.ImmediateContext.TransitionResourceStates(barriers); //This needs to happen on the main thread
            
            placeholderTextureDeviceObject = placeholderTexture.Obj.GetDefaultView(TEXTURE_VIEW_TYPE.TEXTURE_VIEW_SHADER_RESOURCE);

            cc0Textures = new Dictionary<CC0TextureResult, TextureBinding>(MaxTextures);
            spriteTextures = new Dictionary<SpriteMaterial, TextureBinding>(MaxTextures);
            textures = new List<IDeviceObject>(MaxTextures);
            availableSlots = new Stack<int>(MaxTextures);
            availableSetSlots = new Stack<int>(MaxTextures);
            textureSets = new TextureSet[MaxTextures];
            for (var i = 0; i < MaxTextures; ++i)
            {
                availableSlots.Push(i);
                availableSetSlots.Push(i);
                textures.Add(placeholderTextureDeviceObject);
            }

            this.renderer = renderer;
            this.graphicsEngine = graphicsEngine;
        }

        public void Dispose()
        {
            DestroyShaderBuffers();
            placeholderTexture.Dispose();
        }

        /// <summary>
        /// Add an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture),
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6, CC0TextureResult texture7)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f,
                v2 = AddActiveTexture2(texture7) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6, CC0TextureResult texture7, CC0TextureResult texture8)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f,
                v2 = AddActiveTexture2(texture7) + 0.5f,
                u3 = AddActiveTexture2(texture8) + 0.5f,
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6, CC0TextureResult texture7, CC0TextureResult texture8, CC0TextureResult texture9)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f,
                v2 = AddActiveTexture2(texture7) + 0.5f,
                u3 = AddActiveTexture2(texture8) + 0.5f,
                v3 = AddActiveTexture2(texture9) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6, CC0TextureResult texture7, CC0TextureResult texture8, CC0TextureResult texture9, CC0TextureResult texture10)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f,
                v2 = AddActiveTexture2(texture7) + 0.5f,
                u3 = AddActiveTexture2(texture8) + 0.5f,
                v3 = AddActiveTexture2(texture9) + 0.5f,
                u4 = AddActiveTexture2(texture10) + 0.5f
            };
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2, CC0TextureResult texture3, CC0TextureResult texture4, CC0TextureResult texture5, CC0TextureResult texture6, CC0TextureResult texture7, CC0TextureResult texture8, CC0TextureResult texture9, CC0TextureResult texture10, CC0TextureResult texture11)
        {
            return new HLSL.BlasInstanceData()
            {
                tex0 = AddActiveTexture2(texture0),
                tex1 = AddActiveTexture2(texture1),
                tex2 = AddActiveTexture2(texture2),
                tex3 = AddActiveTexture2(texture3),
                u1 = AddActiveTexture2(texture4) + 0.5f,
                v1 = AddActiveTexture2(texture5) + 0.5f,
                u2 = AddActiveTexture2(texture6) + 0.5f,
                v2 = AddActiveTexture2(texture7) + 0.5f,
                u3 = AddActiveTexture2(texture8) + 0.5f,
                v3 = AddActiveTexture2(texture9) + 0.5f,
                u4 = AddActiveTexture2(texture10) + 0.5f,
                v4 = AddActiveTexture2(texture11) + 0.5f
            };
        }

        public int AddActiveTexture2(CC0TextureResult texture)
        {
            TextureBinding binding;
            if (!cc0Textures.TryGetValue(texture, out binding))
            {
                binding = new TextureBinding();
                cc0Textures.Add(texture, binding);
                binding.textureSetIndex = GetTextureSetSlot();
                if (texture.BaseColorSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].baseTexture = slot;
                    textures[slot] = texture.BaseColorSRV;
                }
                if (texture.NormalMapSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].normalTexture = slot;
                    textures[slot] = texture.NormalMapSRV;
                }
                if (texture.PhysicalDescriptorMapSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].physicalTexture = slot;
                    textures[slot] = texture.PhysicalDescriptorMapSRV;
                }
                if (texture.EmissiveSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].emissiveTexture = slot;
                    textures[slot] = texture.EmissiveSRV;
                }
                RequestTextureSetUpdate();
            }
            binding.count++;
            return binding.textureSetIndex;
        }

        /// <summary>
        /// Remove an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public void RemoveActiveTexture(CC0TextureResult texture)
        {
            if(texture == null)
            {
                return; //Do nothing if we get null
            }

            if(cc0Textures.TryGetValue(texture, out var binding))
            {
                binding.count--;
                if(binding.count == 0)
                {
                    var textureSet = textureSets[binding.textureSetIndex];
                    if (texture.BaseColorSRV != null)
                    {
                        ReturnTextureSlot(textureSet.baseTexture);
                        textures[textureSet.baseTexture] = placeholderTextureDeviceObject;
                    }
                    if (texture.NormalMapSRV != null)
                    {
                        ReturnTextureSlot(textureSet.normalTexture);
                        textures[textureSet.normalTexture] = placeholderTextureDeviceObject;
                    }
                    if (texture.PhysicalDescriptorMapSRV != null)
                    {
                        ReturnTextureSlot(textureSet.physicalTexture);
                        textures[textureSet.physicalTexture] = placeholderTextureDeviceObject;
                    }
                    if (texture.EmissiveSRV != null)
                    {
                        ReturnTextureSlot(textureSet.emissiveTexture);
                        textures[textureSet.emissiveTexture] = placeholderTextureDeviceObject;
                    }
                    ReturnTextureSetSlot(binding.textureSetIndex);
                    cc0Textures.Remove(texture);
                    RequestTextureSetUpdate();
                }
            }
        }

        /// <summary>
        /// Add an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public HLSL.BlasInstanceData AddActiveTexture(SpriteMaterial texture)
        {
            return new HLSL.BlasInstanceData
            {
                tex0 = AddActiveTexture2(texture)
            };
        }

        public int AddActiveTexture2(SpriteMaterial texture)
        {
            TextureBinding binding;
            if (!spriteTextures.TryGetValue(texture, out binding))
            {
                binding = new TextureBinding();
                spriteTextures.Add(texture, binding);
                binding.textureSetIndex = GetTextureSetSlot();
                if (texture.ColorSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].baseTexture = slot;
                    textures[slot] = texture.ColorSRV;
                }
                if (texture.NormalSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].normalTexture = slot;
                    textures[slot] = texture.NormalSRV;
                }
                if (texture.PhysicalSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].physicalTexture = slot;
                    textures[slot] = texture.PhysicalSRV;
                }
                RequestTextureSetUpdate();
            }
            binding.count++;
            return binding.textureSetIndex;
        }

        /// <summary>
        /// Remove an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public void RemoveActiveTexture(SpriteMaterial texture)
        {
            if (texture == null)
            {
                return; //Do nothing if we get null
            }

            if (spriteTextures.TryGetValue(texture, out var binding))
            {
                binding.count--;
                if (binding.count == 0)
                {
                    var textureSet = textureSets[binding.textureSetIndex];
                    if (texture.ColorSRV != null)
                    {
                        ReturnTextureSlot(textureSet.baseTexture);
                        textures[textureSet.baseTexture] = placeholderTextureDeviceObject;
                    }
                    if (texture.NormalSRV != null)
                    {
                        ReturnTextureSlot(textureSet.normalTexture);
                        textures[textureSet.normalTexture] = placeholderTextureDeviceObject;
                    }
                    if (texture.PhysicalSRV != null)
                    {
                        ReturnTextureSlot(textureSet.physicalTexture);
                        textures[textureSet.physicalTexture] = placeholderTextureDeviceObject;
                    }
                    ReturnTextureSetSlot(binding.textureSetIndex);
                    spriteTextures.Remove(texture);
                    RequestTextureSetUpdate();
                }
            }
        }

        private int GetTextureSlot()
        {
            if(availableSlots.Count == 0)
            {
                throw new InvalidOperationException($"Ran out of texture slots. The current max is {this.MaxTextures}.");
            }
            return availableSlots.Pop();
        }

        private void ReturnTextureSlot(int slot)
        {
            availableSlots.Push(slot);
        }

        private int GetTextureSetSlot()
        {
            if (availableSetSlots.Count == 0)
            {
                throw new InvalidOperationException($"Ran out of texture set slots. The current max is {this.MaxTextures}.");
            }
            return availableSetSlots.Pop();
        }

        private void ReturnTextureSetSlot(int slot)
        {
            availableSetSlots.Push(slot);
        }

        private void RequestTextureSetUpdate()
        {
            renderer.RequestRebind();
            requestBufferRecreate = true;
        }

        private void DestroyShaderBuffers()
        {
            texSetBuffer?.Dispose();
            texSetBuffer = null;
        }

        public void UpdateSharedBuffers()
        {
            if (requestBufferRecreate)
            {
                requestBufferRecreate = false;
                var barriers = new List<StateTransitionDesc>();
                var m_pDevice = graphicsEngine.RenderDevice;

                DestroyShaderBuffers();

                // Create attribs vertex buffer
                unsafe
                {
                    if (textureSets.Length == 0)
                    {
                        return; //No texture sets, bail
                    }

                    var BuffDesc = new BufferDesc();
                    BuffDesc.Name = "Texture set buffer";
                    BuffDesc.Usage = USAGE.USAGE_IMMUTABLE;
                    BuffDesc.BindFlags = BIND_FLAGS.BIND_SHADER_RESOURCE;
                    BuffDesc.ElementByteStride = (uint)sizeof(TextureSet);
                    BuffDesc.Mode = BUFFER_MODE.BUFFER_MODE_STRUCTURED;

                    BufferData BufData = new BufferData();
                    fixed (TextureSet* p_vertices = textureSets)
                    {
                        BufData.pData = new IntPtr(p_vertices);
                        BufData.DataSize = BuffDesc.Size = BuffDesc.ElementByteStride * (uint)textureSets.Length;
                        texSetBuffer = m_pDevice.CreateBuffer(BuffDesc, BufData)
                            ?? throw new InvalidOperationException("Cannot create texture set buffer");
                    }

                    barriers.Add(new StateTransitionDesc { pResource = texSetBuffer.Obj, OldState = RESOURCE_STATE.RESOURCE_STATE_UNKNOWN, NewState = RESOURCE_STATE.RESOURCE_STATE_SHADER_RESOURCE, Flags = STATE_TRANSITION_FLAGS.STATE_TRANSITION_FLAG_UPDATE_STATE });
                }

                graphicsEngine.ImmediateContext.TransitionResourceStates(barriers);
            }
        }
    }
}
