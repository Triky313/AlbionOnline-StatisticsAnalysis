using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification : INotifyPropertyChanged
    {
        private Visibility _visibility;

        public TrackingNotification()
        {
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
        public NotificationType Type { get; }
        public Guid InstanceId { get; }

        public Visibility Visibility {
            get => _visibility;
            set {
                _visibility = value;
                OnPropertyChanged();
            }
        }

        public string Hash => $"{DateTime.Ticks}-{Type}-{InstanceId}";

        public int CompareTo(TrackingNotification value)
        {
            var compared = string.Compare(Hash, value.Hash, StringComparison.Ordinal);
            return compared != 0 ? compared : -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}