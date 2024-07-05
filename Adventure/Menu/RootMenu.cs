using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
    ConfirmMenu confirmMenu,
    ICoroutineRunner coroutineRunner
) : IRootMenu
{
    SharpButton skills = new SharpButton() { Text = "Skills" };
    SharpButton items = new SharpButton() { Text = "Items" };
    SharpButton files = new SharpButton() { Text = "Files" };
    SharpButton options = new SharpButton() { Text = "Options" };
    SharpButton debug = new SharpButton() { Text = "Debug" };
    SharpButton feedback = new SharpButton() { Text = "Feedback" };
    SharpButton close = new SharpButton() { Text = "Close" };

    SharpText undefeated = new SharpText() { Text = "Undefeated", Color = Color.White };
    SharpText oldSchool = new SharpText() { Text = "Old School", Color = Color.White };
    SharpText timePlayed = new SharpText() { Color = Color.White };
    List<SharpText> infos;

    private TaskCompletionSource currentTask;

    public Task WaitForClose()
    {
        if (currentTask == null)
        {
            currentTask = new TaskCompletionSource();
        }
        return currentTask.Task;
    }

    private IEnumerable<SharpButton> GetMenuItems()
    {
        yield return skills;
        yield return items;
        yield return files;
        yield return options;
        if (gameOptions.Debug)
        {
            yield return debug;
        }
        yield return feedback;
        yield return close;
    }

    public void Update(IExplorationMenu explorationMenu, GamepadId gamepad)
    {
        var time = TimeSpan.FromMilliseconds(persistence.Current.Time.Total * Clock.MicroToMilliseconds);
        timePlayed.Text = $"{(time.Hours + time.Days * 24):00}:{time.Minutes:00}:{time.Seconds:00}";

        if (infos == null)
        {
            infos = characterStatsTextService.GetVitalStats(persistence.Current.Party.Members).ToList();
        }

        var layout =
           new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
           new MaxWidthLayout(scaleHelper.Scaled(600),
           new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
        ));
        layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

        foreach(var info in infos)
        {
            sharpGui.Text(info);
        }

        layout =
          new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
          new MaxWidthLayout(scaleHelper.Scaled(300),
          new ColumnLayout(undefeated, oldSchool, timePlayed) { Margin = new IntPad(10) }
        ));
        var infoDesiredSize = layout.GetDesiredSize(sharpGui);
        layout.SetRect(screenPositioner.GetBottomLeftRect(infoDesiredSize));

        sharpGui.Text(timePlayed);
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
        else if (sharpGui.Button(files, gamepad, navDown: options.Id, navUp: items.Id))
        {
            infos = null;
            fileMenu.PreviousMenu = this;
            explorationMenu.RequestSubMenu(fileMenu, gamepad);
        }
        else if (sharpGui.Button(options, gamepad, navDown: gameOptions.Debug ? debug.Id : feedback.Id, navUp: files.Id))
        {
            infos = null;
            optionsMenu.PreviousMenu = this;
            explorationMenu.RequestSubMenu(optionsMenu, gamepad);
        }
        else if (gameOptions.Debug && sharpGui.Button(debug, gamepad, navDown: feedback.Id, navUp: options.Id))
        {
            infos = null;
            explorationMenu.RequestSubMenu(explorationMenu.DebugGui, gamepad);
        }
        else if(sharpGui.Button(feedback, navDown: close.Id, navUp: gameOptions.Debug ? debug.Id : options.Id))
        {
            coroutineRunner.RunTask(async () =>
            {
                var open = await confirmMenu.ShowAndWait("This will open your browser. Do you want to continue?", null, gamepad);
                if (open)
                {
                    Process.Start(new ProcessStartInfo("https://docs.google.com/forms/d/e/1FAIpQLSd8TUYdRgNfZi6zIdTmRTC5IpoiHxHRaJtq0O8mOH1qlEnQ0A/viewform?usp=sf_link")
                    {
                        UseShellExecute = RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)
                    });
                }
            });
        }
        else if (sharpGui.Button(close, gamepad, navDown: skills.Id, navUp: feedback.Id) || sharpGui.IsStandardBackPressed(gamepad))
        {
            infos = null;
            explorationMenu.RequestSubMenu(null, gamepad);
            var tempTask = currentTask;
            currentTask = null;
            tempTask?.SetResult();
        }
    }
}
