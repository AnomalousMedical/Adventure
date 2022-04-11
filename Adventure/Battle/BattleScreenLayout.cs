﻿using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Battle
{
    interface IBattleScreenLayout
    {
        ColumnLayout InfoColumn { get; }
        int DynamicButtonBottom { get; }

        void LayoutBattleMenu(params ILayoutItem[] items);
        public void LayoutBattleMenu(IEnumerable<ILayoutItem> items);
        void LayoutCommonItems();
        IntRect DynamicButtonLocation(IntSize2 s);
    }

    class BattleScreenLayout : IBattleScreenLayout
    {
        private readonly IScreenPositioner screenPositioner;
        private readonly IScaleHelper scaleHelper;
        private readonly ISharpGui sharpGui;

        private ILayoutItem battleMenuLayout;
        private ColumnLayout battleMenuColumn;

        private ILayoutItem infoColumnLayout;
        private ColumnLayout infoColumn;
        private IntRect infoColumnRect;

        public BattleScreenLayout(
            IScreenPositioner screenPositioner,
            IScaleHelper scaleHelper,
            ISharpGui sharpGui
        )
        {
            this.screenPositioner = screenPositioner;
            this.scaleHelper = scaleHelper;
            this.sharpGui = sharpGui;
            battleMenuColumn = new ColumnLayout() { Margin = new IntPad(scaleHelper.Scaled(10)) };
            battleMenuLayout =
                new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                new MaxWidthLayout(scaleHelper.Scaled(300),
                battleMenuColumn
                ));

            infoColumn = new ColumnLayout() { Margin = new IntPad(scaleHelper.Scaled(10)) };
            infoColumnLayout =
                new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
                infoColumn
                );
        }

        public void LayoutBattleMenu(params ILayoutItem[] items)
        {
            LayoutBattleMenu((IEnumerable<ILayoutItem>)items);
        }

        public void LayoutBattleMenu(IEnumerable<ILayoutItem> items)
        {
            battleMenuColumn.Add(items);
            var desiredSize = battleMenuLayout.GetDesiredSize(sharpGui);
            var rect = screenPositioner.GetBottomRightRect(desiredSize);
            rect.Top -= infoColumnRect.Height;
            battleMenuLayout.SetRect(rect);
            battleMenuColumn.Clear();
        }

        public void LayoutCommonItems()
        {
            var desiredSize = infoColumnLayout.GetDesiredSize(sharpGui);
            infoColumnRect = screenPositioner.GetBottomRightRect(desiredSize);
            infoColumnLayout.SetRect(infoColumnRect);
        }

        public IntRect DynamicButtonLocation(IntSize2 s)
        {
            return new IntRect(screenPositioner.ScreenSize.Width - s.Width, infoColumnRect.Top - s.Height, s.Width, s.Height);
        }

        public ColumnLayout InfoColumn => infoColumn;

        public int DynamicButtonBottom => infoColumnRect.Top;
    }
}
