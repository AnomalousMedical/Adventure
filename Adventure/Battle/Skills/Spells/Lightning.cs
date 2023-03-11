using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakLightning : BaseBlast
    {
        public WeakLightning()
            : base(Element.Electricity)
        {
            Name = "Weak Lightning";
            MpCost = 4;
            TriggeredMpCost = 6;
            Power = 2;
        }
    }

    class Lightning : BaseBlast
    {
        public Lightning()
            : base(Element.Electricity)
        {
            Name = "Lightning";
            MpCost = 8;
            TriggeredMpCost = 11;
            Power = 4;
        }
    }

    class StrongLightning : BaseBlast
    {
        public StrongLightning()
            : base(Element.Electricity)
        {
            Name = "Strong Lightning";
            MpCost = 17;
            TriggeredMpCost = 21;
            Power = 8;
            HitGroupOnTrigger = true;
        }
    }

    class ArchLightning : BaseBlast
    {
        public ArchLightning()
            : base(Element.Electricity)
        {
            Name = "Arch Lightning";
            MpCost = 32;
            TriggeredMpCost = 41;
            Power = 16;
            HitGroupOnTrigger = true;
        }
    }
}
