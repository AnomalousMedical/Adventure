using Engine;
using Engine.CameraMovement;
using Engine.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Services
{
    class FlyCameraManager
    {
        private readonly CameraMover cameraMover;
        private readonly FirstPersonFlyCamera flyCamera;

        public bool Enabled { get; set; } = false;

        public FlyCameraManager
        (
            CameraMover cameraMover,
            FirstPersonFlyCamera flyCamera
        )
        {
            this.cameraMover = cameraMover;
            this.flyCamera = flyCamera;

            flyCamera.Position = new Vector3(0, 0, -10);
        }

        public void Update(Clock clock)
        {
            if (Enabled)
            {
                flyCamera.UpdateInput(clock);
                cameraMover.Position = flyCamera.Position;
                cameraMover.Orientation = flyCamera.Orientation;
                cameraMover.SceneCenter = flyCamera.Position;
            }
        }
    }
}
