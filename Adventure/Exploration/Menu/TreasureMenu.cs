﻿using Adventure.Services;
using Engine;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Exploration.Menu
{
    class TreasureMenu : IExplorationSubMenu
    {
        private readonly Persistence persistence;
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        private Stack<ITreasure> currentTreasure;
        SharpButton take = new SharpButton();
        SharpButton discard = new SharpButton() { Text = "Discard" };
        SharpButton next = new SharpButton() { Text = "Next" };
        SharpButton previous = new SharpButton() { Text = "Previous" };
        SharpText info = new SharpText() { Color = Color.White };
        private int currentSheet;

        public TreasureMenu
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

        public void GatherTreasures(IEnumerable<ITreasure> treasure)
        {
            this.currentTreasure = new Stack<ITreasure>(treasure);
        }

        public void Update(IExplorationGameState explorationGameState, IExplorationMenu menu)
        {
            if (currentTreasure == null || currentTreasure.Count == 0)
            {
                menu.RequestSubMenu(null);
                return;
            }

            if (currentSheet > persistence.Current.Party.Members.Count)
            {
                currentSheet = 0;
            }
            var sheet = persistence.Current.Party.Members[currentSheet];

            take.Text = $"Take {sheet.CharacterSheet.Name}";

            var layout =
               new MarginLayout(new IntPad(scaleHelper.Scaled(10)),
               new MaxWidthLayout(scaleHelper.Scaled(600),
               new ColumnLayout(take, discard, new RowLayout(previous, next)) { Margin = new IntPad(scaleHelper.Scaled(10)) }
            ));

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));
            var treasure = currentTreasure.Peek();

            info.Text = treasure.InfoText;
            info.Rect = screenPositioner.GetCenterRect(info.GetDesiredSize(sharpGui));
            sharpGui.Text(info);

            var hasInventoryRoom = sheet.HasRoom;

            if (hasInventoryRoom && sharpGui.Button(take, navUp: previous.Id, navDown: discard.Id))
            {
                currentTreasure.Pop();
                treasure.GiveTo(sheet.Inventory);
            }

            if (sharpGui.Button(discard, navUp: hasInventoryRoom ? take.Id : previous.Id, navDown: previous.Id))
            {
                currentTreasure.Pop();
            }

            var bottomNavDown = hasInventoryRoom ? take.Id : discard.Id;
            if (sharpGui.Button(previous, navUp: discard.Id, navDown: bottomNavDown, navLeft: next.Id, navRight: next.Id) || sharpGui.IsStandardPreviousPressed())
            {
                --currentSheet;
                if (currentSheet < 0)
                {
                    currentSheet = persistence.Current.Party.Members.Count - 1;
                }
            }
            if (sharpGui.Button(next, navUp: discard.Id, navDown: bottomNavDown, navLeft: previous.Id, navRight: previous.Id) || sharpGui.IsStandardNextPressed())
            {
                ++currentSheet;
                if (currentSheet >= persistence.Current.Party.Members.Count)
                {
                    currentSheet = 0;
                }
            }
        }
    }
}
