using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpAircraftOwnershipView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpAircraftOwnershipView : UserControl
    {
        public AfrpAircraftOwnershipView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpAircraftOwnershipViewModel ViewModel
        {
            get { return this.DataContext as AfrpAircraftOwnershipViewModel; }
            set { this.DataContext = value; }
        }
    }
}
