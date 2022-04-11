using log4net;
using StatisticsAnalysisTool.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DamageMeterControl.xaml
    /// </summary>
    public partial class MailMonitoringControl
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public MailMonitoringControl()
        {
            InitializeComponent();
        }

        //public void ResetDamageMeter()
        //{
        //    var dialog = new DialogWindow(LanguageController.Translation("RESET_DAMAGE_METER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DAMAGE_METER"));
        //    var dialogResult = dialog.ShowDialog();

        //    if (dialogResult is true)
        //    {
        //        var vm = (MainWindowViewModel)DataContext;
        //        vm?.TrackingController?.CombatController?.ResetDamageMeter();
        //    }
        //}

        //public void DamageMeterActivationToggle()
        //{
        //    if (DataContext is MainWindowViewModel vm)
        //    {
        //        vm.IsDamageMeterTrackingActive = !vm.IsDamageMeterTrackingActive;
        //    }
        //}

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

        #endregion
    }
}
