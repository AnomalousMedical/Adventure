﻿using Engine.Attributes;
using Engine.Editing;
using Engine.ObjectManagement;
using Engine.Saving;
using Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEPUikPlugin
{
    public class BEPUikBoneDefinition : BEPUikElementDefinition
    {
        [DoNotSave]
        private EditInterface editInterface;

        public BEPUikBoneDefinition(String name)
            :base(name)
        {
            Radius = 1;
            Height = 3;
        }

        [Editable]
        public bool Pinned { get; set; }

        [Editable]
        public float Radius { get; set; }

        [Editable]
        public float Height { get; set; }

        public override void registerScene(SimSubScene subscene, SimObjectBase instance)
        {
            if (subscene.hasSimElementManagerType(typeof(BEPUikScene)))
            {
                BEPUikScene sceneManager = subscene.getSimElementManager<BEPUikScene>();
                sceneManager.IkFactory.addBone(this, instance);
            }
            else
            {
                Log.Default.sendMessage("Cannot add BEPUikBone {0} to SimSubScene {1} because it does not contain a BEPUikScene.", LogLevel.Warning, BEPUikInterface.PluginName, Name, subscene.Name);
            }
        }

        protected override EditInterface createEditInterface()
        {
            if (editInterface == null)
            {
                editInterface = ReflectedEditInterface.createEditInterface(this, String.Format("{0} - IK Bone", Name));
            }
            return editInterface;
        }

        internal override void createProduct(SimObjectBase instance, BEPUikScene scene)
        {
            BEPUikBone bone = new BEPUikBone(this, scene);
            instance.addElement(bone);
        }

        public BEPUikBoneDefinition(LoadInfo info)
            :base(info)
        {
            Pinned = info.GetBoolean("Pinned");
            Radius = info.GetFloat("Radius");
            Height = info.GetFloat("Height");
        }

        public override void getInfo(SaveInfo info)
        {
            base.getInfo(info);
            info.AddValue("Pinned", Pinned);
            info.AddValue("Radius", Radius);
            info.AddValue("Height", Height);
        }
    }
}
