using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                var selectedMails = vm?.MailMonitoringBindings?.Mails?.Where(x => x?.IsSelectedForDeletion ?? false).Select(x => x.MailId);
                vm?.TrackingController.MailController.RemoveMailsByIdsAsync(selectedMails);
            }
        }

        #region Ui events

        private void OpenMailMonitoringPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.MailMonitoringBindings.IsMailMonitoringPopupVisible = Visibility.Visible;
        }

        private void CloseMailMonitoringPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.MailMonitoringBindings.IsMailMonitoringPopupVisible = Visibility.Collapsed;
        }

        private void BtnDeleteSelectedMails_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedMails();
        }

        private bool _isSelectAllActive;

        private void BtnSelectSwitchAllMails_Click(object sender, RoutedEventArgs e)
        {
            if ((MainWindowViewModel)DataContext is not { MailMonitoringBindings.Mails: { } } mainWindowViewModel)
            {
                return;
            }

            foreach (var mail in mainWindowViewModel.MailMonitoringBindings.Mails)
            {
                mail.IsSelectedForDeletion = !_isSelectAllActive;
            }

            _isSelectAllActive = !_isSelectAllActive;
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            CollectionViewSource.GetDefaultView(vm.MailMonitoringBindings.Mails).Refresh();
        }

        private void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            CollectionViewSource.GetDefaultView(vm.MailMonitoringBindings.Mails).Refresh();
        }

        #endregion
    }
}
