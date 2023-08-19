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
    private Visibility _visibility;
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

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
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