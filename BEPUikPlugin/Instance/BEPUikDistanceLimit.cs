﻿using BEPUik;
using Engine;
using Engine.ObjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BEPUikPlugin
{
    public class BEPUikDistanceLimit : BEPUikLimit
    {
        private IKDistanceLimit limit;

        public BEPUikDistanceLimit(BEPUikBone connectionA, BEPUikBone connectionB, Vector3 anchor, BEPUikDistanceLimitDefinition definition, String name, Subscription subscription)
            :base(connectionA, connectionB, name, subscription)
        {
            limit = new IKDistanceLimit(connectionA.IkBone, connectionB.IkBone, anchor.toBepuVec3(), anchor.toBepuVec3(), definition.MinimumDistance, definition.MaximumDistance);
            setupLimit(definition);
        }

        public override SimElementDefinition saveToDefinition()
        {
            var definition = new BEPUikDistanceLimitDefinition(Name)
            {
                MinimumDistance = limit.MinimumDistance,
                MaximumDistance = limit.MaximumDistance,
            };
            setupLimitDefinition(definition);
            return definition;
        }

        public override IKLimit IKLimit
        {
            get
            {
                return limit;
            }
        }
    }
}
