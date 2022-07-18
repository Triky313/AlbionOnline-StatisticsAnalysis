using System;
using System.Linq;
using System.Reflection;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Views;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DamageMeterControl.xaml
    /// </summary>
    public partial class DamageMeterControl
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public DamageMeterControl()
        {
            InitializeComponent();
        }
        
        public void ResetDamageMeter()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_DAMAGE_METER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DAMAGE_METER"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                var vm = (MainWindowViewModel)DataContext;
                vm?.TrackingController?.CombatController?.ResetDamageMeter();
            }
        }

        private void OpenDamageMeterWindow()
        {
            try
            {
                if (Utilities.IsWindowOpen<DamageMeterWindow>())
                {
                    var existItemWindow = Application.Current.Windows.OfType<DamageMeterWindow>().FirstOrDefault();
                    existItemWindow?.Activate();
                }
                else
                {
                    var vm = (MainWindowViewModel)DataContext;
                    var itemWindow = new DamageMeterWindow(vm?.DamageMeterBindings?.DamageMeter);
                    itemWindow.Show();
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public void CopyDamageMeterToClipboard()
        {
            var output = string.Empty;
            var counter = 1;

            var vm = (MainWindowViewModel)DataContext;

            switch (vm?.DamageMeterSortSelection.DamageMeterSortType)
            {
                case DamageMeterSortType.Damage:
                    Clipboard.SetDataObject(SettingsController.CurrentSettings.ShortDamageMeterToClipboard
                        ? vm.DamageMeterBindings.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.DamagePercentage:N2}%\n")
                        : vm.DamageMeterBindings.DamageMeter.Aggregate(output,
                            (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));

                    break;
                case DamageMeterSortType.Dps:
                    Clipboard.SetDataObject(vm.DamageMeterBindings.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Dps:N2} DPS\n"));
                    break;
                case DamageMeterSortType.Name:
                    Clipboard.SetDataObject(SettingsController.CurrentSettings.ShortDamageMeterToClipboard
                        ? vm.DamageMeterBindings.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.DamagePercentage:N2}%\n")
                        : vm.DamageMeterBindings.DamageMeter.Aggregate(output, 
                            (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));
                    break;
                case DamageMeterSortType.Heal:
                    Clipboard.SetDataObject(SettingsController.CurrentSettings.ShortDamageMeterToClipboard
                        ? vm.DamageMeterBindings.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.HealPercentage:N2}%\n")
                        : vm.DamageMeterBindings.DamageMeter.Aggregate(output,
                            (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Heal}({entity.HealPercentage:N2}%)|{entity.Hps:N2} HPS\n"));
                    break;
                case DamageMeterSortType.Hps:
                    Clipboard.SetDataObject(vm.DamageMeterBindings.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Hps:N2} HPS\n"));
                    break;
                case null:
                    break;
            }
        }

        public void DamageMeterActivationToggle()
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsDamageMeterTrackingActive = !vm.IsDamageMeterTrackingActive;
            }
        }

        #region Ui events

        private void DamageMeterModeActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DamageMeterActivationToggle();
        }

        private void BtnDamageMeterReset_Click(object sender, RoutedEventArgs e)
        {
            ResetDamageMeter();
        }

        private void OpenDamageMeterInfoPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsDamageMeterPopupVisible = Visibility.Visible;
        }

        private void CloseDamageMeterInfoPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsDamageMeterPopupVisible = Visibility.Hidden;
        }

        private void OpenDamageMeterWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenDamageMeterWindow();
        }

        private void CopyDamageMeterToClipboard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CopyDamageMeterToClipboard();
        }

        private void TakeASnapShot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.DamageMeterBindings?.GetSnapshot();
        }

        private void BtnDeleteSelectedSnapshot_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.DamageMeterBindings?.DeleteSnapshot();
        }

        #endregion
    }
}
