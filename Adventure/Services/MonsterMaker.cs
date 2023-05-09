using Adventure.Assets;
using Adventure.Assets.Enemies;
using Engine;
using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    interface IMonsterMaker
    {
        List<MonsterInfo> CreateBaseMonsters(int seed);
        List<MonsterInfo> CreateElemental(int seed, Element absorbElement);
        void PopulateBiome(IBiome biome, IEnumerable<MonsterInfo> regularEnemies, MonsterInfo boss);
    }

    record MonsterInfo(ISpriteAsset Asset, Dictionary<Element, Resistance> Resistances, BiomeType NativeBiome)
    {
    }

    record MonsterAssetInfo(ISpriteAsset Asset, BiomeType NativeBiome);

    class MonsterMaker : IMonsterMaker
    {
        private List<MonsterAssetInfo> monsterAssets = new List<MonsterAssetInfo>();
        private StandardEnemyCurve standardEnemyCurve = new StandardEnemyCurve();

        public MonsterMaker()
        {
            monsterAssets.Add(new MonsterAssetInfo(new Bat(), BiomeType.Countryside));
            monsterAssets.Add(new MonsterAssetInfo(new OgreNew(), BiomeType.Snowy));
            monsterAssets.Add(new MonsterAssetInfo(new Wolf(), BiomeType.Snowy));
            monsterAssets.Add(new MonsterAssetInfo(new OrcKnightOld(), BiomeType.Snowy));
            monsterAssets.Add(new MonsterAssetInfo(new SalamanderFirebrand(), BiomeType.Desert));
            monsterAssets.Add(new MonsterAssetInfo(new Minotaur(), BiomeType.Countryside));
            monsterAssets.Add(new MonsterAssetInfo(new Skeleton(), BiomeType.Desert));
            monsterAssets.Add(new MonsterAssetInfo(new ThornHunter(), BiomeType.Forest));
            monsterAssets.Add(new MonsterAssetInfo(new TinyDino(), BiomeType.Countryside));
            monsterAssets.Add(new MonsterAssetInfo(new WanderingMushroomNew(), BiomeType.Forest));
            monsterAssets.Add(new MonsterAssetInfo(new TinyDino(), BiomeType.Beach));
            monsterAssets.Add(new MonsterAssetInfo(new Alligator(), BiomeType.Swamp));
        }

        /// <summary>
        /// Associate assets with weaknesses, the actual mapping happens when its created.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<MonsterInfo> CreateBaseMonsters(int seed)
        {
            var weaknessRandom = new FIRandom(seed);
            var monsters = new List<MonsterInfo>();
            var availablePhysicalElements = new List<Element>();
            var availableElements = new List<Element>();
            void RefillElements()
            {
                availablePhysicalElements.Clear();
                availablePhysicalElements.Add(Element.Slashing);
                availablePhysicalElements.Add(Element.Piercing);
                availablePhysicalElements.Add(Element.Bludgeoning);

                availableElements.Clear();
                availableElements.Add(Element.Fire);
                availableElements.Add(Element.Ice);
                availableElements.Add(Element.Electricity);
            }

            foreach (var monsterAsset in monsterAssets)
            {
                RefillElements();
                int index;
                
                index = weaknessRandom.Next(availablePhysicalElements.Count);
                var weakPhysicalElement = availablePhysicalElements[index];
                availablePhysicalElements.RemoveAt(index);

                index = weaknessRandom.Next(availablePhysicalElements.Count);
                var resistPhysicalElement = availablePhysicalElements[index];

                index = weaknessRandom.Next(availableElements.Count);
                var weakElement = availableElements[index];
                availableElements.RemoveAt(index);

                index = weaknessRandom.Next(availableElements.Count);
                var resistElement = availableElements[index];

                var monster = new MonsterInfo
                (
                    Asset: monsterAsset.Asset,
                    Resistances: new Dictionary<Element, Resistance>
                    {
                        { weakPhysicalElement, Resistance.Weak },
                        { resistPhysicalElement, Resistance.Resist },
                        { weakElement, Resistance.Weak },
                        { resistElement, Resistance.Resist }
                    },
                    NativeBiome: monsterAsset.NativeBiome
                );

                monsters.Add(monster);
            }

            return monsters;
        }

        public List<MonsterInfo> CreateElemental(int seed, Element absorbElement)
        {
            var monsters = CreateBaseMonsters(seed);
            foreach(var monster in monsters)
            {
                monster.Resistances[absorbElement] = Resistance.Absorb;
            }
            return monsters;
        }

        public void PopulateBiome(IBiome biome, IEnumerable<MonsterInfo> regularEnemies, MonsterInfo boss)
        {
            foreach(var monster in regularEnemies)
            {
                var enemy = CreateEnemy(monster);
                biome.RegularEnemies.Add(enemy);
            }

            biome.BossEnemy = CreateEnemy(boss);
        }

        private BiomeEnemy CreateEnemy(MonsterInfo monster)
        {
            //Make resistances, this is setup to make the monster's intrinsic stats override any zone settings

            var enemyResistances = new Dictionary<Element, Resistance>();

            Element? absorbElement = null;
            foreach (var resistance in monster.Resistances)
            {
                if(resistance.Value == Resistance.Absorb && resistance.Key > Element.MagicStart && resistance.Key < Element.MagicEnd)
                {
                    absorbElement = resistance.Key;
                }
                enemyResistances[resistance.Key] = resistance.Value;
            }
            
            var enemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            if (absorbElement != null)
            {
                var elementColor = ElementColors.GetElementalHue(absorbElement.Value);
                enemy.Asset.SetupSwap(elementColor, 100, 50);
            }

            return enemy;
        }
    }
}
