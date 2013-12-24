﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;
using CAAC.CAFM.ViewModels;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmImportTypeView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmImportTypeView : UserControl
    {
        public CafmImportTypeView()
        {
            InitializeComponent();
        }
        [Import]
        public CafmImportTypeViewModel ViewModel
        {
            get { return this.DataContext as CafmImportTypeViewModel; }
            set { this.DataContext = value; }
        }
    }
}
