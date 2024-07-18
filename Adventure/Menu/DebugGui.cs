using Adventure.Services;
using Engine;
using Engine.Platform;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    interface IDebugGui : IExplorationSubMenu
    {
        void Link(IExplorationGameState explorationGameState);
    }

    class DebugGui
    (
        ISharpGui sharpGui,
        IScaleHelper scaleHelper,
        IScreenPositioner screenPositioner,
        IZoneManager zoneManager,
        ITimeClock timeClock,
        Party party,
        FlyCameraManager flyCameraManager,
        Persistence persistence,
        IAchievementService achievementService
    //TextDialog textDialog
    ) : IDebugGui, IExplorationSubMenu
    {
        private IExplorationGameState explorationGameState;

        SharpButton goStart = new SharpButton() { Text = "Go Start" };
        SharpButton goEnd = new SharpButton() { Text = "Go End" };
        SharpButton goWorld = new SharpButton() { Text = "Go World" };
        SharpButton toggleCamera = new SharpButton() { Text = "Toggle Camera" };
        SharpButton levelWorld = new SharpButton() { Text = "Level World" };
        SharpButton allowBattle = new SharpButton() { Text = "Allow Battle" };
        SharpText averageLevel = new SharpText() { Color = Color.UIWhite };
        SharpText accountName = new SharpText(achievementService.AccountName) { Color = Color.UIWhite };
        SharpSliderHorizontal currentHour = new SharpSliderHorizontal() { Rect = scaleHelper.Scaled(new IntRect(100, 10, 500, 35)), Max = 24 };

        public void Link(IExplorationGameState explorationGameState)
        {
            this.explorationGameState = explorationGameState;
        }

        public void Update(IExplorationMenu explorationMenu, GamepadId gamepad)
        {
            averageLevel.Text = $"Zone: {zoneManager.Current?.Index} Level: {party.GetAverageLevel()} World: {persistence.Current.World.Level}";
            allowBattle.Text = explorationGameState.AllowBattles ? "Battles Allowed" : "Battles Disabled";
            toggleCamera.Text = flyCameraManager.Enabled ? "Disable Fly Camera" : "Enable Fly Camera";

            var layout =
                new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                new MaxWidthLayout(scaleHelper.Scaled(800),
                new ColumnLayout(averageLevel, new RowLayout(allowBattle), new RowLayout(levelWorld), new RowLayout(goStart, goEnd, goWorld), toggleCamera) { Margin = new IntPad(10) }
            ));
            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(averageLevel);

            if (sharpGui.Button(allowBattle, gamepad, navUp: toggleCamera.Id, navDown: levelWorld.Id, navLeft: currentHour.Id))
            {
                explorationGameState.AllowBattles = !explorationGameState.AllowBattles;
            }

            if (sharpGui.Button(levelWorld, gamepad, navUp: allowBattle.Id, navDown: goEnd.Id, navLeft: levelWorld.Id, navRight: levelWorld.Id))
            {
                explorationGameState.LevelUpWorld();
            }

            if (sharpGui.Button(goStart, gamepad, navUp: levelWorld.Id, navDown: toggleCamera.Id, navLeft: goWorld.Id, navRight: goEnd.Id))
            {
                zoneManager.GoStartPoint();
                explorationMenu.RequestSubMenu(null, gamepad);
            }

            if (sharpGui.Button(goEnd, gamepad, navUp: levelWorld.Id, navDown: toggleCamera.Id, navLeft: goStart.Id, navRight: goWorld.Id))
            {
                zoneManager.GoEndPoint();
                explorationMenu.RequestSubMenu(null, gamepad);
            }

            if (sharpGui.Button(goWorld, gamepad, navUp: levelWorld.Id, navDown: toggleCamera.Id, navLeft: goEnd.Id, navRight: goStart.Id))
            {
                explorationGameState.RequestWorldMap();
                explorationMenu.RequestSubMenu(null, gamepad);
            }

            if (sharpGui.Button(toggleCamera, gamepad, navUp: goEnd.Id, navDown: allowBattle.Id))
            {
                flyCameraManager.Enabled = !flyCameraManager.Enabled;
            }

            int currentTime = (int)(timeClock.CurrentTimeMicro * Clock.MicroToSeconds / (60 * 60));
            if (sharpGui.Slider(currentHour, ref currentTime, gamepad, navUp: allowBattle.Id, navDown: allowBattle.Id) || sharpGui.ActiveItem == currentHour.Id)
            {
                timeClock.CurrentTimeMicro = (long)currentTime * 60L * 60L * Clock.SecondsToMicro;
            }
            var time = TimeSpan.FromMilliseconds(timeClock.CurrentTimeMicro * Clock.MicroToMilliseconds);
            sharpGui.Text(currentHour.Rect.Right, currentHour.Rect.Top, timeClock.IsDay ? Color.Black : Color.UIWhite, $"Time: {time}");

            if (sharpGui.IsStandardBackPressed(gamepad))
            {
                explorationMenu.RequestSubMenu(explorationMenu.RootMenu, gamepad);
            }

            if(accountName.Text != null)
            {
                accountName.Color = timeClock.IsDay ? Color.Black : Color.UIWhite;
                var accountLayout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), accountName);
                accountLayout.SetRect(screenPositioner.GetTopRightRect(accountLayout.GetDesiredSize(sharpGui)));
                sharpGui.Text(accountName);
            }
        }
    }
}
