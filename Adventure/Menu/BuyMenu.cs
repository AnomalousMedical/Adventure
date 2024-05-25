using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.Menu
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

        public void Update(Persistence.CharacterData characterData, GamepadId gamepadId)
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

            if (sharpGui.Button(buy, gamepadId, navUp: cancel.Id, navDown: cancel.Id))
            {
                if (persistence.Current.Party.Gold - SelectedItem.Cost > 0)
                {
                    persistence.Current.Party.Gold -= SelectedItem.Cost;
                    var item = SelectedItem.CreateItem?.Invoke();
                    if (item != null)
                    {
                        characterData.Inventory.Items.Insert(0, item);
                    }
                    if(SelectedItem.UniqueSalePlotItem != null)
                    {
                        persistence.Current.PlotItems.Add(SelectedItem.UniqueSalePlotItem.Value);
                    }
                }
                this.SelectedItem = null;
            }
            if (sharpGui.Button(cancel, gamepadId, navUp: buy.Id, navDown: buy.Id) || sharpGui.IsStandardBackPressed(gamepadId))
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
        private readonly ConfirmBuyMenu confirmBuyMenu;
        private readonly IWorldDatabase worldDatabase;
        private readonly ILanguageService languageService;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        SharpText info = new SharpText() { Color = Color.White };
        SharpText info2 = new SharpText() { Color = Color.White };
        private int currentSheet;

        private TaskCompletionSource menuClosedTask;

        public BuyMenu
        (
            Persistence persistence,
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ConfirmBuyMenu confirmBuyMenu,
            IWorldDatabase worldDatabase,
            ILanguageService languageService
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.confirmBuyMenu = confirmBuyMenu;
            this.worldDatabase = worldDatabase;
            this.languageService = languageService;
        }

        public IExplorationSubMenu PreviousMenu { get; set; }

        public ShopType CurrentShopType { get; set; }

        public Task WaitForClose()
        {
            if(menuClosedTask == null)
            {
                menuClosedTask = new TaskCompletionSource();
            }
            return menuClosedTask.Task;
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu, GamepadId gamepadId)
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
 
Str: {characterData.CharacterSheet.TotalStrength}
Mag: {characterData.CharacterSheet.TotalMagic}
Vit: {characterData.CharacterSheet.TotalVitality}
Spr: {characterData.CharacterSheet.TotalSpirit}
Dex: {characterData.CharacterSheet.TotalDexterity}
Lck: {characterData.CharacterSheet.TotalLuck}
";

            foreach (var item in characterData.CharacterSheet.EquippedItems())
            {
                info.Text += $@"
{languageService.Current.Items.GetText(item.InfoId)}";
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

            confirmBuyMenu.Update(characterData, gamepadId);

            var shopItems = worldDatabase.CreateShopItems(CurrentShopType, persistence.Current.PlotItems)
                .Select(i => new ButtonColumnItem<ShopEntry>($"{languageService.Current.Items.GetText(i.InfoId)} - {i.Cost}", i))
                .ToArray();
            var selectedItem = itemButtons.Show(sharpGui, shopItems, shopItems.Length, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: previous.Id, navRight: next.Id);
            if (selectedItem != null)
            {
                var canBuy = selectedItem.CreateItem == null || characterData.HasRoom;
                if (canBuy)
                {
                    confirmBuyMenu.SelectedItem = selectedItem;
                }
            }

            if (sharpGui.Button(previous, gamepadId, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: itemButtons.TopButton) || sharpGui.IsStandardPreviousPressed(gamepadId))
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
            if (sharpGui.Button(next, gamepadId, navUp: back.Id, navDown: back.Id, navLeft: itemButtons.TopButton, navRight: previous.Id) || sharpGui.IsStandardNextPressed(gamepadId))
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
            if (sharpGui.Button(back, gamepadId, navUp: next.Id, navDown: next.Id, navLeft: itemButtons.TopButton, navRight: previous.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                if (allowChanges)
                {
                    //This order with the task is very important
                    var tempTask = menuClosedTask;
                    menuClosedTask = null;
                    menu.RequestSubMenu(PreviousMenu, gamepadId);
                    tempTask?.SetResult();
                }
            }
        }
    }
}
