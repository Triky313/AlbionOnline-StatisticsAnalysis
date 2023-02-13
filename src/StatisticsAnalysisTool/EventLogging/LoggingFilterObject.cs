using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingFilterObject : INotifyPropertyChanged
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private bool? _isSelected;
    private string _name;

    public LoggingFilterObject(TrackingController trackingController, MainWindowViewModel mainWindowViewModel, LoggingFilterType loggingFilterType)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        LoggingFilterType = loggingFilterType;
    }

    public LoggingFilterType LoggingFilterType { get; }

    private void SetFilter(string searchText)
    {
        switch (LoggingFilterType)
        {
            case LoggingFilterType.Fame:
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
            case LoggingFilterType.Silver:
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
            case LoggingFilterType.Faction:
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
            case LoggingFilterType.EquipmentLoot:
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
            case LoggingFilterType.ConsumableLoot:
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
            case LoggingFilterType.SimpleLoot:
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
            case LoggingFilterType.UnknownLoot:
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
            case LoggingFilterType.SeasonPoints:
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
            case LoggingFilterType.Kill:
                if (IsSelected ?? false)
                {
                    _trackingController?.AddFilterType(NotificationType.Kill);
                }
                else
                {
                    _trackingController?.RemoveFilterType(NotificationType.Kill);
                }

                SettingsController.CurrentSettings.IsMainTrackerFilterKill = IsSelected ?? false;
                break;
            case LoggingFilterType.ShowLootFromMob:
                _trackingController.IsLootFromMobShown = IsSelected ?? false;
                SettingsController.CurrentSettings.IsLootFromMobShown = IsSelected ?? false;
                break;
        }

        _trackingController?.NotificationUiFilteringAsync(searchText);
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            SetFilter(_mainWindowViewModel.LoggingSearchText);
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