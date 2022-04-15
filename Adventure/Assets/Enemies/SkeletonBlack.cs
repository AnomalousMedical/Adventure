using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Assets.Enemies
{ 
    class SkeletonBlack : Skeleton
    {
        public SkeletonBlack()
        {
            PalletSwap = new Dictionary<uint, uint>
            {
                { Bone, 0xff404040 }
            };
        }
    }
}
