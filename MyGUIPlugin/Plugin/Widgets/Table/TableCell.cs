﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace MyGUIPlugin
{
    public abstract class TableCell : TableElement
    {
        private IntSize2 size;
        private IntVector2 position;

        public abstract void setStaticMode();

        public abstract void setEditMode();

        public abstract TableCell clone();

        protected abstract void sizeChanged();

        protected abstract void positionChanged();

        public abstract Object Value { get; set; }

        public IntSize2 Size
        {
            get
            {
                return size;
            }
            set
            {
                if (size != value)
                {
                    size = value;
                    sizeChanged();
                }
            }
        }

        public IntVector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                if (position != value)
                {
                    position = value;
                    positionChanged();
                }
            }
        }

        protected Widget TableWidget
        {
            get
            {
                return Table.TableWidget;
            }
        }

        protected void fireValueChanged()
        {
            Table.fireCellValueChanged(this);
        }

        /// <summary>
        /// Make this cell the cell currently being edited.
        /// </summary>
        protected void requestCellEdit()
        {
            Table.requestCellEdit(this);
        }

        /// <summary>
        /// Make no cells editing.
        /// </summary>
        protected void clearCellEdit()
        {
            Table.requestCellEdit(null);
        }
    }
}
