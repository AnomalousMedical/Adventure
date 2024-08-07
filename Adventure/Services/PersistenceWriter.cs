﻿using Adventure.Assets.World;
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
        void DeleteFile(String saveFile);
        IAsyncEnumerable<SaveDataInfo> GetAllSaveData();
        IEnumerable<string> GetSaveFiles();
        void Load();
        void RemoveSaveBlock(object blocker);
        void Save();
        void SaveDefeated();
        void SaveGameOver(bool isGameOver);
        void SaveNewSchool();
    }

    class SaveDataInfo
    {
        public Persistence.GameState GameState { get; set; }

        public String FileName { get; set; }
    }

    class PersistenceWriter : IPersistenceWriter, IDisposable
    {
        private readonly ILogger<PersistenceWriter> logger;
        private readonly Persistence persistence;
        private readonly IGenesysModule genesysModule;
        private readonly ISeedProvider seedProvider;
        private readonly GameOptions options;
        private HashSet<object> saveBlockers = new HashSet<object>();

        public PersistenceWriter(ILogger<PersistenceWriter> logger, Persistence persistence, IGenesysModule genesysModule, ISeedProvider seedProvider, GameOptions options)
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

        public void SaveGameOver(bool isGameOver)
        {
            var savedState = LoadData();
            savedState.Party.GameOver = isGameOver;
            SaveData(savedState);
        }

        public void SaveNewSchool()
        {
            var savedState = LoadData();
            if (savedState.Party.OldSchool)
            {
                savedState.Party.OldSchool = false;
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
                logger.LogInformation("File '{outFile}' does not exist. Creating new save.", outFile);
            }
            else
            {
                try
                {
                    logger.LogInformation($"Loading save from '{outFile}'.");
                    using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var save = JsonSerializer.Deserialize<Persistence.GameState>(stream, PersistenceWriterSourceGenerationContext.Default.GameState);
                    UpdateSave(save);
                    return save;
                }
                catch(Exception ex)
                {
                    var oldOutFile = outFile;
                    options.CurrentSave = null;
                    outFile = GetSaveFile();
                    logger.LogError("{exception} loading save from '{oldOutFile}'. Creating new save '{outFile}'.", ex.GetType(), oldOutFile, outFile);
                }
            }

            return genesysModule.SeedWorld(seedProvider.GetSeed());
        }

        public void DeleteFile(String saveFile)
        {
            var saveDir = Path.GetFullPath(GetSaveDirectory());
            saveFile = Path.GetFullPath(Path.Combine(saveDir, saveFile));

            if (saveFile.StartsWith(saveDir) && File.Exists(saveFile))
            {
                File.Delete(saveFile);
            }
        }

        private void SaveData(Persistence.GameState state)
        {
            state.SaveTime = DateTime.Now;
            var outFile = GetSaveFile();
            using var stream = File.Open(outFile, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            JsonSerializer.Serialize(stream, state, PersistenceWriterSourceGenerationContext.Default.GameState);
            logger.LogInformation($"Wrote save to '{outFile}'.");
        }

        private String GetSaveFile()
        {
            //This can happen if a user erases the current file, just make up a new name and use it
            if(options.CurrentSave == null)
            {
                options.CurrentSave = CreateSaveFileName();
            }

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

        public async IAsyncEnumerable<SaveDataInfo> GetAllSaveData()
        {
            var outDir = GetSaveDirectory();

            foreach (var file in GetSaveFiles())
            {
                SaveDataInfo info = null;
                try
                {
                    var outFile = Path.Combine(outDir, Path.GetFileName(file));
                    using var stream = File.Open(outFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var save = await JsonSerializer.DeserializeAsync<Persistence.GameState>(stream, PersistenceWriterSourceGenerationContext.Default.GameState);
                    UpdateSave(save);
                    info = new SaveDataInfo()
                    {
                        GameState = save,
                        FileName = file,
                    };
                }
                catch(Exception ex)
                {
                    logger.LogError("{exception} loading save from '{file}'. This save will be skipped.", ex.GetType(), file);
                }
                if(info != null)
                {
                    yield return info;
                }
            }
        }

        private void UpdateSave(Persistence.GameState state)
        {
            if(state.GoldPiles == null)
            {
                state.GoldPiles = new Persistence.PersistenceEntry<GoldPile.GoldPilePersistenceData>();
            }
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
