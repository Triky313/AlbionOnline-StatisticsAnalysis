using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.ViewModels;

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

        private void OpenMainTrackerInfoPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsMainTrackerPopupVisible = Visibility.Visible;
        }

        private void CloseMainTrackerInfoPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsMainTrackerPopupVisible = Visibility.Hidden;
        }

        #endregion
    }
}
