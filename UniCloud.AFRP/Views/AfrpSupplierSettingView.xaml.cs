using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpSupplierSettingView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpSupplierSettingView : UserControl
    {
        public AfrpSupplierSettingView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpSupplierSettingViewModel ViewModel
        {
            get
            {
                return this.DataContext as AfrpSupplierSettingViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }
    }
}
