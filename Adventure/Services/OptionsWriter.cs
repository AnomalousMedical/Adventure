using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RpgMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class OptionsWriter : IDisposable
    {
        private Options options;
        private JsonSerializer serializer;

        public OptionsWriter()
        {
            serializer = new JsonSerializer()
            {
                Formatting = Formatting.Indented,
            };
            serializer.Converters.Add(new StringEnumConverter());
        }

        public void Dispose()
        {
            Save();
        }

        public void Save()
        {
            //TODO: With how this works right now anything sent as an arg is also saved

            if (options == null) { return; }

            var outFile = GetFile();
            using var stream = new StreamWriter(File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
            serializer.Serialize(stream, options);
        }

        public Options Load()
        {
            var outFile = GetFile();

            if (!File.Exists(outFile))
            {
                options = new Options();
            }
            else
            {
                using var stream = new JsonTextReader(new StreamReader(File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read)));
                options = serializer.Deserialize<Options>(stream);
            }

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddCommandLine(Environment.GetCommandLineArgs());
            var envConfiguration = configBuilder.Build();
            envConfiguration.Bind(options);

            return options ?? new Options();
        }

        private String GetFile()
        {
            var outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anomalous Adventure");
            var outFile = Path.Combine(outDir, "options.json");

            Directory.CreateDirectory(outDir);

            return outFile;
        }
    }
}
