using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class Ice : ElementalBase
    {
        public Ice()
            : base(Element.Ice, new IceEffect(), IceSpellSoundEffect.Instance)
        {
            Name = "Ice";
            MpCost = 16;
            TriggeredMpCost = 22;
            Power = 4;
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
