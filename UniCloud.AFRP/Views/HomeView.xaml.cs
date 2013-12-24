using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(HomeView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
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
