using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager
{
    public interface ITrackingController
    {
        ClusterInfo CurrentCluster { get; }

        void RegisterEvents() { }

        void UnregisterEvents() { }

        event Action<ClusterInfo> OnChangeCluster;

        bool ExistIndispensableInfos { get; }

        void SetNewCluster(MapType mapType, Guid? mapGuid, string clusterIndex, string mainClusterIndex) { }

        bool IsMainWindowNull();

        Task AddNotificationAsync(TrackingNotification item);

        void RemovesUnnecessaryNotifications() { }

        void ClearNotifications() { }

        void NotificationUiFilteringAsync() { }

        void AddFilterType(NotificationType notificationType) { }

        void RemoveFilterType(NotificationType notificationType) { }

        void IsTrackingAllowedByMainCharacter() { }
    }
}