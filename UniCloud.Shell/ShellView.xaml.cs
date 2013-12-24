using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace UniCloud.Shell
{
    [Export]
    public partial class ShellView : UserControl
    {
        public ShellView()
        {
            InitializeComponent();
        }

        [Import(typeof(ShellViewModel))]
        public ShellViewModel ViewModel
        {
            get { return this.DataContext as ShellViewModel; }
            set { this.DataContext = value; }
        }

    }
}