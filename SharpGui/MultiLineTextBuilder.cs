﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGui
{
    public static class MultiLineTextBuilder
    {
        public static String CreateMultiLineString(String contents, int width, ISharpGui sharpGui, Font font = null)
        {
            if(contents == null)
            {
                return null;
            }

            var lineWidth = 0;
            var sb = new StringBuilder(500);

            var addSpace = false;

            foreach (var word in FindWords(contents))
            {
                if(word == "\n")
                {
                    sb.Append('\n');
                    lineWidth = 0;
                    addSpace = false;
                }
                else
                {
                    var wordWidth = sharpGui.MeasureText(word, font).Width;
                    if (lineWidth + wordWidth >= width)
                    {
                        sb.Append('\n');
                        lineWidth = 0;
                        addSpace = false;
                    }

                    if (addSpace)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(word);
                    lineWidth += wordWidth;
                    addSpace = true;
                }
            }

            return sb.ToString();
        }

        private static IEnumerable<String> FindWords(string contents)
        {
            var contentsLength = contents.Length;
            int wordStart = 0;
            for (int textPosition = 0; textPosition < contentsLength; ++textPosition)
            {
                var i = contents[textPosition];
                if (char.IsWhiteSpace(i))
                {
                    var word = contents.Substring(wordStart, textPosition - wordStart);
                    wordStart = textPosition + 1;
                    yield return word;
                    if (i == '\n')
                    {
                        yield return "\n";
                    }
                }
            }

            if (wordStart < contentsLength)
            {
                yield return contents.Substring(wordStart, contentsLength - wordStart);
            }
        }
    }
}
