using Serilog;
using SharpPcap;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.PartyBuilder;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class TrackingController : ITrackingController
{
    private const int MaxNotifications = 4000;

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
    public readonly PartyBuilderController PartyBuilderController;
    private readonly List<LoggingFilterType> _notificationTypesFilters = new();

    public TrackingController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        ClusterController = new ClusterController(this, mainWindowViewModel);
        EntityController = new EntityController(this, mainWindowViewModel);
        DungeonController = new DungeonController(this, mainWindowViewModel);
        CombatController = new CombatController(this, mainWindowViewModel);
        LootController = new LootController(this, mainWindowViewModel);
        StatisticController = new StatisticController(this, mainWindowViewModel);
        TreasureController = new TreasureController(this, mainWindowViewModel);
        MailController = new MailController(this, mainWindowViewModel);
        MarketController = new MarketController(this);
        TradeController = new TradeController(mainWindowViewModel);
        VaultController = new VaultController(mainWindowViewModel);
        GatheringController = new GatheringController(this, mainWindowViewModel);
        PartyBuilderController = new PartyBuilderController(this, mainWindowViewModel);
        LiveStatsTracker = new LiveStatsTracker(this, mainWindowViewModel);

        _ = InitTrackingAsync();
    }

    #region Tracking

    public async Task InitTrackingAsync()
    {
        await StartTrackingAsync();

        _mainWindowViewModel.IsDamageMeterTrackingActive = SettingsController.CurrentSettings.IsDamageMeterTrackingActive;
        _mainWindowViewModel.IsTrackingPartyLootOnly = SettingsController.CurrentSettings.IsTrackingPartyLootOnly;
        _mainWindowViewModel.LoggingBindings.IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
        _mainWindowViewModel.LoggingBindings.IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;
        _mainWindowViewModel.LoggingBindings.IsTrackingMobLoot = SettingsController.CurrentSettings.IsTrackingMobLoot;

        _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView = CollectionViewSource.GetDefaultView(_mainWindowViewModel.LoggingBindings.TrackingNotifications) as ListCollectionView;
        if (_mainWindowViewModel.LoggingBindings?.GameLoggingCollectionView != null)
        {
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.IsLiveSorting = true;
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.IsLiveFiltering = true;
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.SortDescriptions.Add(new SortDescription(nameof(DateTime), ListSortDirection.Descending));
        }
    }

    public async Task StartTrackingAsync()
    {
        if (NetworkManager.IsNetworkCaptureRunning())
        {
            return;
        }

        await Task.WhenAll(
            EstimatedMarketValueController.LoadFromFileAsync(),
            StatisticController.LoadFromFileAsync(),
            TradeController.LoadFromFileAsync(),
            TreasureController.LoadFromFileAsync(),
            DungeonController.LoadDungeonFromFileAsync(),
            GatheringController.LoadFromFileAsync(),
            VaultController.LoadFromFileAsync()
        );

        ClusterController?.RegisterEvents();
        LootController?.RegisterEvents();
        TreasureController?.RegisterEvents();

        LiveStatsTracker.Start();

        _mainWindowViewModel.DungeonBindings.DungeonStatsFilter = new DungeonStatsFilter(_mainWindowViewModel.DungeonBindings);

        try
        {
            NetworkManager.StartNetworkCapture(this);
            _mainWindowViewModel.IsTrackingActive = true;
        }
        catch (PcapException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation(e.Message));
        }
        catch (NoListeningAdaptersException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("NO_LISTENING_ADAPTERS"));
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _mainWindowViewModel.SetErrorBar(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));

            StopTracking();
        }
    }

    public void StopTracking()
    {
        if (!_mainWindowViewModel.IsTrackingActive)
        {
            return;
        }

        NetworkManager.StopDeviceCapture();

        LiveStatsTracker?.Stop();

        TreasureController.UnregisterEvents();
        LootController.UnregisterEvents();
        ClusterController.UnregisterEvents();

        _mainWindowViewModel.IsTrackingActive = false;

        Debug.Print("Stopped tracking");
    }

    public async Task SaveDataAsync()
    {
        await Task.WhenAll(
            VaultController.SaveInFileAsync(),
            TradeController.SaveInFileAsync(),
            TreasureController.SaveInFileAsync(),
            StatisticController.SaveInFileAsync(),
            DungeonController.SaveInFileAsync(),
            GatheringController.SaveInFileAsync(true),
            EstimatedMarketValueController.SaveInFileAsync(),
            FileController.SaveAsync(_mainWindowViewModel.DamageMeterBindings?.DamageMeterSnapshots,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DamageMeterSnapshotsFileName))
        );
        Debug.Print("Damage Meter snapshots saved");
    }

    public bool ExistIndispensableInfos => ClusterController.CurrentCluster != null && EntityController.ExistLocalEntity();

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

    public async Task ResetTrackingNotificationsAsync()
    {
        var dialog = new DialogWindow(LanguageController.Translation("RESET_TRACKING_NOTIFICATIONS"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            await ClearNotificationsAsync()!;
            Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel?.LoggingBindings?.TopLooters?.Clear());
            LootController?.ClearLootLogger();
        }
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