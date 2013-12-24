using System;
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
using CAAC.CAFM.ViewModels;
using System.ComponentModel.Composition;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmPlanQueryView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmPlanQueryView : UserControl
    {
        public CafmPlanQueryView()
        {
            InitializeComponent();
        }

        [Import]
        public CafmPlanQueryViewModel ViewModel
        {
            get { return this.DataContext as CafmPlanQueryViewModel; }
            set { this.DataContext = value; }
        }
    }
}
