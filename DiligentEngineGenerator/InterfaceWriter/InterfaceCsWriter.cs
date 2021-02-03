﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiligentEngineGenerator
{
    class InterfaceCsWriter : ICodeRenderer
    {
        private readonly CodeInterface code;

        public InterfaceCsWriter(CodeInterface code)
        {
            this.code = code;
        }

        public void Render(TextWriter writer, CodeRendererContext context)
        {
            writer.WriteLine(
$@"using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Engine;

{DiligentTypeMapper.Usings}

namespace DiligentEngine
{{");
            WriteClassDefinitionAndConstructors(code, writer);

            //Public interface
            foreach (var item in code.Methods)
            {
                writer.Write($"        public {GetCSharpType(item.ReturnType, context)} {item.Name}(");

                var sep = "";

                foreach (var arg in item.Args)
                {
                    writer.Write($"{sep}{GetCSharpType(arg.Type, context)} {arg.Name}");
                    sep = ", ";
                }

                writer.WriteLine(")");
                writer.WriteLine("        {");

                if (item.ReturnType != "void")
                {
                    writer.WriteLine(
@$"            return new {GetCSharpType(item.ReturnType, context)}({code.Name}_{item.Name}(
                this.objPtr");
                }
                else
                {
                    writer.WriteLine(
@$"            {code.Name}_{item.Name}(
                this.objPtr");
                }

                foreach (var arg in item.Args)
                {
                    if (context.CodeTypeInfo.Structs.TryGetValue(arg.LookupType, out var structInfo))
                    {
                        var argWriter = new StructCsFunctionCallArgsWriter(arg.Name, structInfo, "                ");
                        argWriter.Render(writer, context);
                    }
                    else
                    {
                        writer.Write($"                , {arg.Name}");
                        if(context.CodeTypeInfo.Interfaces.ContainsKey(arg.LookupType) || context.CodeTypeInfo.Structs.ContainsKey(arg.LookupType))
                        {
                            writer.WriteLine(".objPtr");
                        }
                        else
                        {
                            writer.WriteLine();
                        }
                    }
                }

                if (item.ReturnType != "void")
                {
                    writer.WriteLine($"            ));");
                }
                else
                {
                    writer.WriteLine($"            );");
                }

                writer.WriteLine("        }");
            }

            writer.WriteLine();
            writer.WriteLine();

            //PInvoke
            foreach (var item in code.Methods)
            {
                writer.WriteLine("        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]");
                writer.WriteLine(
@$"        private static extern {GetPInvokeType(item, context)} {code.Name}_{item.Name}(
            IntPtr objPtr");

                foreach (var arg in item.Args)
                {
                    if (context.CodeTypeInfo.Structs.TryGetValue(arg.LookupType, out var structInfo))
                    {
                        var argWriter = new StructCsFunctionSignatureArgsWriter(structInfo, "            ");
                        argWriter.Render(writer, context);
                    }
                    else
                    {
                        writer.WriteLine($"            , {GetPInvokeType(arg, context)} {arg.Name}");
                    }
                }

                writer.WriteLine($"        );");
            }

            writer.WriteLine("    }");

            writer.WriteLine("}");
        }

        private void WriteClassDefinitionAndConstructors(CodeInterface code, TextWriter writer)
        {
            if (code.BaseType != null)
            {
                writer.WriteLine($"    public partial class {code.Name} : {code.BaseType}");

                writer.WriteLine(
$@"    {{
        public {code.Name}(IntPtr objPtr)
            : base(objPtr)
        {{

        }}");
            }
            else
            {
                writer.WriteLine($"    public partial class {code.Name}");

                writer.WriteLine(
$@"    {{
        internal protected IntPtr objPtr;

        public IntPtr ObjPtr => objPtr;

        public {code.Name}(IntPtr objPtr)
        {{
            this.objPtr = objPtr;
        }}");
            }
        }

        private static String GetCSharpType(String type, CodeRendererContext context)
        {
            switch (type)
            {
                case "Char*":
                    return "String";
            }

            return type.Replace("*", "");
        }

        private static String GetPInvokeType(InterfaceMethodArgument arg, CodeRendererContext context)
        {
            if (context.CodeTypeInfo.Interfaces.ContainsKey(arg.LookupType))
            {
                return "IntPtr";
            }

            switch (arg.Type)
            {
                case "Char*":
                    return "String";
                case "Color":
                    return "Color";
            }

            if (arg.IsPtr)
            {
                return "IntPtr";
            }

            return arg.Type;
        }

        private static String GetPInvokeType(InterfaceMethod method, CodeRendererContext context)
        {
            if (context.CodeTypeInfo.Interfaces.ContainsKey(method.LookupReturnType))
            {
                return "IntPtr";
            }

            switch (method.ReturnType)
            {
                case "Char*":
                    return "String";
            }

            if (method.IsPtr)
            {
                return "IntPtr";
            }

            return method.ReturnType;
        }
    }
}