﻿using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Adventure.Menu;

interface IRootMenu : IExplorationSubMenu
{
    Task WaitForClose();
}

class RootMenu
(
    ISharpGui sharpGui,
    IScaleHelper scaleHelper,
    IScreenPositioner screenPositioner,
    ItemMenu itemMenu,
    SkillMenu skillMenu,
    OptionsMenu optionsMenu,
    CharacterStatsTextService characterStatsTextService,
    Persistence persistence,
    FileMenu fileMenu,
    GameOptions gameOptions,
    HelpMenu helpMenu,
    ILanguageService languageService,
    CharacterMenuPositionService characterMenuPositionService,
    CameraMover cameraMover
) : IRootMenu
{
    SharpButton skills = new SharpButton() { Text = languageService.Current.RootMenu.Skills };
    SharpButton items = new SharpButton() { Text = languageService.Current.RootMenu.Items };
    SharpButton files = new SharpButton() { Text = languageService.Current.RootMenu.Files };
    SharpButton help = new SharpButton() { Text = languageService.Current.RootMenu.Help };
    SharpButton options = new SharpButton() { Text = languageService.Current.RootMenu.Options };
    SharpButton debug = new SharpButton() { Text = languageService.Current.RootMenu.Debug };
    SharpButton close = new SharpButton() { Text = languageService.Current.RootMenu.Close };

    SharpText undefeated = new SharpText() { Text = languageService.Current.RootMenu.Undefeated, Color = Color.UIWhite };
    SharpText oldSchool = new SharpText() { Text = languageService.Current.RootMenu.OldSchool, Color = Color.UIWhite };
    SharpText gold = new SharpText() { Color = Color.UIWhite };
    SharpText timePlayed = new SharpText() { Color = Color.UIWhite };
    List<SharpText> infos;

    private TaskCompletionSource currentTask;

    private SharpPanel descriptionPanel = new SharpPanel();
    private SharpPanel infoPanel = new SharpPanel();
    private SharpStyle panelStyle = new SharpStyle() { Background = Color.UITransparentBg };

    public record Text
    (
        String Skills,
        String Items,
        String Files,
        String Help,
        String Options,
        String Debug,
        String Close,
        String Undefeated,
        String OldSchool,
        String Gold
    );

    public Task WaitForClose()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    private bool HasHelpBook => persistence.Current.PlotItems.Contains(PlotItems.GuideToPowerAndMayhem);

    private IEnumerable<SharpButton> GetMenuItems()
    {
        yield return skills;
        yield return items;
        yield return files;
        if (HasHelpBook)
        {
            yield return help;
        }
        yield return options;
        if (gameOptions.Debug)
        {
            yield return debug;
        }
        yield return close;
    }

    private IEnumerable<ILayoutItem> GetStatsItems()
    {
        if (persistence.Current.Party.Undefeated)
        {
            yield return undefeated;
        }
        if (persistence.Current.Party.OldSchool)
        {
            yield return oldSchool;
        }
        yield return gold;
        yield return timePlayed;
    }

    public void Update(IExplorationMenu explorationMenu, GamepadId gamepad)
    {
        characterMenuPositionService.GetNormalCameraPosition(out var camPos, out var camRot);
        cameraMover.SetInterpolatedGoalPosition(camPos, camRot);

        var time = TimeSpan.FromMilliseconds(persistence.Current.Time.Total * Clock.MicroToMilliseconds);
        timePlayed.Text = $"{(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";

        if (infos == null)
        {
            infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
        }

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
           new PanelLayout(infoPanel,
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        sharpGui.Panel(infoPanel, panelStyle);
        foreach (var info in infos)
        {
            sharpGui.Text(info);
        }

        gold.Text = persistence.Current.Party.Gold + languageService.Current.RootMenu.Gold;

        layout =
          new MarginLayout(new IntPad(scaleHelper.Scaled(20)),
          new PanelLayout(descriptionPanel,
          new ColumnLayout(GetStatsItems()) { Margin = new IntPad(scaleHelper.Scaled(5)) }
        ));
        var infoDesiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomLeftRect(infoDesiredSize));

        sharpGui.Panel(descriptionPanel, panelStyle);
        sharpGui.Text(timePlayed);
        sharpGui.Text(gold);
        if (persistence.Current.Party.Undefeated)
        {
            sharpGui.Text(undefeated);
        }
        if (persistence.Current.Party.OldSchool)
        {
            sharpGui.Text(oldSchool);
        }

        layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(300),
           new ColumnLayout(GetMenuItems()) { Margin = new IntPad(10) }
        ));

        var desiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

        if (sharpGui.Button(skills, gamepad, navDown: items.Id, navUp: close.Id))
        {
            infos = null;
            explorationMenu.RequestSubMenu(skillMenu, gamepad);
        }
        else if (sharpGui.Button(items, gamepad, navDown: files.Id, navUp: skills.Id))
        {
            infos = null;
            explorationMenu.RequestSubMenu(itemMenu, gamepad);
        }
        else if (sharpGui.Button(files, gamepad, navDown: HasHelpBook ? help.Id : options.Id, navUp: items.Id))
        {
            infos = null;
            fileMenu.PreviousMenu = this;
            explorationMenu.RequestSubMenu(fileMenu, gamepad);
        }
        else if (HasHelpBook && sharpGui.Button(help, gamepad, navDown: options.Id, navUp: files.Id))
        {
            infos = null;
            helpMenu.PreviousMenu = this;
            explorationMenu.RequestSubMenu(helpMenu, gamepad);
        }
        else if (sharpGui.Button(options, gamepad, navDown: gameOptions.Debug ? debug.Id : close.Id, navUp: HasHelpBook ? help.Id : files.Id))
        {
            infos = null;
            optionsMenu.PreviousMenu = this;
            explorationMenu.RequestSubMenu(optionsMenu, gamepad);
        }
        else if (gameOptions.Debug && sharpGui.Button(debug, gamepad, navDown: close.Id, navUp: options.Id))
        {
            infos = null;
            explorationMenu.RequestSubMenu(explorationMenu.DebugGui, gamepad);
        }
        else if (sharpGui.Button(close, gamepad, navDown: skills.Id, navUp: gameOptions.Debug ? debug.Id : options.Id) || sharpGui.IsStandardBackPressed(gamepad))
        {
            infos = null;
            explorationMenu.RequestSubMenu(null, gamepad);
            var tempTask = currentTask;
            currentTask = null;
            tempTask?.SetResult();
        }
    }
}
