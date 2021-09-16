using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification : INotifyPropertyChanged
    {
        private NotificationType _type;
        private Visibility _visibility;

        public TrackingNotification(DateTime dateTime, IEnumerable<LineFragment> fragments, int itemIndex)
        {
            DateTime = dateTime;
            Fragments = fragments;
            InstanceId = Guid.NewGuid();
            _ = SetTypeAsync(itemIndex);
        }

        public TrackingNotification(DateTime dateTime, IEnumerable<LineFragment> fragments, NotificationType type)
        {
            DateTime = dateTime;
            Fragments = fragments;
            Type = type;
            InstanceId = Guid.NewGuid();
        }

        public DateTime DateTime { get; }
        public IEnumerable<LineFragment> Fragments { get; }

        public NotificationType Type {
            get => _type;
            set {
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

        private async Task SetTypeAsync(int itemIndex)
        {
            Type = GetNotificationType(await ItemController.GetItemTypeAsync(itemIndex));
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