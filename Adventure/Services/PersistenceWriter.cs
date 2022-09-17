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
    interface IPersistenceWriter
    {
        void AddSaveBlock(object blocker);
        void Load();
        void RemoveSaveBlock(object blocker);
        void Save();
    }

    class PersistenceWriter : IPersistenceWriter, IDisposable
    {
        private readonly ILogger<PersistenceWriter> logger;
        private readonly Persistence persistence;
        private readonly IGenesysModule genesysModule;
        private readonly ISeedProvider seedProvider;
        private readonly Options options;
        private HashSet<object> saveBlockers = new HashSet<object>();
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = true,
        };

        public PersistenceWriter(ILogger<PersistenceWriter> logger, Persistence persistence, IGenesysModule genesysModule, ISeedProvider seedProvider, Options options)
        {
            this.logger = logger;
            this.persistence = persistence;
            this.genesysModule = genesysModule;
            this.seedProvider = seedProvider;
            this.options = options;
        }

        public void Dispose()
        {
            Save();
        }

        public void Save()
        {
            if (persistence == null) { return; }

            if (saveBlockers.Count > 0)
            {
                logger.LogInformation($"Save is currently disabled. Skipping save. Reasons: {String.Concat(saveBlockers.Select(i => i?.ToString()))}");
                return;
            }

            var outFile = GetSaveFile();
            using var stream = File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            JsonSerializer.Serialize(stream, persistence.Current, jsonSerializerOptions);
            logger.LogInformation($"Wrote save to '{outFile}'.");
        }

        public void AddSaveBlock(Object blocker)
        {
            saveBlockers.Add(blocker);
        }

        public void RemoveSaveBlock(Object blocker)
        {
            saveBlockers.Remove(blocker);
        }

        public void Load()
        {
            var outFile = GetSaveFile();

            if (!File.Exists(outFile))
            {
                logger.LogInformation($"Creating new save.");
                persistence.Current = genesysModule.SeedWorld(seedProvider.GetSeed());
            }
            else
            {
                logger.LogInformation($"Loading save from '{outFile}'.");
                using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                persistence.Current = JsonSerializer.Deserialize<Persistence.GameState>(stream, jsonSerializerOptions);
            }
        }

        private String GetSaveFile()
        {
            var outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anomalous Adventure");
            var outFile = Path.Combine(outDir, Path.GetFileName(options.CurrentSave));

            Directory.CreateDirectory(outDir);

            return outFile;
        }
    }
}
