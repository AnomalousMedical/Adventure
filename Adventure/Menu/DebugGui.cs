﻿using Adventure.Services;
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
    }

    class DebugGui : IDebugGui, IExplorationSubMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IZoneManager zoneManager;
        private readonly ITimeClock timeClock;
        private readonly Party party;
        private readonly ILevelCalculator levelCalculator;
        private readonly FlyCameraManager flyCameraManager;
        private readonly Persistence persistence;
        SharpButton goStart = new SharpButton() { Text = "Go Start" };
        SharpButton goEnd = new SharpButton() { Text = "Go End" };
        SharpButton goWorld = new SharpButton() { Text = "Go World" };
        SharpButton toggleCamera = new SharpButton() { Text = "Toggle Camera" };
        SharpButton levelWorld = new SharpButton() { Text = "Level World" };
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
            FlyCameraManager flyCameraManager,
            Persistence persistence
            //TextDialog textDialog
        )
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.zoneManager = zoneManager;
            this.timeClock = timeClock;
            this.party = party;
            this.levelCalculator = levelCalculator;
            this.flyCameraManager = flyCameraManager;
            this.persistence = persistence;
            currentHour = new SharpSliderHorizontal() { Rect = scaleHelper.Scaled(new IntRect(100, 10, 500, 35)), Max = 24 };
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu, GamepadId gamepad)
        {
            averageLevel.Text = $"Zone: {zoneManager.Current?.Index} Level: {party.GetAverageLevel()} World: {persistence.Current.World.Level}";
            allowBattle.Text = explorationGameState.AllowBattles ? "Battles Allowed" : "Battles Disabled";
            toggleCamera.Text = flyCameraManager.Enabled ? "Disable Fly Camera" : "Enable Fly Camera";

            var layout =
                new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                new MaxWidthLayout(scaleHelper.Scaled(800),
                new ColumnLayout(averageLevel, new RowLayout(battle, allowBattle), new RowLayout(levelWorld), new RowLayout(goStart, goEnd, goWorld), toggleCamera) { Margin = new IntPad(10) }
            ));
            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(averageLevel);

            if (sharpGui.Button(battle, gamepad, navUp: toggleCamera.Id, navDown: goStart.Id, navRight: allowBattle.Id, navLeft: allowBattle.Id))
            {
                explorationGameState.RequestBattle();
                explorationMenu.RequestSubMenu(null, gamepad);
            }

            if (sharpGui.Button(allowBattle, gamepad, navUp: toggleCamera.Id, navDown: levelWorld.Id, navRight: battle.Id, navLeft: battle.Id))
            {
                explorationGameState.AllowBattles = !explorationGameState.AllowBattles;
            }

            if (sharpGui.Button(levelWorld, gamepad, navUp: allowBattle.Id, navDown: goEnd.Id, navLeft: levelWorld.Id, navRight: levelWorld.Id))
            {
                explorationGameState.LevelUpWorld();
            }

            if (sharpGui.Button(goStart, gamepad, navUp: battle.Id, navDown: toggleCamera.Id, navLeft: goWorld.Id, navRight: goEnd.Id))
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

            if (sharpGui.Button(toggleCamera, gamepad, navUp: goStart.Id, navDown: battle.Id))
            {
                flyCameraManager.Enabled = !flyCameraManager.Enabled;
            }

            int currentTime = (int)(timeClock.CurrentTimeMicro * Clock.MicroToSeconds / (60 * 60));
            if (sharpGui.Slider(currentHour, ref currentTime, gamepad) || sharpGui.ActiveItem == currentHour.Id)
            {
                timeClock.CurrentTimeMicro = (long)currentTime * 60L * 60L * Clock.SecondsToMicro;
            }
            var time = TimeSpan.FromMilliseconds(timeClock.CurrentTimeMicro * Clock.MicroToMilliseconds);
            sharpGui.Text(currentHour.Rect.Right, currentHour.Rect.Top, timeClock.IsDay ? Engine.Color.Black : Engine.Color.White, $"Time: {time}");

            if (sharpGui.IsStandardBackPressed(gamepad))
            {
                explorationMenu.RequestSubMenu(explorationMenu.RootMenu, gamepad);
            }
        }
    }
}
