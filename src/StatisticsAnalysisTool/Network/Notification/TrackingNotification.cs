using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification : INotifyPropertyChanged
    {
        private const int SetTypesMaxTries = 3;

        private NotificationType _type;
        private Visibility _visibility;
        private readonly int _itemIndex;
        private int _trySetTypeCounter;

        public TrackingNotification(DateTime dateTime, LineFragment fragment, NotificationType type)
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

        public NotificationType Type
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
            if ((Type == NotificationType.Unknown && _trySetTypeCounter <= SetTypesMaxTries) || forceSetType)
            {
                Type = GetNotificationType(ItemController.GetItemType(_itemIndex));
            }

            _trySetTypeCounter++;
        }

        private static NotificationType GetNotificationType(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Weapon => NotificationType.EquipmentLoot,
                ItemType.Consumable => NotificationType.ConsumableLoot,
                ItemType.Simple => NotificationType.SimpleLoot,
                _ => NotificationType.UnknownLoot,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}