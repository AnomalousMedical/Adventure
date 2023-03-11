using RpgMath;

namespace Adventure.Battle.Skills
{
    class FireBlast : BaseBlast
    {
        public FireBlast()
            :base("Fire Blast", Element.Fire, 18, 6)
        {

        }
    }

    class Furnace : BaseBlast
    {
        public Furnace()
            : base("Furnace", Element.Fire, 75, 19)
        {

        }
    }

    class Inferno : BaseBlast
    {
        public Inferno()
            : base("Inferno", Element.Fire, 110, 26)
        {

        }
    }
}
