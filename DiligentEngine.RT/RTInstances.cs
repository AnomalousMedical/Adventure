using DiligentEngine.RT.Sprites;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine.RT
{
    //public class RTInstanceHandle
    //{
    //    public String InstanceName { get; set; }
    //    public IBottomLevelAS pBLAS { get; set; }
    //    public InstanceMatrix Transform { get; set; }
    //    public UInt32 CustomId { get; set; }
    //    public RAYTRACING_INSTANCE_FLAGS Flags { get; set; }
    //    public Byte Mask { get; set; }
    //    public UInt32 ContributionToHitGroupIndex { get; set; }
    //    internal int Index { get; set; } = -1;
    //}

    public class RTInstances
    {
        public delegate void ShaderTableBinder(IShaderBindingTable sbt, ITopLevelAS tlas);

        List<TLASBuildInstanceData> instances = new List<TLASBuildInstanceData>();
        List<ShaderTableBinder> shaderTableBinders = new List<ShaderTableBinder>();
        List<SpriteBlasLinker> sprites = new List<SpriteBlasLinker>();

        TLASBuildInstanceDataPassStruct[] passInstances = new TLASBuildInstanceDataPassStruct[0];
        bool updatePassInstances = false;

        internal TLASBuildInstanceDataPassStruct[] Instances
        {
            get
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
                    passInstances = TLASBuildInstanceDataPassStruct.ToStruct(instances);
                    //passInstances = new TLASBuildInstanceDataPassStruct[instances.Count];
                    //var numInstancs = instances.Count;
                    //for (int i = 0; i < numInstancs; i++)
                    //{
                    //    var instance = instances[i];
                    //    instance.Index = i;
                    //    passInstances[i] = new TLASBuildInstanceDataPassStruct
                    //    {
                    //        InstanceName = instance.InstanceName,
                    //        pBLAS = TLASBuildInstanceDataPassStruct.GetPblasPtr(instance.pBLAS),
                    //        Transform = instance.Transform,
                    //        CustomId = instance.CustomId,
                    //        Flags = instance.Flags,
                    //        Mask = instance.Mask,
                    //        ContributionToHitGroupIndex = instance.ContributionToHitGroupIndex,
                    //    };
                    //}
                }
                return passInstances;
            }
        }

        public uint InstanceCount => (uint)instances.Count;

        public void AddTlasBuild(TLASBuildInstanceData instance)
        {
            updatePassInstances = true;
            instances.Add(instance);
        }

        public void RemoveTlasBuild(TLASBuildInstanceData instance)
        {
            updatePassInstances = true;
            instances.Remove(instance);
        }

        public void AddShaderTableBinder(ShaderTableBinder binder)
        {
            shaderTableBinders.Add(binder);
        }

        public void RemoveShaderTableBinder(ShaderTableBinder binder)
        {
            shaderTableBinders.Remove(binder);
        }

        public void AddSprite(ISprite sprite, TLASBuildInstanceData instanceBuildData, SpriteInstance spriteInstance)
        {
            sprites.Add(new SpriteBlasLinker(sprite, instanceBuildData, spriteInstance));
        }

        public void RemoveSprite(ISprite sprite)
        {
            var max = sprites.Count;
            for(int i = 0; i < max; ++i)
            {
                if(sprites[i].WrappedSprite == sprite)
                {
                    sprites.RemoveAt(i);
                    i = max;
                }
            }
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
