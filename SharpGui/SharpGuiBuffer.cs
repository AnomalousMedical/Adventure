using DiligentEngine;
using Engine;
using Engine.Platform;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public class SharpGuiBuffer
    {
        private readonly OSWindow osWindow;
        private readonly ILogger<SharpGuiBuffer> logger;
        SharpGuiVertex[] quadVerts;
        private uint currentQuad = 0;
        private uint maxNumberOfQuads;

        SharpGuiTextVertex[] textVerts;
        private uint currentText = 0;
        private uint maxNumberOfTextQuads;

        private float zStep;
        private float currentZ;

        public SharpGuiBuffer(OSWindow osWindow, ILogger<SharpGuiBuffer> logger, SharpGuiOptions options)
        {
            this.maxNumberOfQuads = options.MaxNumberOfQuads;
            this.maxNumberOfTextQuads = options.MaxNumberOfTextQuads;

            zStep = 1.0f / (float)(this.maxNumberOfQuads + this.maxNumberOfTextQuads);

            quadVerts = new SharpGuiVertex[maxNumberOfQuads * 4];
            textVerts = new SharpGuiTextVertex[maxNumberOfTextQuads * 4];

            this.osWindow = osWindow;
            this.logger = logger;
        }

        public void Begin()
        {
            currentZ = 1.0f - zStep;
            currentText = 0;
            currentQuad = 0;
            NumQuadIndices = 0;
            NumTextIndices = 0;
        }

        public void DrawQuad(int left, int top, int right, int bottom, Color color, float layer)
        {
            if (currentQuad >= quadVerts.Length)
            {
                logger.LogWarning($"Exceeded maximum number of quads '{quadVerts.Length / 4}'.");
                return;
            }

            float fleft = left / (float)osWindow.WindowWidth * 2.0f - 1.0f;
            float ftop = top / (float)osWindow.WindowHeight * -2.0f + 1.0f;
            float fright = right / (float)osWindow.WindowWidth * 2.0f - 1.0f;
            float fbottom = bottom / (float)osWindow.WindowHeight * -2.0f + 1.0f;

            quadVerts[currentQuad].pos = new Vector3(fleft, ftop, currentZ - layer);
            quadVerts[currentQuad + 1].pos = new Vector3(fright, ftop, currentZ - layer);
            quadVerts[currentQuad + 2].pos = new Vector3(fright, fbottom, currentZ - layer);
            quadVerts[currentQuad + 3].pos = new Vector3(fleft, fbottom, currentZ - layer);

            quadVerts[currentQuad].color = color;
            quadVerts[currentQuad + 1].color = color;
            quadVerts[currentQuad + 2].color = color;
            quadVerts[currentQuad + 3].color = color;

            currentQuad += 4;
            NumQuadIndices += 6;
            currentZ -= zStep;
        }

        public void DrawText(int x, int y, int right, Color color, String text, Font font, float layer)
        {
            if (text == null)
            {
                return;
            }

            ///This is closely related to <see cref="Font.MeasureText(string)"/>
            int xOffset = x;

            int smallestBearingY = font.SmallestBearingY;
            int yOffset = y - smallestBearingY;
            int tallestLineChar = 0;
            foreach (var c in text)
            {
                if (xOffset < right && font.TryGetGlyphInfo(c, out var glyphInfo))
                {
                    DrawTextQuad(xOffset + glyphInfo.bearingX, yOffset + glyphInfo.bearingY, glyphInfo.width, glyphInfo.height, ref color, ref glyphInfo.uvRect, layer);
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }

                if (c == '\n')
                {
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = x;
                }
            }
        }

        public void DrawText(int x, int y, int right, Color color, StringBuilder text, Font font, float layer)
        {
            ///This is closely related to <see cref="Font.MeasureText(StringBuilder)"/>
            int xOffset = x;

            int smallestBearingY = font.SmallestBearingY;
            int yOffset = y - smallestBearingY;
            int tallestLineChar = 0;
            var textLength = text.Length;
            for (int i = 0; i < textLength; ++i)
            {
                var c = text[i];
                if (xOffset < right && font.TryGetGlyphInfo(c, out var glyphInfo))
                {
                    DrawTextQuad(xOffset + glyphInfo.bearingX, yOffset + glyphInfo.bearingY, glyphInfo.width, glyphInfo.height, ref color, ref glyphInfo.uvRect, layer);
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset += fullAdvance;
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }

                if (c == '\n')
                {
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = x;
                }
            }
        }

        public void DrawTextReverse(int x, int y, int right, Color color, StringBuilder text, Font font, float layer)
        {
            ///This is closely related to <see cref="Font.MeasureText(StringBuilder)"/>
            int xOffset = right;

            int smallestBearingY = font.SmallestBearingY;
            int yOffset = y - smallestBearingY;
            int tallestLineChar = 0;
            for (int i = text.Length - 1; i > -1; --i)
            {
                var c = text[i];
                if (xOffset > x && font.TryGetGlyphInfo(c, out var glyphInfo))
                {
                    int fullAdvance = glyphInfo.advance + glyphInfo.bearingX;
                    xOffset -= fullAdvance;
                    DrawTextQuad(xOffset + glyphInfo.bearingX, yOffset + glyphInfo.bearingY, glyphInfo.width, glyphInfo.height, ref color, ref glyphInfo.uvRect, layer);
                    tallestLineChar = Math.Max(glyphInfo.height + glyphInfo.bearingY, tallestLineChar);
                }

                if (c == '\n')
                {
                    yOffset += tallestLineChar;
                    tallestLineChar = 0;
                    xOffset = x;
                }
            }
        }

        public void DrawTextQuad(int x, int y, int width, int height, ref Color color, ref GlyphRect uvRect, float layer)
        {
            if (currentText >= textVerts.Length)
            {
                logger.LogWarning($"Exceeded maximum number of text quads '{textVerts.Length / 4}'.");
                return;
            }

            float left = x / (float)osWindow.WindowWidth * 2.0f - 1.0f;
            float right = (x + width) / (float)osWindow.WindowWidth * 2.0f - 1.0f;
            float top = y / (float)osWindow.WindowHeight * -2.0f + 1.0f;
            float bottom = (y + height) / (float)osWindow.WindowHeight * -2.0f + 1.0f;

            textVerts[currentText].pos = new Vector3(left, top, currentZ - layer);
            textVerts[currentText + 1].pos = new Vector3(right, top, currentZ - layer);
            textVerts[currentText + 2].pos = new Vector3(right, bottom, currentZ - layer);
            textVerts[currentText + 3].pos = new Vector3(left, bottom, currentZ - layer);

            textVerts[currentText].color = color;
            textVerts[currentText + 1].color = color;
            textVerts[currentText + 2].color = color;
            textVerts[currentText + 3].color = color;

            textVerts[currentText].uv = new Vector2(uvRect.Left, uvRect.Top);
            textVerts[currentText + 1].uv = new Vector2(uvRect.Right, uvRect.Top);
            textVerts[currentText + 2].uv = new Vector2(uvRect.Right, uvRect.Bottom);
            textVerts[currentText + 3].uv = new Vector2(uvRect.Left, uvRect.Bottom);

            currentText += 4;
            NumTextIndices += 6;
            currentZ -= zStep;
        }

        public uint NumQuadIndices { get; private set; }

        public uint NumTextIndices { get; private set; }

        internal SharpGuiVertex[] QuadVerts => quadVerts;

        internal SharpGuiTextVertex[] TextVerts => textVerts;

        // vertices

        // -1, +1 --------------- +1, +1
        //    |  0               1   |
        //    |                      |
        //    |                      |
        //    |                      |
        //    |                      |
        //    |  3               2   |
        // -1, -1 --------------- +1, -1
    }
}
