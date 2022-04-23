﻿using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    internal class PlayerMenu : IExplorationSubMenu
    {
        private readonly IScaleHelper scaleHelper;
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;
        private readonly IZoneManager zoneManager;
        private readonly SharpButton[] buttons = new[] { new SharpButton(), new SharpButton(), new SharpButton(), new SharpButton() };

        private readonly SharpButton close = new SharpButton() { Text = "Close" };

        private const int NoSelectedCharacter = -1;
        private int selectedCharacter = NoSelectedCharacter;

        public PlayerMenu
        (
            IScaleHelper scaleHelper,
            Persistence persistence,
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner,
            IZoneManager zoneManager
        )
        {
            this.scaleHelper = scaleHelper;
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
            this.zoneManager = zoneManager;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
        {
            ILayoutItem layout;
            Guid closeNavDown;
            Guid closeNavUp;

            if(selectedCharacter != NoSelectedCharacter)
            {
                for (var i = 0; i < buttons.Length; i++)
                {
                    var character = persistence.Current.Party.Members[i];
                    buttons[i].Text = $"Player {character.Player + 1}";
                }

                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(buttons.Append(close)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
                layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

                if (sharpGui.Button(buttons[0], gamepadId, navUp: close.Id, navDown: buttons[1].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 0;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(buttons[1], gamepadId, navUp: buttons[0].Id, navDown: buttons[2].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 1;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(buttons[2], gamepadId, navUp: buttons[1].Id, navDown: buttons[3].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 2;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(buttons[3], gamepadId, navUp: buttons[2].Id, navDown: close.Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 3;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                closeNavUp = buttons[3].Id;
                closeNavDown = buttons[0].Id;
            }
            else
            {
                for(var i = 0; i < buttons.Length; i++)
                {
                    var character = persistence.Current.Party.Members[i];
                    buttons[i].Text = $"{character.CharacterSheet.Name}: Player {character.Player + 1}";
                }

                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(buttons.Append(close)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
                layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

                if (sharpGui.Button(buttons[0], gamepadId, navUp: close.Id, navDown: buttons[1].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 0;
                }

                if (sharpGui.Button(buttons[1], gamepadId, navUp: buttons[0].Id, navDown: buttons[2].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 1;
                }

                if (sharpGui.Button(buttons[2], gamepadId, navUp: buttons[1].Id, navDown: buttons[3].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 2;
                }

                if (sharpGui.Button(buttons[3], gamepadId, navUp: buttons[2].Id, navDown: close.Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 3;
                }

                closeNavUp = buttons[3].Id;
                closeNavDown = buttons[0].Id;
            }

            if (sharpGui.Button(close, gamepadId, navUp: closeNavUp, navDown: closeNavDown) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                if (selectedCharacter != NoSelectedCharacter)
                {
                    selectedCharacter = NoSelectedCharacter;
                }
                else
                {
                    menu.RequestSubMenu(null, gamepadId);
                }
            }
        }
    }
}
