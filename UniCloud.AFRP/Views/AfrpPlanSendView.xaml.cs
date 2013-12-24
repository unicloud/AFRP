using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpPlanSendView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpPlanSendView
    {
        public AfrpPlanSendView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpPlanSendViewModel ViewModel
        {
            get { return this.DataContext as AfrpPlanSendViewModel; }
            set { this.DataContext = value; }
        }
    }
}
