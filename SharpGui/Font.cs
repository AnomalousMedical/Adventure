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
        private Dictionary<char, uint> CharMap;
        private Dictionary<uint, GlyphInfo> GlyphInfo;

        public Font(Dictionary<char, uint> charMap, Dictionary<uint, GlyphInfo> glyphInfo, uint substituteCodePoint, GlyphInfo substituteCodePointGlyphInfo, uint fontTexture)
        {
            this.CharMap = charMap;
            this.GlyphInfo = glyphInfo;
            this.SubstituteCodePoint = substituteCodePoint;
            this.SubstituteCodePointGlyphInfo = substituteCodePointGlyphInfo;
            this.FontTexture = fontTexture;
        }

        public uint FontTexture { get; internal set; }

        public uint SubstituteCodePoint { get; private set; }

        public GlyphInfo SubstituteCodePointGlyphInfo { get; private set; }

        public int SubstituteGlyphInfoSize => SubstituteCodePointGlyphInfo.height + SubstituteCodePointGlyphInfo.bearingY - SmallestBearingY;

        const String TallEnglishLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        int? smallestBearingY;
        public int SmallestBearingY
        {
            get
            {
                if (smallestBearingY == null)
                {
                    smallestBearingY = int.MaxValue;
                    foreach (var c in TallEnglishLetters)
                    {
                        if (TryGetGlyphInfo(c, out var g))
                        {
                            smallestBearingY = Math.Min(SmallestBearingY, g.bearingY);
                        }
                    }
                }
                return smallestBearingY.Value;
            }
        }

        public bool TryGetGlyphInfo(char c, out GlyphInfo glyphInfo)
        {
            glyphInfo = null;
            return CharMap.TryGetValue(c, out var cMap) && GlyphInfo.TryGetValue(cMap, out glyphInfo);
        }

        public IntSize2 MeasureText(String text)
        {
            if (text == null)
            {
                return new IntSize2(0, 0);
            }

            ///This is closely related to <see cref="SharpGuiBuffer.DrawText(int, int, Color, string, Font)"/>
            int xOffset = 0;
            int yOffset = 0;
            int widest = 0;
            int tallestLineChar = 0;
            foreach (var c in text)
            {
                if (TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }
                if (c == '\n')
                {
                    widest = Math.Max(widest, xOffset);
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = 0;
                }
            }

            widest = Math.Max(widest, xOffset);

            return new IntSize2(widest, yOffset + tallestLineChar - SmallestBearingY);
        }

        public IntSize2 MeasureText(StringBuilder text)
        {
            ///This is closely related to <see cref="SharpGuiBuffer.DrawText(int, int, Color, StringBuilder, Font)"/>
            int xOffset = 0;
            int yOffset = 0;
            int widest = 0;
            int tallestLineChar = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }
                if (c == '\n')
                {
                    widest = Math.Max(widest, xOffset);
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = 0;
                }
            }

            widest = Math.Max(widest, xOffset);

            return new IntSize2(widest, yOffset + tallestLineChar - SmallestBearingY);
        }

        public IntVector2 FindCursorPos(StringBuilder text, out int tallestLineChar)
        {
            ///This is closely related to <see cref="SharpGuiBuffer.DrawText(int, int, Color, StringBuilder, Font)"/>
            int xOffset = 0;
            int yOffset = 0;
            int widest = 0;
            tallestLineChar = SubstituteGlyphInfoSize + SmallestBearingY;
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }
                if (c == '\n')
                {
                    widest = Math.Max(widest, xOffset);
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = 0;
                }
            }

            widest = Math.Max(widest, xOffset);

            return new IntVector2(widest, yOffset - SmallestBearingY);
        }

        public bool IsTextWider(StringBuilder text, int width)
        {
            ///This is closely related to <see cref="SharpGuiBuffer.DrawText(int, int, Color, StringBuilder, Font)"/>
            int xOffset = 0;
            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                }

                //Wider, done, return true
                if (xOffset > width)
                {
                    return true;
                }

                if (c == '\n')
                {
                    xOffset = 0;
                }
            }

            //Not wider
            return false;
        }
    }
}
