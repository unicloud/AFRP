using System.ComponentModel.Composition;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpSubsidiarySettingView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class AfrpSubsidiarySettingView
    {
        public AfrpSubsidiarySettingView()
        {
            InitializeComponent();
        }
        [Import]
        public AfrpSubsidiarySettingViewModel ViewModel
        {
            get { return this.DataContext as AfrpSubsidiarySettingViewModel; }
            set { this.DataContext = value; }
        }

    }
}