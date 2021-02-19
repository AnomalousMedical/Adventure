﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public class Font
    {
        public Dictionary<char, uint> CharMap { get; internal set; }
        public Dictionary<uint, GlyphInfo> GlyphInfo { get; internal set; }

        public bool TryGetGlyphInfo(char c, out GlyphInfo glyphInfo)
        {
            glyphInfo = null;
            return CharMap.TryGetValue(c, out var cMap) && GlyphInfo.TryGetValue(cMap, out glyphInfo);
        }

        public IntSize2 MeasureText(String text)
        {
            int width = 0;
            int height = 0;
            foreach (var c in text)
            {
                uint charCode = c;
                if (TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = (int)glyphInfo.advance + (int)glyphInfo.bearingX;
                    width += fullAdvance;
                    height = Math.Max(height, (int)glyphInfo.height + (int)glyphInfo.bearingY);
                }
            }
            return new IntSize2(width, height);
        }
    }
}
