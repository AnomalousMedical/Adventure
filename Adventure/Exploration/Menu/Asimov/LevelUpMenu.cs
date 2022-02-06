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
        private readonly ILevelCalculator levelCalculator;
        SharpButton levelFighter = new SharpButton() { Text = "Level Up Fighter" };
        SharpButton levelMage = new SharpButton() { Text = "Level Up Mage" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText();
        private int currentSheet;

        public LevelUpMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            Persistence persistence,
            ILevelCalculator levelCalculator)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.persistence = persistence;
            this.levelCalculator = levelCalculator;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Party.Members[currentSheet];

            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(levelFighter, levelMage, new RowLayout(previous, next), back) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            info.Text = $@"{sheet.CharacterSheet.Name}
Lvl: {sheet.CharacterSheet.Level}
HP:  {sheet.CharacterSheet.Hp}
MP:  {sheet.CharacterSheet.Mp}
Str: {sheet.CharacterSheet.BaseStrength}
Mag: {sheet.CharacterSheet.BaseMagic}
Vit: {sheet.CharacterSheet.BaseVitality}
Spr: {sheet.CharacterSheet.BaseSpirit}
Dex: {sheet.CharacterSheet.BaseDexterity}
Lck: {sheet.CharacterSheet.Luck}";
            sharpGui.Text(info);

            if (sharpGui.Button(levelFighter))
            {
                sheet.CharacterSheet.LevelUpFighter(levelCalculator);
            }
            if (sharpGui.Button(levelMage))
            {
                sheet.CharacterSheet.LevelUpMage(levelCalculator);
            }
            if (sharpGui.Button(previous))
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next))
            {
                ++currentSheet;
                if (currentSheet >= persistence.Party.Members.Count)
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
