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
    class GameOptionsWriter : IDisposable
    {
        private GameOptions options;

        public GameOptionsWriter()
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
            JsonSerializer.Serialize(stream, options, GameOptionsSourceGenerationContext.Default.GameOptions);
        }

        public GameOptions Load()
        {
            var outFile = GetFile();

            if (!File.Exists(outFile))
            {
                options = new GameOptions();
            }
            else
            {
                using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                options = JsonSerializer.Deserialize<GameOptions>(stream, GameOptionsSourceGenerationContext.Default.GameOptions);
            }

            return options ?? new GameOptions();
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
