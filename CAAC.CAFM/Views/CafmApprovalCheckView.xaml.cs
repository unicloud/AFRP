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
using CAAC.CAFM;
using CAAC.Infrastructure;
using System.ComponentModel.Composition;
using CAAC.CAFM.ViewModels;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmApprovalCheckView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmApprovalCheckView : UserControl
    {
        public CafmApprovalCheckView()
        {
            InitializeComponent();
        }

        [Import]
        public CafmApprovalCheckViewModel ViewModel
        {
            get { return this.DataContext as CafmApprovalCheckViewModel; }
            set { this.DataContext = value; }
        } 
    }
}
