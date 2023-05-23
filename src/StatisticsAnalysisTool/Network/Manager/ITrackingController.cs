using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ITrackingController
{
    bool ExistIndispensableInfos { get; }
    
    Task AddNotificationAsync(TrackingNotification item);

    void RemovesUnnecessaryNotifications() { }

    void ClearNotifications() { }

    void NotificationUiFilteringAsync() { }

    void UpdateFilterType(LoggingFilterType notificationType) { }

    void RemoveFilterType(LoggingFilterType notificationType) { }

    void IsTrackingAllowedByMainCharacter() { }
}