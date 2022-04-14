using Adventure.Services.Monsters;
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

    interface IMonsterTypeMaker
    {
        void Populate(IBiome biome);
    }

    class MonsterMaker : IMonsterMaker
    {
        private List<IMonsterTypeMaker> monsterTypeMakers = new List<IMonsterTypeMaker>();

        public MonsterMaker()
        {
            monsterTypeMakers.Add(new GhoulMaker());
            monsterTypeMakers.Add(new OrcKnightMaker());
            monsterTypeMakers.Add(new SalamanderMaker());
            monsterTypeMakers.Add(new SirenMaker());
            monsterTypeMakers.Add(new SkeletonMaker());
            monsterTypeMakers.Add(new TinyDinoMaker());
            monsterTypeMakers.Add(new TinyDinoSwap1Maker());
        }

        public void PopulateBiome(IBiome biome)
        {
            monsterTypeMakers[0].Populate(biome);
        }
    }
}
