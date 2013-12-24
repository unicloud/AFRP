using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpApprovalView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpApprovalView : UserControl
    {
        public AfrpApprovalView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpApprovalViewModel ViewModel
        {
            get { return this.DataContext as AfrpApprovalViewModel; }
            set { this.DataContext = value; }
        }
    }
}
