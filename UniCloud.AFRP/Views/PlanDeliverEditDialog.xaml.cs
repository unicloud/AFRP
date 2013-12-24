using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(PlanDeliverEditDialog))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class PlanDeliverEditDialog
    {
        public PlanDeliverEditDialog()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpDeliverViewModel ViewModel
        {
            get { return this.DataContext as AfrpDeliverViewModel; }
            set { this.DataContext = value; }
        }

        private void RadWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

    }
}
