using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiligentEngineGenerator
{
    class LineReader
    {
        public static IEnumerable<String> ReadLines(IEnumerable<String> inputLines, int startLine, int endLine, IEnumerable<int> skipLines = null)
        {
            if (skipLines == null)
            {
                skipLines = new List<int>(0);
            }

            var commentBuilder = new StringBuilder();
            var beforeStart = startLine - 1;
            var lines = inputLines.Skip(startLine - 1).Take(endLine - beforeStart);
            CodeStruct code = new CodeStruct();
            ICodeStructParserState currentState = new StartStructParseState();

            var lineNumber = startLine;
            foreach (var line in lines.Select(l => l.Replace(";", "")))
            {
                try
                {
                    if (!skipLines.Contains(lineNumber))
                    {
                        Console.WriteLine($"{lineNumber}: {line}");
                        yield return line;
                    }

                }
                finally
                {
                    ++lineNumber;
                }
            }
        }

        public record LineBlock(int Start, int End);

        public static LineBlock FindBlock(IEnumerable<String> inputLines, string startContains, string endContains)
        {
            bool lookStart = true;
            var start = -1;
            var end = -1;

            var lineNumber = 0;
            foreach (var line in inputLines)
            {
                if (lookStart)
                {
                    if (line.Contains(startContains))
                    {
                        start = lineNumber;
                        lookStart = false;
                    }
                }
                else
                {
                    if (line.Contains(endContains))
                    {
                        end = lineNumber;
                        break;
                    }
                }
                ++lineNumber;
            }

            if (start == -1)
            {
                throw new InvalidOperationException($"Cannot find start line that contains the string '{startContains}'.");
            }

            if(end == -1)
            {
                throw new InvalidOperationException($"Cannot find end line that contains the string '{endContains}'.");
            }

            return new LineBlock(start, end);
        }
    }
}
