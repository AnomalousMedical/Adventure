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
        void PopulateBiome(IBiome biome);
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
            monsterAssets.Add(new TinyDino());
            monsterAssets.Add(new TinyDinoBlue());
            monsterAssets.Add(new TinyDinoRed());
            monsterAssets.Add(new TinyDinoPurple());
        }

        public Dictionary<Element, List<ISpriteAsset>> CreatePrimaryWeaknesses(Random random)
        {
            //create primary weaknesses here
            //Then zones generate what weaknesses they want
            //based on the zone's primary weakness choose an enemy
            //add any other zone weaknesses to that enemy
            //That is now the zone's enemies

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
            return primaryWeaknesses;
        }

        public void PopulateBiome(IBiome biome)
        {
            //This is not how this is going to work
            biome.RegularEnemy = new BiomeEnemy
            {
                Asset = monsterAssets[9],
                EnemyCurve = standardEnemyCurve
            };

            biome.BadassEnemy = new BiomeEnemy
            {
                Asset = monsterAssets[2],
                EnemyCurve = standardEnemyCurve
            };

            biome.PeonEnemy = new BiomeEnemy
            {
                Asset = monsterAssets[2],
                EnemyCurve = standardEnemyCurve
            };

            biome.BossEnemy = new BiomeEnemy
            {
                Asset = monsterAssets[2],
                EnemyCurve = standardEnemyCurve
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
