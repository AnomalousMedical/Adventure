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
        List<MonsterInfo> CreateBaseMonsters(FIRandom random);
        void PopulateBiome(IBiome biome, IEnumerable<MonsterInfo> regularEnemies, MonsterInfo boss, Element weakElement, Element resistElement);
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
        }

        /// <summary>
        /// Associate assets with weaknesses, the actual mapping happens when its created.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<MonsterInfo> CreateBaseMonsters(FIRandom random)
        {
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
                
                index = random.Next(availablePhysicalElements.Count);
                var weakPhysicalElement = availablePhysicalElements[index];
                availablePhysicalElements.RemoveAt(index);

                index = random.Next(availablePhysicalElements.Count);
                var resistPhysicalElement = availablePhysicalElements[index];

                index = random.Next(availableElements.Count);
                var weakElement = availableElements[index];
                availableElements.RemoveAt(index);

                index = random.Next(availableElements.Count);
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

        public void PopulateBiome(IBiome biome, IEnumerable<MonsterInfo> regularEnemies, MonsterInfo boss, Element weakElement, Element resistElement)
        {
            foreach(var monster in regularEnemies)
            {
                var enemy = CreateEnemy(monster, weakElement, resistElement);
                biome.RegularEnemies.Add(enemy);
            }

            biome.BossEnemy = CreateEnemy(boss, weakElement, resistElement);

            var elementColor = ElementColors.GetElementalHue(weakElement);
            if (weakElement != Element.None)
            {

                foreach (var regularEnemy in biome.RegularEnemies)
                {
                    regularEnemy.Asset.SetupSwap(elementColor, 100, 50);
                }
                biome.BossEnemy.Asset.SetupSwap(elementColor, 100, 50);
            }
            else if(resistElement != Element.None)
            {
                foreach(var regularEnemy in biome.RegularEnemies)
                {
                    regularEnemy.Asset.SetupSwap(elementColor + 180f, 100, 50);
                }
                biome.BossEnemy.Asset.SetupSwap(elementColor + 180f, 100, 50);
            }
        }

        private BiomeEnemy CreateEnemy(MonsterInfo monster, Element weakElement, Element resistElement)
        {
            //Make resistances, this is setup to make the monster's intrinsic stats override any zone settings

            var enemyResistances = new Dictionary<Element, Resistance>();
            if (weakElement != Element.None)
            {
                enemyResistances[weakElement] = Resistance.Weak;
            }
            else
            {
                enemyResistances[resistElement] = Resistance.Resist;
            }

            foreach (var resistance in monster.Resistances)
            {
                enemyResistances[resistance.Key] = resistance.Value;
            }

            var enemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };
            return enemy;
        }
    }
}
