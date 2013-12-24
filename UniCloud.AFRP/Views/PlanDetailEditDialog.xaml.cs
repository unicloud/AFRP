using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(PlanDetailEditDialog))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class PlanDetailEditDialog
    {
        public PlanDetailEditDialog()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpPlanLayViewModel ViewModel
        {
            get { return this.DataContext as AfrpPlanLayViewModel; }
            set { this.DataContext = value; }
        }

        private void RadWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

    }
}
