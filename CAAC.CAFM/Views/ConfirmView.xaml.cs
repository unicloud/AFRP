using System.ComponentModel.Composition;
using System.Windows.Controls;
using CAAC.CAFM.ViewModels;
using Telerik.Windows.Controls;

namespace CAAC.CAFM.Views
{
    [Export(typeof(ConfirmView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class ConfirmView : ChildWindow
    {
        public ConfirmView()
        {
            InitializeComponent();
        }

        [Import]
        public ConfirmViewModel ViewModel
        {
            get { return this.DataContext as ConfirmViewModel; }
            set { this.DataContext = value; }
        }
    }
}
