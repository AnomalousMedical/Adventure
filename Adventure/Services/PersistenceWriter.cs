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
    interface IPersistenceWriter
    {
        void AddSaveBlock(object blocker);
        Persistence Load(Func<Persistence.GameState> createNewWorld);
        void RemoveSaveBlock(object blocker);
        void Save();
    }

    class PersistenceWriter : IPersistenceWriter, IDisposable
    {
        private Persistence persistence;
        private readonly ILogger<PersistenceWriter> logger;
        private JsonSerializer serializer;
        private HashSet<object> saveBlockers = new HashSet<object>();

        public PersistenceWriter(ILogger<PersistenceWriter> logger)
        {
            this.logger = logger;
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
            if (persistence == null) { return; }

            if (saveBlockers.Count > 0)
            {
                logger.LogInformation($"Save is currently disabled. Skipping save. Reasons: {String.Concat(saveBlockers.Select(i => i?.ToString()))}");
                return;
            }

            var outFile = GetSaveFile();
            using var stream = new StreamWriter(File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None));
            serializer.Serialize(stream, persistence);
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

        public Persistence Load(Func<Persistence.GameState> createNewWorld)
        {
            var outFile = GetSaveFile();

            if (!File.Exists(outFile))
            {
                logger.LogInformation($"Creating new save.");
                persistence = new Persistence()
                {
                    Current = createNewWorld()
                };
            }
            else
            {
                logger.LogInformation($"Loading save from '{outFile}'.");
                using var stream = new JsonTextReader(new StreamReader(File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read)));
                persistence = serializer.Deserialize<Persistence>(stream);
            }

            return persistence;
        }

        private String GetSaveFile()
        {
            var outDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Anomalous Adventure");
            var outFile = Path.Combine(outDir, "save.json");

            Directory.CreateDirectory(outDir);

            return outFile;
        }
    }
}
