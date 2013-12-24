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
using System.ComponentModel.Composition;
using CAAC.CAFM.ViewModels;
using Telerik.Windows.Controls.Charting;
using Telerik.Windows.Controls;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmFleetTrendPnrView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmFleetTrendPnrView : UserControl
    {
        public CafmFleetTrendPnrView()
        {
            InitializeComponent();
        }
        [Import]
        public CafmFleetTrendPnrViewModel ViewModel
        {
            get { return this.DataContext as CafmFleetTrendPnrViewModel; }
            set { this.DataContext = value; }
        }
    }
}
