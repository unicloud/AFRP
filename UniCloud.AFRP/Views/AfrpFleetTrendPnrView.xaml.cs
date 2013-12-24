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
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpFleetTrendPnrView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpFleetTrendPnrView : UserControl
    {
        public AfrpFleetTrendPnrView()
        {
            InitializeComponent();
        }
        [Import]
        public AfrpFleetTrendPnrViewModel ViewModel
        {
            get { return this.DataContext as AfrpFleetTrendPnrViewModel; }
            set { this.DataContext = value; }
        }
    }
}
