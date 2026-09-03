using Bloxstrap.UI.ViewModels.Settings;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class MaintenancePage
    {
        public MaintenancePage()
        {
            DataContext = new BehaviourViewModel();
            InitializeComponent();
        }
    }
}
