using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services.Monsters
{
    class SkeletonMaker : IMonsterTypeMaker
    {
        public void Populate(IBiome biome)
        {
            biome.RegularEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Skeleton(),
                EnemyCurve = new StandardEnemyCurve(),
                Resistances = new Dictionary<Element, Resistance>
                {
                    { Element.Healing, Resistance.Absorb },
                    { Element.Fire, Resistance.Weak },
                    { Element.Piercing, Resistance.Resist },
                    { Element.Bludgeoning, Resistance.Weak }
                }
            };
            biome.BadassEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Skeleton()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.Skeleton.Bone, 0xff404040 }
                    }
                },
                EnemyCurve = new StandardEnemyCurve(),
                Resistances = new Dictionary<Element, Resistance>
                {
                    { Element.Healing, Resistance.Absorb },
                    { Element.Fire, Resistance.Weak }
                }
            };
            biome.PeonEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Skeleton()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.Skeleton.Bone, 0xffd1cbb6 }
                    }
                },
                EnemyCurve = new StandardEnemyCurve(),
                Resistances = new Dictionary<Element, Resistance>
                {
                    { Element.Healing, Resistance.Absorb },
                    { Element.Fire, Resistance.Weak }
                }
            };
        }
    }
}
