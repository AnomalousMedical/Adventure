using Adventure.Exploration;
using Adventure.Items;
using DiligentEngine;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Adventure.Services
{
    [JsonConverter(typeof(JsonStringEnumConverter<PlotItems>))]
    enum PlotItems
    {
        AirshipFuel,
        AirshipWheel,
        AirshipKey,
        BlacksmithUpgrade,
        AlchemistUpgrade,
        RuneOfFire,
        RuneOfIce,
        RuneOfElectricity,
        ElementalStone,
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Persistence))]
    internal partial class PersistenceWriterSourceGenerationContext : JsonSerializerContext
    {
    }

    class Persistence
    {
        /// <summary>
        /// The current game state. Do not make copies of this or anything under it as they
        /// can change at any time.
        /// </summary>
        public GameState Current { get; set; }

        public class GameState
        {
            public PersistenceEntry<BattleTrigger.BattleTriggerPersistenceData> BattleTriggers { get; init; } = new PersistenceEntry<BattleTrigger.BattleTriggerPersistenceData>();

            public bool IsBossDead(int zoneIndex) => BossBattleTriggers.GetData(zoneIndex, 0).Dead;

            public PersistenceEntry<BattleTrigger.BattleTriggerPersistenceData> BossBattleTriggers { get; init; } = new PersistenceEntry<BattleTrigger.BattleTriggerPersistenceData>();

            public PersistenceEntry<BattleTrigger.UniqueStolenTreasureData> UniqueStolenTreasure { get; init; } = new PersistenceEntry<BattleTrigger.UniqueStolenTreasureData>();

            public PersistenceEntry<BattleTrigger.UniqueStolenTreasureData> UniqueBossStolenTreasure { get; init; } = new PersistenceEntry<BattleTrigger.UniqueStolenTreasureData>();

            public PersistenceEntry<TreasureTrigger.TreasureTriggerPersistenceData> TreasureTriggers { get; init; } = new PersistenceEntry<TreasureTrigger.TreasureTriggerPersistenceData>();

            public PersistenceEntry<PartyMemberTrigger.PartyMemberTriggerPersistenceData> PartyMemberTriggers { get; init; } = new PersistenceEntry<PartyMemberTrigger.PartyMemberTriggerPersistenceData>();

            public PersistenceEntry<Key.KeyPersistenceData> Keys { get; init; } = new PersistenceEntry<Key.KeyPersistenceData>();

            public PersistenceEntry<Torch.TorchPersistenceData> Torches { get; init; } = new PersistenceEntry<Torch.TorchPersistenceData>();

            public ZoneData Zone { get; init; } = new ZoneData();

            public PlayerData Player { get; init; } = new PlayerData();

            public HashSet<PlotItems> PlotItems { get; init; } = new HashSet<PlotItems>();

            public TimeData Time { get; init; } = new TimeData();

            public WorldData World { get; init; } = new WorldData();

            public PartyData Party { get; init; } = new PartyData();

            public DateTime SaveTime { get; set; }

            public List<InventoryItem> ItemVoid { get; set; } = new List<InventoryItem>();
        }

        public class PersistenceEntry<T>
                where T : struct
        {
            public Dictionary<int, Dictionary<int, T>> Entries
            {
                get { return entryDictionary; }
                init { entryDictionary = value; }
            }

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

            public Vector3? WorldPosition { get; set; }

            public Vector3? AirshipPosition { get; set; }

            public int? RespawnZone { get; set; }

            public int LastArea { get; set; }

            public bool InWorld { get; set; }

            public Vector3? RespawnPosition { get; set; }

            public int? LootDropZone { get; set; }

            public Vector3? LootDropPosition { get; set; }

            public long LootDropGold { get; set; }

            public bool InAirship { get; set; }

            public bool Started { get; set; }

            public bool InBattle { get; set; }

            public bool LastBattleIsBoss { get; set; }

            public int LastBattleIndex { get; set; }
        }

        public class TimeData
        {
            public long? Current { get; set; }

            public long Total { get; set; }
        }

        public class WorldData
        {
            public int Seed { get; set; }

            public int Level { get; set; }

            public Dictionary<int, int> CompletedAreaLevels { get; set; } = new Dictionary<int, int>();
        }

        public class CharacterData
        {
            public CharacterSheet CharacterSheet { get; set; }

            public Inventory Inventory { get; set; } = new Inventory();

            public String PlayerSprite { get; set; }

            public int Player { get; set; }

            public int StyleIndex { get; set; }

            public void RemoveItem(InventoryItem item)
            {
                if (item.Equipment != null)
                {
                    var id = item.Equipment.Id;
                    if (id.HasValue)
                    {
                        CharacterSheet.RemoveEquipment(id.Value);
                    }
                }
                Inventory.Items.Remove(item);
            }

            public bool HasRoom => Inventory.Items.Count < CharacterSheet.InventorySize;
        }

        public class PartyData
        {
            public List<CharacterData> Members { get; set; } = new List<CharacterData>();

            public long Gold { get; set; }

            public bool Undefeated { get; set; } = true;

            public bool OldSchool { get; set; } = true;

            public bool GameOver { get; set; }
        }
    }
}
