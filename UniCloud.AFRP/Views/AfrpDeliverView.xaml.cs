using System.ComponentModel.Composition;
using System.Windows.Controls;
using UniCloud.AFRP.ViewModels;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpDeliverView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.Shared)]
    public partial class AfrpDeliverView : UserControl
    {
        public AfrpDeliverView()
        {
            InitializeComponent();
        }

        [Import]
        public AfrpDeliverViewModel ViewModel
        {
            get { return this.DataContext as AfrpDeliverViewModel; }
            set { this.DataContext = value; }
        }

        private void RadPaneGroup_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }


    }
}
