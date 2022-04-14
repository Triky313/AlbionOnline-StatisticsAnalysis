using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for MailMonitoringControl.xaml
    /// </summary>
    public partial class MailMonitoringControl
    {
        public MailMonitoringControl()
        {
            InitializeComponent();
        }
        
        public void DeleteSelectedMails()
        {
            var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_MAILS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_MAIL"));
            var dialogResult = dialog.ShowDialog();

            var vm = (MainWindowViewModel)DataContext;

            if (dialogResult is true)
            {
                var selectedMails = vm?.Mails.Where(x => x.IsSelectedForDeletion ?? false).Select(x => x.MailId);
                vm?.TrackingController.MailController.RemoveMailsByIdsAsync(selectedMails);
            }
        }

        #region Ui events

        private void OpenMailMonitoringPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsMailMonitoringPopupVisible = Visibility.Visible;
        }

        private void CloseMailMonitoringPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsMailMonitoringPopupVisible = Visibility.Hidden;
        }

        private void BtnDeleteSelectedMails_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedMails();
        }

        #endregion
    }
}
