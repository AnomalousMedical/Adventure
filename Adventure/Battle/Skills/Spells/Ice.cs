using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakIce : ElementalBase
    {
        public WeakIce()
            : base(Element.Ice, new IceEffect(), IceSpellSoundEffect.Instance)
        {
            Name = "Weak Ice";
            MpCost = 8;
            TriggeredMpCost = 12;
            Power = 2;
        }
    }

    class Ice : ElementalBase
    {
        public Ice()
            : base(Element.Ice, new IceEffect(), IceSpellSoundEffect.Instance)
        {
            Name = "Ice";
            MpCost = 16;
            TriggeredMpCost = 22;
            Power = 4;
            HitGroupOnTrigger = true;
        }
    }

    class StrongIce : ElementalBase
    {
        public StrongIce()
            : base(Element.Ice, new IceEffect(), IceSpellSoundEffect.Instance)
        {
            Name = "Strong Ice";
            MpCost = 34;
            TriggeredMpCost = 42;
            Power = 8;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }

    class ArchIce : ElementalBase
    {
        public ArchIce()
            : base(Element.Ice, new IceEffect(), IceSpellSoundEffect.Instance)
        {
            Name = "Arch Ice";
            MpCost = 64;
            TriggeredMpCost = 82;
            Power = 16;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }
}
