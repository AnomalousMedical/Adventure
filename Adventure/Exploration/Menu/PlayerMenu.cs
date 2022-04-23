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
        private readonly Party party;
        private readonly ISharpGui sharpGui;
        private readonly IScreenPositioner screenPositioner;

        private readonly SharpButton[] characters = new[] { new SharpButton(), new SharpButton(), new SharpButton(), new SharpButton() };

        private readonly SharpButton player1 = new SharpButton(){ Text = "Player 1" };
        private readonly SharpButton player2 = new SharpButton(){ Text = "Player 2" };
        private readonly SharpButton player3 = new SharpButton(){ Text = "Player 3" };
        private readonly SharpButton player4 = new SharpButton(){ Text = "Player 4" };

        private readonly SharpButton close = new SharpButton() { Text = "Close" };

        public PlayerMenu
        (
            IScaleHelper scaleHelper, 
            Party party, 
            ISharpGui sharpGui,
            IScreenPositioner screenPositioner
        )
        {
            this.scaleHelper = scaleHelper;
            this.party = party;
            this.sharpGui = sharpGui;
            this.screenPositioner = screenPositioner;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
        {
            var i = 0;
            foreach(var character in party.ActiveCharacters)
            {
                if(i < characters.Length)
                {
                    characters[i].Text = $"{character.CharacterSheet.Name}: Player {character.Player + 1}";
                }
                ++i;
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(characters.Append(close)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            if (sharpGui.Button(characters[0], gamepadId, navUp: close.Id, navDown: characters[1].Id))
            {

            }

            if (sharpGui.Button(characters[1], gamepadId, navUp: characters[0].Id, navDown: characters[2].Id))
            {

            }

            if (sharpGui.Button(characters[2], gamepadId, navUp: characters[1].Id, navDown: characters[3].Id))
            {

            }

            if (sharpGui.Button(characters[3], gamepadId, navUp: characters[2].Id, navDown: close.Id))
            {

            }

            if (sharpGui.Button(close, gamepadId, navUp: characters[3].Id, navDown: characters[0].Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                menu.RequestSubMenu(null, gamepadId);
            }
        }
    }
}
