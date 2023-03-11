using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakLightning : Base
    {
        public WeakLightning()
            : base("Weak Lightning", Element.Electricity, 4, 2)
        {

        }
    }

    class Lightning : Base
    {
        public Lightning()
            : base("Lightning", Element.Electricity, 8, 4)
        {

        }
    }

    class StrongLightning : Base
    {
        public StrongLightning()
            : base("Strong Lightning", Element.Electricity, 17, 8)
        {

        }
    }

    class ArchLightning : Base
    {
        public ArchLightning()
            : base("Arch Lightning", Element.Electricity, 50, 16)
        {

        }
    }
}
