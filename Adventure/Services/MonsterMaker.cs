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
        Dictionary<Element, List<ISpriteAsset>> CreatePrimaryWeaknesses(Random random);

        void PopulateBiome(IBiome biome, Dictionary<Element, List<ISpriteAsset>> elementAssets, Element primaryElement, IEnumerable<KeyValuePair<Element, Resistance>> resistances, Random random);
    }

    class MonsterMaker : IMonsterMaker
    {
        private List<ISpriteAsset> monsterAssets = new List<ISpriteAsset>();
        private StandardEnemyCurve standardEnemyCurve = new StandardEnemyCurve();

        public MonsterMaker()
        {
            monsterAssets.Add(new Ghoul());
            monsterAssets.Add(new GhoulPurple());
            monsterAssets.Add(new OrcKnightOld());
            monsterAssets.Add(new OrcKnightOldBronze());
            monsterAssets.Add(new SalamanderFirebrand());
            monsterAssets.Add(new SalamanderFirebrandGreen());
            monsterAssets.Add(new SirenNew());
            monsterAssets.Add(new SirenNewRed());
            monsterAssets.Add(new Skeleton());
            monsterAssets.Add(new SkeletonBlack());
            monsterAssets.Add(new ThornHunter());
            monsterAssets.Add(new TinyDino());
            monsterAssets.Add(new TinyDinoBlue());
            monsterAssets.Add(new TinyDinoRed());
            monsterAssets.Add(new TinyDinoPurple());
            monsterAssets.Add(new WanderingMushroomNew());
        }

        /// <summary>
        /// Associate assets with weaknesses, the actual mapping happens when its created.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public Dictionary<Element, List<ISpriteAsset>> CreatePrimaryWeaknesses(Random random)
        {
            //This assumes there are more monsters than elements
            var primaryWeaknesses = new Dictionary<Element, List<ISpriteAsset>>();
            var availableMonsters = new List<ISpriteAsset>(monsterAssets);
            foreach (var element in ElementTypes())
            {
                var index = random.Next(0, availableMonsters.Count);
                if (!primaryWeaknesses.ContainsKey(element))
                {
                    primaryWeaknesses[element] = new List<ISpriteAsset>();
                }
                primaryWeaknesses[element].Add(availableMonsters[index]);
                availableMonsters.RemoveAt(index);
            }
            foreach(var monster in availableMonsters)
            {
                var element = (Element)random.Next((int)Element.RandStart, (int)Element.RandEnd);
                primaryWeaknesses[element].Add(monster);
            }
            return primaryWeaknesses;
        }

        public void PopulateBiome(IBiome biome, Dictionary<Element, List<ISpriteAsset>> elementAssets, Element primaryElement, IEnumerable<KeyValuePair<Element, Resistance>> resistances, Random random)
        {
            var assets = elementAssets[primaryElement];
            var assetIndex = random.Next(0, assets.Count);
            var asset = assets[assetIndex];
            var enemyResistances = new Dictionary<Element, Resistance>(resistances);

            //This is not how this is going to work
            biome.RegularEnemy = new BiomeEnemy
            {
                Asset = asset,
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.BadassEnemy = new BiomeEnemy
            {
                Asset = asset,
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.PeonEnemy = new BiomeEnemy
            {
                Asset = asset,
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };

            biome.BossEnemy = new BiomeEnemy
            {
                Asset = asset,
                EnemyCurve = standardEnemyCurve,
                Resistances = enemyResistances
            };
        }

        private IEnumerable<Element> ElementTypes()
        {
            yield return Element.Piercing;
            yield return Element.Slashing;
            yield return Element.Bludgeoning;
            yield return Element.Fire;
            yield return Element.Ice;
            yield return Element.Electricity;
            yield return Element.Acid;
            yield return Element.Light;
            yield return Element.Darkness;
            yield return Element.Water;
            yield return Element.Poison;
            yield return Element.Air;
            yield return Element.Earth;
        }
    }
}
