using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Core;
using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.OpenWorld;
using StatisticsAnalysisTool.Party;
using StatisticsAnalysisTool.StorageHistory;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class TrackingController : ITrackingController
{
    private const int MaxNotifications = 4000;
    private static readonly TimeSpan LogoutMinimumDuration = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan LogoutServerSilenceDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan LogoutMaximumWaitDuration = TimeSpan.FromSeconds(35);

    private NetworkManager _networkManager;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private string _currentNotificationSearchText = string.Empty;
    private CancellationTokenSource _logoutDetectionCancellationTokenSource;

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
    public readonly OpenWorldController OpenWorldController;
    public readonly PartyController PartyController;
    public readonly GuildController GuildController;
    public readonly CraftingController CraftingController;
    private readonly List<LoggingFilterType> _notificationTypesFilters = [];

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
        MarketController = new MarketController(this, mainWindowViewModel);
        TradeController = new TradeController(this, mainWindowViewModel);
        VaultController = new VaultController(mainWindowViewModel);
        GatheringController = new GatheringController(this, mainWindowViewModel);
        OpenWorldController = new OpenWorldController(this, mainWindowViewModel);
        PartyController = new PartyController(this, mainWindowViewModel);
        GuildController = new GuildController(this, mainWindowViewModel);
        CraftingController = new CraftingController(this, mainWindowViewModel);
        LiveStatsTracker = new LiveStatsTracker(this, mainWindowViewModel);
    }

    #region Tracking

    public async Task InitTrackingAsync()
    {
        await StartTrackingAsync();

        _mainWindowViewModel.IsDamageMeterTrackingActive = SettingsController.CurrentSettings.IsDamageMeterTrackingActive;
        _mainWindowViewModel.LoggingBindings.IsTrackingPartyLootOnly = SettingsController.CurrentSettings.IsTrackingPartyLootOnly;
        _mainWindowViewModel.LoggingBindings.IsTrackingSilver = SettingsController.CurrentSettings.IsTrackingSilver;
        _mainWindowViewModel.LoggingBindings.IsTrackingFame = SettingsController.CurrentSettings.IsTrackingFame;
        _mainWindowViewModel.LoggingBindings.IsTrackingMobLoot = SettingsController.CurrentSettings.IsTrackingMobLoot;
        _mainWindowViewModel.LoggingBindings.IsTrackingKill = SettingsController.CurrentSettings.IsTrackingKill;

        _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView = CollectionViewSource.GetDefaultView(_mainWindowViewModel.LoggingBindings.TrackingNotifications) as ListCollectionView;
        if (_mainWindowViewModel.LoggingBindings?.GameLoggingCollectionView != null)
        {
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.Filter = IsNotificationVisible;
            _mainWindowViewModel.LoggingBindings.GameLoggingCollectionView.SortDescriptions.Add(new SortDescription(nameof(TrackingNotification.DateTime), ListSortDirection.Descending));
        }
    }

    public async Task StartTrackingAsync()
    {
        if (_networkManager?.IsAnySocketActive() ?? false)
        {
            return;
        }

        _networkManager = new NetworkManager(this);

        var provider = SettingsController.CurrentSettings.PacketProvider;

        if (provider == PacketProviderKind.Sockets && !ApplicationCore.IsAppStartedAsAdministrator())
        {
            _mainWindowViewModel.SetErrorBar(Visibility.Visible, LocalizationController.Translation("START_APPLICATION_AS_ADMINISTRATOR"));
            return;
        }

        try
        {
            ClusterController?.RegisterEvents();
            LootController?.RegisterEvents();
            TreasureController?.RegisterEvents();

            LiveStatsTracker.Start();

            _mainWindowViewModel.DungeonBindings.DungeonStatsFilter =
                new DungeonStatsFilter(_mainWindowViewModel.DungeonBindings);

            _networkManager.Start();
            _mainWindowViewModel.IsTrackingActive = true;
        }
        catch (Exception ex)
        {
            string userMsg = GetTrackingStartErrorMessage(ex);

            Log.Error(ex, "StartTracking failed | provider={Provider} | admin={IsAdmin} | msg={UserMsg}", provider, ApplicationCore.IsAppStartedAsAdministrator(), userMsg);

            _mainWindowViewModel.SetErrorBar(Visibility.Visible, userMsg, ex);

            try
            {
                StopTracking();
            }
            catch
            {
                // ignored
            }

            _mainWindowViewModel.IsTrackingActive = false;
        }
    }

    private static string GetTrackingStartErrorMessage(Exception ex)
    {
        if (ex is NoListeningAdaptersException)
        {
            return LocalizationController.Translation("NO_LISTENING_ADAPTERS");
        }

        if (ex is SocketException se)
        {
            return string.Format(LocalizationController.Translation("ERR_SOCKET_FAILED_WITH_CODE"), se.SocketErrorCode);
        }

        if (ex is UnauthorizedAccessException)
        {
            return LocalizationController.Translation("START_APPLICATION_AS_ADMINISTRATOR");
        }

        if (ex is DllNotFoundException d && (d.Message.Contains("wpcap", StringComparison.OrdinalIgnoreCase) || d.Message.Contains("npcap", StringComparison.OrdinalIgnoreCase)))
        {
            return LocalizationController.Translation("ERR_NPCAP_DLL_MISSING");
        }

        if (ex is TypeInitializationException { InnerException: DllNotFoundException inner } &&
            (inner.Message.Contains("wpcap", StringComparison.OrdinalIgnoreCase) ||
             inner.Message.Contains("npcap", StringComparison.OrdinalIgnoreCase)))
        {
            return LocalizationController.Translation("ERR_NPCAP_DLL_MISSING");
        }

        if (ex.GetType().Name.Equals("PcapException", StringComparison.OrdinalIgnoreCase))
        {
            return LocalizationController.Translation("ERR_NPCAP_OPEN_FAILED");
        }

        if (ex is InvalidOperationException)
        {
            return LocalizationController.Translation("ERR_CAPTURE_START_INVALID_OPERATION");
        }

        return LocalizationController.Translation("PACKET_HANDLER_ERROR_MESSAGE");
    }

    public void StopTracking()
    {
        if (!_mainWindowViewModel.IsTrackingActive && _networkManager is null)
        {
            return;
        }

        _networkManager?.Stop();
        _networkManager = null;

        LiveStatsTracker?.Stop();

        TreasureController.UnregisterEvents();
        LootController.UnregisterEvents();
        ClusterController.UnregisterEvents();

        _mainWindowViewModel.IsTrackingActive = false;

        Debug.Print("Stopped tracking");
    }

    public void BeginLogoutDetection()
    {
        CancelLogoutDetection();

        var cancellationTokenSource = new CancellationTokenSource();
        _logoutDetectionCancellationTokenSource = cancellationTokenSource;

        _ = DetectLogoutAsync(DateTime.UtcNow, cancellationTokenSource);
    }

    public void CancelLogoutDetection()
    {
        var cancellationTokenSource = _logoutDetectionCancellationTokenSource;
        _logoutDetectionCancellationTokenSource = null;

        if (cancellationTokenSource is null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
    }

    private async Task DetectLogoutAsync(DateTime logoutStartUtc, CancellationTokenSource cancellationTokenSource)
    {
        var cancellationToken = cancellationTokenSource.Token;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var logoutDuration = now - logoutStartUtc;

                if (logoutDuration >= LogoutMaximumWaitDuration || IsLogoutConfirmedByServerSilence(now, logoutStartUtc, logoutDuration))
                {
                    _mainWindowViewModel.MainStatusBindings.SetInGame(false);
                    if (ReferenceEquals(_logoutDetectionCancellationTokenSource, cancellationTokenSource))
                    {
                        _logoutDetectionCancellationTokenSource = null;
                    }
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static bool IsLogoutConfirmedByServerSilence(DateTime now, DateTime logoutStartUtc, TimeSpan logoutDuration)
    {
        if (logoutDuration < LogoutMinimumDuration)
        {
            return false;
        }

        var lastServerPacketReceivedUtc = GetLastServerPacketReceivedUtc();

        return lastServerPacketReceivedUtc <= logoutStartUtc
               || now - lastServerPacketReceivedUtc >= LogoutServerSilenceDuration;
    }

    private static DateTime GetLastServerPacketReceivedUtc()
    {
        if (!ServiceLocator.IsServiceInDictionary<AlbionServerDetectionService>())
        {
            return DateTime.MinValue;
        }

        return ServiceLocator.Resolve<AlbionServerDetectionService>().LastServerPacketReceivedUtc;
    }

    public async Task RestartTrackingAsync()
    {
        var wasTrackingActive = _mainWindowViewModel.IsTrackingActive;

        StopTracking();

        if (!wasTrackingActive)
        {
            return;
        }

        await StartTrackingAsync();
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
            OpenWorldController.SaveInFileAsync(),
            GuildController.SaveInFileAsync(),
            CombatController.SaveInFileAsync(),
            MarketController.SaveInFileAsync(),
            CraftingController.SaveInFileAsync(),
            ClusterController.SaveInFileAsync(),
            EstimatedMarketValueController.SaveInFileAsync()
        );
    }

    public async Task LoadDataAsync()
    {
        await Task.WhenAll(
            EstimatedMarketValueController.LoadFromFileAsync(),
            StatisticController.LoadFromFileAsync(),
            TradeController.LoadFromFileAsync(),
            TreasureController.LoadFromFileAsync(),
            DungeonController.LoadDungeonFromFileAsync(),
            GatheringController.LoadFromFileAsync(),
            OpenWorldController.LoadFromFileAsync(),
            VaultController.LoadFromFileAsync(),
            GuildController.LoadFromFileAsync(),
            CombatController.LoadFromFileAsync(),
            MarketController.LoadFromFileAsync(),
            ClusterController.LoadMapHistoryFromFileAsync(),
            CraftingController.LoadFromFileAsync()
        );
    }

    public bool ExistIndispensableInfos => ClusterController.CurrentCluster != null && EntityController.ExistLocalEntity();

    #endregion

    #region Notifications

    public async Task AddNotificationAsync(TrackingNotification item)
    {
        if (string.IsNullOrWhiteSpace(item.ClusterName))
        {
            item.SetClusterName(ClusterController.GetCurrentClusterDisplayName());
        }

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

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingKill && item.Type == LoggingFilterType.Kill)
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingMobLoot && item.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: true })
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(delegate
        {
            _mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Add(item);
        });

        _mainWindowViewModel?.LoggingBindings?.LootLoggerStats?.RecordNotification(item);

        await RemovesUnnecessaryNotificationsAsync();
    }

    private async Task RemovesUnnecessaryNotificationsAsync()
    {
        if (!IsRemovesUnnecessaryNotificationsActiveAllowed())
        {
            return;
        }

        _isRemovesUnnecessaryNotificationsActive = true;

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var notifications = _mainWindowViewModel?.LoggingBindings?.TrackingNotifications;
            if (notifications == null)
            {
                return;
            }

            while (notifications.Count > MaxNotifications)
            {
                notifications.RemoveAt(0);
            }
        });

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
            _currentNotificationSearchText = text?.Trim() ?? string.Empty;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var view = _mainWindowViewModel?.LoggingBindings?.GameLoggingCollectionView;
                if (view == null)
                {
                    return;
                }

                view.Refresh();
                _mainWindowViewModel?.LoggingBindings?.LootLoggerStats?.Refresh();
            });
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private bool IsNotificationVisible(object item)
    {
        if (item is not TrackingNotification notification)
        {
            return false;
        }

        return MatchesNotificationFilters(notification)
               && (string.IsNullOrWhiteSpace(_currentNotificationSearchText)
                   || MatchesNotificationSearch(notification, _currentNotificationSearchText));
    }

    private bool MatchesNotificationFilters(TrackingNotification notification)
    {
        if (notification == null)
        {
            return false;
        }

        return (_notificationTypesFilters?.Contains(notification.Type) ?? false)
               && (IsLootFromMobShown || notification.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment);
    }

    private static bool MatchesNotificationSearch(TrackingNotification notification, string searchText)
    {
        if (notification?.Fragment == null || string.IsNullOrWhiteSpace(searchText))
        {
            return false;
        }

        return notification.Fragment switch
        {
            OtherGrabbedLootNotificationFragment fragment =>
                ContainsIgnoreCase(fragment.LootedByName, searchText)
                || ContainsIgnoreCase(fragment.LocalizedName, searchText)
                || ContainsIgnoreCase(fragment.LootedFromName, searchText),
            KillNotificationFragment fragment =>
                ContainsIgnoreCase(fragment.Died, searchText)
                || ContainsIgnoreCase(fragment.KilledBy, searchText),
            _ => false
        };
    }

    private static bool ContainsIgnoreCase(string source, string value)
    {
        return !string.IsNullOrEmpty(source)
               && !string.IsNullOrEmpty(value)
               && source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
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
        _ = NotificationUiFilteringAsync(_currentNotificationSearchText);
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

    public bool IsKillTrackingEnabled => _mainWindowViewModel?.LoggingBindings?.IsTrackingKill ?? false;

    public bool IsLocalOrPartyEntity(long objectId)
    {
        var localEntity = EntityController.GetLocalEntity();
        if (localEntity?.Value?.ObjectId == objectId)
        {
            return true;
        }

        return EntityController.IsEntityInParty(objectId);
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
        var dialog = new DialogWindow(LocalizationController.Translation("RESET_TRACKING_NOTIFICATIONS"), LocalizationController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
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
