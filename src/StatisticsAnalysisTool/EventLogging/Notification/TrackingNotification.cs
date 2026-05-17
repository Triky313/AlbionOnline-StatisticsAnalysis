using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class TrackingNotification : BaseViewModel
{
    private const int SetTypesMaxTries = 3;

    private LoggingFilterType _type;
    private readonly int _itemIndex;
    private int _trySetTypeCounter;

    public TrackingNotification(DateTime dateTime, LineFragment fragment, LoggingFilterType type)
    {
        DateTime = dateTime;
        Fragment = fragment;
        Type = type;
        InstanceId = Guid.NewGuid();
    }

    public TrackingNotification(DateTime dateTime, OtherGrabbedLootNotificationFragment fragment, int itemIndex)
    {
        DateTime = dateTime;
        Fragment = fragment;
        _itemIndex = itemIndex;
        InstanceId = Guid.NewGuid();
    }

    public DateTime DateTime { get; }
    public LineFragment Fragment { get; }
    public string ClusterName { get; private set; } = string.Empty;
    public string ClusterDisplayText => string.IsNullOrWhiteSpace(ClusterName) ? string.Empty : $"[{ClusterName}]";
    public Visibility ClusterDisplayVisibility => string.IsNullOrWhiteSpace(ClusterName) ? Visibility.Collapsed : Visibility.Visible;

    public LoggingFilterType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged();
        }
    }

    public Guid InstanceId { get; }

    public void SetClusterName(string clusterName)
    {
        var normalizedClusterName = clusterName?.Trim() ?? string.Empty;
        if (ClusterName == normalizedClusterName)
        {
            return;
        }

        ClusterName = normalizedClusterName;
        OnPropertyChanged(nameof(ClusterName));
        OnPropertyChanged(nameof(ClusterDisplayText));
        OnPropertyChanged(nameof(ClusterDisplayVisibility));
    }

    public void SetType(bool forceSetType = false)
    {
        if ((Type == LoggingFilterType.Unknown && _trySetTypeCounter <= SetTypesMaxTries) || forceSetType)
        {
            Type = GetNotificationType(ItemController.GetItemType(_itemIndex));
        }

        _trySetTypeCounter++;
    }

    private static LoggingFilterType GetNotificationType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => LoggingFilterType.EquipmentLoot,
            ItemType.Consumable => LoggingFilterType.ConsumableLoot,
            ItemType.Simple => LoggingFilterType.SimpleLoot,
            _ => LoggingFilterType.UnknownLoot,
        };
    }
}