using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Notification;

public interface ISatNotificationManager
{ 
    Task ShowSuccessAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task ShowTrackingStatusAsync(string title, string message);
    Task ShowTradeAsync(Trade.Trade trade);
}