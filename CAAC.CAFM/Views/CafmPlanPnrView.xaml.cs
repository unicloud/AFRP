using System.Windows.Controls;
using System.ComponentModel.Composition;
using CAAC.CAFM.ViewModels;
using Telerik.Windows;
using Telerik.Windows.Controls;
namespace CAAC.CAFM.Views
{

    [Export(typeof(CafmPlanPnrView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmPlanPnrView : UserControl
    {
        public CafmPlanPnrView()
        {
            InitializeComponent();
 
        }
        [Import]
        public CafmPlanPnrViewModel ViewModel
        {
            get { return this.DataContext as CafmPlanPnrViewModel; }
            set { this.DataContext = value; }
        }

    }
}
