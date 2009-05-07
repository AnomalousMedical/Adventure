﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Engine;
using Engine.ObjectManagement;

namespace Anomaly
{
    partial class FourWaySplit : UserControl, SplitView
    {
        public FourWaySplit()
        {
            InitializeComponent();
        }

        #region SplitView Members

        public Control FrontView
        {
            get
            {
                return leftVertical.Panel1;
            }
        }

        public Control BackView
        {
            get
            {
                return rightVertical.Panel1;
            }
        }

        public Control LeftView
        {
            get
            {
                return leftVertical.Panel2;
            }
        }

        public Control RightView
        {
            get
            {
                return rightVertical.Panel2;
            }
        }

        #endregion
    }
}
