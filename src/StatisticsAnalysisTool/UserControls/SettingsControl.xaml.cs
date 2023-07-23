using log4net;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Shortcut;
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
        SettingsWindowViewModel.OpenConsoleWindow();
    }

    private void ReloadSettings_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _settingsWindowViewModel.ReloadSettings();
    }

    private async void CheckForUpdate_Click(object sender, RoutedEventArgs e)
    {
        AutoUpdateController.RemoveUpdateFiles();
        await AutoUpdateController.AutoUpdateAsync(true);
    }

    private void ResetPacketFilter_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.ResetPacketFilter();
    }

    private void ResetPlayerSelectionWithSameNameInDb_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.ResetPlayerSelectionWithSameNameInDb();
    }

    private async void UpdateItemListNow_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.IsUpdateItemListNowButtonEnabled = false;
        _settingsWindowViewModel.IsUpdateItemsJsonNowButtonEnabled = false;
        await ItemController.DownloadItemListAsync();
        _settingsWindowViewModel.IsUpdateItemListNowButtonEnabled = true;
        _settingsWindowViewModel.IsUpdateItemsJsonNowButtonEnabled = true;
    }

    private async void UpdateItemsJsonNow_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.IsUpdateItemListNowButtonEnabled = false;
        _settingsWindowViewModel.IsUpdateItemsJsonNowButtonEnabled = false;
        await ItemController.DownloadItemsJsonAsync();
        _settingsWindowViewModel.IsUpdateItemListNowButtonEnabled = true;
        _settingsWindowViewModel.IsUpdateItemsJsonNowButtonEnabled = true;
    }

    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        _settingsWindowViewModel.IsBackupNowButtonEnabled = false;
        await BackupController.SaveAsync();
        await Task.Delay(200);
        _settingsWindowViewModel.IsBackupNowButtonEnabled = true;
    }
}