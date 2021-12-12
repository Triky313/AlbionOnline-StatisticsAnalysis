using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for TrackingControl.xaml
    /// </summary>
    public partial class TrackingControl
    {
        public TrackingControl()
        {
            InitializeComponent();
        }

        #region Ui events

        private void BtnTrackingNotificationsReset_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.ResetTrackingNotificationsAsync().ConfigureAwait(false);
        }

        private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.ExportLootToFile();
        }

        #endregion
    }
}
