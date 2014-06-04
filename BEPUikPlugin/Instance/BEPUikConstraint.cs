﻿using BEPUik;
using Engine.ObjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEPUikPlugin
{
    public abstract class BEPUikConstraint : SimElement
    {
        public BEPUikConstraint(String name, Subscription subscription)
            :base(name, subscription)
        {
            
        }

        protected void setupConstraint(BEPUikConstraintDefinition definition)
        {
            IKConstraint.MaximumForce = definition.MaximumForce;
            IKConstraint.Rigidity = definition.Rigidity;
        }

        protected void setupConstraintDefinition(BEPUikConstraintDefinition definition)
        {
            definition.MaximumForce = IKConstraint.MaximumForce;
            definition.Rigidity = IKConstraint.Rigidity;
            definition.Subscription = this.Subscription;
        }

        public abstract IKConstraint IKConstraint { get; }
    }
}
