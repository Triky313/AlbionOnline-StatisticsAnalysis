using FontAwesome5;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for DashboardControl.xaml
/// </summary>
public partial class DashboardControl
{
    public static readonly DependencyProperty ShowOpenWindowButtonProperty = DependencyProperty.Register(
        nameof(ShowOpenWindowButton),
        typeof(bool),
        typeof(DashboardControl),
        new PropertyMetadata(true));

    public bool ShowOpenWindowButton
    {
        get => (bool) GetValue(ShowOpenWindowButtonProperty);
        set => SetValue(ShowOpenWindowButtonProperty, value);
    }

    public DashboardControl()
    {
        InitializeComponent();
    }

    private void OpenDashboardWindow()
    {
        try
        {
            if (Utilities.IsWindowOpen<DashboardWindow>())
            {
                var existItemWindow = Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault();
                existItemWindow?.Activate();
            }
            else
            {
                var vm = (MainWindowViewModel) DataContext;
                var itemWindow = new DashboardWindow(vm);
                itemWindow.Show();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{Message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private async void BtnSessionReset_Click(object sender, RoutedEventArgs e)
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        await trackingController.StatisticController.ResetSessionAsync();
    }

    private async void DeleteDashboardSession_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (sender is not Button
            {
                DataContext: DashboardSessionFilterOption
                {
                    SessionId: Guid sessionId,
                    CanDelete: true
                } sessionFilter
            } deleteButton)
        {
            return;
        }

        var confirmationMessage = string.Format(
            CultureInfo.CurrentCulture,
            LocalizationController.Translation("DELETE_SESSION_CONFIRMATION"),
            sessionFilter.Name);
        var confirmationWindow = new DialogWindow(
            LocalizationController.Translation("DELETE_SESSION"),
            confirmationMessage);

        if (confirmationWindow.ShowDialog() is not true)
        {
            return;
        }

        deleteButton.IsEnabled = false;

        try
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            var wasDeleted = await trackingController.StatisticController.DeleteSessionAsync(sessionId);
            if (!wasDeleted)
            {
                ShowSessionDeletionFailedMessage();
            }
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            Log.Error(exception, "Statistics session deletion failed. SessionId={SessionId}", sessionId);
            ShowSessionDeletionFailedMessage();
        }
        finally
        {
            deleteButton.IsEnabled = true;
        }
    }

    private void ShowSessionDeletionFailedMessage()
    {
        var errorWindow = new DialogWindow(
            LocalizationController.Translation("DELETE_SESSION"),
            LocalizationController.Translation("DELETE_SESSION_FAILED"),
            DialogType.Error);
        _ = errorWindow.ShowDialog();
    }

    private void OpenDashboardWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
        OpenDashboardWindow();
    }

    private void KillDeathToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.KillDeathStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.KillDeathStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.KillDeathStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.KillDeathStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.KillDeathStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void FactionSummaryToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.FactionSummaryVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.FactionSummaryVisibility = Visibility.Collapsed;
            vm.DashboardBindings.FactionSummaryToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.FactionSummaryVisibility = Visibility.Visible;
            vm.DashboardBindings.FactionSummaryToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void FameContentRankingToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.FameContentRankingVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.FameContentRankingVisibility = Visibility.Collapsed;
            vm.DashboardBindings.FameContentRankingToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.FameContentRankingVisibility = Visibility.Visible;
            vm.DashboardBindings.FameContentRankingToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void SilverContentRankingToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.SilverContentRankingVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.SilverContentRankingVisibility = Visibility.Collapsed;
            vm.DashboardBindings.SilverContentRankingToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.SilverContentRankingVisibility = Visibility.Visible;
            vm.DashboardBindings.SilverContentRankingToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void LootedChestsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.LootedChestsStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.LootedChestsStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.LootedChestsStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.LootedChestsStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.LootedChestsStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void LootStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.LootStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.LootStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.LootStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.LootStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.LootStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void ReSpecStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.ReSpecStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.ReSpecStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.ReSpecStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.ReSpecStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.ReSpecStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void RepairCostsStatsToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.RepairCostsStatsVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.RepairCostsStatsVisibility = Visibility.Collapsed;
            vm.DashboardBindings.RepairCostsStatsToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.RepairCostsStatsVisibility = Visibility.Visible;
            vm.DashboardBindings.RepairCostsStatsToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void ActivityChartToggle_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        if (vm.DashboardBindings.ActivityChartVisibility == Visibility.Visible)
        {
            vm.DashboardBindings.ActivityChartVisibility = Visibility.Collapsed;
            vm.DashboardBindings.ActivityChartToggleIcon = EFontAwesomeIcon.Solid_Plus;
        }
        else
        {
            vm.DashboardBindings.ActivityChartVisibility = Visibility.Visible;
            vm.DashboardBindings.ActivityChartToggleIcon = EFontAwesomeIcon.Solid_Minus;
        }
    }

    private void DashboardChartRange_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshDailyChart();
    }

    private void DashboardMetadataFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshDailyChart();
    }

    private void DashboardChartSeriesVisibility_Changed(object sender, RoutedEventArgs e)
    {
        RefreshDailyChart();
    }

    private static void RefreshDailyChart()
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.StatisticController?.UpdateDailyChart(true);
    }
}
