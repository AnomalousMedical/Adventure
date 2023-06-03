using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Adventure.Services
{
    interface IPersistenceWriter
    {
        void AddSaveBlock(object blocker);
        string CreateSaveFileName();
        IEnumerable<string> GetSaveFiles();
        void Load();
        void RemoveSaveBlock(object blocker);
        void Save();
        void SaveDefeated();
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

            SaveData(persistence.Current);
        }

        public void SaveDefeated()
        {
            var savedState = LoadData();
            if (savedState.Party.Undefeated)
            {
                savedState.Party.Undefeated = false;
                SaveData(savedState);
            }
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
            persistence.Current = LoadData();
        }

        private Persistence.GameState LoadData()
        {
            var outFile = GetSaveFile();

            if (!File.Exists(outFile))
            {
                logger.LogInformation($"Creating new save.");
                return genesysModule.SeedWorld(seedProvider.GetSeed());
            }
            else
            {
                logger.LogInformation($"Loading save from '{outFile}'.");
                using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                return JsonSerializer.Deserialize<Persistence.GameState>(stream, jsonSerializerOptions);
            }
        }

        private void SaveData(Persistence.GameState state)
        {
            var outFile = GetSaveFile();
            using var stream = File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            JsonSerializer.Serialize(stream, state, jsonSerializerOptions);
            logger.LogInformation($"Wrote save to '{outFile}'.");
        }

        private String GetSaveFile()
        {
            var outDir = GetSaveDirectory();
            var outFile = Path.Combine(outDir, Path.GetFileName(options.CurrentSave));

            Directory.CreateDirectory(outDir);

            return outFile;
        }

        public IEnumerable<string> GetSaveFiles()
        {
            var outDir = GetSaveDirectory();

            Directory.CreateDirectory(outDir);

            return Directory.GetFiles(outDir, "save*.json", SearchOption.TopDirectoryOnly).Select(i => Path.GetFileName(i));
        }

        private string GetSaveDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anomalous Adventure");
        }

        public string CreateSaveFileName()
        {
            return $"save-{Guid.NewGuid()}.json";
        }
    }
}
