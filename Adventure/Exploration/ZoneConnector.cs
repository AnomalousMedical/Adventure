using Adventure.Menu;
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
            
        }

        private readonly IDestructionRequest destructionRequest;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IExplorationGameState explorationGameState;
        private readonly FadeScreenMenu fadeScreenMenu;
        private readonly IZoneManager zoneManager;
        private Rect collisionRect;

        public ZoneConnector
        (
            IDestructionRequest destructionRequest,
            Description description,
            ICoroutineRunner coroutineRunner,
            IExplorationGameState explorationGameState,
            FadeScreenMenu fadeScreenMenu,
            IZoneManager zoneManager
        )
        {
            this.destructionRequest = destructionRequest;
            this.coroutineRunner = coroutineRunner;
            this.explorationGameState = explorationGameState;
            this.fadeScreenMenu = fadeScreenMenu;
            this.zoneManager = zoneManager;
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
                zoneManager.StopPlayer();

                await fadeScreenMenu.ShowAndWait(0.0f, 1.0f, 0.6f, Engine.Platform.GamepadId.Pad1);

                explorationGameState.RequestWorldMap();

                await fadeScreenMenu.ShowAndWait(1.0f, 0.0f, 0.6f, Engine.Platform.GamepadId.Pad1);

                fadeScreenMenu.Close();
            });
        }
    }
}
