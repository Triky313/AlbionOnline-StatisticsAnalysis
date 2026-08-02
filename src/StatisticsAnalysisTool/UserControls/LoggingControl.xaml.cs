using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for LoggingControl.xaml
/// </summary>
public partial class LoggingControl
{
    public LoggingControl()
    {
        InitializeComponent();
    }

    #region Ui events

    private void BtnTrackingNotificationsReset_Click(object sender, RoutedEventArgs e)
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.ResetTrackingNotificationsAsync();
    }

    private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.ExportLootToFile();
    }

    private void BtnExportLootToJsonFile_MouseUp(object sender, MouseEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.ExportLootToJsonFile();
    }

    private void BtnLoadVaultLogFiles_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.OpenVaultFilePathSelection();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnLoadVaultLogText_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        var addedItems = mainWindowViewModel.LoggingBindings.AddVaultLogText(mainWindowViewModel.LoggingBindings.ChestLogText);
        if (addedItems > 0)
        {
            mainWindowViewModel.LoggingBindings.ChestLogText = string.Empty;
        }

        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnAddLootLogFiles_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.OpenLootLogFilePathSelection();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private async void BtnLogCompare_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        await mainWindowViewModel.LoggingBindings.CompareLootLogsAsync();
    }

    private void BtnClearVaultLogItems_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.ClearVaultLogs();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnClearLootLogs_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.ClearAllLootComparatorLogs();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnSaveLootComparator_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        if (!mainWindowViewModel.LoggingBindings.CanSaveLootComparator)
        {
            return;
        }

        var nameWindow = new LootComparatorSaveNameWindow();
        if (nameWindow.ShowDialog() is not true)
        {
            return;
        }

        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        try
        {
            mainWindowViewModel.LoggingBindings.SaveLootComparator(nameWindow.SaveName);
        }
        catch (Exception ex)
        {
            Debug.Print($"Error saving loot comparator state: {ex.Message}");
            ShowLootComparatorError("LOOT_COMPARATOR_SAVE_FAILED");
        }
        finally
        {
            mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
        }
    }

    private void BtnLoadLootComparatorSave_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        var loggingBindings = mainWindowViewModel.LoggingBindings;
        if (!loggingBindings.CanLoadLootComparatorSave)
        {
            return;
        }

        if (loggingBindings.HasLootComparatorLogs)
        {
            var confirmationWindow = new DialogWindow(
                LocalizationController.Translation("LOAD_LOOT_COMPARATOR_STATE"),
                LocalizationController.Translation("LOOT_COMPARATOR_LOAD_CONFIRMATION"));
            if (confirmationWindow.ShowDialog() is not true)
            {
                return;
            }
        }

        loggingBindings.IsAllButtonsEnabled = false;
        try
        {
            if (!loggingBindings.LoadSelectedLootComparatorSave())
            {
                ShowLootComparatorError("LOOT_COMPARATOR_SAVE_LOAD_FAILED");
            }
        }
        finally
        {
            loggingBindings.IsAllButtonsEnabled = true;
        }
    }

    private void BtnDeleteLootComparatorSave_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        var loggingBindings = mainWindowViewModel.LoggingBindings;
        var selectedSave = loggingBindings.SelectedLootComparatorSave;
        if (!loggingBindings.CanDeleteLootComparatorSave || selectedSave is null)
        {
            return;
        }

        var confirmationWindow = new DialogWindow(
            LocalizationController.Translation("DELETE_LOOT_COMPARATOR_STATE"),
            LocalizationController.Translation(
                "LOOT_COMPARATOR_DELETE_CONFIRMATION",
                ["SAVE_NAME"],
                [selectedSave.Name]));
        if (confirmationWindow.ShowDialog() is not true)
        {
            return;
        }

        loggingBindings.IsAllButtonsEnabled = false;
        try
        {
            if (!loggingBindings.DeleteSelectedLootComparatorSave())
            {
                ShowLootComparatorError("LOOT_COMPARATOR_SAVE_DELETE_FAILED");
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"Error deleting loot comparator state: {ex.Message}");
            ShowLootComparatorError("LOOT_COMPARATOR_SAVE_DELETE_FAILED");
        }
        finally
        {
            loggingBindings.IsAllButtonsEnabled = true;
        }
    }

    private static void ShowLootComparatorError(string messageTranslationKey)
    {
        var errorWindow = new DialogWindow(
            LocalizationController.Translation("ERROR"),
            LocalizationController.Translation(messageTranslationKey),
            DialogType.Error);
        _ = errorWindow.ShowDialog();
    }

    private void ToggleLootComparatorInfoPopup_MouseUp(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.LoggingBindings.ToggleLootComparatorInfoPopupVisibility();
    }

    #endregion
}
