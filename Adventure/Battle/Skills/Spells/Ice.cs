using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakIce : Base
    {
        public WeakIce()
            : base("Weak Ice", Element.Ice, 4, 2)
        {

        }
    }

    class Ice : Base
    {
        public Ice()
            : base("Ice", Element.Ice, 8, 4)
        {

        }
    }

    class StrongIce : Base
    {
        public StrongIce()
            : base("Strong Ice", Element.Ice, 17, 8)
        {

        }
    }

    class ArchIce : Base
    {
        public ArchIce()
            : base("Arch Ice", Element.Ice, 50, 16)
        {

        }
    }
}
