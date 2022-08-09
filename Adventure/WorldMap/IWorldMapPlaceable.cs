using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    interface IWorldMapPlaceable
    {
        void CreatePhysics();
        void DestroyPhysics();
        void RequestDestruction();
    }
}
