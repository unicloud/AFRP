using System.ComponentModel.Composition;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using CAAC.CAFM.ViewModels;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmAgeDeployView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmAgeDeployView : RadWindow
    {
        public CafmAgeDeployView()
        {
            InitializeComponent();
        }

        [Import]
        public CafmAgeDeployViewModel ViewModel
        {
            get { return this.DataContext as CafmAgeDeployViewModel; }
            set { this.DataContext = value; }
        }
    }
}
