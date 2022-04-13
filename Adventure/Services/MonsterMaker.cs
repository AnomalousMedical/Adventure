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
        void PopulateTinyDinos(IBiome biome);
    }

    class MonsterMaker : IMonsterMaker
    {
        public void PopulateBiome(IBiome biome)
        {
            PopulateTinyDinos(biome);
        }

        public void PopulateTinyDinos(IBiome biome)
        {
            biome.RegularEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino(),
                EnemyCurve = new StandardEnemyCurve()
            };
            biome.BadassEnemy = new BiomeEnemy()
            {
                Asset = new Assets.Enemies.TinyDino()
                {
                    PalletSwap = new Dictionary<uint, uint>
                    {
                        { Assets.Enemies.TinyDino.Skin, 0xff166416 }
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
                        { Assets.Enemies.TinyDino.Skin, 0xff168543 }
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
                        { Assets.Enemies.TinyDino.Skin, 0xffc12935 },
                        { Assets.Enemies.TinyDino.Spine, 0xff9105bd },
                        { Assets.Enemies.TinyDino.Eye, 0xffe28516 },
                    }
                },
                EnemyCurve = new StandardEnemyCurve()
            };
        }
    }
}
