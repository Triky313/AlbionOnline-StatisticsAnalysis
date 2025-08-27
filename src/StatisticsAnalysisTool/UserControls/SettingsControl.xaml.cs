
using Serilog;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Shortcut;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.PhotonPackageParser;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl
{
    private readonly SettingsWindowViewModel _settingsWindowViewModel;

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
        catch (Exception ex)
        {
            _ = MessageBox.Show(ex.Message, LocalizationController.Translation("ERROR"));
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private void CreateDesktopShortcut_Click(object sender, RoutedEventArgs e)
    {
        ShortcutController.CreateShortcut();
    }

    private void OpenDebugConsole_Click(object sender, RoutedEventArgs e)
    {
        DebugConsole.Attach("SAT Photon Console");
        var args = _settingsWindowViewModel.DebugConsoleFilter;
        DebugConsole.Configure(args);
    }

    private void CloseDebugConsole_Click(object sender, RoutedEventArgs e)
    {
        DebugConsole.Detach();
    }

    private void OpenEventValidation_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindowViewModel.OpenEventValidationWindow();
    }

    private void ReloadSettings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _settingsWindowViewModel.ReloadSettings();
    }

    private async void CheckForUpdate_Click(object sender, RoutedEventArgs e)
    {
        await AutoUpdateController.AutoUpdateAsync(true);
    }

    private void ResetPlayerSelectionWithSameNameInDb_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.ResetPlayerSelectionWithSameNameInDb();
    }

    private void ResetBackupStorageDirPath_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.ResetBackupStorageDirPath();
    }

    private void ResetPacketFilter_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.ResetPacketFilter();
    }

    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.IsBackupNowButtonEnabled = false;
        BackupController.Save();
        await Task.Delay(200);
        _settingsWindowViewModel.IsBackupNowButtonEnabled = true;
    }
}