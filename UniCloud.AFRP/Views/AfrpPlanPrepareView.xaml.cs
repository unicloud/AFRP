using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpPlanPrepareView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpPlanPrepareView
    {
        public AfrpPlanPrepareView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpPlanPrepareViewModel ViewModel
        {
            get { return this.DataContext as AfrpPlanPrepareViewModel; }
            set { this.DataContext = value; }
        }
    }
}
