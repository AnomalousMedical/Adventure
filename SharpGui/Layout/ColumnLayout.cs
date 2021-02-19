﻿using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpGui
{
    public struct ColumnLayout : ILayoutItem
    {
        public ILayoutItem Item0;
        public ILayoutItem Item1;
        public ILayoutItem Item2;
        public ILayoutItem Item3;
        public ILayoutItem Item4;
        public ILayoutItem Item5;
        public ILayoutItem Item6;
        public ILayoutItem Item7;
        public ILayoutItem Item8;
        public ILayoutItem Item9;
        public ILayoutItem Item10;
        public ILayoutItem Item11;

        private int ItemDesiredHeight0;
        private int ItemDesiredHeight1;
        private int ItemDesiredHeight2;
        private int ItemDesiredHeight3;
        private int ItemDesiredHeight4;
        private int ItemDesiredHeight5;
        private int ItemDesiredHeight6;
        private int ItemDesiredHeight7;
        private int ItemDesiredHeight8;
        private int ItemDesiredHeight9;
        private int ItemDesiredHeight10;
        private int ItemDesiredHeight11;

        public IntSize2 GetDesiredSize(ISharpGui sharpGui)
        {
            int width = 0;
            int height = 0;

            var test = 
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item0 , ref ItemDesiredHeight0 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item1 , ref ItemDesiredHeight1 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item2 , ref ItemDesiredHeight2 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item3 , ref ItemDesiredHeight3 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item4 , ref ItemDesiredHeight4 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item5 , ref ItemDesiredHeight5 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item6 , ref ItemDesiredHeight6 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item7 , ref ItemDesiredHeight7 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item8 , ref ItemDesiredHeight8 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item9 , ref ItemDesiredHeight9 ) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item10, ref ItemDesiredHeight10) &&
            GetDesiredSizeItem(sharpGui, ref width, ref height, Item11, ref ItemDesiredHeight11);

            return new IntSize2(width, height);
        }

        private static bool GetDesiredSizeItem(ISharpGui sharpGui, ref int width, ref int height, ILayoutItem item, ref int desiredHeight)
        {
            if(item == null)
            {
                return false;
            }

            var itemSize = item.GetDesiredSize(sharpGui);
            if (itemSize.Width > width)
            {
                width = itemSize.Width;
            }
            desiredHeight = itemSize.Height;
            height += desiredHeight;

            return true;
        }

        public void SetRect(IntRect rect)
        {
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width;

            var test = 
            SetRectItem(Item0, left, ref top, width, ItemDesiredHeight0  ) &&
            SetRectItem(Item1, left, ref top, width, ItemDesiredHeight1  ) &&
            SetRectItem(Item2, left, ref top, width, ItemDesiredHeight2  ) &&
            SetRectItem(Item3, left, ref top, width, ItemDesiredHeight3  ) &&
            SetRectItem(Item4, left, ref top, width, ItemDesiredHeight4  ) &&
            SetRectItem(Item5, left, ref top, width, ItemDesiredHeight5  ) &&
            SetRectItem(Item6, left, ref top, width, ItemDesiredHeight6  ) &&
            SetRectItem(Item7, left, ref top, width, ItemDesiredHeight7  ) &&
            SetRectItem(Item8, left, ref top, width, ItemDesiredHeight8  ) &&
            SetRectItem(Item9, left, ref top, width, ItemDesiredHeight9  ) &&
            SetRectItem(Item10, left, ref top, width, ItemDesiredHeight10) &&
            SetRectItem(Item11, left, ref top, width, ItemDesiredHeight11);
        }

        private static bool SetRectItem(ILayoutItem item, int left, ref int top, int width, int height)
        {
            if(item == null)
            {
                return false;
            }

            item.SetRect(new IntRect(left, top, width, height));
            top += height;

            return true;
        }

        public ILayoutItem this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return Item0;
                    case 1:
                        return Item1;
                    case 2:
                        return Item2;
                    case 3:
                        return Item3;
                    case 4:
                        return Item4;
                    case 5:
                        return Item5;
                    case 6:
                        return Item6;
                    case 7:
                        return Item7;
                    case 8:
                        return Item8;
                    case 9:
                        return Item9;
                    case 10:
                        return Item10;
                    case 11:
                        return Item11;
                }
                throw new IndexOutOfRangeException($"Index '{index}' is out of the range 0-11.");
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Item0 =  value;
                        break;
                    case 1:
                        Item1 = value;
                        break;
                    case 2:
                        Item2 = value;
                        break;
                    case 3:
                        Item3 = value;
                        break;
                    case 4:
                        Item4 = value;
                        break;
                    case 5:
                        Item5 = value;
                        break;
                    case 6:
                        Item6 = value;
                        break;
                    case 7:
                        Item7 = value;
                        break;
                    case 8:
                        Item8 = value;
                        break;
                    case 9:
                        Item9 = value;
                        break;
                    case 10:
                        Item10 = value;
                        break;
                    case 11:
                        Item11 = value;
                        break;
                }
                throw new IndexOutOfRangeException($"Index '{index}' is out of the range 0-11.");
            }
        }
    }
}