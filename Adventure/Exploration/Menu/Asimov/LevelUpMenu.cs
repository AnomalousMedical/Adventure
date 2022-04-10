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
        private readonly IZoneManager zoneManager;
        SharpButton levelFighter = new SharpButton() { Text = "Level Up Fighter" };
        SharpButton levelMage = new SharpButton() { Text = "Level Up Mage" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;

        public LevelUpMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            Persistence persistence,
            ILevelCalculator levelCalculator,
            IZoneManager zoneManager)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.persistence = persistence;
            this.levelCalculator = levelCalculator;
            this.zoneManager = zoneManager;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu explorationMenu)
        {
            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Current.Party.Members[currentSheet];

            long levelUpCost = 100;
            var currentLevel = sheet.CharacterSheet.Level;
            var zoneLevel = zoneManager.Current?.EnemyLevel ?? 0;
            var levelDiff = zoneLevel - currentLevel;
            if (levelDiff > 6)
            {
                levelUpCost = 0;
            }
            else if(levelDiff > 0)
            {
                levelUpCost = 100;
            }
            else
            {
                levelUpCost += levelDiff * -50;
            }

            info.Text = $@"{sheet.CharacterSheet.Name}
 
Lvl: {currentLevel}
Zone Level: {zoneLevel}
 
Level Cost: {levelUpCost}
 
HP:  {sheet.CharacterSheet.Hp}
MP:  {sheet.CharacterSheet.Mp}
Str: {sheet.CharacterSheet.BaseStrength}
Mag: {sheet.CharacterSheet.BaseMagic}
Vit: {sheet.CharacterSheet.BaseVitality}
Spr: {sheet.CharacterSheet.BaseSpirit}
Dex: {sheet.CharacterSheet.BaseDexterity}
Lck: {sheet.CharacterSheet.Luck}";

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(previous, info) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(next) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(levelFighter, levelMage) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetCenterTopRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            sharpGui.Text(info);

            if (sharpGui.Button(levelFighter, navUp: levelMage.Id, navDown: levelMage.Id, navLeft: previous.Id, navRight: next.Id))
            {
                if (levelUpCost == 0 || persistence.Current.Party.Gold - levelUpCost > 0)
                {
                    sheet.CharacterSheet.LevelUpFighter(levelCalculator);
                    persistence.Current.Party.Gold -= levelUpCost;
                }
            }
            if (sharpGui.Button(levelMage, navUp: levelFighter.Id, navDown: levelFighter.Id, navLeft: previous.Id, navRight: next.Id))
            {
                if (levelUpCost == 0 || persistence.Current.Party.Gold - levelUpCost > 0)
                {
                    sheet.CharacterSheet.LevelUpMage(levelCalculator);
                    persistence.Current.Party.Gold -= levelUpCost;
                }
            }
            if (sharpGui.Button(previous, navLeft: next.Id, navRight: levelFighter.Id) || sharpGui.IsStandardPreviousPressed())
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Current.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, navUp: back.Id, navDown: back.Id, navLeft: levelFighter.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed())
            {
                ++currentSheet;
                if (currentSheet >= persistence.Current.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }
            if (sharpGui.Button(back, navUp: next.Id, navDown: next.Id))
            {
                explorationMenu.RequestSubMenu(PreviousMenu);
            }
            else if (sharpGui.IsStandardBackPressed())
            {
                explorationMenu.RequestSubMenu(PreviousMenu);
            }
        }
    }
}
