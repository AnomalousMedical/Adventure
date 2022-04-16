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
