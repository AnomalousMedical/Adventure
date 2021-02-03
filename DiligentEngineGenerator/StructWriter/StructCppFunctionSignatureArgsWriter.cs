﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiligentEngineGenerator
{
    class StructCppFunctionSignatureArgsWriter : ICodeRenderer
    {
        private readonly string argName;
        private CodeStruct code;
        private readonly string tabs;

        public StructCppFunctionSignatureArgsWriter(String argName, CodeStruct code, String tabs)
        {
            this.argName = argName;
            this.code = code;
            this.tabs = tabs;
        }

        public void Render(TextWriter writer, CodeRendererContext context)
        {
            foreach (var item in code.Properties)
            {
                writer.WriteLine($"{tabs}, {item.Type} {argName}_{item.Name}");
            }
        }
    }
}
