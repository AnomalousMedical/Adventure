using Adventure.Exploration.Menu.Asimov;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class DebugGui : IDebugGui, IExplorationSubMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IZoneManager zoneManager;
        private readonly ITimeClock timeClock;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly Party party;
        private readonly ILevelCalculator levelCalculator;
        private readonly AsimovRootMenu asimovRoot;
        SharpButton goNextLevel = new SharpButton() { Text = "Next Stage" };
        SharpButton goPreviousLevel = new SharpButton() { Text = "Previous Stage" };
        SharpButton goStart = new SharpButton() { Text = "Go Start" };
        SharpButton goEnd = new SharpButton() { Text = "Go End" };
        //SharpButton toggleCamera = new SharpButton() { Text = "Toggle Camera" };
        SharpButton asimov = new SharpButton() { Text = "Asimov" };
        SharpButton battle = new SharpButton() { Text = "Battle" };
        SharpButton allowBattle = new SharpButton() { Text = "Allow Battle" };
        SharpText averageLevel = new SharpText() { Color = Color.White };
        SharpSliderHorizontal currentHour;

        public DebugGui
        (
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            IZoneManager zoneManager,
            ITimeClock timeClock,
            ICoroutineRunner coroutineRunner,
            Party party,
            ILevelCalculator levelCalculator,
            AsimovRootMenu asimovRoot
        )
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.zoneManager = zoneManager;
            this.timeClock = timeClock;
            this.coroutineRunner = coroutineRunner;
            this.party = party;
            this.levelCalculator = levelCalculator;
            this.asimovRoot = asimovRoot;
            currentHour = new SharpSliderHorizontal() { Rect = scaleHelper.Scaled(new IntRect(100, 10, 500, 35)), Max = 24 };
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            averageLevel.Text = $"Level: {party.ActiveCharacterSheets.GetAverageLevel()}";
            allowBattle.Text = explorationGameState.AllowBattles ? "Battles Allowed" : "Battles Disabled";

            var layout =
                new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                new MaxWidthLayout(scaleHelper.Scaled(800),
                new ColumnLayout(averageLevel, new RowLayout(battle, allowBattle), new RowLayout(asimov), new RowLayout(goStart, goEnd), new RowLayout(goNextLevel, goPreviousLevel) /*, toggleCamera*/) { Margin = new IntPad(10) }
            ));
            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(averageLevel);

            if (sharpGui.Button(battle, navUp: goNextLevel.Id, navDown: asimov.Id, navRight: allowBattle.Id, navLeft: allowBattle.Id))
            {
                explorationGameState.RequestBattle();
                explorationMenu.RequestSubMenu(null);
            }

            if (sharpGui.Button(allowBattle, navUp: goPreviousLevel.Id, navDown: asimov.Id, navRight: battle.Id, navLeft: battle.Id))
            {
                explorationGameState.AllowBattles = !explorationGameState.AllowBattles;
            }

            if (sharpGui.Button(asimov, navUp: battle.Id, navDown: goStart.Id))
            {
                explorationMenu.RequestSubMenu(asimovRoot);
            }

            if(sharpGui.Button(goStart, navUp: asimov.Id, navDown: goNextLevel.Id, navLeft: goEnd.Id, navRight: goEnd.Id))
            {
                zoneManager.GoStartPoint();
                explorationMenu.RequestSubMenu(null);
            }

            if (sharpGui.Button(goEnd, navUp: asimov.Id, navDown: goPreviousLevel.Id, navLeft: goStart.Id, navRight: goStart.Id))
            {
                zoneManager.GoEndPoint();
                explorationMenu.RequestSubMenu(null);
            }

            if (!zoneManager.ChangingZone && sharpGui.Button(goNextLevel, navUp: goStart.Id, navDown: battle.Id, navLeft: goPreviousLevel.Id, navRight: goPreviousLevel.Id))
            {
                coroutineRunner.RunTask(zoneManager.GoNext());
                explorationMenu.RequestSubMenu(null);
            }

            if (!zoneManager.ChangingZone && sharpGui.Button(goPreviousLevel, navUp: goEnd.Id, navDown: allowBattle.Id, navLeft: goNextLevel.Id, navRight: goNextLevel.Id))
            {
                coroutineRunner.RunTask(zoneManager.GoPrevious());
                explorationMenu.RequestSubMenu(null);
            }

            //if (sharpGui.Button(toggleCamera, navUp: goPreviousLevel.Id, navDown: battle.Id))
            //{
            //    useFirstPersonCamera = !useFirstPersonCamera;
            //}

            int currentTime = (int)(timeClock.CurrentTimeMicro * Clock.MicroToSeconds / (60 * 60));
            if (sharpGui.Slider(currentHour, ref currentTime) || sharpGui.ActiveItem == currentHour.Id)
            {
                timeClock.CurrentTimeMicro = (long)currentTime * 60L * 60L * Clock.SecondsToMicro;
            }
            var time = TimeSpan.FromMilliseconds(timeClock.CurrentTimeMicro * Clock.MicroToMilliseconds);
            sharpGui.Text(currentHour.Rect.Right, currentHour.Rect.Top, timeClock.IsDay ? Engine.Color.Black : Engine.Color.White, $"Time: {time}");

            if (sharpGui.GamepadButtonEntered == GamepadButtonCode.XInput_B || sharpGui.KeyEntered == KeyboardButtonCode.KC_ESCAPE)
            {
                explorationMenu.RequestSubMenu(explorationMenu.RootMenu);
            }
        }
    }
}
