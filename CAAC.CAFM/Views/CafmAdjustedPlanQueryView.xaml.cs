using CAAC.CAFM.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmAdjustedPlanQueryView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmAdjustedPlanQueryView : UserControl
    {
        public CafmAdjustedPlanQueryView()
        {
            InitializeComponent();
        }

        [Import]
        public CafmAdjustedPlanQueryViewModel ViewModel
        {
            get { return this.DataContext as CafmAdjustedPlanQueryViewModel; }
            set { this.DataContext = value; }
        }
    }
}