using Adventure.Assets.SoundEffects;
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
        ILanguageService languageService,
        ISoundEffectPlayer soundEffectPlayer
    )
    {
        SharpText prompt = new SharpText() { Color = Color.UIWhite, Layer = BuyMenu.UseItemMenuLayer };
        SharpButton buy = new SharpButton() { Text = "Yes", Layer = BuyMenu.UseItemMenuLayer };
        SharpButton cancel = new SharpButton() { Text = "No", Layer = BuyMenu.UseItemMenuLayer };
        private SharpPanel promptPanel = new SharpPanel();
        private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

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
               new ColumnLayout(new KeepWidthCenterLayout(new PanelLayout(promptPanel, prompt)),
                   new KeepWidthCenterLayout(new ColumnLayout(buy, cancel) { Margin = new IntPad(scaleHelper.Scaled(10)) })
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetCenterTopRect(desiredSize));

            sharpGui.Panel(promptPanel, panelStyle);
            sharpGui.Text(prompt);

            if (sharpGui.Button(buy, gamepadId, navUp: cancel.Id, navDown: cancel.Id))
            {
                if (persistence.Current.Party.Gold - SelectedItem.Cost > 0)
                {
                    soundEffectPlayer.PlaySound(BuySoundEffect.Instance);
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
                else
                {
                    soundEffectPlayer.PlaySound(ErrorSoundEffect.Instance);
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
        private readonly ISoundEffectPlayer soundEffectPlayer;
        private ButtonColumn itemButtons = new ButtonColumn(25, ItemButtonsLayer);
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpButton back = new SharpButton() { Text = "Back" };
        List<SharpText> infos;
        List<SharpText> descriptions;
        private int currentSheet;
        private SharpPanel descriptionPanel = new SharpPanel();
        private SharpPanel infoPanel = new SharpPanel();
        private SharpStyle panelStyle = new SharpStyle() { Background = Color.FromARGB(0xbb020202) };

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
            CharacterStyleService characterStyleService,
            ISoundEffectPlayer soundEffectPlayer
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
            this.soundEffectPlayer = soundEffectPlayer;
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

        public void Update(IExplorationMenu menu, GamepadId gamepadId)
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
                    var description = new SharpText() { Color = Color.UIWhite };
                    description.Text = MultiLineTextBuilder.CreateMultiLineString(languageService.Current.Items.GetDescription(item.Item.InfoId), scaleHelper.Scaled(520), sharpGui);

                    descriptions = new List<SharpText>();
                    descriptions.Add(description);
                    descriptions.Add(new SharpText($@"Gold: {persistence.Current.Party.Gold}") { Color = Color.UIWhite });
                    descriptions.Add(new SharpText($@"Cost: {item.Item.Cost}") { Color = item.Item.Cost > persistence.Current.Party.Gold ? Color.UIRed : Color.UIWhite });
                    if (item.Item.IsEquipment)
                    {
                        var equipment = item.Item.CreateItem?.Invoke();
                        if (equipment != null)
                        {
                            descriptions.Add(new SharpText(" \n") { Color = Color.UIWhite });
                            descriptions.AddRange(equipmentTextService.BuildEquipmentText(equipment));
                            descriptions.Add(new SharpText(" \nStat Changes") { Color = Color.UIWhite });
                            descriptions.AddRange(equipmentTextService.GetComparisonText(equipment, characterData));
                        }
                    }
                }
            }

            ILayoutItem layout;

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new PanelLayout(infoPanel,
               new ColumnLayout(infos) { Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5)) }
            ));
            layout.SetRect(screenPositioner.GetTopLeftRect(layout.GetDesiredSize(sharpGui)));

            layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new PanelLayout(descriptionPanel,
               new ColumnLayout(descriptions.Select(i => new KeepWidthRightLayout(i)))
               {
                   Margin = new IntPad(scaleHelper.Scaled(10), scaleHelper.Scaled(5), scaleHelper.Scaled(10), scaleHelper.Scaled(5))
               }
            ));
            layout.SetRect(screenPositioner.GetTopRightRect(layout.GetDesiredSize(sharpGui)));

            layout = new RowLayout(previous, next, back) { Margin = new IntPad(scaleHelper.Scaled(10)) };
            layout.SetRect(screenPositioner.GetBottomRightRect(layout.GetDesiredSize(sharpGui)));

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = screenPositioner.ScreenSize.Height;

            sharpGui.Panel(infoPanel, panelStyle);
            foreach (var info in infos)
            {
                sharpGui.Text(info);
            }

            sharpGui.Panel(descriptionPanel, panelStyle);
            foreach (var description in descriptions)
            {
                sharpGui.Text(description);
            }

            confirmBuyMenu.Update(characterData, gamepadId);

            if (allowChanges)
            {
                var lastItemIndex = itemButtons.FocusedIndex(sharpGui);
                var selectedItem = itemButtons.Show(sharpGui, shopItems, shopItems.Count, p => screenPositioner.GetCenterTopRect(p), gamepadId, navDown: previous.Id, navUp: previous.Id, style: currentCharacterStyle);
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
                    else
                    {
                        soundEffectPlayer.PlaySound(ErrorSoundEffect.Instance);
                    }
                }

                if (sharpGui.Button(previous, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton, navLeft: back.Id, navRight: next.Id, style: currentCharacterStyle) || sharpGui.IsStandardPreviousPressed(gamepadId))
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
                if (sharpGui.Button(next, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton, navLeft: previous.Id, navRight: back.Id, style: currentCharacterStyle) || sharpGui.IsStandardNextPressed(gamepadId))
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
                if (sharpGui.Button(back, gamepadId, navUp: itemButtons.BottomButton, navDown: itemButtons.TopButton, navLeft: next.Id, navRight: previous.Id, style: currentCharacterStyle) || sharpGui.IsStandardBackPressed(gamepadId))
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
