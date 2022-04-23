using Adventure.Services;
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
        private readonly SharpButton[] characters = new[] { new SharpButton(), new SharpButton(), new SharpButton(), new SharpButton() };

        private readonly SharpButton[] players = new[] 
        { 
            new SharpButton() { Text = "Player 1" }, 
            new SharpButton() { Text = "Player 2" }, 
            new SharpButton() { Text = "Player 3" }, 
            new SharpButton() { Text = "Player 4" }
        };

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
                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(players.Append(close)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
                layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

                if (sharpGui.Button(players[0], gamepadId, navUp: close.Id, navDown: players[1].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 0;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(players[1], gamepadId, navUp: players[0].Id, navDown: players[2].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 1;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(players[2], gamepadId, navUp: players[1].Id, navDown: players[3].Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 2;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                if (sharpGui.Button(players[3], gamepadId, navUp: players[2].Id, navDown: close.Id))
                {
                    persistence.Current.Party.Members[selectedCharacter].Player = 3;
                    selectedCharacter = NoSelectedCharacter;
                    zoneManager.ManagePlayers();
                }

                closeNavUp = players[3].Id;
                closeNavDown = players[0].Id;
            }
            else
            {
                for(var i = 0; i < characters.Length; i++)
                {
                    var character = persistence.Current.Party.Members[i];
                    characters[i].Text = $"{character.CharacterSheet.Name}: Player {character.Player + 1}";
                }

                layout =
                   new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                   new MaxWidthLayout(scaleHelper.Scaled(600),
                   new ColumnLayout(characters.Append(close)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
                ));
                layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

                if (sharpGui.Button(characters[0], gamepadId, navUp: close.Id, navDown: characters[1].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 0;
                }

                if (sharpGui.Button(characters[1], gamepadId, navUp: characters[0].Id, navDown: characters[2].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 1;
                }

                if (sharpGui.Button(characters[2], gamepadId, navUp: characters[1].Id, navDown: characters[3].Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 2;
                }

                if (sharpGui.Button(characters[3], gamepadId, navUp: characters[2].Id, navDown: close.Id) && selectedCharacter == NoSelectedCharacter)
                {
                    selectedCharacter = 3;
                }

                closeNavUp = characters[3].Id;
                closeNavDown = characters[0].Id;
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
