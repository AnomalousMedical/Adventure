using Adventure.Items;
using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class BuyMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IZoneManager zoneManager;
        private ButtonColumn itemButtons = new ButtonColumn(25);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;

        public BuyMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            IZoneManager zoneManager
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.zoneManager = zoneManager;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (currentSheet > persistence.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Party.Members[currentSheet];

            info.Rect = new IntRect(scaleHelper.Scaled(10), scaleHelper.Scaled(10), scaleHelper.Scaled(500), scaleHelper.Scaled(500));

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new RowLayout(previous, next), back) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            sharpGui.Text(info);
            info.Text = 
$@"{characterData.CharacterSheet.Name}
 
Lvl: {characterData.CharacterSheet.Level}

HP:  {characterData.CharacterSheet.CurrentHp} / {characterData.CharacterSheet.Hp}
MP:  {characterData.CharacterSheet.CurrentMp} / {characterData.CharacterSheet.Mp}
 
Att:   {characterData.CharacterSheet.Attack}
Att%:  {characterData.CharacterSheet.AttackPercent}
MAtt:  {characterData.CharacterSheet.MagicAttack}
MAtt%: {characterData.CharacterSheet.MagicAttackPercent}
Def:   {characterData.CharacterSheet.Defense}
Def%:  {characterData.CharacterSheet.DefensePercent}
MDef:  {characterData.CharacterSheet.MagicDefense}
MDef%: {characterData.CharacterSheet.MagicDefensePercent}
 
Str: {characterData.CharacterSheet.BaseStrength}
Mag: {characterData.CharacterSheet.BaseMagic}
Vit: {characterData.CharacterSheet.BaseVitality}
Spr: {characterData.CharacterSheet.BaseSpirit}
Dex: {characterData.CharacterSheet.BaseDexterity}
Lck: {characterData.CharacterSheet.Luck}";

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;
            var selectedItem = itemButtons.Show(sharpGui, ShopItems(), characterData.Inventory.Items.Count, p => screenPositioner.GetCenterTopRect(p), navLeft: next.Id, navRight: previous.Id);

            var hasItems = characterData.Inventory.Items.Count > 0;

            if (selectedItem != null)
            {
                //characterData.Inventory.Use(selectedItem, characterData.CharacterSheet);
            }

            if (sharpGui.Button(previous, navUp: back.Id, navDown: back.Id, navLeft: hasItems ? itemButtons.TopButton : next.Id, navRight: next.Id) || sharpGui.IsStandardPreviousPressed())
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, navUp: back.Id, navDown: back.Id, navLeft: previous.Id, navRight: hasItems ? itemButtons.TopButton : previous.Id) || sharpGui.IsStandardNextPressed())
            {
                ++currentSheet;
                if (currentSheet >= persistence.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }
            if (sharpGui.Button(back, navUp: previous.Id, navDown: previous.Id, navLeft: hasItems ? itemButtons.TopButton : back.Id, navRight: hasItems ? itemButtons.TopButton : back.Id) || sharpGui.IsStandardBackPressed())
            {
                menu.RequestSubMenu(menu.RootMenu);
            }
        }

        private IEnumerable<ButtonColumnItem<String>> ShopItems()
        {
            var level = zoneManager.Current.EnemyLevel;

            yield return new ButtonColumnItem<String>() { Text = "Item 0" };
            yield return new ButtonColumnItem<String>() { Text = "Item 1" };
        }
    }
}
