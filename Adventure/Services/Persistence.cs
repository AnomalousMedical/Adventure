using Adventure.Items;
using Engine;
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

        public ZoneData Zone { get; } = new ZoneData();

        public PlayerData Player { get; } = new PlayerData();

        public TimeData Time { get; } = new TimeData();

        public WorldData World { get; set; } = new WorldData();

        public PartyData Party { get; set; } = new PartyData();

        public Inventory Storage { get; set; } = new Inventory()
        {
            Size = 10000
        };

        public class PersistenceEntry<T>
                where T : struct
        {
            public Dictionary<int, Dictionary<int, T>> Entries => entryDictionary;

            private Dictionary<int, Dictionary<int, T>> entryDictionary = new Dictionary<int, Dictionary<int, T>>();

            public T GetData(int zone, int key)
            {
                if (entryDictionary.TryGetValue(zone, out var levelData))
                {
                    if (levelData.TryGetValue(key, out var val))
                    {
                        return (T)val;
                    }
                }

                return default(T);
            }

            public void SetData(int zone, int key, T value)
            {
                Dictionary<int, T> levelData;
                if (!entryDictionary.TryGetValue(zone, out levelData))
                {
                    levelData = new Dictionary<int, T>();
                    entryDictionary[zone] = levelData;
                }
                levelData[key] = value;
            }

            internal void ClearData()
            {
                entryDictionary.Clear();
            }
        }

        public class ZoneData
        {
            public int CurrentIndex { get; set; }
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

            public Inventory Inventory { get; set; } = new Inventory();

            public String PlayerSprite { get; set; }

            public String PrimaryHandAsset { get; set; }

            public String SecondaryHandAsset { get; set; }

            public IEnumerable<String> Spells { get; set; }
        }

        public class PartyData
        {
            public List<CharacterData> Members { get; set; } = new List<CharacterData>();
            public long Gold { get; set; }
        }
    }
}
