using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for GatheringControl.xaml
/// </summary>
public partial class GatheringControl
{
    public GatheringControl()
    {
        InitializeComponent();
        Loaded += GatheringControl_Loaded;
        IsVisibleChanged += GatheringControl_IsVisibleChanged;
    }

    private void GatheringControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateViewVisibility();
    }

    private void GatheringControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        UpdateViewVisibility();
    }

    private void UpdateViewVisibility()
    {
        if (DataContext is MainWindowViewModel mainWindowViewModel)
        {
            mainWindowViewModel.GatheringBindings.SetViewVisibility(IsVisible);
        }
    }

    private void GatheringActivationToggle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            return;
        }

        mainWindowViewModel.GatheringBindings.IsGatheringActive = !mainWindowViewModel.GatheringBindings.IsGatheringActive;
    }

    private async void BtnSessionReset_Click(object sender, RoutedEventArgs e)
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        await trackingController.GatheringController.ResetSessionAsync();
    }

    private async void DeleteGatheringSession_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (sender is not Button
            {
                DataContext: GatheringSessionFilterOption
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
            var wasDeleted = await trackingController.GatheringController.DeleteSessionAsync(sessionId);
            if (!wasDeleted)
            {
                ShowSessionDeletionFailedMessage();
            }
        }
        catch (Exception exception)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            Log.Error(exception, "Gathering session deletion failed. SessionId={SessionId}", sessionId);
            ShowSessionDeletionFailedMessage();
        }
        finally
        {
            deleteButton.IsEnabled = true;
        }
    }

    private static void ShowSessionDeletionFailedMessage()
    {
        var errorWindow = new DialogWindow(
            LocalizationController.Translation("DELETE_SESSION"),
            LocalizationController.Translation("DELETE_SESSION_FAILED"),
            DialogType.Error);
        _ = errorWindow.ShowDialog();
    }
}
