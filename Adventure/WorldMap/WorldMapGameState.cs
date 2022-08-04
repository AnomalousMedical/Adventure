using DiligentEngine.RT;
using Engine;
using Engine.Platform;
using Adventure.Battle;
using Adventure.Services;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.CameraMovement;

namespace Adventure.WorldMap
{
    interface IWorldMapGameState : IGameState
    {
        void Link(IExplorationGameState explorationState);
    }

    class WorldMapGameState : IWorldMapGameState
    {
        private readonly ISharpGui sharpGui;
        private readonly RTInstances<IWorldMapGameState> rtInstances;
        private readonly IScreenPositioner screenPositioner;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly IZoneManager zoneManager;
        private readonly Persistence persistence;
        private readonly IWorldMapManager worldMapManager;
        private readonly CameraMover cameraMover;
        private readonly FirstPersonFlyCamera flyCamera;
        private IExplorationGameState explorationState;
        private SharpButton restart = new SharpButton() { Text = "Restart" };
        private SharpSliderHorizontal zoneSelect;
        private int currentZone = 0;
        private SharpText worldMapText = new SharpText("Zone 0") { Color = Color.White };
        private ILayoutItem layout;

        public RTInstances Instances => rtInstances;

        public WorldMapGameState
        (
            ISharpGui sharpGui,
            RTInstances<IWorldMapGameState> rtInstances,
            IScreenPositioner screenPositioner,
            ICoroutineRunner coroutineRunner,
            IZoneManager zoneManager,
            Persistence persistence,
            IScaleHelper scaleHelper,
            IWorldMapManager worldMapManager,
            CameraMover cameraMover,
            FirstPersonFlyCamera flyCamera
        )
        {
            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            this.screenPositioner = screenPositioner;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            this.worldMapManager = worldMapManager;
            this.cameraMover = cameraMover;
            this.flyCamera = flyCamera;
            worldMapManager.SetupWorldMap();
            layout = new ColumnLayout(worldMapText, restart) { Margin = new IntPad(scaleHelper.Scaled(10)) };
            zoneSelect = new SharpSliderHorizontal() { Rect = scaleHelper.Scaled(new IntRect(100, 10, 500, 35)), Max = 99 };
        }

        public void Link(IExplorationGameState explorationState)
        {
            this.explorationState = explorationState;
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                //persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                //persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                persistence.Current.BattleTriggers.ClearData();
            }
        }

        public IGameState Update(Clock clock)
        {
            flyCamera.UpdateInput(clock);

            cameraMover.Position = flyCamera.Position;
            cameraMover.Orientation = flyCamera.Orientation;
            cameraMover.SceneCenter = flyCamera.Position;

            IGameState nextState = this;

            var size = layout.GetDesiredSize(sharpGui);
            layout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetCenterRect(size);
            layout.SetRect(rect);

            sharpGui.Text(worldMapText);

            if(sharpGui.Slider(zoneSelect, ref currentZone, GamepadId.Pad1))
            {
                worldMapText.Text = $"Zone {currentZone}";
            }

            if (sharpGui.Button(restart, GamepadId.Pad1))
            {
                persistence.Current.Player.Position = null;
                persistence.Current.Zone.CurrentIndex = currentZone;
                coroutineRunner.RunTask(zoneManager.Restart());
                nextState = this.explorationState;
            }

            return nextState;
        }
    }
}
