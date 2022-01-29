﻿using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Adventure.Services
{

    class Persistence
    {
        public PersistenceEntry<BattleTrigger.PersistenceData> BattleTriggers { get; } = new PersistenceEntry<BattleTrigger.PersistenceData>();

        public PersistenceEntry<TreasureTrigger.PersistenceData> TreasureTriggers { get; } = new PersistenceEntry<TreasureTrigger.PersistenceData>();

        public LevelData Level { get; } = new LevelData();

        public PlayerData Player { get; } = new PlayerData();

        public TimeData Time { get; } = new TimeData();

        public WorldData World { get; set; } = new WorldData();

        public PartyData Party { get; set; } = new PartyData();

        public class PersistenceEntry<T>
                where T : struct
        {
            public Dictionary<int, Dictionary<int, T>> Entries => entryDictionary;

            private Dictionary<int, Dictionary<int, T>> entryDictionary = new Dictionary<int, Dictionary<int, T>>();

            public T GetData(int level, int key)
            {
                if (entryDictionary.TryGetValue(level, out var levelData))
                {
                    if (levelData.TryGetValue(key, out var val))
                    {
                        return (T)val;
                    }
                }

                return default(T);
            }

            public void SetData(int level, int key, T value)
            {
                Dictionary<int, T> levelData;
                if (!entryDictionary.TryGetValue(level, out levelData))
                {
                    levelData = new Dictionary<int, T>();
                    entryDictionary[level] = levelData;
                }
                levelData[key] = value;
            }

            internal void ClearData()
            {
                entryDictionary.Clear();
            }
        }

        public class LevelData
        {
            public int CurrentLevelIndex { get; set; }
        }

        public class PlayerData
        {
            public Vector3? Position { get; set; }

            public int? RespawnLevel { get; set; }

            public Vector3? RespawnPosition { get; set; }
        }

        public class TimeData
        {
            public long? Current { get; set; }
        }

        public class WorldData
        {
            public int Seed { get; set; }
        }

        public class CharacterData
        {
            public CharacterSheet CharacterSheet { get; set; }

            public String PlayerSprite { get; set; }

            public String PrimaryHandAsset { get; set; }

            public String SecondaryHandAsset { get; set; }

            public IEnumerable<String> Spells { get; set; }
        }

        public class PartyData
        {
            public List<CharacterData> Members { get; set; } = new List<CharacterData>();
        }
    }
}