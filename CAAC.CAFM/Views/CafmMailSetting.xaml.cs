using System.ComponentModel.Composition;
using System.Windows.Controls;
using CAAC.CAFM.ViewModels;

namespace CAAC.CAFM.Views
{
    [Export(typeof(CafmMailSettingView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class CafmMailSettingView : UserControl
    {
        public CafmMailSettingView()
        {
            InitializeComponent();
        }

        [Import]
        public CafmMailSettingViewModel ViewModel
        {
            get { return this.DataContext as CafmMailSettingViewModel; }
            set { this.DataContext = value; }
        }
    }
}
