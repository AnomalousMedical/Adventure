using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPlugin;
using DiligentEngine;
using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class ZoneConnector : IDisposable
    {
        public class Description : SceneObjectDesc
        {
            /// <summary>
            /// Set this to true to go to the previous level. False to go to the next.
            /// </summary>
            public bool GoPrevious { get; set; }

            /// <summary>
            /// Set this to true to go to the world from this connector.
            /// </summary>
            public bool GoWorld { get; set; }
        }

        private readonly IDestructionRequest destructionRequest;
        private readonly IBepuScene<IExplorationGameState> bepuScene;
        private readonly ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly IExplorationGameState explorationGameState;
        private StaticHandle staticHandle;
        private TypedIndex shapeIndex;
        private bool goPrevious;
        private bool goWorld;

        public ZoneConnector(
            IDestructionRequest destructionRequest,
            IScopedCoroutine coroutine,
            IBepuScene<IExplorationGameState> bepuScene,
            Description description,
            ICollidableTypeIdentifier<IExplorationGameState> collidableIdentifier,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            IExplorationGameState explorationGameState)
        {
            this.goWorld = description.GoWorld;
            this.goPrevious = description.GoPrevious;
            this.destructionRequest = destructionRequest;
            this.bepuScene = bepuScene;
            this.collidableIdentifier = collidableIdentifier;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.explorationGameState = explorationGameState;
            var shape = new Box(description.Scale.x, description.Scale.y, description.Scale.z); //TODO: Each one creates its own, try to load from resources
            shapeIndex = bepuScene.Simulation.Shapes.Add(shape);

            staticHandle = bepuScene.Simulation.Statics.Add(
                new StaticDescription(
                    new System.Numerics.Vector3(description.Translation.x, description.Translation.y, description.Translation.z),
                    new System.Numerics.Quaternion(description.Orientation.x, description.Orientation.y, description.Orientation.z, description.Orientation.w),
                    new CollidableDescription(shapeIndex, 0.1f)));

            bepuScene.RegisterCollisionListener(new CollidableReference(staticHandle), HandleCollision);
        }

        public void Dispose()
        {
            bepuScene.UnregisterCollisionListener(new CollidableReference(staticHandle));
            bepuScene.Simulation.Shapes.Remove(shapeIndex);
            bepuScene.Simulation.Statics.Remove(staticHandle);
        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        private void HandleCollision(CollisionEvent evt)
        {
            //Don't want to do this during the physics update. Trigger to run later.

            if (collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.A, out var player)
                || collidableIdentifier.TryGetIdentifier<Player>(evt.Pair.B, out player))
            {
                coroutineRunner.RunTask(async () =>
                {
                    if (this.goWorld)
                    {
                        explorationGameState.RequestWorldMap();
                    }
                    else
                    {
                        var playerLoc = player.GetLocation();

                        if (this.goPrevious)
                        {
                            await zoneManager.GoPrevious(playerLoc);
                        }
                        else
                        {
                            await zoneManager.GoNext(playerLoc);
                        }
                    }
                });
            }            
        }
    }
}
