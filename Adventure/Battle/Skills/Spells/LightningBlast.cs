using RpgMath;

namespace Adventure.Battle.Skills
{
    class LightningBlast : BaseBlast
    {
        public LightningBlast()
            : base("Lightning Blast", Element.Fire, 18, 6)
        {

        }
    }

    class Thunderstorm : BaseBlast
    {
        public Thunderstorm()
            : base("Thunderstorm", Element.Fire, 75, 19)
        {

        }
    }

    class BallLightning : BaseBlast
    {
        public BallLightning()
            : base("Ball Lightning", Element.Fire, 110, 26)
        {

        }
    }
}
