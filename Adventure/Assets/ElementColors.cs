using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets
{
    internal class ElementColors
    {
        public const uint Fire = 0xffde4509;
        public const uint Ice = 0xff0962de;
        public const uint Electricity = 0xffe3c923;
        public const uint Acid = 0xff18c81b;
        public const uint Darkness = 0xffe109bb;
        public const uint Earth = 0xffcd880b;

        public float GetElementalHue(Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return Fire;
                case Element.Ice:
                    return Ice;
                case Element.Electricity:
                    return Electricity;
                case Element.Acid:
                    return Acid;
                case Element.Darkness:
                    return Darkness;
                case Element.Earth:
                    return Earth;
            }

            return 120;
        }
    }
}
