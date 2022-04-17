using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class IntColor
    {
        private const int AShift = 24;
        private const int RShift = 16;
        private const int GShift = 8;
        private const int BShift = 0;

        private const uint AMask = 0xff000000;
        private const uint RMask = 0x00ff0000;
        private const uint GMask = 0x0000ff00;
        private const uint BMask = 0x000000ff;

        private const uint ARemove = ~AMask;
        private const uint RRemove = ~RMask;
        private const uint GRemove = ~GMask;
        private const uint BRemove = ~BMask;

        private UInt32 argb;

        public IntColor()
        {

        }

        public IntColor(UInt32 argb)
        {
            this.argb = argb;
        }

        public IntColor(byte a, byte r, byte g, byte b)
        {
            SetValues(a, r, g, b);
        }

        public byte A
        {
            get
            {
                return (byte)((argb & AMask) >> AShift);
            }
            set
            {
                argb = (argb & ARemove) + ((uint)value << AShift);
            }
        }

        public byte R
        {
            get
            {
                return (byte)((argb & RMask) >> RShift);
            }
            set
            {
                argb = (argb & RRemove) + ((uint)value << RShift);
            }
        }

        public byte G
        {
            get
            {
                return (byte)((argb & GMask) >> GShift);
            }
            set
            {
                argb = (argb & GRemove) + ((uint)value << GShift);
            }
        }

        public byte B
        {
            get
            {
                return (byte)((argb & BMask) >> BShift);
            }
            set
            {
                argb = (argb & BRemove) + ((uint)value << BShift);
            }
        }

        public UInt32 ARGB
        {
            get
            {
                return argb;
            }
            set
            {
                argb = value;
            }
        }

        public void SetValues(byte a, byte r, byte g, byte b)
        {
            argb = (uint)(a << AShift) + (uint)(r << RShift) + (uint)(g << GShift) + (uint)(b << BShift);
        }

        //https://stackoverflow.com/questions/27374550/how-to-compare-color-object-and-get-closest-color-in-an-color
        // distance in RGB space
        public IntColor ClosestRgb(IEnumerable<IntColor> colors)
        {
            int ColorDiff(IntColor c1, IntColor c2)
            {
                return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
                                       + (c1.G - c2.G) * (c1.G - c2.G)
                                       + (c1.B - c2.B) * (c1.B - c2.B));
            }

            var colorDiffs = colors.Select(n => new { color = n, diff = ColorDiff(n, this) }).MinBy(n => n.diff);
            return colorDiffs.color;
        }

        public static IntColor FromHslOffset(HslColor hslColor, float inputH, float baseH)
        {
            var h = (inputH + (hslColor.H - baseH)) % 360;
            return FromHsl(h, hslColor.S, hslColor.L);
        }

        public static IntColor FromHsl(HslColor hslColor)
        {
            return FromHsl(hslColor.H, hslColor.S, hslColor.L);
        }

        // https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b
        public static IntColor FromHsl(float hue, float saturation, float light)
        {
            double red, green, blue;

            var h = hue / 360.0;
            var s = saturation / 100.0;
            var l = light / 100.0;

            if (Math.Abs(s - 0.0) < double.Epsilon)
            {
                red = l;
                green = l;
                blue = l;
            }
            else
            {
                double var2;

                if (l < 0.5)
                {
                    var2 = l * (1.0 + s);
                }
                else
                {
                    var2 = l + s - s * l;
                }

                var var1 = 2.0 * l - var2;

                red = Hue2Rgb(var1, var2, h + 1.0 / 3.0);
                green = Hue2Rgb(var1, var2, h);
                blue = Hue2Rgb(var1, var2, h - 1.0 / 3.0);
            }

            var nRed = Convert.ToByte(red * 255.0);
            var nGreen = Convert.ToByte(green * 255.0);
            var nBlue = Convert.ToByte(blue * 255.0);

            return new IntColor(255, nRed, nGreen, nBlue);
        }

        public HslColor ToHsl()
        {

            var varR = R / 255.0; //Where RGB values = 0 ÷ 255
            var varG = G / 255.0;
            var varB = B / 255.0;

            var varMin = getMinimumValue(varR, varG, varB); //Min. value of RGB
            var varMax = getMaximumValue(varR, varG, varB); //Max. value of RGB
            var delMax = varMax - varMin; //Delta RGB value

            double h;
            double s;
            var l = (varMax + varMin) / 2;

            if (Math.Abs(delMax - 0) < double.Epsilon) //This is a gray, no chroma...
            {
                h = 0; //HSL results = 0 ÷ 1
                s = 0;
                // UK:
                //				s = 1.0;
            }
            else //Chromatic data...
            {
                if (l < 0.5)
                {
                    s = delMax / (varMax + varMin);
                }
                else
                {
                    s = delMax / (2.0 - varMax - varMin);
                }

                var delR = ((varMax - varR) / 6.0 + delMax / 2.0) / delMax;
                var delG = ((varMax - varG) / 6.0 + delMax / 2.0) / delMax;
                var delB = ((varMax - varB) / 6.0 + delMax / 2.0) / delMax;

                if (Math.Abs(varR - varMax) < double.Epsilon)
                {
                    h = delB - delG;
                }
                else if (Math.Abs(varG - varMax) < double.Epsilon)
                {
                    h = 1.0 / 3.0 + delR - delB;
                }
                else if (Math.Abs(varB - varMax) < double.Epsilon)
                {
                    h = 2.0 / 3.0 + delG - delR;
                }
                else
                {
                    // Uwe Keim.
                    h = 0.0;
                }

                if (h < 0.0)
                {
                    h += 1.0;
                }
                if (h > 1.0)
                {
                    h -= 1.0;
                }
            }

            // --

            return new HslColor(
                (float)h * 360.0f,
                (float)s * 100.0f,
                (float)l * 100.0f);
        }
        static double getMinimumValue(params double[] values)
        {
            var minValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    minValue = Math.Min(minValue, num);
                }
            }

            return minValue;
        }

        static double getMaximumValue(params double[] values)
        {
            var maxValue = values[0];

            if (values.Length >= 2)
            {
                for (var i = 1; i < values.Length; i++)
                {
                    var num = values[i];
                    maxValue = Math.Max(maxValue, num);
                }
            }

            return maxValue;
        }

        // https://gist.github.com/UweKeim/fb7f829b852c209557bc49c51ba14c8b
        private static double Hue2Rgb(
            double v1,
            double v2,
            double vH)
        {
            if (vH < 0.0)
            {
                vH += 1.0;
            }
            if (vH > 1.0)
            {
                vH -= 1.0;
            }
            if (6.0 * vH < 1.0)
            {
                return v1 + (v2 - v1) * 6.0 * vH;
            }
            if (2.0 * vH < 1.0)
            {
                return v2;
            }
            if (3.0 * vH < 2.0)
            {
                return v1 + (v2 - v1) * (2.0 / 3.0 - vH) * 6.0;
            }

            return v1;
        }
    }
}
