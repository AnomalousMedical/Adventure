﻿using Engine.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiligentEngine
{
    public class ShaderLoader<T> : ShaderLoader
    {
        public ShaderLoader(IResourceProvider<ShaderLoader<T>> resourceProvider) : base(resourceProvider)
        {
        }
    }

    public class ShaderLoader
    {
        private readonly char[] Include = new char[] { '#', 'i', 'n', 'c', 'l', 'u', 'd', 'e' };
        private readonly IResourceProvider resourceProvider;

        public ShaderLoader(IResourceProvider resourceProvider)
        {
            this.resourceProvider = resourceProvider;
        }

        private String GetIncludePath(String original, String include)
        {
            var dir = Path.GetDirectoryName(original);
            var combined = Path.Combine(dir, include);
            return combined;
        }

        public String LoadShader(Dictionary<String, String> variables, String file, params String[] additionalIncludeDirs)
        {
            return LoadShaderInclude(variables, file, additionalIncludeDirs);
        }

        public String LoadShader(String file, params String[] additionalIncludeDirs)
        {
            return LoadShaderInclude(file, additionalIncludeDirs);
        }

        public String LoadShaderInclude(Dictionary<String, String> variables, String file, IEnumerable<String> additionalIncludeDirs)
        {
            var shaderString = LoadShaderInclude(file, additionalIncludeDirs);
            return ReplaceVariables(variables, new StringBuilder(shaderString)).ToString();
        }

        public String LoadShaderInclude(String file, IEnumerable<String> additionalIncludeDirs)
        {
            using var stream = resourceProvider.openFile(file);
            return LoadShader(stream,
                getIncludeContent: s =>
                {
                    var incPath = GetIncludePath(file, s);
                    if (!resourceProvider.fileExists(incPath))
                    {
                        if(additionalIncludeDirs == null)
                        {
                            throw new FileNotFoundException($"Cannot find file '{incPath}' Full Path '{resourceProvider.getFullFilePath(incPath)}'");
                        }
                        else
                        {
                            foreach(var inc in additionalIncludeDirs)
                            {
                                incPath = Path.Combine(inc, s);
                                if (resourceProvider.fileExists(incPath))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    return LoadShaderInclude(incPath, additionalIncludeDirs);
                });
        }

        public String LoadShader(Dictionary<String, String> variables, Stream stream, Func<String, String> getIncludeContent)
        {
            var shaderString = LoadShader(stream, getIncludeContent);
            return ReplaceVariables(variables, new StringBuilder(shaderString)).ToString();
        }

        public String LoadShader(Stream stream, Func<String, String> getIncludeContent)
        {
            StringBuilder sb = new StringBuilder();
            using var reader = new StreamReader(stream);
            var line = reader.ReadLine();
            while (line != null)
            {
                int firstNonWhitespace;
                for (firstNonWhitespace = 0; firstNonWhitespace < line.Length; ++firstNonWhitespace)
                {
                    if (!Char.IsWhiteSpace(line[firstNonWhitespace]))
                    {
                        break;
                    }
                }

                bool hasInclude = HasInclude(line, firstNonWhitespace);

                if (hasInclude)
                {
                    int firstQuote = line.IndexOf('"', firstNonWhitespace + Include.Length);
                    if (firstQuote == -1)
                    {
                        throw new InvalidOperationException($"Cannot find first quote to include path for line '{line}'");
                    }

                    int nextQuoteStart = firstQuote + 1;
                    int secondQuote = -1;
                    if (nextQuoteStart < line.Length)
                    {
                        secondQuote = line.IndexOf('"', nextQuoteStart);
                    }
                    if (secondQuote == -1)
                    {
                        throw new InvalidOperationException($"Cannot find second quote to include path for line '{line}'");
                    }

                    var file = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                    var newLines = getIncludeContent(file);
                    sb.AppendLine(newLines);
                }
                else
                {
                    sb.AppendLine(line);
                }
                line = reader.ReadLine();
            }
            return sb.ToString();
        }

        private bool HasInclude(string line, int firstNonWhitespace)
        {
            var lineSizeCompare = line.Length - 1;
            bool hasInclude = false;
            for (int i = 0; i < Include.Length; ++i)
            {
                var current = i + firstNonWhitespace;
                if (current > lineSizeCompare)
                {
                    break;
                }
                else if (line[current] != Include[i])
                {
                    break;
                }
                else if (i == Include.Length - 1)
                {
                    hasInclude = true;
                }
            }

            return hasInclude;
        }

        private static StringBuilder ReplaceVariables(Dictionary<string, string> variables, StringBuilder shaderString)
        {
            //TODO: Replace this with good variable replacement algo from Edity McEditface
            foreach (var i in variables)
            {
                shaderString = shaderString.Replace($"$$({i.Key})", i.Value);
            }

            return shaderString;
        }
    }
}
