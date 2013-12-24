using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpPlanQueryView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpPlanQueryView
    {
        public AfrpPlanQueryView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpPlanQueryViewModel ViewModel
        {
            get { return this.DataContext as AfrpPlanQueryViewModel; }
            set { this.DataContext = value; }
        }
    }
}
