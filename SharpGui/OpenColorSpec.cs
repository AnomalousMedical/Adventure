using Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace SharpGui
{
    /// <summary>
    /// Implementation of colors from open-color https://yeun.github.io/open-color/ingredients.html
    /// </summary>
    public class OpenColorSpec
    {
        public List<Color> Colors { get; set; }
    }

    public static class OpenColor
    {
        public static OpenColorSpec Gray { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(248,249,250).ToSrgb(),
                Color.FromRGB(241,243,245).ToSrgb(),
                Color.FromRGB(233,236,239).ToSrgb(),
                Color.FromRGB(222,226,230).ToSrgb(),
                Color.FromRGB(206,212,218).ToSrgb(),
                Color.FromRGB(173,181,189).ToSrgb(),
                Color.FromRGB(134,142,150).ToSrgb(),
                Color.FromRGB(73,80,87).ToSrgb(),
                Color.FromRGB(52,58,64).ToSrgb(),
                Color.FromRGB(33,37,41).ToSrgb(),
            }
        };

        public static OpenColorSpec Red { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(255,245,245).ToSrgb(),
                Color.FromRGB(255,227,227).ToSrgb(),
                Color.FromRGB(255,201,201).ToSrgb(),
                Color.FromRGB(255,168,168).ToSrgb(),
                Color.FromRGB(255,135,135).ToSrgb(),
                Color.FromRGB(255,107,107).ToSrgb(),
                Color.FromRGB(250,82 ,82).ToSrgb(),
                Color.FromRGB(240,62 ,62).ToSrgb(),
                Color.FromRGB(224,49 ,49).ToSrgb(),
                Color.FromRGB(201,42 ,42).ToSrgb(),
            }
        };

        public static OpenColorSpec Pink { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(255,240,246).ToSrgb(),
                Color.FromRGB(255,222,235).ToSrgb(),
                Color.FromRGB(252,194,215).ToSrgb(),
                Color.FromRGB(250,162,193).ToSrgb(),
                Color.FromRGB(247,131,172).ToSrgb(),
                Color.FromRGB(240,101,149).ToSrgb(),
                Color.FromRGB(230,73,128).ToSrgb(),
                Color.FromRGB(214,51,108).ToSrgb(),
                Color.FromRGB(194,37,92).ToSrgb(),
                Color.FromRGB(166,30,77).ToSrgb(),
            }
        };

        public static OpenColorSpec Grape { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(248,240,252).ToSrgb(),
                Color.FromRGB(243,217,250).ToSrgb(),
                Color.FromRGB(238,190,250).ToSrgb(),
                Color.FromRGB(229,153,247).ToSrgb(),
                Color.FromRGB(218,119,242).ToSrgb(),
                Color.FromRGB(204,93,232).ToSrgb(),
                Color.FromRGB(190,75,219).ToSrgb(),
                Color.FromRGB(174,62,201).ToSrgb(),
                Color.FromRGB(156,54,181).ToSrgb(),
                Color.FromRGB(134,46,156).ToSrgb(),
            }
        };

        public static OpenColorSpec Violet { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(243,240,255).ToSrgb(),
                Color.FromRGB(229,219,255).ToSrgb(),
                Color.FromRGB(208,191,255).ToSrgb(),
                Color.FromRGB(177,151,252).ToSrgb(),
                Color.FromRGB(151,117,250).ToSrgb(),
                Color.FromRGB(132,94,247).ToSrgb(),
                Color.FromRGB(121,80,242).ToSrgb(),
                Color.FromRGB(112,72,232).ToSrgb(),
                Color.FromRGB(103,65,217).ToSrgb(),
                Color.FromRGB(95,61,196).ToSrgb(),
            }
        };

        public static OpenColorSpec Indigo { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(237,242,255).ToSrgb(),
                Color.FromRGB(219,228,255).ToSrgb(),
                Color.FromRGB(186,200,255).ToSrgb(),
                Color.FromRGB(145,167,255).ToSrgb(),
                Color.FromRGB(116,143,252).ToSrgb(),
                Color.FromRGB(92,124,250).ToSrgb(),
                Color.FromRGB(76,110,245).ToSrgb(),
                Color.FromRGB(66,99,235).ToSrgb(),
                Color.FromRGB(59,91,219).ToSrgb(),
                Color.FromRGB(54,79,199).ToSrgb(),
            }
        };

        public static OpenColorSpec Blue { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(231 ,245 ,255).ToSrgb(),
                Color.FromRGB(208 ,235 ,255).ToSrgb(),
                Color.FromRGB(165 ,216 ,255).ToSrgb(),
                Color.FromRGB(116 ,192 ,252).ToSrgb(),
                Color.FromRGB(77 ,171 ,247).ToSrgb(),
                Color.FromRGB(51 ,154 ,240).ToSrgb(),
                Color.FromRGB(34 ,139 ,230).ToSrgb(),
                Color.FromRGB(28 ,126 ,214).ToSrgb(),
                Color.FromRGB(25 ,113 ,194).ToSrgb(),
                Color.FromRGB(24 ,100 ,171).ToSrgb(),
            }
        };

        public static OpenColorSpec Cyan { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(227,250,252).ToSrgb(),
                Color.FromRGB(197,246,250).ToSrgb(),
                Color.FromRGB(153,233,242).ToSrgb(),
                Color.FromRGB(102,217,232).ToSrgb(),
                Color.FromRGB(59,201,219).ToSrgb(),
                Color.FromRGB(34,184,207).ToSrgb(),
                Color.FromRGB(21,170,191).ToSrgb(),
                Color.FromRGB(16,152,173).ToSrgb(),
                Color.FromRGB(12,133,153).ToSrgb(),
                Color.FromRGB(11,114,133).ToSrgb(),
            }
        };

        public static OpenColorSpec Teal { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(230,252,245).ToSrgb(),
                Color.FromRGB(195,250,232).ToSrgb(),
                Color.FromRGB(150,242,215).ToSrgb(),
                Color.FromRGB(99,230,190).ToSrgb(),
                Color.FromRGB(56,217,169).ToSrgb(),
                Color.FromRGB(32,201,151).ToSrgb(),
                Color.FromRGB(18,184,134).ToSrgb(),
                Color.FromRGB(12,166,120).ToSrgb(),
                Color.FromRGB(9,146,104).ToSrgb(),
                Color.FromRGB(8,127,91).ToSrgb(),
            }
        };

        public static OpenColorSpec Green { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(235,251,238).ToSrgb(),
                Color.FromRGB(211,249,216).ToSrgb(),
                Color.FromRGB(178,242,187).ToSrgb(),
                Color.FromRGB(140,233,154).ToSrgb(),
                Color.FromRGB(105,219,124).ToSrgb(),
                Color.FromRGB(81,207,102).ToSrgb(),
                Color.FromRGB(64,192,87).ToSrgb(),
                Color.FromRGB(55,178,77).ToSrgb(),
                Color.FromRGB(47,158,68).ToSrgb(),
                Color.FromRGB(43,138,62).ToSrgb(),
            }
        };

        public static OpenColorSpec Lime { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(244,252,227).ToSrgb(),
                Color.FromRGB(233,250,200).ToSrgb(),
                Color.FromRGB(216,245,162).ToSrgb(),
                Color.FromRGB(192,235,117).ToSrgb(),
                Color.FromRGB(169,227,75).ToSrgb(),
                Color.FromRGB(148,216,45).ToSrgb(),
                Color.FromRGB(130,201,30).ToSrgb(),
                Color.FromRGB(116,184,22).ToSrgb(),
                Color.FromRGB(102,168,15).ToSrgb(),
                Color.FromRGB(92,148,13).ToSrgb(),
            }
        };

        public static OpenColorSpec Yellow { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(255,249,219).ToSrgb(),
                Color.FromRGB(255,243,191).ToSrgb(),
                Color.FromRGB(255,236,153).ToSrgb(),
                Color.FromRGB(255,224,102).ToSrgb(),
                Color.FromRGB(255,212,59).ToSrgb(),
                Color.FromRGB(252,196,25).ToSrgb(),
                Color.FromRGB(250,176,5).ToSrgb(),
                Color.FromRGB(245,159,0).ToSrgb(),
                Color.FromRGB(240,140,0).ToSrgb(),
                Color.FromRGB(230,119,0).ToSrgb(),
            }
        };

        public static OpenColorSpec Orange { get; } = new OpenColorSpec
        {
            Colors = new List<Color>
            {
                Color.FromRGB(255,244,230).ToSrgb(),
                Color.FromRGB(255,232,204).ToSrgb(),
                Color.FromRGB(255,216,168).ToSrgb(),
                Color.FromRGB(255,192,120).ToSrgb(),
                Color.FromRGB(255,169,77).ToSrgb(),
                Color.FromRGB(255,146,43).ToSrgb(),
                Color.FromRGB(253,126,20).ToSrgb(),
                Color.FromRGB(247,103,7).ToSrgb(),
                Color.FromRGB(232,89,12).ToSrgb(),
                Color.FromRGB(217,72,15).ToSrgb(),
            }
        };
    }
}
