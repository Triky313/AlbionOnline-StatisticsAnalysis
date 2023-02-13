using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;

namespace StatisticsAnalysisTool.Network.Manager;

public class TrackingController : ITrackingController
{
    private const int MaxNotifications = 4000;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly MainWindow _mainWindow;
    private readonly MainWindowViewModel _mainWindowViewModel;
    public readonly LiveStatsTracker LiveStatsTracker;
    public readonly CombatController CombatController;
    public readonly DungeonController DungeonController;
    public readonly ClusterController ClusterController;
    public readonly EntityController EntityController;
    public readonly LootController LootController;
    public readonly StatisticController StatisticController;
    public readonly TreasureController TreasureController;
    public readonly MailController MailController;
    public readonly MarketController MarketController;
    public readonly TradeController TradeController;
    public readonly VaultController VaultController;
    public readonly GatheringController GatheringController;
    private readonly List<NotificationType> _notificationTypesFilters = new();

    public TrackingController(MainWindowViewModel mainWindowViewModel, MainWindow mainWindow)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _mainWindow = mainWindow;
        ClusterController = new ClusterController(this, mainWindowViewModel);
        EntityController = new EntityController(mainWindowViewModel);
        DungeonController = new DungeonController(this, mainWindowViewModel);
        CombatController = new CombatController(this, _mainWindow, mainWindowViewModel);
        LootController = new LootController(this, mainWindowViewModel);
        StatisticController = new StatisticController(this, mainWindowViewModel);
        TreasureController = new TreasureController(this, mainWindowViewModel);
        MailController = new MailController(this, mainWindowViewModel);
        MarketController = new MarketController(this);
        TradeController = new TradeController(mainWindowViewModel);
        VaultController = new VaultController(mainWindowViewModel);
        GatheringController = new GatheringController(this, mainWindowViewModel);
        LiveStatsTracker = new LiveStatsTracker(this, mainWindowViewModel);
    }

    public bool ExistIndispensableInfos => ClusterController.CurrentCluster != null && EntityController.ExistLocalEntity();

    #region Tracking Controller Helper

    public bool IsMainWindowNull()
    {
        if (_mainWindow != null)
        {
            return false;
        }

        Log.Error($"{MethodBase.GetCurrentMethod()?.DeclaringType}: _mainWindow is null.");
        return true;
    }

    #endregion

    #region Notifications

    public async Task AddNotificationAsync(TrackingNotification item)
    {
        item.SetType();

        if (!IsTrackingAllowedByMainCharacter() && item.Type is NotificationType.Fame or NotificationType.Silver or NotificationType.Faction)
        {
            return;
        }

        if (_mainWindowViewModel?.LoggingBindings?.TrackingNotifications == null)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingFame && item.Type == NotificationType.Fame)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingSilver && item.Type == NotificationType.Silver)
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
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications?.ToList().OrderBy(x => x?.DateTime).Take((int)numberToBeRemoved).ToAsyncEnumerable();
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

    public async Task ClearNotificationsAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Clear();
        });
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

    public void AddFilterType(NotificationType notificationType)
    {
        if (!_notificationTypesFilters.Exists(x => x == notificationType))
        {
            _notificationTypesFilters.Add(notificationType);
        }
    }

    public void RemoveFilterType(NotificationType notificationType)
    {
        if (_notificationTypesFilters.Exists(x => x == notificationType))
        {
            _ = _notificationTypesFilters.Remove(notificationType);
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
        var localEntity = EntityController.GetLocalEntity();

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
        if (EntityController.LocalUserData.UserObjectId != userObjectId || _upcomingRepairCosts <= 0 || _buildingObjectId != buildingObjectId)
        {
            return;
        }

        StatisticController?.AddValue(ValueType.RepairCosts, FixPoint.FromInternalValue(_upcomingRepairCosts).DoubleValue);
        StatisticController?.UpdateRepairCostsUi();
    }

    #endregion
}