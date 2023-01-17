using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ITrackingController
{
    bool ExistIndispensableInfos { get; }

    bool IsMainWindowNull();

    Task AddNotificationAsync(TrackingNotification item);

    void RemovesUnnecessaryNotifications() { }

    void ClearNotifications() { }

    void NotificationUiFilteringAsync() { }

    void AddFilterType(NotificationType notificationType) { }

    void RemoveFilterType(NotificationType notificationType) { }

    void IsTrackingAllowedByMainCharacter() { }
}