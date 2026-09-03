using Bloxstrap.UI.Elements.Dialogs;
using Bloxstrap.UI.ViewModels.Settings;
using System.Windows;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class LaunchOptionsPage
    {
        public LaunchOptionsPage()
        {
            DataContext = new BehaviourViewModel();
            InitializeComponent();
        }

        private void OpenMultiblox_Click(object sender, RoutedEventArgs e)
        {
            new MultibloxDialog { Owner = Window.GetWindow(this) }.ShowDialog();
        }
    }
}
