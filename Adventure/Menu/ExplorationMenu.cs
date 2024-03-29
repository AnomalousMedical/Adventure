﻿using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    interface IExplorationMenu
    {
        IDebugGui DebugGui { get; }
        IRootMenu RootMenu { get; }

        void RequestSubMenu(IExplorationSubMenu subMenu, GamepadId gamepad);
        bool Update(IExplorationGameState explorationGameState);
    }

    class ExplorationMenu : IExplorationMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IDebugGui debugGui;
        private readonly IRootMenu rootMenu;
        
        private IExplorationSubMenu currentMenu = null;
        private GamepadId currentGamepad;

        public IDebugGui DebugGui => debugGui;
        public IRootMenu RootMenu => rootMenu;

        public ExplorationMenu(ISharpGui sharpGui, IDebugGui debugGui, IRootMenu rootMenu)
        {
            this.sharpGui = sharpGui;
            this.debugGui = debugGui;
            this.rootMenu = rootMenu;
        }

        /// <summary>
        /// Update the menu. Returns true if something was done. False if nothing was done and the menu wasn't shown
        /// </summary>
        /// <returns></returns>
        public bool Update(IExplorationGameState explorationGameState)
        {
            bool handled = false;
            if (currentMenu != null)
            {
                handled = true;
                currentMenu.Update(explorationGameState, this, currentGamepad);
            }
            else
            {
                if (sharpGui.GamepadButtonEntered[0] == Engine.Platform.GamepadButtonCode.XInput_Y || sharpGui.KeyEntered == Engine.Platform.KeyboardButtonCode.KC_TAB)
                {
                    RequestSubMenu(rootMenu, GamepadId.Pad1);
                    handled = true;
                }
                else if (sharpGui.GamepadButtonEntered[1] == Engine.Platform.GamepadButtonCode.XInput_Y)
                {
                    RequestSubMenu(rootMenu, GamepadId.Pad2);
                    handled = true;
                }
                else if (sharpGui.GamepadButtonEntered[2] == Engine.Platform.GamepadButtonCode.XInput_Y)
                {
                    RequestSubMenu(rootMenu, GamepadId.Pad3);
                    handled = true;
                }
                else if (sharpGui.GamepadButtonEntered[3] == Engine.Platform.GamepadButtonCode.XInput_Y)
                {
                    RequestSubMenu(rootMenu, GamepadId.Pad4);
                    handled = true;
                }
            }
            return handled;
        }

        public void RequestSubMenu(IExplorationSubMenu subMenu, GamepadId gamepad)
        {
            currentMenu = subMenu;
            currentGamepad = gamepad;
        }
    }
}
