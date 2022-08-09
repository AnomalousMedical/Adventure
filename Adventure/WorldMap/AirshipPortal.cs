using Adventure.Exploration.Menu;
using BepuPlugin;
using DiligentEngine.RT;
using DiligentEngine.RT.Sprites;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.WorldMap
{
    class AirshipPortal : IslandPortal
    {
        public AirshipPortal(RTInstances<IWorldMapGameState> rtInstances, IDestructionRequest destructionRequest, IScopedCoroutine coroutine, IBepuScene<IWorldMapGameState> bepuScene, Description description, ICollidableTypeIdentifier<IWorldMapGameState> collidableIdentifier, SpriteInstanceFactory spriteInstanceFactory, IContextMenu contextMenu, IWorldMapManager worldMapManager) : base(rtInstances, destructionRequest, coroutine, bepuScene, description, collidableIdentifier, spriteInstanceFactory, contextMenu, worldMapManager)
        {
        }

        protected override void HandleCollision(CollisionEvent evt)
        {
            //Do nothing
        }
    }
}
