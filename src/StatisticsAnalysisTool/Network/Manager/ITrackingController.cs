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

    void AddFilterType(NotificationType notificationType) { }

    void RemoveFilterType(NotificationType notificationType) { }

    void IsTrackingAllowedByMainCharacter() { }
}