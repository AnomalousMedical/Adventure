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
using Adventure.Exploration.Menu;

namespace Adventure.WorldMap
{
    interface IWorldMapGameState : IGameState
    {
        void Link(IExplorationGameState explorationState);
        void RequestZone(int zoneIndex);
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
        private readonly IContextMenu contextMenu;
        private readonly IWorldDatabase worldDatabase;
        private IExplorationGameState explorationState;
        private SharpButton restart = new SharpButton() { Text = "Restart" };
        private SharpSliderHorizontal zoneSelect;
        private int currentZone = 0;
        private SharpText worldMapText = new SharpText("Zone 0") { Color = Color.White };
        private ILayoutItem layout;
        private IGameState nextState;

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
            IBepuScene<IWorldMapGameState> bepuScene,
            IContextMenu contextMenu,
            IWorldDatabase worldDatabase
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
            this.contextMenu = contextMenu;
            this.worldDatabase = worldDatabase;
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
            persistence.Current.Player.InWorld = active;
            if (active)
            {
                nextState = this;
                persistence.Current.BattleTriggers.ClearData();
                if (persistence.Current.Player.WorldPosition == null)
                {
                    worldMapManager.MovePlayerToArea(persistence.Current.Player.LastArea);
                }
            }
        }

        public void RequestZone(int zoneIndex)
        {
            persistence.Current.Player.Position = null;
            persistence.Current.Zone.CurrentIndex = zoneIndex;
            persistence.Current.Player.LastArea = worldDatabase.GetAreaBuilder(zoneIndex).Index;
            coroutineRunner.RunTask(zoneManager.Restart());
            nextState = this.explorationState;
        }

        public IGameState Update(Clock clock)
        {
            flyCameraManager.Update(clock);

            var size = layout.GetDesiredSize(sharpGui);
            layout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetBottomLeftRect(size);
            layout.SetRect(rect);

            sharpGui.Text(worldMapText);

            if(sharpGui.Slider(zoneSelect, ref currentZone, GamepadId.Pad1))
            {
                worldMapText.Text = $"Zone {currentZone}";
            }

            if (sharpGui.Button(restart, GamepadId.Pad1))
            {
                RequestZone(currentZone);
            }

            bepuScene.Update(clock, new System.Numerics.Vector3(0, 0, 1));
            contextMenu.Update();

            return nextState;
        }
    }
}
