using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class RobloxUpdatesPage
    {
        public RobloxUpdatesPage()
        {
            DataContext = new ChannelViewModel();
            InitializeComponent();
        }
    }
}
