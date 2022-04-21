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
        public const uint Acid = 0xff18c81b;
        public const uint Darkness = 0xffe109bb;
        public const uint Earth = 0xffcd880b;

        public static readonly HslColor FireHsl = new IntColor(Fire).ToHsl();
        public static readonly HslColor IceHsl = new IntColor(Ice).ToHsl();
        public static readonly HslColor ElectricityHsl = new IntColor(Electricity).ToHsl();
        public static readonly HslColor AcidHsl = new IntColor(Acid).ToHsl();
        public static readonly HslColor DarknessHsl = new IntColor(Darkness).ToHsl();
        public static readonly HslColor EarthHsl = new IntColor(Earth).ToHsl();

        public static readonly Color FireColor = Color.FromARGB(Fire);
        public static readonly Color IceColor = Color.FromARGB(Ice);
        public static readonly Color ElectricityColor = Color.FromARGB(Electricity);
        public static readonly Color AcidColor = Color.FromARGB(Acid);
        public static readonly Color DarknessColor = Color.FromARGB(Darkness);
        public static readonly Color EarthColor = Color.FromARGB(Earth);

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
                case Element.Acid:
                    return AcidHsl.H;
                case Element.Darkness:
                    return DarknessHsl.H;
                case Element.Earth:
                    return EarthHsl.H;
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
                case Element.Acid:
                    return AcidColor;
                case Element.Darkness:
                    return DarknessColor;
                case Element.Earth:
                    return EarthColor;
            }

            return Color.Green;
        }
    }
}
