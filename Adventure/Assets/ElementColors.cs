using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets
{
    public static class ElementColors
    {
        public const uint Fire = 0xffde4509;
        public const uint Ice = 0xff0962de;
        public const uint Electricity = 0xffe3c923;

        public static readonly HslColor FireHsl = new IntColor(Fire).ToHsl();
        public static readonly HslColor IceHsl = new IntColor(Ice).ToHsl();
        public static readonly HslColor ElectricityHsl = new IntColor(Electricity).ToHsl();

        public static readonly Color FireColor = Color.FromARGB(Fire);
        public static readonly Color IceColor = Color.FromARGB(Ice);
        public static readonly Color ElectricityColor = Color.FromARGB(Electricity);

        public static float GetElementalHue(Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return FireHsl.H;
                case Element.Ice:
                    return IceHsl.H;
                case Element.Electricity:
                    return ElectricityHsl.H;
            }

            return 120f;
        }

        public static Color GetElementalColor(Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return FireColor;
                case Element.Ice:
                    return IceColor;
                case Element.Electricity:
                    return ElectricityColor;
            }

            return Color.Green;
        }
    }
}
