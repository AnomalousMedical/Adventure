using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakFire : ElementalBase
    {
        public WeakFire()
            : base(Element.Fire, new FireEffect(), FireSpellSoundEffect.Instance)
        {
            Name = "Weak Fire";
            MpCost = 8;
            TriggeredMpCost = 12;
            Power = 2;
        }
    }

    class Fire : ElementalBase
    {
        public Fire()
            :base(Element.Fire, new FireEffect(), FireSpellSoundEffect.Instance)
        {
            Name = "Fire";
            MpCost = 16;
            TriggeredMpCost = 22;
            Power = 4;
            HitGroupOnTrigger = true;
        }
    }

    class StrongFire : ElementalBase
    {
        public StrongFire()
            : base(Element.Fire, new FireEffect(), FireSpellSoundEffect.Instance)
        {
            Name = "Strong Fire";
            MpCost = 34;
            TriggeredMpCost = 42;
            Power = 8;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }

    class ArchFire : ElementalBase
    {
        public ArchFire()
            : base(Element.Fire, new FireEffect(), FireSpellSoundEffect.Instance)
        {
            Name = "Arch Fire";
            MpCost = 64;
            TriggeredMpCost = 82;
            Power = 16;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }
}
