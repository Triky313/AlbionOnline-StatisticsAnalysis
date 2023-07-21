using Notification.Wpf;
using StatisticsAnalysisTool.Common.UserSettings;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Notification;

public class SatNotificationManager
{
    private readonly NotificationManager _notificationManager;

    public SatNotificationManager(NotificationManager notificationManager)
    {
        _notificationManager = notificationManager;

#if DEBUG
        for (int i = 0; i < 5; i++)
        {
            _ = ShowTestNotificationsAsync();
        }
#endif
    }

    public async Task ShowTrackingStatusAsync(string title, string message)
    {
        if (!SettingsController.CurrentSettings.IsNotificationTrackingStatusActive
            || ForegroundText1 == null
            || BackgroundBlue == null)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var content = new NotificationContent
            {
                Title = title,
                Message = message,
                Type = NotificationType.Information,
                TrimType = NotificationTextTrimType.AttachIfMoreRows,
                RowsCount = 1,
                CloseOnClick = true,
                Foreground = ForegroundText1,
                Background = BackgroundBlue
            };

            _notificationManager.Show(content);
        });
    }

    public async Task ShowTradeAsync(Trade.Trade trade)
    {
        if (!SettingsController.CurrentSettings.IsNotificationFilterTradeActive
            || trade == null
            || ForegroundText1 == null
            || BackgroundBlue == null)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var content = new NotificationContent
            {
                Title = trade.TradeNotificationTitleText,
                Message = $"{trade.LocationName} - {trade.Item?.LocalizedName}",
                Type = NotificationType.Success,
                TrimType = NotificationTextTrimType.AttachIfMoreRows,
                RowsCount = 2,
                CloseOnClick = true,
                Foreground = ForegroundText1,
                Background = BackgroundGreen
            };

            _notificationManager.Show(content);
        });
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