using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification : INotifyPropertyChanged
    {
        private const int _setTypesMaxTries = 3;

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

        public async Task SetTypeAsync(bool forceSetType = false)
        {
            if (Type == NotificationType.Unknown && _trySetTypeCounter <= _setTypesMaxTries || forceSetType)
            {
                Type = GetNotificationType(await ItemController.GetItemTypeAsync(_itemIndex));
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