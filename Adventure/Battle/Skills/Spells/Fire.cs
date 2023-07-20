using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakFire : ElementalBase
    {
        public WeakFire()
            : base(Element.Fire)
        {
            Name = "Weak Fire";
            MpCost = 4;
            TriggeredMpCost = 6;
            Power = 2;
        }
    }

    class Fire : ElementalBase
    {
        public Fire()
            :base(Element.Fire)
        {
            Name = "Fire";
            MpCost = 8;
            TriggeredMpCost = 11;
            Power = 4;
        }
    }

    class StrongFire : ElementalBase
    {
        public StrongFire()
            : base(Element.Fire)
        {
            Name = "Strong Fire";
            MpCost = 17;
            TriggeredMpCost = 21;
            Power = 8;
            HitGroupOnTrigger = true;
        }
    }

    class ArchFire : ElementalBase
    {
        public ArchFire()
            : base(Element.Fire)
        {
            Name = "Arch Fire";
            MpCost = 32;
            TriggeredMpCost = 41;
            Power = 16;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }
}
