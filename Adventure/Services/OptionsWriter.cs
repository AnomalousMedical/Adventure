using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        }

        public void Dispose()
        {
            Save();
        }

        public void Save()
        {
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

            return options;
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
