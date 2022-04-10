using Adventure.Items;
using Adventure.Items.Creators;
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
    class ConfirmBuyMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        SharpButton buy = new SharpButton() { Text = "Buy", Layer = BuyMenu.UseItemMenuLayer };
        SharpButton cancel = new SharpButton() { Text = "Cancel", Layer = BuyMenu.UseItemMenuLayer };

        public ShopEntry SelectedItem { get; set; }

        public ConfirmBuyMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
        }

        public void Update(Persistence.CharacterData characterData)
        {
            if (SelectedItem == null) { return; }

            if (sharpGui.FocusedItem != cancel.Id
               && sharpGui.FocusedItem != buy.Id)
            {
                sharpGui.StealFocus(buy.Id);
            }

            buy.Text = $"Buy {SelectedItem.Cost} gold";

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(buy, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetCenterRect(desiredSize));

            if (sharpGui.Button(buy, navUp: cancel.Id, navDown: cancel.Id))
            {
                if (persistence.Current.Party.Gold - SelectedItem.Cost > 0)
                {
                    persistence.Current.Party.Gold -= SelectedItem.Cost;
                    var item = SelectedItem.CreateItem();
                    characterData.Inventory.Items.Add(item);
                }
                this.SelectedItem = null;
            }
            if (sharpGui.Button(cancel, navUp: buy.Id, navDown: buy.Id) || sharpGui.IsStandardBackPressed())
            {
                this.SelectedItem = null;
            }
        }
    }

    class BuyMenu : IExplorationSubMenu
    {
        public const float ItemButtonsLayer = 0.15f;
        public const float UseItemMenuLayer = 0.25f;

        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly IZoneManager zoneManager;
        private readonly ConfirmBuyMenu confirmBuyMenu;
        private readonly SwordCreator swordCreator;
        private readonly SpearCreator spearCreator;
        private readonly MaceCreator maceCreator;
        private readonly ShieldCreator shieldCreator;
        private readonly StaffCreator staffCreator;
        private readonly AccessoryCreator accessoryCreator;
        private readonly ArmorCreator armorCreator;
        private readonly PotionCreator potionCreator;
        private readonly AxeCreator axeCreator;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        SharpText info2 = new SharpText() { Color = Color.White };
        private int currentSheet;

        public BuyMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            IZoneManager zoneManager,
            ConfirmBuyMenu confirmBuyMenu,
            SwordCreator swordCreator,
            SpearCreator spearCreator,
            MaceCreator maceCreator,
            ShieldCreator shieldCreator,
            StaffCreator staffCreator,
            AccessoryCreator accessoryCreator,
            ArmorCreator armorCreator,
            PotionCreator potionCreator,
            AxeCreator axeCreator
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.zoneManager = zoneManager;
            this.confirmBuyMenu = confirmBuyMenu;
            this.swordCreator = swordCreator;
            this.spearCreator = spearCreator;
            this.maceCreator = maceCreator;
            this.shieldCreator = shieldCreator;
            this.staffCreator = staffCreator;
            this.accessoryCreator = accessoryCreator;
            this.armorCreator = armorCreator;
            this.potionCreator = potionCreator;
            this.axeCreator = axeCreator;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            bool allowChanges = confirmBuyMenu.SelectedItem == null;

            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var characterData = persistence.Current.Party.Members[currentSheet];

            info.Text =
$@"{characterData.CharacterSheet.Name}
 
Lvl: {characterData.CharacterSheet.Level}
 
Items:  {characterData.Inventory.Items.Count} / {characterData.CharacterSheet.InventorySize}
 
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
Lck: {characterData.CharacterSheet.Luck}
";

            foreach (var item in characterData.CharacterSheet.EquippedItems())
            {
                info.Text += $@"
{item.Name}";
            }

            info2.Text = $@"Gold: {persistence.Current.Party.Gold}";

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
               new ColumnLayout(next, info2) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            sharpGui.Text(info);
            sharpGui.Text(info2);

            confirmBuyMenu.Update(characterData);

            var canBuy = characterData.HasRoom;

            var shopItems = ShopItems().Select(i => new ButtonColumnItem<ShopEntry>($"{i.Text} - {i.Cost}", i)).ToArray(); //TODO: Cache this somehow, don't keep making it
            var selectedItem = itemButtons.Show(sharpGui, shopItems, shopItems.Length, p => screenPositioner.GetCenterTopRect(p), navLeft: previous.Id, navRight: next.Id);
            if (canBuy && selectedItem != null)
            {
                confirmBuyMenu.SelectedItem = selectedItem;
            }

            if (sharpGui.Button(previous, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: itemButtons.TopButton) || sharpGui.IsStandardPreviousPressed())
            {
                if (allowChanges)
                {
                    --currentSheet;
                    if (currentSheet < 0)
                    {
                        currentSheet = persistence.Current.Party.Members.Count - 1;
                    }
                }
            }
            if (sharpGui.Button(next, navUp: back.Id, navDown: back.Id, navLeft: itemButtons.TopButton, navRight: previous.Id) || sharpGui.IsStandardNextPressed())
            {
                if (allowChanges)
                {
                    ++currentSheet;
                    if (currentSheet >= persistence.Current.Party.Members.Count)
                    {
                        currentSheet = 0;
                    }
                }
            }
            if (sharpGui.Button(back, navUp: next.Id, navDown: next.Id, navLeft: itemButtons.TopButton, navRight: previous.Id) || sharpGui.IsStandardBackPressed())
            {
                if (allowChanges)
                {
                    menu.RequestSubMenu(PreviousMenu);
                }
            }
        }

        private IEnumerable<ShopEntry> ShopItems()
        {
            var level = zoneManager.Current.EnemyLevel;

            yield return potionCreator.CreateManaPotionShopEntry(level);

            if (level > 89)
            {
                foreach (var item in CreateShopLevel(90))
                {
                    yield return item;
                }
            }

            if (level > 79)
            {
                foreach (var item in CreateShopLevel(80))
                {
                    yield return item;
                }
            }

            if (level > 69)
            {
                foreach (var item in CreateShopLevel(70))
                {
                    yield return item;
                }
            }

            if (level > 59)
            {
                foreach (var item in CreateShopLevel(60))
                {
                    yield return item;
                }
            }

            if (level > 49)
            {
                foreach (var item in CreateShopLevel(50))
                {
                    yield return item;
                }
            }

            if (level > 39)
            {
                foreach (var item in CreateShopLevel(40))
                {
                    yield return item;
                }
            }

            if (level > 29)
            {
                foreach (var item in CreateShopLevel(30))
                {
                    yield return item;
                }
            }

            if (level > 19)
            {
                foreach (var item in CreateShopLevel(20))
                {
                    yield return item;
                }
            }

            if (level > 5) //A small cheat to give you access to the first tier a bit sooner.
            {
                foreach (var item in CreateShopLevel(10))
                {
                    yield return item;
                }
            }
        }

        private IEnumerable<ShopEntry> CreateShopLevel(int level)
        {
            yield return swordCreator.CreateShopEntry(level);
            yield return spearCreator.CreateShopEntry(level);
            yield return maceCreator.CreateShopEntry(level);
            yield return staffCreator.CreateShopEntry(level);
            yield return axeCreator.CreateShopEntry(level);
            yield return shieldCreator.CreateShopEntry(level);
        }
    }

    record ShopEntry(String Text, long Cost, Func<InventoryItem> CreateItem) { }
}
