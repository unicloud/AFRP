using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpFleetAllotView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpFleetAllotView : UserControl
    {
        public AfrpFleetAllotView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpFleetAllotViewModel ViewModel
        {
            get { return this.DataContext as AfrpFleetAllotViewModel; }
            set { this.DataContext = value; }
        }
    }
}
