using Adventure.Services;
using Engine;
using RpgMath;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu.Asimov
{
    class LevelUpMenu : IExplorationSubMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly Persistence persistence;
        SharpButton levelFighter = new SharpButton() { Text = "Level Up Fighter" };
        SharpButton levelMage = new SharpButton() { Text = "Level Up Mage" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Rect = new IntRect(10, 10, 500, 500) };
        private int currentSheet;

        public LevelUpMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            Persistence persistence)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.persistence = persistence;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            if(currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Party.Members[currentSheet];

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(300),
               new ColumnLayout(levelFighter, levelMage, new RowLayout(previous, next), back) { Margin = new IntPad(10) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            info.Text = $@"{sheet.CharacterSheet.Name}
{sheet.CharacterSheet.Hp}
{sheet.CharacterSheet.Mp}
{sheet.CharacterSheet.BaseDexterity}";
            sharpGui.Text(info);

            if (sharpGui.Button(levelFighter))
            {
                
            }
            if (sharpGui.Button(levelMage))
            {

            }
            if (sharpGui.Button(previous))
            {
                --currentSheet;
                if(currentSheet < 0)
                {
                    currentSheet = persistence.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next))
            {
                ++currentSheet;
                if(currentSheet >= persistence.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }
            if (sharpGui.Button(back))
            {
                explorationMenu.RequestSubMenu(PreviousMenu);
            }
            else if (sharpGui.GamepadButtonEntered == Engine.Platform.GamepadButtonCode.XInput_B || sharpGui.KeyEntered == Engine.Platform.KeyboardButtonCode.KC_ESCAPE)
            {
                explorationMenu.RequestSubMenu(PreviousMenu);
            }
        }
    }
}
