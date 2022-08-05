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
using BepuPlugin;

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
        private readonly FlyCameraManager flyCameraManager;
        private readonly IBepuScene<IWorldMapGameState> bepuScene;
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
            FlyCameraManager flyCameraManager,
            IBepuScene<IWorldMapGameState> bepuScene
        )
        {
            this.sharpGui = sharpGui;
            this.rtInstances = rtInstances;
            this.screenPositioner = screenPositioner;
            this.coroutineRunner = coroutineRunner;
            this.zoneManager = zoneManager;
            this.persistence = persistence;
            this.worldMapManager = worldMapManager;
            this.flyCameraManager = flyCameraManager;
            this.bepuScene = bepuScene;
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
            flyCameraManager.Enabled = active;
            if (active)
            {
                //persistence.Current.Zone.CurrentIndex = persistence.Current.Player.RespawnZone ?? 0;
                //persistence.Current.Player.Position = persistence.Current.Player.RespawnPosition;
                persistence.Current.BattleTriggers.ClearData();
            }
        }

        public IGameState Update(Clock clock)
        {
            flyCameraManager.Update(clock);

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

            bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));

            return nextState;
        }
    }
}
