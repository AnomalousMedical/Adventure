using Adventure.Items;
using Adventure.Services;
using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    internal class BattleItemMenu
    {
        private readonly IBattleScreenLayout battleScreenLayout;
        private readonly IBattleManager battleManager;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private readonly ILanguageService languageService;
        private ButtonColumn itemButtons = new ButtonColumn(25);

        public BattleItemMenu
        (
            IBattleScreenLayout battleScreenLayout, 
            IBattleManager battleManager, 
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner,
            ILanguageService languageService
        )
        {
            this.battleScreenLayout = battleScreenLayout;
            this.battleManager = battleManager;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
            this.languageService = languageService;
        }

        public bool UpdateGui(ISharpGui sharpGui, IBattleTarget user, Inventory inventory, IScopedCoroutine coroutine, ref BattlePlayer.MenuMode menuMode, Action<IBattleTarget, InventoryItem> itemSelectedCb, GamepadId gamepadId, SharpStyle style)
        {
            var didSomething = false;

            itemButtons.Margin = scaleHelper.Scaled(10);
            itemButtons.MaxWidth = scaleHelper.Scaled(900);
            itemButtons.Bottom = battleScreenLayout.DynamicButtonBottom;
            itemButtons.ScrollBarWidth = scaleHelper.Scaled(25);
            itemButtons.ScrollMargin = scaleHelper.Scaled(5);

            var selectedItem = itemButtons.Show<InventoryItem>(sharpGui
                , inventory.Items.Select(i => new ButtonColumnItem<InventoryItem>(languageService.Current.Items.GetText(i.InfoId), i))
                , inventory.Items.Count
                , s => battleScreenLayout.DynamicButtonLocation(s)
                , gamepadId,
                style: style);

            if(selectedItem != null)
            {
                if (selectedItem.Equipment != null)
                {
                    itemSelectedCb(user, selectedItem);
                }
                else
                {
                    coroutine.RunTask(async () =>
                    {
                        var target = await battleManager.GetTarget(true);
                        if (target != null)
                        {
                            itemSelectedCb(target, selectedItem);
                        }
                    });
                }
                menuMode = BattlePlayer.MenuMode.Root;
                didSomething = true;
            }

            if (!didSomething && sharpGui.IsStandardBackPressed(gamepadId))
            {
                menuMode = BattlePlayer.MenuMode.Root;
            }

            return didSomething;
        }
    }
}
