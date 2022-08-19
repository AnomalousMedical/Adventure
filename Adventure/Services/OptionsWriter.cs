using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RpgMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class OptionsWriter : IDisposable
    {
        private Options options;
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true,
        };

        public OptionsWriter()
        {
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
            using var stream = File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            JsonSerializer.Serialize(stream, options, jsonSerializerOptions);
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
                using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                options = JsonSerializer.Deserialize<Options>(stream, jsonSerializerOptions);
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
