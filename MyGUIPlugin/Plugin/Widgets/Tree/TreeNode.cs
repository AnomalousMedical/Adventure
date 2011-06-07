﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyGUIPlugin
{
    public class TreeNode : IDisposable
    {
        private TreeNodeCollection children;
        private Tree tree;
        private bool expanded = false;
        private TreeNodeWidget nodeWidget;

        public TreeNode()
            : this("")
        {

        }

        public TreeNode(String text)
            :this(text, new DefaultTreeNodeWidget())
        {
            
        }

        public TreeNode(String text, TreeNodeWidget nodeWidget)
        {
            children = new TreeNodeCollection(this);
            Text = text;
            this.nodeWidget = nodeWidget;
            nodeWidget.setTreeNode(this);
        }

        public void Dispose()
        {
            nodeWidget.Dispose();
            children.Dispose();
        }

        public void setSelected()
        {

        }

        public bool Visible
        {
            get
            {
                if (Parent == null)
                {
                    return Expanded;
                }
                return Parent.Expanded;
            }
        }

        internal void createWidget(Widget parent)
        {
            nodeWidget.createWidget(parent, Text, null);
            nodeWidget.updateExpandedStatus(expanded);
            if (Expanded)
            {
                foreach (TreeNode child in children)
                {
                    child.createWidget(parent);
                }
            }
        }

        internal void destroyWidget()
        {
            nodeWidget.destroyWidget();
            if (Expanded)
            {
                foreach (TreeNode child in children)
                {
                    child.destroyWidget();
                }
            }
        }

        internal void setWidgetCoord(int left, int top, int width, int height)
        {
            nodeWidget.setCoord(left, top, width, height);
        }

        /// <summary>
        /// This will be called by TreeNodeCollection when this node gains its first child or looses its last child.
        /// </summary>
        internal void alertHasChildrenChanged()
        {
            nodeWidget.updateExpandedStatus(expanded);
        }

        public Tree Tree
        {
            get
            {
                return tree;
            }
            internal set
            {
                tree = value;
                children.Tree = value;
            }
        }

        public TreeNode Parent { get; internal set; }

        public TreeNodeCollection Children
        {
            get
            {
                return children;
            }
        }

        public String Text { get; set; }

        public bool Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                if (expanded != value)
                {
                    expanded = value;
                    if (expanded)
                    {
                        if (Visible)
                        {
                            foreach (TreeNode node in children)
                            {
                                node.createWidget(Tree.Widget);
                            }
                        }
                    }
                    else
                    {
                        foreach (TreeNode node in children)
                        {
                            node.destroyWidget();
                        }
                    }
                    nodeWidget.updateExpandedStatus(expanded);
                    Tree.layout();
                }
            }
        }
    }
}
