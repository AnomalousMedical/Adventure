using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    class ConfirmBuyMenu
    (
        Persistence persistence,
        ISharpGui sharpGui,
        IScaleHelper scaleHelper,
        IScreenPositioner screenPositioner,
        ILanguageService languageService
    )
    {
        SharpText prompt = new SharpText() { Color = Color.White, Layer = BuyMenu.UseItemMenuLayer };
        SharpButton buy = new SharpButton() { Text = "Yes", Layer = BuyMenu.UseItemMenuLayer };
        SharpButton cancel = new SharpButton() { Text = "No", Layer = BuyMenu.UseItemMenuLayer };

        public event Action Closed;

        public ShopEntry SelectedItem { get; set; }

        public void Update(Persistence.CharacterData characterData, GamepadId gamepadId)
        {
            if (SelectedItem == null) { return; }

            if (sharpGui.FocusedItem != cancel.Id
               && sharpGui.FocusedItem != buy.Id)
            {
                sharpGui.StealFocus(buy.Id);
            }

            prompt.Text = $"Should {characterData.CharacterSheet.Name} buy {languageService.Current.Items.GetText(SelectedItem.InfoId)} for {SelectedItem.Cost} gold?";

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new KeepWidthCenterLayout(prompt), buy, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));

            sharpGui.Text(prompt);

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
                FireClosed();
            }
            if (sharpGui.Button(cancel, gamepadId, navUp: buy.Id, navDown: buy.Id) || sharpGui.IsStandardBackPressed(gamepadId))
            {
                FireClosed();
            }
        }

        private void FireClosed()
        {
            this.SelectedItem = null;
            Closed?.Invoke();
        }
    }

    class BuyMenu : IExplorationSubMenu, IDisposable
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
        private readonly CharacterStatsTextService characterStatsTextService;
        private readonly EquipmentTextService equipmentTextService;
        private readonly CameraMover cameraMover;
        private readonly CharacterMenuPositionService characterMenuPositionService;
        private readonly CharacterStyleService characterStyleService;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        List<SharpText> infos;
        List<SharpText> descriptions;
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
            ILanguageService languageService,
            CharacterStatsTextService characterStatsTextService,
            EquipmentTextService equipmentTextService,
            CameraMover cameraMover,
            CharacterMenuPositionService characterMenuPositionService,
            CharacterStyleService characterStyleService
        )
        {
            this.persistence = persistence;
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.confirmBuyMenu = confirmBuyMenu;
            this.worldDatabase = worldDatabase;
            this.languageService = languageService;
            this.characterStatsTextService = characterStatsTextService;
            this.equipmentTextService = equipmentTextService;
            this.cameraMover = cameraMover;
            this.characterMenuPositionService = characterMenuPositionService;
            this.characterStyleService = characterStyleService;
            this.confirmBuyMenu.Closed += ConfirmBuyMenu_Closed;
        }

        public void Dispose()
        {

            this.confirmBuyMenu.Closed -= ConfirmBuyMenu_Closed;
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
            var currentCharacterStyle = characterStyleService.GetCharacterStyle(characterData.StyleIndex);

            if (characterMenuPositionService.TryGetEntry(characterData.CharacterSheet, out var characterMenuPosition))
            {
                cameraMover.SetInterpolatedGoalPosition(characterMenuPosition.CameraPosition, characterMenuPosition.CameraRotation);
                characterMenuPosition.FaceCamera();
            }

            if (infos == null)
            {
                infos = characterStatsTextService.GetFullStats(characterData).ToList();
            }

            var shopItems = worldDatabase.CreateShopItems(CurrentShopType, persistence.Current.PlotItems)
                .Select(i => new ButtonColumnItem<ShopEntry>($"{languageService.Current.Items.GetText(i.InfoId)} - {i.Cost}", i))
                .ToList();

            if (descriptions == null)
            {
                var descriptionIndex = itemButtons.FocusedIndex(sharpGui);
                if (descriptionIndex < shopItems.Count)
                {
                    var item = shopItems[descriptionIndex];
                    var description = new SharpText() { Color = Color.White };
                    description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(item.Item.InfoId), scaleHelper.Scaled(520), sharpGui);

                    descriptions = new List<SharpText>();
                    descriptions.Add(description);
                    descriptions.Add(new SharpText($@"Gold: {persistence.Current.Party.Gold}") { Color = Color.White});
                    descriptions.Add(new SharpText($@"Cost: {item.Item.Cost}") { Color = item.Item.Cost > persistence.Current.Party.Gold ? Color.Red : Color.White });
                    if (item.Item.IsEquipment)
                    {
                        var equipment = item.Item.CreateItem?.Invoke();
                        if (equipment != null)
                        {
                            descriptions.Add(new SharpText(" \n") { Color = Color.White });
                            descriptions.AddRange(equipmentTextService.BuildEquipmentText(equipment));
                            descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.White });
                            descriptions.AddRange(equipmentTextService.GetComparisonText(equipment, characterData));
                        }
                    }
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(new ILayoutItem[] { new KeepWidthLeftLayout(previous) }.Concat(infos)) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            IEnumerable<ILayoutItem> columnItems = new[] { new KeepWidthRightLayout(next) };
            if (descriptions != null)
            {
                columnItems = columnItems.Concat(descriptions.Select(i => new KeepWidthRightLayout(i)));
            }

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(columnItems)
               {
                   Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
               }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), back);
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            foreach (var info in infos)
            {
                sharpGui.Text(info);
            }
            foreach(var description in descriptions)
            {
                sharpGui.Text(description);
            }

            confirmBuyMenu.Update(characterData, gamepadId);

            if (allowChanges)
            {
                var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
                var selectedItem = itemButtons.Show(sharpGui, shopItems, shopItems.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, navLeft: previous.Id, navRight: next.Id, style: currentCharacterStyle);
                if (lastItemIndex != itemButtons.FocusedIndex(sharpGui))
                {
                    descriptions = null;
                    infos = null;
                }
                if (selectedItem != null)
                {
                    var canBuy = (selectedItem.CreateItem == null || characterData.HasRoom) && persistence.Current.Party.Gold - selectedItem.Cost > 0;
                    if (canBuy)
                    {
                        confirmBuyMenu.SelectedItem = selectedItem;
                    }
                }

                if (sharpGui.Button(previous, gamepadId, navUp: back.Id, navDown: back.Id, navLeft: next.Id, navRight: itemButtons.TopButton, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepadId))
                {
                    if (allowChanges)
                    {
                        --currentSheet;
                        if (currentSheet < 0)
                        {
                            currentSheet = persistence.Current.Party.Members.Count - 1;
                        }
                        descriptions = null;
                        infos = null;
                    }
                }
                if (sharpGui.Button(next, gamepadId, navUp: back.Id, navDown: back.Id, navLeft: itemButtons.TopButton, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepadId))
                {
                    if (allowChanges)
                    {
                        ++currentSheet;
                        if (currentSheet >= persistence.Current.Party.Members.Count)
                        {
                            currentSheet = 0;
                        }
                        descriptions = null;
                        infos = null;
                    }
                }
                if (sharpGui.Button(back, gamepadId, navUp: next.Id, navDown: next.Id, navLeft: itemButtons.TopButton, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepadId))
                {
                    if (allowChanges)
                    {
                        //This order with the task is very important
                        var tempTask = menuClosedTask;
                        menuClosedTask = null;
                        menu.RequestSubMenu(PreviousMenu, gamepadId);
                        tempTask?.SetResult();
                        descriptions = null;
                        infos = null;
                    }
                }
            }
        }

        private void ConfirmBuyMenu_Closed()
        {
            descriptions = null;
            infos = null;
        }
    }
}
