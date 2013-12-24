using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpMailSettingView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpMailSettingView : UserControl
    {
        public AfrpMailSettingView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpMailSettingViewModel ViewModel
        {
            get { return this.DataContext as AfrpMailSettingViewModel; }
            set { this.DataContext = value; }
        }

    }
}
