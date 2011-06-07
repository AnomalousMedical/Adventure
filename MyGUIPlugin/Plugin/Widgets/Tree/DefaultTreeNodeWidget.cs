﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGUIPlugin
{
    class DefaultTreeNodeWidget : TreeNodeWidget
    {
        private Widget primaryWidget;
        private Button plusMinusButton;
        private Button mainButton;

        public DefaultTreeNodeWidget()
        {

        }

        public override void createWidget(Widget parent, String caption, String imageResource)
        {
            primaryWidget = parent.createWidgetT("Widget", "Widget", 0, 0, 26, 16, Align.Default, "") as Widget;

            plusMinusButton = primaryWidget.createWidgetT("Button", "ButtonMinusPlus", 0, 0, 16, 16, Align.Left | Align.HCenter, "") as Button;
            plusMinusButton.MouseButtonClick += new MyGUIEvent(plusMinusButton_MouseButtonClick);
            plusMinusButton.Visible = treeNode.Children.Count > 0;

            mainButton = primaryWidget.createWidgetT("Button", "TreeIconButton", 17, 0, 10, 16, Align.Stretch, "") as Button;
            mainButton.Caption = caption;
            StaticImage image = mainButton.StaticImage;
            if (image != null)
            {
                image.setItemResource(imageResource);
            }
        }

        public override void destroyWidget()
        {
            if (primaryWidget != null)
            {
                Gui.Instance.destroyWidget(primaryWidget);
                primaryWidget = null;
            }
        }

        public override void setCoord(int left, int top, int width, int height)
        {
            primaryWidget.setCoord(left, top, width, height);
        }

        public override void updateExpandedStatus(bool expanded)
        {
            plusMinusButton.StateCheck = !expanded;
            plusMinusButton.Visible = treeNode.Children.Count > 0;
        }

        void plusMinusButton_MouseButtonClick(Widget source, EventArgs e)
        {
            fireExpandToggled();
        }
    }
}
