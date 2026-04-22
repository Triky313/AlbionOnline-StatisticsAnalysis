using Notification.Wpf;
using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Notification;

public class SatNotificationManager
{
    private readonly NotificationManager _notificationManager;
    private volatile bool _isShuttingDown;

    public SatNotificationManager(NotificationManager notificationManager)
    {
        _notificationManager = notificationManager;

#if DEBUG
        for (int i = 0; i < 1; i++)
        {
            _ = ShowTestNotificationsAsync();
        }
#endif
    }

    public async Task ShowSuccessAsync(string title, string message)
    {
        if (!SettingsController.CurrentSettings.IsNotificationTrackingStatusActive
            || _isShuttingDown)
        {
            return;
        }

        await ShowNotificationAsync(title, message, NotificationType.Success, 1, "SolidColorBrush.Notification.Background.Green");
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        if (!SettingsController.CurrentSettings.IsNotificationTrackingStatusActive
            || _isShuttingDown)
        {
            return;
        }

        await ShowNotificationAsync(title, message, NotificationType.Error, 3, "SolidColorBrush.Notification.Background.Red");
    }

    public async Task ShowTrackingStatusAsync(string title, string message)
    {
        if (!SettingsController.CurrentSettings.IsNotificationTrackingStatusActive
            || _isShuttingDown)
        {
            return;
        }

        await ShowNotificationAsync(title, message, NotificationType.Information, 1, "SolidColorBrush.Notification.Background.Blue");
    }

    public async Task ShowTradeAsync(Trade.Trade trade)
    {
        if (!SettingsController.CurrentSettings.IsNotificationFilterTradeActive
            || trade == null
            || _isShuttingDown)
        {
            return;
        }

        await ShowNotificationAsync(trade.TradeNotificationTitleText, $"{trade.LocationName} - {trade.Item?.LocalizedName}", NotificationType.Success, 2, "SolidColorBrush.Notification.Background.Green");
    }

    public void StopShowingNotifications()
    {
        _isShuttingDown = true;
    }

    private async Task ShowNotificationAsync(string title, string message, NotificationType notificationType, uint rowsCount, string backgroundResourceKey)
    {
        if (!TryGetActiveDispatcher(out Dispatcher dispatcher))
        {
            return;
        }

        try
        {
            await dispatcher.InvokeAsync(() =>
            {
                if (_isShuttingDown
                    || dispatcher.HasShutdownStarted
                    || dispatcher.HasShutdownFinished
                    || !TryGetBrush("SolidColorBrush.Text.1", out SolidColorBrush foreground)
                    || !TryGetBrush(backgroundResourceKey, out SolidColorBrush background))
                {
                    return;
                }

                var content = new NotificationContent
                {
                    Title = title,
                    Message = message,
                    Type = notificationType,
                    TrimType = NotificationTextTrimType.AttachIfMoreRows,
                    RowsCount = rowsCount,
                    CloseOnClick = true,
                    Foreground = foreground,
                    Background = background
                };

                _notificationManager.Show(content);
            });
        }
        catch (OperationCanceledException)
        {
        }
    }

    private bool TryGetActiveDispatcher(out Dispatcher dispatcher)
    {
        dispatcher = Application.Current?.Dispatcher;
        return !_isShuttingDown
               && dispatcher != null
               && !dispatcher.HasShutdownStarted
               && !dispatcher.HasShutdownFinished;
    }

    private static bool TryGetBrush(string resourceKey, out SolidColorBrush brush)
    {
        brush = null;

        if (Application.Current == null
            || !Application.Current.Resources.Contains(resourceKey)
            || Application.Current.Resources[resourceKey] is not SolidColorBrush resourceBrush)
        {
            return false;
        }

        brush = resourceBrush;
        return true;
    }

    #region Test

    private async Task ShowTestNotificationsAsync()
    {
        var randomNotifyType = Random.Shared.Next(1, 5);
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var content = new NotificationContent
            {
                Title = "Test Notification",
                Message = "I am a test notification just for fun.",
                Type = NotificationType.Success,
                TrimType = NotificationTextTrimType.AttachIfMoreRows,
                CloseOnClick = true,
                Foreground = ForegroundText1,
                Background = BackgroundGreen
            };

            switch (randomNotifyType)
            {
                case 1:
                    content.Title = "Test Success Notification";
                    break;
                case 2:
                    content.Title = "Test Notification";
                    content.Type = NotificationType.Notification;
                    content.Background = BackgroundBlue;
                    break;
                case 3:
                    content.Title = "Test Warning Notification";
                    content.Type = NotificationType.Warning;
                    content.Background = BackgroundYellow;
                    break;
                case 4:
                    content.Title = "Test Error Notification";
                    content.Type = NotificationType.Error;
                    content.Background = BackgroundRed;
                    break;
            }

            var randomStartTime = Random.Shared.Next(0, 10000);
            await Task.Delay(randomStartTime);
            _notificationManager.Show(content);
        });
    }

    #endregion

    private static SolidColorBrush ForegroundText1
    {
        get
        {
            if (Application.Current != null && Application.Current.Resources.Contains("SolidColorBrush.Text.1"))
            {
                return (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Text.1"];
            }

            return null;
        }
    }

    private static SolidColorBrush BackgroundBlue
    {
        get
        {
            if (Application.Current != null && Application.Current.Resources.Contains("SolidColorBrush.Notification.Background.Blue"))
            {
                return (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Blue"];
            }

            return null;
        }
    }

    private static SolidColorBrush BackgroundGreen
    {
        get
        {
            if (Application.Current != null && Application.Current.Resources.Contains("SolidColorBrush.Notification.Background.Green"))
            {
                return (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Green"];
            }

            return null;
        }
    }

    private static SolidColorBrush BackgroundYellow
    {
        get
        {
            if (Application.Current != null && Application.Current.Resources.Contains("SolidColorBrush.Notification.Background.Yellow"))
            {
                return (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Yellow"];
            }

            return null;
        }
    }

    private static SolidColorBrush BackgroundRed
    {
        get
        {
            if (Application.Current != null && Application.Current.Resources.Contains("SolidColorBrush.Notification.Background.Red"))
            {
                return (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Red"];
            }

            return null;
        }
    }
}
