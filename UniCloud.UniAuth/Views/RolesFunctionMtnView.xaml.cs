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
using UniCloud.UniAuth.ViewModels;

namespace UniCloud.UniAuth.Views
{
    [Export(typeof(RolesFunctionMtnView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class RolesFunctionMtnView : UserControl
    {
        public RolesFunctionMtnView()
        {
            InitializeComponent();
        }
        [Import]
        public RolesFunctionMtnViewModel ViewModel
        {
            get { return this.DataContext as RolesFunctionMtnViewModel; }
            set { this.DataContext = value; }
        }
    }
}
