using Engine;
using Engine.Platform;
using SharpGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Menu
{
    interface IContextMenu
    {
        void ClearContext(Action<ContextMenuArgs> activatedCallback);
        void HandleContext(String title, Action<ContextMenuArgs> activatedCallback, GamepadId gamepadId);
        void Update();

        /// <summary>
        /// Force the context to clear. This should only be used for things like changing to the new game.
        /// </summary>
        void ForceClearContext();
    }

    class ContextMenuArgs
    {
        public GamepadId GamepadId { get; set; }
    }

    class ContextMenu : IContextMenu
    {
        private readonly ISharpGui sharpGui;
        private readonly IScaleHelper scaleHelper;
        private readonly IScreenPositioner screenPositioner;
        SharpButton contextButton = new SharpButton();
        private Action<ContextMenuArgs> activatedCallback;
        private GamepadId gamepadId;
        private ContextMenuArgs contextMenuArgs = new ContextMenuArgs();

        public ContextMenu(
            ISharpGui sharpGui,
            IScaleHelper scaleHelper,
            IScreenPositioner screenPositioner)
        {
            this.sharpGui = sharpGui;
            this.scaleHelper = scaleHelper;
            this.screenPositioner = screenPositioner;
        }

        public void HandleContext(String title, Action<ContextMenuArgs> activatedCallback, GamepadId gamepadId)
        {
            contextButton.Text = title;
            this.activatedCallback = activatedCallback;
            this.gamepadId = gamepadId;
        }

        public void ClearContext(Action<ContextMenuArgs> activatedCallback)
        {
            if (this.activatedCallback == activatedCallback)
            {
                this.activatedCallback = null;
            }
        }

        public void ForceClearContext()
        {
            this.activatedCallback = null;
        }

        public void Update()
        {
            if (activatedCallback == null)
            {
                return;
            }

            var layout = new MarginLayout(new IntPad(scaleHelper.Scaled(10)), contextButton);

            var desiredSize = layout.GetDesiredSize(sharpGui);
            layout.SetRect(screenPositioner.GetBottomRightRect(desiredSize));

            contextMenuArgs.GamepadId = gamepadId;

            if (sharpGui.Button(contextButton, gamepadId))
            {
                activatedCallback(contextMenuArgs);
            }
        }
    }
}
