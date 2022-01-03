using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class LoggingFilterObject : INotifyPropertyChanged
    {
        private readonly TrackingController _trackingController;
        private bool? _isSelected;
        private string _name;

        public LoggingFilterObject(TrackingController trackingController, NotificationType notificationType)
        {
            _trackingController = trackingController;
            NotificationType = notificationType;
        }

        public NotificationType NotificationType { get; }

        private void SetFilter()
        {
            switch (NotificationType)
            {
                case NotificationType.Fame:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.Fame);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.Fame);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterFame = IsSelected ?? false;
                    break;
                case NotificationType.Silver:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.Silver);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.Silver);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterSilver = IsSelected ?? false;
                    break;
                case NotificationType.Faction:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.Faction);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.Faction);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterFaction = IsSelected ?? false;
                    break;
                case NotificationType.EquipmentLoot:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.EquipmentLoot);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.EquipmentLoot);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot = IsSelected ?? false;
                    break;
                case NotificationType.ConsumableLoot:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.ConsumableLoot);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.ConsumableLoot);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot = IsSelected ?? false;
                    break;
                case NotificationType.SimpleLoot:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.SimpleLoot);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.SimpleLoot);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot = IsSelected ?? false;
                    break;
                case NotificationType.UnknownLoot:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.UnknownLoot);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.UnknownLoot);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot = IsSelected ?? false;
                    break;
                case NotificationType.SeasonPoints:
                    if (IsSelected ?? false)
                    {
                        _trackingController?.AddFilterType(NotificationType.SeasonPoints);
                    }
                    else
                    {
                        _trackingController?.RemoveFilterType(NotificationType.SeasonPoints);
                    }

                    SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints = IsSelected ?? false;
                    break;
            }

            _trackingController?.NotificationUiFilteringAsync();


            //if (_isTrackingFilteredUnknownLoot)
            //{
            //    _trackingController?.AddFilterType(NotificationType.UnknownLoot);
            //}
            //else
            //{
            //    _trackingController?.RemoveFilterType(NotificationType.UnknownLoot);
            //}

            //_trackingController?.NotificationUiFilteringAsync();
            //SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot = _isTrackingFilteredUnknownLoot;
        }

        public bool? IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                SetFilter();
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}