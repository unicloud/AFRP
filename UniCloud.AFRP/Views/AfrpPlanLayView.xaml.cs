using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpPlanLayView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpPlanLayView
    {
        public AfrpPlanLayView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpPlanLayViewModel ViewModel
        {
            get { return this.DataContext as AfrpPlanLayViewModel; }
            set { this.DataContext = value; }
        }

    }
}
