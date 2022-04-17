using Adventure.Assets;
using Adventure.Assets.Enemies;
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
        List<MonsterInfo> CreateBaseMonsters(Random random);
        void PopulateBiome(IBiome biome, List<MonsterInfo> monsters, Element zoneWeakness, Element zoneResist, Random random);
    }

    record MonsterInfo(ISpriteAsset Asset, Dictionary<Element, Resistance> Resistances);

    class MonsterMaker : IMonsterMaker
    {
        private List<ISpriteAsset> monsterAssets = new List<ISpriteAsset>();
        private StandardEnemyCurve standardEnemyCurve = new StandardEnemyCurve();

        public MonsterMaker()
        {
            monsterAssets.Add(new Bat());
            monsterAssets.Add(new DeepTrollBerserker());
            monsterAssets.Add(new Ghoul());
            monsterAssets.Add(new MerfolkImpalerWaterNew());
            monsterAssets.Add(new MutantBeast());
            monsterAssets.Add(new OgreNew());
            monsterAssets.Add(new OrcKnightOld());
            monsterAssets.Add(new SalamanderFirebrand());
            monsterAssets.Add(new SirenNew());
            monsterAssets.Add(new Skeleton());
            monsterAssets.Add(new ThornHunter());
            monsterAssets.Add(new TinyDino());
            monsterAssets.Add(new WanderingMushroomNew());
        }

        /// <summary>
        /// Associate assets with weaknesses, the actual mapping happens when its created.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<MonsterInfo> CreateBaseMonsters(Random random)
        {
            var monsters = new List<MonsterInfo>();
            var availableElements = new List<Element>();
            void RefillElements()
            {
                availableElements.Clear();
                availableElements.Add(Element.Slashing);
                availableElements.Add(Element.Piercing);
                availableElements.Add(Element.Bludgeoning);
            }

            foreach(var asset in monsterAssets)
            {
                RefillElements();
                int index;
                
                index = random.Next(availableElements.Count);
                var weakElement = availableElements[index];
                availableElements.RemoveAt(index);

                index = random.Next(availableElements.Count);
                var resistElement = availableElements[index];

                var monster = new MonsterInfo
                (
                    Asset: asset,
                    Resistances: new Dictionary<Element, Resistance>
                    {
                        { weakElement, Resistance.Weak },
                        { resistElement, Resistance.Resist }
                    }
                );

                monsters.Add(monster);
            }

            return monsters;
        }

        public void PopulateBiome(IBiome biome, List<MonsterInfo> monsters, Element zoneWeakness, Element zoneResist, Random random)
        {
            var monsterIndex = random.Next(0, monsters.Count);
            var monster = monsters[monsterIndex];

            //Make resistances, this is setup to make the monster's intrinsic stats override any zone settings
            var enemyResistances = new Dictionary<Element, Resistance>();
            if (zoneWeakness != Element.None)
            {
                enemyResistances[zoneWeakness] = Resistance.Weak;
            }
            else
            {
                enemyResistances[zoneResist] = Resistance.Resist;
            }
            foreach(var resistance in monster.Resistances)
            {
                enemyResistances[resistance.Key] = resistance.Value;
            }

            biome.RegularEnemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.BadassEnemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.PeonEnemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.BossEnemy = new BiomeEnemy
            {
                Asset = monster.Asset.CreateAnotherInstance(),
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            if (zoneResist != Element.None)
            {
                //Enemies resisting something should reflect that element
                biome.RegularEnemy.Asset.SetupSwap(random.Next(0, 360), 100, 50);
                biome.RegularEnemy.Asset.SetupSwap(random.Next(0, 360), 100, 50);
                biome.PeonEnemy.Asset.SetupSwap(random.Next(0, 360), 100, 50);
                biome.BossEnemy.Asset.SetupSwap(random.Next(0, 360), 100, 50);
            }
            else
            {
                //Enemies with strengths should look different
            }
        }
    }
}
