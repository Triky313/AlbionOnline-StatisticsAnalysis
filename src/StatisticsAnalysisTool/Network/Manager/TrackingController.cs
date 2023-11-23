using Microsoft.Extensions.Hosting;
using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class TrackingController : ITrackingController, IHostedService
{
    private const int MaxNotifications = 4000;

    private readonly MainWindowViewModelOld _mainWindowViewModel;
    private readonly IEntityController _entityController;
    private readonly IClusterController _clusterController;
    private readonly IDungeonController _dungeonController;
    private readonly ITreasureController _treasureController;
    private readonly ITradeController _tradeController;
    private readonly IVaultController _vaultController;
    private readonly IGuildController _guildController;
    private readonly IGatheringController _gatheringController;
    private readonly ILiveStatsTracker _liveStatsTracker;
    private readonly IStatisticController _statisticController;
    private readonly ErrorBarViewModel _errorBarViewModel;
    private readonly List<LoggingFilterType> _notificationTypesFilters = new();

    public TrackingController(MainWindowViewModelOld mainWindowViewModel,
        IEntityController entityController,
        IClusterController clusterController,
        IDungeonController dungeonController,
        ITreasureController treasureController,
        ITradeController tradeController,
        IVaultController vaultController,
        IGuildController guildController,
        IGatheringController gatheringController,
        ILiveStatsTracker liveStatsTracker,
        IStatisticController statisticController,
        ErrorBarViewModel errorBarViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _entityController = entityController;
        _clusterController = clusterController;
        _dungeonController = dungeonController;
        _treasureController = treasureController;
        _tradeController = tradeController;
        _vaultController = vaultController;
        _guildController = guildController;
        _gatheringController = gatheringController;
        _liveStatsTracker = liveStatsTracker;
        _statisticController = statisticController;
        _errorBarViewModel = errorBarViewModel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        StartEventsFunctionality();
        await LoadDataAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    #region Tracking

    private void StartEventsFunctionality()
    {
        _clusterController?.RegisterEvents();
        _treasureController?.RegisterEvents();

        _liveStatsTracker.Start();
    }

    public void StopTracking()
    {
        _liveStatsTracker?.Stop();
        _treasureController.UnregisterEvents();
        _clusterController.UnregisterEvents();

        Debug.Print("Stopped tracking");
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await Task.WhenAll(
                EstimatedMarketValueController.LoadFromFileAsync(),
                _statisticController.LoadFromFileAsync(),
                _tradeController.LoadFromFileAsync(),
                _treasureController.LoadFromFileAsync(),
                _dungeonController.LoadDungeonFromFileAsync(),
                _gatheringController.LoadFromFileAsync(),
                _vaultController.LoadFromFileAsync(),
                _guildController.LoadFromFileAsync()
            );
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _errorBarViewModel.Set(Visibility.Visible, e.Message);
        }
    }

    public async Task SaveDataAsync()
    {
        await Task.WhenAll(
            _vaultController.SaveInFileAsync(),
            _tradeController.SaveInFileAsync(),
            _treasureController.SaveInFileAsync(),
            _statisticController.SaveInFileAsync(),
            _dungeonController.SaveInFileAsync(),
            _gatheringController.SaveInFileAsync(true),
            _guildController.SaveInFileAsync(),
            EstimatedMarketValueController.SaveInFileAsync(),
            FileController.SaveAsync(_mainWindowViewModel.DamageMeterBindings?.DamageMeterSnapshots,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DamageMeterSnapshotsFileName))
        );
        Debug.Print("Damage Meter snapshots saved");
    }

    public bool ExistIndispensableInfos => ClusterController.CurrentCluster != null && _entityController.ExistLocalEntity();

    #endregion

    #region Notifications

    public async Task AddNotificationAsync(TrackingNotification item)
    {
        item.SetType();

        if (!IsTrackingAllowedByMainCharacter() && item.Type is LoggingFilterType.Fame or LoggingFilterType.Silver or LoggingFilterType.Faction)
        {
            return;
        }

        if (_mainWindowViewModel?.LoggingBindings?.TrackingNotifications == null)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingFame && item.Type == LoggingFilterType.Fame)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingSilver && item.Type == LoggingFilterType.Silver)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingMobLoot && item.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: true })
        {
            return;
        }

        SetNotificationFilteredVisibility(item);

        await Application.Current.Dispatcher.InvokeAsync(delegate
        {
            _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Insert(0, item);
        });

        await RemovesUnnecessaryNotificationsAsync();
        await SetNotificationTypesAsync();
    }

    private async Task RemovesUnnecessaryNotificationsAsync()
    {
        if (!IsRemovesUnnecessaryNotificationsActiveAllowed())
        {
            return;
        }

        _isRemovesUnnecessaryNotificationsActive = true;

        int? numberToBeRemoved = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.Count - MaxNotifications;
        if (numberToBeRemoved is > 0)
        {
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToList().OrderBy(x => x?.DateTime).Take((int) numberToBeRemoved).ToAsyncEnumerable();
            if (notifications != null)
            {
                await foreach (var notification in notifications)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _ = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Remove(notification);
                    });
                }
            }
        }

        _isRemovesUnnecessaryNotificationsActive = false;
    }

    public async Task NotificationUiFilteringAsync(string text = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                })!;

                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().Where(x =>
                    (_notificationTypesFilters?.Contains(x.Type) ?? true)
                    &&
                    (x.Fragment is OtherGrabbedLootNotificationFragment fragment &&
                     (fragment.LootedByName.ToLower().Contains(text.ToLower())
                      || fragment.LocalizedName.ToLower().Contains(text.ToLower())
                      || fragment.LootedFromName.ToLower().Contains(text.ToLower())
                     )
                     ||
                     x.Fragment is KillNotificationFragment killFragment &&
                     (killFragment.Died.ToLower().Contains(text.ToLower())
                      || killFragment.KilledBy.ToLower().Contains(text.ToLower())
                     )
                    )
                    && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                ).ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                })!;
            }
            else
            {
                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                })!;

                await _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.Where(x =>
                    (_notificationTypesFilters?.Contains(x.Type) ?? false)
                    && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                ).ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                })!;
            }
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private void SetNotificationFilteredVisibility(TrackingNotification trackingNotification)
    {
        trackingNotification.Visibility = IsNotificationFiltered(trackingNotification) ? Visibility.Collapsed : Visibility.Visible;
    }

    private bool IsNotificationFiltered(TrackingNotification trackingNotification)
    {
        return !_notificationTypesFilters?.Exists(x => x == trackingNotification.Type) ?? false;
    }

    public void UpdateFilterType(LoggingFilterType notificationType, bool isSelected)
    {
        if (notificationType == LoggingFilterType.ShowLootFromMob)
        {
            IsLootFromMobShown = isSelected;
            SettingsController.CurrentSettings.IsLootFromMobShown = isSelected;
        }
        else if (isSelected && !_notificationTypesFilters.Exists(x => x == notificationType))
        {
            _notificationTypesFilters.Add(notificationType);
        }
        else if (!isSelected && _notificationTypesFilters.Exists(x => x == notificationType))
        {
            _notificationTypesFilters.Remove(notificationType);
        }

        UpdateLoggingFilterSettings(notificationType, isSelected);
    }

    private static void UpdateLoggingFilterSettings(LoggingFilterType notificationType, bool isSelected)
    {
        switch (notificationType)
        {
            case LoggingFilterType.Fame:
                SettingsController.CurrentSettings.IsMainTrackerFilterFame = isSelected;
                break;
            case LoggingFilterType.Silver:
                SettingsController.CurrentSettings.IsMainTrackerFilterSilver = isSelected;
                break;
            case LoggingFilterType.Faction:
                SettingsController.CurrentSettings.IsMainTrackerFilterFaction = isSelected;
                break;
            case LoggingFilterType.EquipmentLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot = isSelected;
                break;
            case LoggingFilterType.ConsumableLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot = isSelected;
                break;
            case LoggingFilterType.SimpleLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot = isSelected;
                break;
            case LoggingFilterType.UnknownLoot:
                SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot = isSelected;
                break;
            case LoggingFilterType.SeasonPoints:
                SettingsController.CurrentSettings.IsMainTrackerFilterSeasonPoints = isSelected;
                break;
            case LoggingFilterType.ShowLootFromMob:
                SettingsController.CurrentSettings.IsLootFromMobShown = isSelected;
                break;
            case LoggingFilterType.Kill:
                SettingsController.CurrentSettings.IsMainTrackerFilterKill = isSelected;
                break;
        }
    }

    public bool IsLootFromMobShown { get; set; }

    private async Task SetNotificationTypesAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToAsyncEnumerable();
            if (notifications != null)
            {
                await foreach (var notification in notifications)
                {
                    notification.SetType();
                }
            }
        });
    }

    private static bool _isRemovesUnnecessaryNotificationsActive;
    private DateTime _lastRemovesUnnecessaryNotifications;

    private bool IsRemovesUnnecessaryNotificationsActiveAllowed(int waitTimeInSeconds = 1)
    {
        var currentDateTime = DateTime.UtcNow;
        var difference = currentDateTime.Subtract(_lastRemovesUnnecessaryNotifications);
        if (difference.Seconds >= waitTimeInSeconds && !_isRemovesUnnecessaryNotificationsActive)
        {
            _lastRemovesUnnecessaryNotifications = currentDateTime;
            return true;
        }

        return false;
    }

    #endregion

    #region Specific character name tracking

    public bool IsTrackingAllowedByMainCharacter()
    {
        var localEntity = _entityController.GetLocalEntity();

        if (localEntity?.Value?.Name == null || string.IsNullOrEmpty(SettingsController.CurrentSettings.MainTrackingCharacterName))
        {
            return true;
        }

        if (localEntity.Value.Value.Name == SettingsController.CurrentSettings.MainTrackingCharacterName)
        {
            return true;
        }

        if (localEntity.Value.Value.Name != SettingsController.CurrentSettings.MainTrackingCharacterName)
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Gear repairing

    private long _buildingObjectId = -1;
    private long _upcomingRepairCosts;

    public void RegisterBuilding(long buildingObjectId)
    {
        _buildingObjectId = buildingObjectId;
    }

    public void UnregisterBuilding(long buildingObjectId)
    {
        if (buildingObjectId != _buildingObjectId)
        {
            return;
        }

        _buildingObjectId = -1;
        _upcomingRepairCosts = 0;
    }

    public void SetUpcomingRepair(long buildingObjectId, long costs)
    {
        if (_buildingObjectId != buildingObjectId)
        {
            return;
        }

        _upcomingRepairCosts = costs;
    }

    public void RepairFinished(long userObjectId, long buildingObjectId)
    {
        if (_entityController.LocalUserData.UserObjectId != userObjectId || _upcomingRepairCosts <= 0 || _buildingObjectId != buildingObjectId)
        {
            return;
        }

        _statisticController?.AddValue(ValueType.RepairCosts, FixPoint.FromInternalValue(_upcomingRepairCosts).DoubleValue);
        _statisticController?.UpdateRepairCostsUi();
    }

    #endregion
}