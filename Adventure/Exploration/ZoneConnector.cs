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
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly IExplorationGameState explorationGameState;
        private bool goPrevious;
        private bool goWorld;
        private Rect collisionRect;

        public ZoneConnector(
            IDestructionRequest destructionRequest,
            Description description,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            IExplorationGameState explorationGameState)
        {
            this.goWorld = description.GoWorld;
            this.goPrevious = description.GoPrevious;
            this.destructionRequest = destructionRequest;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.explorationGameState = explorationGameState;
            var halfScale = description.Scale / 2f;
            this.collisionRect = new Rect(description.Translation.x - halfScale.x, description.Translation.z - halfScale.z, description.Scale.x + halfScale.x, description.Scale.z + halfScale.z);
            
        }

        public void Dispose()
        {

        }

        public void RequestDestruction()
        {
            this.destructionRequest.RequestDestruction();
        }

        public void DetectCollision(in Vector3 testPoint)
        {
            if(testPoint.x > collisionRect.Left && testPoint.x < collisionRect.Right
                && testPoint.z > collisionRect.Top && testPoint.z < collisionRect.Bottom)
            {
                HandleZoneChange(testPoint);
            }
        }

        private void HandleZoneChange(Vector3 playerLoc)
        {
            coroutineRunner.RunTask(async () =>
            {
                explorationGameState.RequestWorldMap();
            });
        }
    }
}
