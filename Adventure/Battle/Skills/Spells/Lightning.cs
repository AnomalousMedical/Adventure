using Adventure.Assets.PixelEffects;
using Adventure.Assets.SoundEffects;
using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakLightning : ElementalBase
    {
        public WeakLightning()
            : base(Element.Electricity, new ElectricEffect(), LightningSpellSoundEffect.Instance)
        {
            Name = "Weak Lightning";
            MpCost = 4;
            TriggeredMpCost = 6;
            Power = 2;
        }
    }

    class Lightning : ElementalBase
    {
        public Lightning()
            : base(Element.Electricity, new ElectricEffect(), LightningSpellSoundEffect.Instance)
        {
            Name = "Lightning";
            MpCost = 8;
            TriggeredMpCost = 11;
            Power = 4;
            HitGroupOnTrigger = true;
        }
    }

    class StrongLightning : ElementalBase
    {
        public StrongLightning()
            : base(Element.Electricity, new ElectricEffect(), LightningSpellSoundEffect.Instance)
        {
            Name = "Strong Lightning";
            MpCost = 17;
            TriggeredMpCost = 21;
            Power = 8;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }

    class ArchLightning : ElementalBase
    {
        public ArchLightning()
            : base(Element.Electricity, new ElectricEffect(), LightningSpellSoundEffect.Instance)
        {
            Name = "Arch Lightning";
            MpCost = 32;
            TriggeredMpCost = 41;
            Power = 16;
            HitGroupOnTrigger = true;
            BuffAlliesWithElement = true;
        }
    }
}
