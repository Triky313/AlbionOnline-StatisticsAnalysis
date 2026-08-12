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
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.OpenWorld;
using StatisticsAnalysisTool.Party;
using StatisticsAnalysisTool.Properties;
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
using System.Linq;
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
                    var statisticsSessionEnded = StatisticController.EndSession(now);
                    if (ReferenceEquals(_logoutDetectionCancellationTokenSource, cancellationTokenSource))
                    {
                        _logoutDetectionCancellationTokenSource = null;
                    }

                    if (statisticsSessionEnded)
                    {
                        await StatisticController.SaveInFileAsync();
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
            GatheringController.SaveInFileAsync(),
            OpenWorldController.SaveInFileAsync(),
            GuildController.SaveInFileAsync(),
            CombatController.SaveInFileAsync(),
            MarketController.SaveInFileAsync(),
            CraftingController.SaveInFileAsync(),
            ClusterController.SaveInFileAsync(),
            EstimatedMarketValueController.SaveInFileAsync()
        );
    }

    public async Task LoadDataAsync(
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
        List<(string Name, Func<Task> TaskFactory)> loadTaskFactories =
        [
            (Settings.Default.EstimatedMarketValueFileName, EstimatedMarketValueController.LoadFromFileAsync),
            ("statistics-*.json", StatisticController.LoadFromFileAsync),
            (Settings.Default.TradesFileName, TradeController.LoadFromFileAsync),
            (Settings.Default.TreasureStatsFileName, TreasureController.LoadFromFileAsync),
            (Settings.Default.DungeonRunsFileName, DungeonController.LoadDungeonFromFileAsync),
            (Settings.Default.GatheringFileName, GatheringController.LoadFromFileAsync),
            ("OpenWorldMobKills.json", OpenWorldController.LoadFromFileAsync),
            (Settings.Default.VaultsFileName, VaultController.LoadFromFileAsync),
            (Settings.Default.GuildFileName, GuildController.LoadFromFileAsync),
            (Settings.Default.DamageMeterSnapshotsFileName, CombatController.LoadFromFileAsync),
            (Settings.Default.MarketFileName, MarketController.LoadFromFileAsync),
            (Settings.Default.MapHistoryFileName, ClusterController.LoadMapHistoryFromFileAsync),
            ("Craftings.json", CraftingController.LoadFromFileAsync)
        ];

        var activeTaskNames = loadTaskFactories.Select(x => x.Name).ToList();
        var completedTaskCount = 0;
        var syncRoot = new object();

        reportProgress?.Invoke(progressStart, activeTaskNames[0]);

        async Task LoadAndReportAsync(string taskName, Func<Task> taskFactory)
        {
            try
            {
                await taskFactory();
            }
            finally
            {
                string currentTaskName;
                double progress;

                lock (syncRoot)
                {
                    activeTaskNames.Remove(taskName);
                    completedTaskCount++;
                    currentTaskName = activeTaskNames.FirstOrDefault() ?? taskName;
                    progress = progressStart + completedTaskCount / (double) loadTaskFactories.Count * (progressEnd - progressStart);
                }

                reportProgress?.Invoke(progress, currentTaskName);
            }
        }

        await Task.WhenAll(loadTaskFactories.Select(x => LoadAndReportAsync(x.Name, x.TaskFactory)));
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

    #region Awakened weapon

    private readonly object _awakenedWeaponSyncRoot = new();
    private long _upcomingAwakenedWeaponActionTicks;
    private long _upcomingAwakenedWeaponBuildingObjectId = -1;
    private long _upcomingAwakenedWeaponCosts;
    private bool _upcomingAwakenedWeaponTraitUpgrade;
    private long _upcomingAwakenedWeaponTraitUpgradeUserObjectId = -1;
    private bool _upcomingAwakenedWeaponTraitUpgradeProc;

    public void SetUpcomingAwakenedWeaponAction(long buildingObjectId, long actionTicks, long costs)
    {
        if (buildingObjectId <= 0
            || actionTicks <= 0
            || costs <= 0)
        {
            return;
        }

        lock (_awakenedWeaponSyncRoot)
        {
            if (_upcomingAwakenedWeaponActionTicks == actionTicks
                && _upcomingAwakenedWeaponBuildingObjectId == buildingObjectId)
            {
                return;
            }

            _upcomingAwakenedWeaponActionTicks = actionTicks;
            _upcomingAwakenedWeaponBuildingObjectId = buildingObjectId;
            _upcomingAwakenedWeaponCosts = costs;
            _upcomingAwakenedWeaponTraitUpgrade = false;
            _upcomingAwakenedWeaponTraitUpgradeUserObjectId = -1;
            _upcomingAwakenedWeaponTraitUpgradeProc = false;
        }
    }

    public void RerollItemTraitValueFinished(long userObjectId, long buildingObjectId, bool isProc)
    {
        lock (_awakenedWeaponSyncRoot)
        {
            if (userObjectId <= 0
                || _upcomingAwakenedWeaponCosts <= 0
                || _upcomingAwakenedWeaponBuildingObjectId != buildingObjectId)
            {
                return;
            }

            _upcomingAwakenedWeaponTraitUpgrade = true;
            _upcomingAwakenedWeaponTraitUpgradeUserObjectId = userObjectId;
            _upcomingAwakenedWeaponTraitUpgradeProc |= isProc;
        }
    }

    public void AwakenedWeaponActionFinished(long userObjectId, long buildingObjectId)
    {
        long costs;
        bool traitUpgraded;
        bool traitUpgradeProcced;

        lock (_awakenedWeaponSyncRoot)
        {
            if (_upcomingAwakenedWeaponCosts <= 0
                || _upcomingAwakenedWeaponBuildingObjectId != buildingObjectId)
            {
                return;
            }

            costs = _upcomingAwakenedWeaponCosts;
            traitUpgraded = _upcomingAwakenedWeaponTraitUpgrade
                            && _upcomingAwakenedWeaponTraitUpgradeUserObjectId == userObjectId;
            traitUpgradeProcced = traitUpgraded && _upcomingAwakenedWeaponTraitUpgradeProc;
            ResetUpcomingAwakenedWeaponAction();
        }

        StatisticController.AddAwakenedWeaponAction(
            FixPoint.FromInternalValue(costs).DoubleValue,
            traitUpgraded,
            traitUpgradeProcced);
    }

    private void ResetUpcomingAwakenedWeaponAction()
    {
        _upcomingAwakenedWeaponActionTicks = 0;
        _upcomingAwakenedWeaponBuildingObjectId = -1;
        _upcomingAwakenedWeaponCosts = 0;
        _upcomingAwakenedWeaponTraitUpgrade = false;
        _upcomingAwakenedWeaponTraitUpgradeUserObjectId = -1;
        _upcomingAwakenedWeaponTraitUpgradeProc = false;
    }

    #endregion

    #region Item quality reroll

    private readonly object _qualityRerollSyncRoot = new();
    private readonly HashSet<long> _upcomingQualityRerollItemObjectIds = [];
    private readonly Dictionary<long, (int Quantity, ItemQuality Quality)> _equipmentItemStates = [];
    private readonly Dictionary<long, QualityRerollItemUpdate> _upcomingQualityRerollItemUpdates = [];
    private readonly Dictionary<ItemQuality, int> _upcomingQualityRerollSourceItemCounts = [];
    private long _upcomingQualityRerollCosts;
    private int _upcomingQualityRerollQuantity;

    public void SetUpcomingQualityReroll(
        IReadOnlyList<long> itemObjectIds,
        IReadOnlyList<int> itemQuantities,
        IReadOnlyList<ItemQuality> itemQualities,
        long costs)
    {
        if (itemObjectIds == null
            || itemObjectIds.Count == 0
            || costs <= 0)
        {
            return;
        }

        lock (_qualityRerollSyncRoot)
        {
            _upcomingQualityRerollItemObjectIds.Clear();
            foreach (var itemObjectId in itemObjectIds)
            {
                _upcomingQualityRerollItemObjectIds.Add(itemObjectId);
            }

            _upcomingQualityRerollSourceItemCounts.Clear();
            long totalQuantity = 0;
            var itemCount = Math.Min(itemObjectIds.Count, itemQuantities.Count);
            for (var itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                var quantity = itemQuantities[itemIndex];
                if (quantity <= 0)
                {
                    continue;
                }

                totalQuantity += quantity;
                var itemQuality = itemQualities.Count == 1
                    ? itemQualities[0]
                    : itemIndex < itemQualities.Count
                        ? itemQualities[itemIndex]
                        : ItemQuality.Unknown;
                if (itemQuality is >= ItemQuality.Normal and < ItemQuality.Masterpiece)
                {
                    var currentQuantity = _upcomingQualityRerollSourceItemCounts.GetValueOrDefault(itemQuality);
                    _upcomingQualityRerollSourceItemCounts[itemQuality] = (int) Math.Min(
                        (long) currentQuantity + quantity,
                        int.MaxValue);
                }
            }

            _upcomingQualityRerollCosts = costs;
            _upcomingQualityRerollQuantity = (int) Math.Min(totalQuantity, int.MaxValue);
            _upcomingQualityRerollItemUpdates.Clear();
        }
    }

    public void TrackEquipmentItem(DiscoveredItem item)
    {
        if (item == null
            || item.ObjectId <= 0
            || item.Quality == ItemQuality.Unknown)
        {
            return;
        }

        lock (_qualityRerollSyncRoot)
        {
            var hasPreviousState = _equipmentItemStates.TryGetValue(item.ObjectId, out var previousState);
            if (_upcomingQualityRerollCosts > 0)
            {
                if (!_upcomingQualityRerollItemUpdates.TryGetValue(item.ObjectId, out var itemUpdate))
                {
                    itemUpdate = new QualityRerollItemUpdate(hasPreviousState);
                    _upcomingQualityRerollItemUpdates[item.ObjectId] = itemUpdate;
                }

                itemUpdate.AddObservation(
                    item,
                    hasPreviousState ? previousState : null);
            }

            _equipmentItemStates[item.ObjectId] = (item.Quantity, item.Quality);
        }
    }

    public void RemoveEquipmentItem(long itemObjectId)
    {
        lock (_qualityRerollSyncRoot)
        {
            _equipmentItemStates.Remove(itemObjectId);
        }
    }

    public void QualityRerollFinished(
        IReadOnlyCollection<long> resultItemObjectIds,
        IReadOnlyCollection<long> sourceItemObjectIds)
    {
        long costs;
        IReadOnlyDictionary<ItemQuality, int> improvedItemCounts;
        IReadOnlyDictionary<ItemQuality, int> sourceItemCounts;

        lock (_qualityRerollSyncRoot)
        {
            if (_upcomingQualityRerollCosts <= 0
                || resultItemObjectIds == null
                || sourceItemObjectIds == null
                || (!resultItemObjectIds.Any(_upcomingQualityRerollItemObjectIds.Contains)
                    && !sourceItemObjectIds.Any(_upcomingQualityRerollItemObjectIds.Contains)))
            {
                return;
            }

            costs = _upcomingQualityRerollCosts;
            improvedItemCounts = GetImprovedQualityRerollItemCounts(resultItemObjectIds);
            sourceItemCounts = new Dictionary<ItemQuality, int>(_upcomingQualityRerollSourceItemCounts);
            ResetUpcomingQualityReroll();
        }

        StatisticController.AddItemQualityReroll(
            FixPoint.FromInternalValue(costs).DoubleValue,
            improvedItemCounts,
            sourceItemCounts);
    }

    private IReadOnlyDictionary<ItemQuality, int> GetImprovedQualityRerollItemCounts(
        IReadOnlyCollection<long> resultItemObjectIds)
    {
        var result = new Dictionary<ItemQuality, int>();
        var remainingQuantity = _upcomingQualityRerollQuantity > 0
            ? _upcomingQualityRerollQuantity
            : int.MaxValue;

        foreach (var itemObjectId in resultItemObjectIds.Distinct())
        {
            if (remainingQuantity <= 0
                || !_upcomingQualityRerollItemUpdates.TryGetValue(itemObjectId, out var itemUpdate)
                || itemUpdate.LatestQuality <= ItemQuality.Normal)
            {
                continue;
            }

            var alreadyCounted = result.GetValueOrDefault(itemUpdate.LatestQuality);
            var eligibleQuantity = GetEligibleQualityRerollQuantity(itemUpdate.LatestQuality);
            var improvedQuantity = Math.Min(
                itemUpdate.GetImprovedQuantity(),
                Math.Min(remainingQuantity, eligibleQuantity - alreadyCounted));
            if (improvedQuantity <= 0)
            {
                continue;
            }

            result[itemUpdate.LatestQuality] = alreadyCounted + improvedQuantity;
            remainingQuantity -= improvedQuantity;
        }

        return result;
    }

    private int GetEligibleQualityRerollQuantity(ItemQuality resultQuality)
    {
        if (_upcomingQualityRerollSourceItemCounts.Count == 0)
        {
            return _upcomingQualityRerollQuantity > 0
                ? _upcomingQualityRerollQuantity
                : int.MaxValue;
        }

        long eligibleQuantity = 0;
        foreach (var sourceItemCount in _upcomingQualityRerollSourceItemCounts)
        {
            if (sourceItemCount.Key >= ItemQuality.Normal
                && sourceItemCount.Key < resultQuality)
            {
                eligibleQuantity += sourceItemCount.Value;
            }
        }

        return (int) Math.Min(eligibleQuantity, int.MaxValue);
    }

    private void ResetUpcomingQualityReroll()
    {
        _upcomingQualityRerollItemObjectIds.Clear();
        _upcomingQualityRerollItemUpdates.Clear();
        _upcomingQualityRerollSourceItemCounts.Clear();
        _upcomingQualityRerollCosts = 0;
        _upcomingQualityRerollQuantity = 0;
    }

    private sealed class QualityRerollItemUpdate
    {
        private readonly bool _hadKnownState;
        private int _observationCount;
        private int _addedQuantity;
        private int _latestQuantity;

        public QualityRerollItemUpdate(bool hadKnownState)
        {
            _hadKnownState = hadKnownState;
        }

        public ItemQuality LatestQuality { get; private set; } = ItemQuality.Unknown;

        public void AddObservation(
            DiscoveredItem item,
            (int Quantity, ItemQuality Quality)? previousState)
        {
            _observationCount++;
            _latestQuantity = item.Quantity;
            LatestQuality = item.Quality;

            if (!previousState.HasValue)
            {
                return;
            }

            var quantityAdded = item.Quality switch
            {
                _ when item.Quality > previousState.Value.Quality => item.Quantity,
                _ when item.Quality == previousState.Value.Quality
                       && item.Quantity > previousState.Value.Quantity => item.Quantity - previousState.Value.Quantity,
                _ => 0
            };
            _addedQuantity = (int) Math.Min((long) _addedQuantity + quantityAdded, int.MaxValue);
        }

        public int GetImprovedQuantity()
        {
            if (_addedQuantity > 0)
            {
                return _addedQuantity;
            }

            return !_hadKnownState && _observationCount == 1
                ? Math.Max(0, _latestQuantity)
                : 0;
        }
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
    }

    #endregion
}
