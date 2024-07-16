using Adventure.Assets;
using Adventure.Assets.Enemies;
using Adventure.Assets.SoundEffects;
using Adventure.Skills.Spells;
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

    record MonsterInfo(ISpriteAsset Asset, Dictionary<Element, Resistance> Resistances, BiomeType NativeBiome, ISoundEffect AttackSound)
    {
    }

    record MonsterAssetInfo(ISpriteAsset Asset, BiomeType NativeBiome, ISoundEffect AttackSound);

    class MonsterMaker : IMonsterMaker
    {
        private List<MonsterAssetInfo> monsterAssets = new List<MonsterAssetInfo>();
        private StandardEnemyCurve standardEnemyCurve = new StandardEnemyCurve();

        public MonsterMaker()
        {
            monsterAssets.Add(new MonsterAssetInfo(new Bat(), BiomeType.Countryside, BatSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new OgreNew(), BiomeType.Snowy, OgreNewSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new Wolf(), BiomeType.Snowy, WolfSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new SalamanderFirebrand(), BiomeType.Desert, SalamanderFirebrandSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new Skeleton(), BiomeType.Desert, SkeletonSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new ThornHunter(), BiomeType.Forest, ThornHunterSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new WanderingMushroomNew(), BiomeType.Forest, WanderingMushroomNewSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new GreatWhiteShark(), BiomeType.Beach, GreatWhiteSharkSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new Skeleton(), BiomeType.Beach, SkeletonSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new OgreNew(), BiomeType.Swamp, OgreNewSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new Alligator(), BiomeType.Swamp, AlligatorSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new Minotaur(), BiomeType.Mountain, MinotaurSoundEffect.Instance));
            monsterAssets.Add(new MonsterAssetInfo(new MountainLion(), BiomeType.Mountain, MinotaurSoundEffect.Instance));

            monsterAssets.Add(new MonsterAssetInfo(new Dragon(), BiomeType.FinalBoss, TinyDinoSoundEffect.Instance));
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
                    NativeBiome: monsterAsset.NativeBiome,
                    AttackSound: monsterAsset.AttackSound
                );

                monsters.Add(monster);
            }

            return monsters;
        }

        public List<MonsterInfo> CreateElemental(int seed, Element absorbElement)
        {
            var monsters = CreateBaseMonsters(seed);
            foreach (var monster in monsters)
            {
                if (monster.Resistances.TryGetValue(absorbElement, out var resistance))
                {
                    switch (resistance)
                    {
                        case Resistance.Weak:
                            var elements = new List<Element> { Element.Ice, Element.Electricity, Element.Fire };

                            //Remove the new absorb element
                            elements.Remove(absorbElement);

                            //Remove the resist elements
                            foreach (var resist in monster.Resistances.Where(i => i.Value == Resistance.Resist).Select(i => i.Key))
                            {
                                elements.Remove(resist);
                            }

                            //Use the element left over
                            if (elements.Count > 0)
                            {
                                monster.Resistances[elements[0]] = Resistance.Weak;
                            }
                            break;
                    }
                }
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
                Resistances = enemyResistances,
                AttackSound = monster.AttackSound,
            };

            if (absorbElement != null)
            {
                var elementColor = ElementColors.GetElementalHue(absorbElement.Value);
                enemy.Asset.SetupSwap(elementColor, 100, 50);

                const int weakStart = 35;
                const int medStart = 50;
                const int strongStart = 60;
                const int strongEnd = 99;

                const float weakChance = 0.25f;
                const float medChance = 0.35f;
                const float strongChance = 0.5f;

                switch (absorbElement)
                {
                    case Element.Ice:
                        enemy.Skills = new[]
                        {
                            new BattleStats.SkillInfo(nameof(Ice), weakStart, medStart, weakChance),
                            new BattleStats.SkillInfo(nameof(StrongIce), medStart, strongStart, medChance),
                            new BattleStats.SkillInfo(nameof(ArchIce), strongStart, strongEnd, strongChance),
                        };
                        break;
                    case Element.Fire:
                        enemy.Skills = new[]
                        {
                            new BattleStats.SkillInfo(nameof(Fire), weakStart, medStart, weakChance),
                            new BattleStats.SkillInfo(nameof(StrongFire), medStart, strongStart, medChance),
                            new BattleStats.SkillInfo(nameof(ArchFire), strongStart, strongEnd, strongChance),
                        };
                        break;
                    case Element.Electricity:
                        enemy.Skills = new[]
                        {
                            new BattleStats.SkillInfo(nameof(Lightning), weakStart, medStart, weakChance),
                            new BattleStats.SkillInfo(nameof(StrongLightning), medStart, strongStart, medChance),
                            new BattleStats.SkillInfo(nameof(ArchLightning), strongStart, strongEnd, strongChance),
                        };
                        break;
                }
            }

            return enemy;
        }
    }
}
