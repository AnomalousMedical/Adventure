using RpgMath;

namespace Adventure.Battle.Skills
{
    class IceBlast : BaseBlast
    {
        public IceBlast()
            : base("Ice Blast", Element.Fire, 18, 6)
        {

        }
    }

    class Freezer : BaseBlast
    {
        public Freezer()
            : base("Freezer", Element.Fire, 75, 19)
        {

        }
    }

    class Blizzard : BaseBlast
    {
        public Blizzard()
            : base("Blizzard", Element.Fire, 110, 26)
        {

        }
    }
}
