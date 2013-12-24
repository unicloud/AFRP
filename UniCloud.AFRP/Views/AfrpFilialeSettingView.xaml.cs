using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpFilialeSettingView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpFilialeSettingView
    {
        public AfrpFilialeSettingView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpFilialeSettingViewModel ViewModel
        {
            get { return this.DataContext as AfrpFilialeSettingViewModel; }
            set { this.DataContext = value; }
        }
    }
}
