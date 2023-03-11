using RpgMath;

namespace Adventure.Battle.Skills
{
    class WeakFire : Base
    {
        public WeakFire()
            : base("Weak Fire", Element.Fire, 4, 2)
        {

        }
    }

    class Fire : Base
    {
        public Fire()
            :base("Fire", Element.Fire, 8, 4)
        {

        }
    }

    class StrongFire : Base
    {
        public StrongFire()
            : base("Strong Fire", Element.Fire, 17, 8)
        {

        }
    }

    class ArchFire : Base
    {
        public ArchFire()
            : base("Arch Fire", Element.Fire, 50, 16)
        {

        }
    }
}
