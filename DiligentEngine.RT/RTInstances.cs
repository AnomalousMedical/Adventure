using DiligentEngine.RT.Sprites;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    public class RTInstances
    {
        public delegate void ShaderTableBinder(IShaderBindingTable sbt, ITopLevelAS tlas);

        List<TLASInstanceData> instances = new List<TLASInstanceData>();
        List<ShaderTableBinder> shaderTableBinders = new List<ShaderTableBinder>();
        List<ISprite> sprites = new List<ISprite>();
        bool updatePassInstances = false;

        //There is always an extra dummy instance to serve as the lookup for anything new that has been added
        internal TLASBuildInstanceDataPassStruct[] passInstances = new TLASBuildInstanceDataPassStruct[1];

        internal void UpdateInstances()
        {
            if (updatePassInstances)
            {
                //The lifecycle of these is interesting. When they are added the updatePassInstances flag is set
                //which will make a whole new copy of the instances into the passInstances. Once this is done and
                //the relationship established any updates to the returned RTInstanceHandle will be reflected in
                //the copied passInstances. In the event that the list is changed the new instances are added / removed
                //however, the copied list persists until this function call. This means that any updates to the returned
                //RTInstanceHandles will work, but really those updates are ignored. The next time this is accessed
                //that out of data list is thrown away and a new copy is made, which will have all the correct values
                //from the original build data.

                updatePassInstances = false;
                passInstances = new TLASBuildInstanceDataPassStruct[instances.Count + 1];
                var numInstancs = instances.Count;
                for (int i = 0; i < numInstancs; i++)
                {
                    var instance = instances[i];
                    instance.InstanceIndex = i;
                    passInstances[i] = new TLASBuildInstanceDataPassStruct
                    {
                        InstanceName = instance.InstanceName,
                        pBLAS = instance.pBLAS.ObjPtr,
                        Transform = instance.Transform,
                        CustomId = instance.CustomId,
                        Flags = instance.Flags,
                        Mask = instance.Mask,
                        ContributionToHitGroupIndex = instance.ContributionToHitGroupIndex,
                    };
                }
            }
        }

        internal uint InstanceCount => (uint)instances.Count; //This will be 1 less than passInstances.Length

        public void AddTlasBuild(TLASInstanceData instance)
        {
            if(instance.InstanceIndex != -1)
            {
                throw new InvalidOperationException("TLASInstanceData already added. These can only be added to once RTInstances one time.");
            }

            updatePassInstances = true;
            instance.InstanceIndex = passInstances.Length - 1;
            instance.RTInstances = this;
            instances.Add(instance);
        }

        public void RemoveTlasBuild(TLASInstanceData instance)
        {
            if (instances.Remove(instance))
            {
                updatePassInstances = true;
                instance.InstanceIndex = -1;
                instance.RTInstances = null;
            }
        }

        public void AddShaderTableBinder(ShaderTableBinder binder)
        {
            shaderTableBinders.Add(binder);
        }

        public void RemoveShaderTableBinder(ShaderTableBinder binder)
        {
            shaderTableBinders.Remove(binder);
        }

        public void AddSprite(ISprite sprite, TLASInstanceData instanceData, SpriteInstance spriteInstance)
        {
            spriteInstance.UpdateBlas(instanceData);
            sprites.Add(sprite);
        }

        public void RemoveSprite(ISprite sprite)
        {
            sprites.Remove(sprite);
        }

        public void UpdateSprites(Clock clock)
        {
            foreach (var sprite in sprites)
            {
                sprite.Update(clock);
            }
        }

        internal void BindShaders(IShaderBindingTable sbt, ITopLevelAS tlas)
        {
            foreach (var i in shaderTableBinders)
            {
                i(sbt, tlas);
            }
        }
    }

    public class RTInstances<T> : RTInstances { }
}
