using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services.Monsters
{
    class GhoulMaker : IMonsterTypeMaker
    {
        public void Populate(IBiome biome)
        {
            biome.RegularEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Ghoul(),
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.BadassEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Ghoul(),
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.PeonEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Ghoul(),
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.BossEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.Ghoul(),
                EnemyCurve = new StandardEnemyCurve()
            };
        }
    }
}
