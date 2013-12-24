using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;
using Telerik.Windows.Controls;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpAgeDeployView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpAgeDeployView : RadWindow
    {
        public AfrpAgeDeployView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpAgeDeployViewModel ViewModel
        {
            get { return this.DataContext as AfrpAgeDeployViewModel; }
            set { this.DataContext = value; }
        }
    }
}
