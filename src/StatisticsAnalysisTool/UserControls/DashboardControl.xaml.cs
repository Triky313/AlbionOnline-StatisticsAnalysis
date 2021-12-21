using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl
    {
        public DashboardControl()
        {
            InitializeComponent();
        }

        #region Ui events

        private void BtnTrackingReset_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.ResetMainCounters();
        }

        #endregion
    }
}
