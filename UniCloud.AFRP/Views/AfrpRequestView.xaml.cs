using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpRequestView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpRequestView
    {
        public AfrpRequestView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpRequestViewModel ViewModel
        {
            get { return this.DataContext as AfrpRequestViewModel; }
            set { this.DataContext = value; }
        }
    }
}
