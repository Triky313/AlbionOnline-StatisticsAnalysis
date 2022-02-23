using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Shortcut;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl
    {
        private readonly SettingsWindowViewModel _settingsWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public SettingsControl()
        {
            InitializeComponent();
            _settingsWindowViewModel = new SettingsWindowViewModel();
            DataContext = _settingsWindowViewModel;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindowViewModel.SaveSettings();
        }

        private void OpenToolDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo { FileName = _settingsWindowViewModel.ToolDirectory, UseShellExecute = true });
            }
            catch (Exception exception)
            {
                _ = MessageBox.Show(exception.Message, LanguageController.Translation("ERROR"));
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            }
        }

        private void CreateDesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            ShortcutController.CreateShortcut();
        }

        private void OpenDebugConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();
        }

        private void ReloadSettings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _settingsWindowViewModel.ReloadSettings();
        }
    }
}
