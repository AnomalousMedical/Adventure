using RpgMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services.Monsters
{
    class TinyDinoSwap1Maker : IMonsterTypeMaker
    {
        public void Populate(IBiome biome)
        {
            biome.RegularEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.TinyDino.Skin, 0xff317a89 }
                    }
                },
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.BadassEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.TinyDino.Skin, 0xff024f59 }
                    }
                },
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.PeonEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.TinyDino.Skin, 0xff7babaf }
                    }
                },
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.BossEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.TinyDino.Skin, 0xff9105bd },
                        { Assets.Enemies.TinyDino.Eye, 0xff2ccdca }
                    }
                },
                EnemyCurve = new StandardEnemyCurve()
            };
        }
    }
}
