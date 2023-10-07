﻿using Adventure.Assets.PixelEffects;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakIce : ElementalBase
    {
        public WeakIce()
            : base(Element.Ice, new IceEffect())
        {
            Name = "Weak Ice";
            MpCost = 4;
            TriggeredMpCost = 6;
            Power = 2;
        }
    }

    class Ice : ElementalBase
    {
        public Ice()
            : base(Element.Ice, new IceEffect())
        {
            Name = "Ice";
            MpCost = 8;
            TriggeredMpCost = 11;
            Power = 4;
        }
    }

    class StrongIce : ElementalBase
    {
        public StrongIce()
            : base(Element.Ice, new IceEffect())
        {
            Name = "Strong Ice";
            MpCost = 17;
            TriggeredMpCost = 21;
            Power = 8;
            HitGroupOnTrigger = true;
        }
    }

    class ArchIce : ElementalBase
    {
        public ArchIce()
            : base(Element.Ice, new IceEffect())
        {
            Name = "Arch Ice";
            MpCost = 32;
            TriggeredMpCost = 41;
            Power = 16;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }
}
