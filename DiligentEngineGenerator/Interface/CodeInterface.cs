﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DiligentEngineGenerator
{
    class CodeInterface
    {
        public String BaseType { get; set; }

        public String Name { get; set; }

        public IEnumerable<String> Comment { get; set; }

        public List<InterfaceMethod> Methods { get; set; } = new List<InterfaceMethod>();

        public static CodeInterface Find(String file, string startLineContains, string endLineContains)
        {
            //This reads the file twice, but perf is not the primary concern here
            using var reader = new StreamReader(File.OpenRead(file));
            var block = LineReader.FindBlock(reader.ReadLines(), startLineContains, endLineContains);

            return Find(file, block.Start, block.End);
        }

        public static CodeInterface Find(String file, int startLine, int endLine, IEnumerable<int> skipLines = null)
        {
            var comments = new List<String>();
            using var reader = new StreamReader(File.OpenRead(file));
            var lines = LineReader.ReadLines(reader.ReadLines(), startLine, endLine, skipLines);
            CodeInterface code = new CodeInterface();
            ICodeInterfaceParserState currentState = new StartInterfaceParseState();

            foreach (var line in lines.Select(l => l.Replace(";", "")))
            {
                if (!String.IsNullOrWhiteSpace(line) && !CommentParser.Find(line, comments))
                {
                    var parsed = CommentParser.RemoveComments(line);
                    currentState = currentState.Parse(parsed, comments, code);
                    comments = new List<string>();
                    if (currentState == null)
                    {
                        break;
                    }
                }
            }

            return code;
        }
    }

    class InterfaceMethod
    {
        public IEnumerable<String> Comment { get; set; }

        public String Name { get; set; }

        public string ReturnType { get; set; }

        public string LookupReturnType => ReturnType.Replace("*", "").Trim();

        public bool IsRef { get; set; }

        public bool IsConst { get; set; }

        public bool IsPtr { get; set; }

        public bool IsPtrToPtr { get; set; }

        public List<InterfaceMethodArgument> Args { get; set; } = new List<InterfaceMethodArgument>();
        public bool PoolManagedObject { get; set; } = false;

        /// <summary>
        /// Set this to true if the value being returned should be an AutoPtr to indicate that it should be disposed.
        /// </summary>
        public bool ReturnAsAutoPtr { get; set; }

        /// <summary>
        /// Set this to true if you need to add a ref to this pointer. Default is false, which assumes the unmanaged side did it.
        /// </summary>
        public bool AddRefToAutoPtr { get; set; }
    }

    class InterfaceMethodArgument
    {
        public String Name { get; set; }

        public string Type { get; set; }

        public string LookupType => Type.Replace("*", "").Trim();

        public bool IsRef { get; set; }

        public bool IsConst { get; set; }

        public bool IsPtr { get; set; }

        public bool IsPtrToPtr { get; set; }

        public String CppPrefix { get; set; }

        public bool MakeReturnVal { get; set; }

        public bool IsArray { get; set; }
    }
}
