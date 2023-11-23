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

    void NotificationUiFilteringAsync(string text = null) { }

    void UpdateFilterType(LoggingFilterType notificationType) { }

    void RemoveFilterType(LoggingFilterType notificationType) { }

    bool IsTrackingAllowedByMainCharacter();

    void SetUpcomingRepair(long buildingObjectId, long costs);

    void RepairFinished(long userObjectId, long buildingObjectId);

    void RegisterBuilding(long buildingObjectId);

    void UnregisterBuilding(long buildingObjectId);

    void StopTracking();

    Task SaveDataAsync();
}