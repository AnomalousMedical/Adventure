﻿using Adventure.Services;
using Adventure.WorldMap;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu;

internal class PlayerMenu
(
    IScaleHelper scaleHelper,
    Persistence persistence,
    ISharpGui sharpGui,
    IScreenPositioner screenPositioner,
    IZoneManager zoneManager,
    IWorldMapManager worldMapManager
) : IExplorationSubMenu
{
    private readonly SharpButton[] buttons = new[] { new SharpButton(), new SharpButton(), new SharpButton(), new SharpButton() };

    private readonly SharpButton back = new SharpButton() { Text = "Back" };

    private const int NoSelectedCharacter = -1;
    private int selectedCharacter = NoSelectedCharacter;

    public IExplorationSubMenu PreviousMenu { get; set; }

    public void Update(IExplorationMenu menu, GamepadId gamepadId)
    {
        //No party members, just close
        if(persistence.Current.Party.Members.Count == 0)
        {
            menu.RequestSubMenu(PreviousMenu, gamepadId);
            return;
        }

        ILayoutItem layout;
        Guid closeNavDown;
        Guid closeNavUp;

        if(selectedCharacter != NoSelectedCharacter)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].Text = $"Pad {i + 1}";
            }

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(buttons.Append(back)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            if (sharpGui.Button(buttons[0], gamepadId, navUp: back.Id, navDown: buttons[1].Id))
            {
                persistence.Current.Party.Members[selectedCharacter].Player = 0;
                EnsurePosition(0);
                selectedCharacter = NoSelectedCharacter;
                zoneManager.ManagePlayers();
                worldMapManager.CreatePlayersAndFollowers();
            }

            if (sharpGui.Button(buttons[1], gamepadId, navUp: buttons[0].Id, navDown: buttons[2].Id))
            {
                persistence.Current.Party.Members[selectedCharacter].Player = 1;
                EnsurePosition(1);
                selectedCharacter = NoSelectedCharacter;
                zoneManager.ManagePlayers();
                worldMapManager.CreatePlayersAndFollowers();
            }

            if (sharpGui.Button(buttons[2], gamepadId, navUp: buttons[1].Id, navDown: buttons[3].Id))
            {
                persistence.Current.Party.Members[selectedCharacter].Player = 2;
                EnsurePosition(2);
                selectedCharacter = NoSelectedCharacter;
                zoneManager.ManagePlayers();
                worldMapManager.CreatePlayersAndFollowers();
            }

            if (sharpGui.Button(buttons[3], gamepadId, navUp: buttons[2].Id, navDown: back.Id))
            {
                persistence.Current.Party.Members[selectedCharacter].Player = 3;
                EnsurePosition(3);
                selectedCharacter = NoSelectedCharacter;
                zoneManager.ManagePlayers();
                worldMapManager.CreatePlayersAndFollowers();
            }

            closeNavUp = buttons[3].Id;
            closeNavDown = buttons[0].Id;
        }
        else
        {
            var iterations = Math.Min(persistence.Current.Party.Members.Count, buttons.Length);
            for(var i = 0; i < iterations; i++)
            {
                var character = persistence.Current.Party.Members[i];
                buttons[i].Text = $"{character.CharacterSheet.Name}: Pad {character.Player + 1}";
            }

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(buttons.Append(back)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            if (iterations > 0 && sharpGui.Button(buttons[0], gamepadId, navUp: back.Id, navDown: buttons[1].Id) && selectedCharacter == NoSelectedCharacter)
            {
                selectedCharacter = 0;
            }

            if (iterations > 1 && sharpGui.Button(buttons[1], gamepadId, navUp: buttons[0].Id, navDown: buttons[2].Id) && selectedCharacter == NoSelectedCharacter)
            {
                selectedCharacter = 1;
            }

            if (iterations > 2 && sharpGui.Button(buttons[2], gamepadId, navUp: buttons[1].Id, navDown: buttons[3].Id) && selectedCharacter == NoSelectedCharacter)
            {
                selectedCharacter = 2;
            }

            if (iterations > 3 && sharpGui.Button(buttons[3], gamepadId, navUp: buttons[2].Id, navDown: back.Id) && selectedCharacter == NoSelectedCharacter)
            {
                selectedCharacter = 3;
            }

            closeNavUp = buttons[iterations - 1].Id;
            closeNavDown = buttons[0].Id;
        }

        if (sharpGui.Button(back, gamepadId, navUp: closeNavUp, navDown: closeNavDown) || sharpGui.IsStandardBackPressed(gamepadId))
        {
            if (selectedCharacter != NoSelectedCharacter)
            {
                selectedCharacter = NoSelectedCharacter;
            }
            else
            {
                menu.RequestSubMenu(PreviousMenu, gamepadId);
            }
        }
    }

    private void EnsurePosition(int index)
    {
        if (persistence.Current.Player.Position[index] == null)
        {
            persistence.Current.Player.Position[index] = persistence.Current.Player.Position.FirstOrDefault(i => i != null);
        }

        if (persistence.Current.Player.WorldPosition[index] == null)
        {
            persistence.Current.Player.WorldPosition[index] = persistence.Current.Player.WorldPosition.FirstOrDefault(i => i != null);
        }
    }
}
