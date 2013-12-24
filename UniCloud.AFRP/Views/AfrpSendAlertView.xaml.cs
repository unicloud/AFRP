using System.ComponentModel.Composition;
using Telerik.Windows.Controls;

namespace UniCloud.AFRP.Views
{
    [Export(typeof(AfrpSendAlertView))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    public partial class AfrpSendAlertView : RadWindow
    {
        public AfrpSendAlertView()
        {
            InitializeComponent();
        }
    }
}
