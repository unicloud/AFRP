using System.ComponentModel.Composition;
using System.Windows.Controls;
using CAAC.CAFM.ViewModels;

namespace CAAC.CAFM.Views
{
    [Export(typeof(HomeView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }

        [Import]
        public HomeViewModel ViewModel
        {
            get { return this.DataContext as HomeViewModel; }
            set { this.DataContext = value; }
        }
    }
}
