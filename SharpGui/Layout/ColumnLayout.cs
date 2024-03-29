﻿using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGui
{
    public class ColumnLayout : ILayoutItem
    {
        class Entry
        {
            public Entry(ILayoutItem layoutItem)
            {
                LayoutItem = layoutItem;
            }

            public ILayoutItem LayoutItem { get; set; }

            public int DesiredHeight { get; set; }
        }

        public IntPad Margin;

        private List<Entry> items;

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            int width = 0;
            int height = 0;

            foreach (var item in items)
            {
                var itemSize = item.LayoutItem.GetDesiredSize(sharpGui);
                width = Math.Max(width, itemSize.Width);
                item.DesiredHeight = itemSize.Height + Margin.Top + Margin.Bottom;
                height += item.DesiredHeight;
            }

            return new IntSize2(width + Margin.Left + Margin.Right, height);
        }

        public void SetRect(in IntRect rect)
        {
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width;

            foreach (var item in items)
            {
                int height = item.DesiredHeight;
                item.LayoutItem.SetRect(new IntRect(left + Margin.Left, top + Margin.Top, width - Margin.Left - Margin.Right, height - Margin.Top - Margin.Bottom));
                top += height;
            }
        }

        public ColumnLayout(int capacity = 10)
        {
            this.items = new List<Entry>(capacity);
        }

        public ColumnLayout(params ILayoutItem[] items)
        {
            this.items = new List<Entry>(items.Select(i => new Entry(i)));
        }

        public ColumnLayout(IEnumerable<ILayoutItem> items)
        {
            this.items = new List<Entry>(items.Select(i => new Entry(i)));
        }

        public void Clear()
        {
            items.Clear();
        }

        public void Add(ILayoutItem item)
        {
            items.Add(new Entry(item));
        }

        public void Add(params ILayoutItem[] items)
        {
            this.items.AddRange(items.Select(i => new Entry(i)));
        }

        public void Add(IEnumerable<ILayoutItem> items)
        {
            this.items.AddRange(items.Select(i => new Entry(i)));
        }

        public void Remove(ILayoutItem item)
        {
            var itemCount = items.Count;
            for (var i = 0; i < itemCount; ++i)
            {
                var current = this.items[i];
                if(current.LayoutItem == item)
                {
                    items.RemoveAt(i);
                    return; //Done
                }
            }
        }
    }
}
