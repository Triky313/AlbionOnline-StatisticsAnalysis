using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager
{
    public interface ITrackingController
    {
        ClusterInfo CurrentCluster { get; }

        void RegisterEvents() { }

        void UnregisterEvents() { }

        void DamageMeterUpdate(long objectId, GameTimeStamp timeStamp, double healthChange, double newHealthValue, EffectType effectType, EffectOrigin effectOrigin, long causerId, int causingSpellType) { }

        event Action<ClusterInfo> OnChangeCluster;

        bool ExistIndispensableInfos { get; }

        void SetNewCluster(MapType mapType, Guid? mapGuid, string clusterIndex, string mainClusterIndex) { }
        
        void RemovesClusterIfMoreThanLimit() { }

        bool IsMainWindowNull();

        Task AddNotificationAsync(TrackingNotification item);

        void RemovesUnnecessaryNotifications() { }

        void ClearNotifications() { }

        void NotificationUiFilteringAsync() { }

        void AddFilterType(NotificationType notificationType) { }

        void RemoveFilterType(NotificationType notificationType) { }

    }
}