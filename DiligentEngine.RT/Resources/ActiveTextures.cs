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

        internal TextureSet[] TextureSets => textureSets;

        private Stack<int> availableSlots;
        private Stack<int> availableSetSlots;

        private Dictionary<CC0TextureResult, TextureBinding> cc0Textures;
        private Dictionary<SpriteMaterial, TextureBinding> spriteTextures;

        private List<IDeviceObject> textures;
        private TextureSet[] textureSets;
        private AutoPtr<ITexture> placeholderTexture;
        private IDeviceObject placeholderTextureDeviceObject;
        private readonly RayTracingRenderer renderer;

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
        }

        public void Dispose()
        {
            placeholderTexture.Dispose();
        }

        /// <summary>
        /// Add an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture)
        {
            TextureBinding binding;
            if(!cc0Textures.TryGetValue(texture, out binding))
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
                renderer.RequestRebind();
            }
            binding.count++;
            return binding.textureSetIndex; //return the index, and get rid of all the overloads, just let the clients call what they need
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1)
        {
            var instance0 = AddActiveTexture(texture0);
            var instance1 = AddActiveTexture(texture1);

            instance0.u1 = instance1.baseTexture;
            instance0.u2 = instance1.normalTexture;
            instance0.u3 = instance1.physicalTexture;
            instance0.u4 = instance1.emissiveTexture;

            return instance0;
        }

        public HLSL.BlasInstanceData AddActiveTexture(CC0TextureResult texture0, CC0TextureResult texture1, CC0TextureResult texture2)
        {
            var instance0 = AddActiveTexture(texture0);
            var instance1 = AddActiveTexture(texture1);
            var instance2 = AddActiveTexture(texture2);

            instance0.u1 = instance1.baseTexture;
            instance0.u2 = instance1.normalTexture;
            instance0.u3 = instance1.physicalTexture;
            instance0.u4 = instance1.emissiveTexture;

            instance0.v1 = instance2.baseTexture;
            instance0.v2 = instance2.normalTexture;
            instance0.v3 = instance2.physicalTexture;
            instance0.v4 = instance2.emissiveTexture;

            return instance0;
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
                    renderer.RequestRebind();
                }
            }
        }

        /// <summary>
        /// Add an active texture. Must be done from main thread.
        /// </summary>
        /// <param name="texture"></param>
        public HLSL.BlasInstanceData AddActiveTexture(SpriteMaterial texture)
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
                    textureSets[binding.textureSetIndex].baseTexture = GetTextureSlot();
                    textures[slot] = texture.ColorSRV;
                }
                if (texture.NormalSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].normalTexture = GetTextureSlot();
                    textures[slot] = texture.NormalSRV;
                }
                if (texture.PhysicalSRV != null)
                {
                    var slot = GetTextureSlot();
                    textureSets[binding.textureSetIndex].physicalTexture = GetTextureSlot();
                    textures[slot] = texture.PhysicalSRV;
                }
                renderer.RequestRebind();
            }
            binding.count++;
            return binding.textureSetIndex; //like above
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
                    renderer.RequestRebind();
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
    }
}
