using Bloxstrap.UI.Elements.Dialogs;
using Bloxstrap.UI.ViewModels.Settings;
using System.Windows;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class RobloxChannelPage
    {
        public RobloxChannelPage()
        {
            DataContext = new ChannelViewModel();
            InitializeComponent();
        }

        private void OpenChannelListDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ChannelListsDialog { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
        }
    }
}
