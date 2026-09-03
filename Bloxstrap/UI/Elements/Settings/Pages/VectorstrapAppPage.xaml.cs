using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class VectorstrapAppPage
    {
        public VectorstrapAppPage()
        {
            DataContext = new ChannelViewModel();
            InitializeComponent();
        }
    }
}
