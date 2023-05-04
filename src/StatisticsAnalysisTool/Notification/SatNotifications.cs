using FontAwesome5;
using Notification.Wpf;
using Notification.Wpf.Classes;
using StatisticsAnalysisTool.Common;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Notification.Wpf.Base;

namespace StatisticsAnalysisTool.Notification;

public class SatNotifications : NotificationManager
{
    private readonly NotificationManager _notificationManager;

    public SatNotifications(NotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
    }

    public async Task ShowTradeAsync(Trade.Trade trade)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var content = new NotificationContent
            {
                Title = "Item erfolgreich hinzugefügt!",
                Message = $"{trade.Item.LocalizedName}",
                Type = NotificationType.Success,
                TrimType = NotificationTextTrimType.AttachIfMoreRows,
                RowsCount = 2,
                CloseOnClick = true,
                Background = new SolidColorBrush(Color.FromRgb(8, 153, 122)),
                Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            };

            _notificationManager.Show(content);
        });
    }
    public void ShowAsync(string uniqueName)
    {
        _notificationManager.Show($"Added {uniqueName}", NotificationType.Success);

        var content = new NotificationContent
        {
            Title = "Sample notification",
            Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            Type = NotificationType.Information,
            TrimType = NotificationTextTrimType.Attach, // will show attach button on message
            RowsCount = 3, //Will show 3 rows and trim after
            //LeftButtonAction = () => SomeAction(), //Action on left button click, button will not show if it null 
            //RightButtonAction = () => SomeAction(), //Action on right button click,  button will not show if it null
            //LeftButtonContent, // Left button content (string or what u want
            //RightButtonContent, // Right button content (string or what u want
            CloseOnClick = true, // Set true if u want close message when left mouse button click on message (base = true)

            Background = new SolidColorBrush(Colors.White),
            Foreground = new SolidColorBrush(Colors.DarkRed),

            Icon = new SvgAwesome()
            {
                Icon = EFontAwesomeIcon.Regular_Star,
                Height = 25,
                Foreground = new SolidColorBrush(Colors.Yellow)
            },

            //Image = new NotificationImage()
            //{
            //    Source = new BitmapImage(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\Test image.png"))),
            //    Position = ImagePosition.Top
            //}
        };

        //Application.Current.Dispatcher.Invoke(() => NotificationManager.Show(content));

        //_notificationManager.Show(content);
        //_notificationManager.Show("Message", NotificationType.Information);
        //_notificationManager.Show("Message", NotificationType.Success);
        //_notificationManager.Show("Message", NotificationType.Warning);
    }
}