using System.ComponentModel.Composition;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using UniCloud.UniAuth.ViewModels;

namespace UniCloud.UniAuth.Views
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
