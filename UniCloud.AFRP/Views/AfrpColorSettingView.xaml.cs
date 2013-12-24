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
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpColorSettingView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpColorSettingView : UserControl
    {
        public AfrpColorSettingView()
        {
            InitializeComponent();
        }
        [Import]
        public AfrpColorSettingViewModel ViewModel
        {
            get { return this.DataContext as AfrpColorSettingViewModel; }
            set { this.DataContext = value; }
        }
    }
}
