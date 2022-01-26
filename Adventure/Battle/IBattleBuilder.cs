using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleBuilder
    {
        IEnumerable<Enemy> CreateEnemies(IObjectResolver objectResolver, Party party, IBiome biome);
    }
}
