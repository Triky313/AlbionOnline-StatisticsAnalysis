using Notification.Wpf;
using StatisticsAnalysisTool.Common.UserSettings;
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
    }

    public async Task ShowTradeAsync(Trade.Trade trade)
    {
        if (!SettingsController.CurrentSettings.IsNotificationFilterTradeActive)
        {
            return;
        }

        if (trade == null)
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

    private static SolidColorBrush ForegroundText1 => (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Text.1"];
    private static SolidColorBrush BackgroundBlue => (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Blue"];
    private static SolidColorBrush BackgroundGreen => (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Green"];
    private static SolidColorBrush BackgroundYellow => (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Yellow"];
    private static SolidColorBrush BackgroundRed => (SolidColorBrush) Application.Current.Resources["SolidColorBrush.Notification.Background.Red"];
}