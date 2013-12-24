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
    [Export(typeof(AfrpApprovalQueryView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpApprovalQueryView : UserControl
    {
        public AfrpApprovalQueryView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpApprovalQueryViewModel ViewModel
        {
            get { return this.DataContext as AfrpApprovalQueryViewModel; }
            set { this.DataContext = value; }
        }
    }
}
