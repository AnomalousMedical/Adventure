using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakFire : BaseBlast
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

    class Fire : BaseBlast
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

    class StrongFire : BaseBlast
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

    class ArchFire : BaseBlast
    {
        public ArchFire()
            : base(Element.Fire)
        {
            Name = "Arch Fire";
            MpCost = 32;
            TriggeredMpCost = 41;
            Power = 16;
            HitGroupOnTrigger = true;
        }
    }
}
