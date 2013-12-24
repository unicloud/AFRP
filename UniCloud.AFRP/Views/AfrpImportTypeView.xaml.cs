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
    [Export(typeof(AfrpImportTypeView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpImportTypeView : UserControl
    {
        public AfrpImportTypeView()
        {
            InitializeComponent();
        }
        [Import]
        public AfrpImportTypeViewModel ViewModel
        {
            get { return this.DataContext as AfrpImportTypeViewModel; }
            set { this.DataContext = value; }
        }
    }
}
