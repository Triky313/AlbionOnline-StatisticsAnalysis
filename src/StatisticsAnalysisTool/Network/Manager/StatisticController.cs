using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Combat;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
using static StatisticsAnalysisTool.Models.DashboardLootStatisticsCalculator;

namespace StatisticsAnalysisTool.Network.Manager;

public class StatisticController
{
    private static readonly TimeSpan DashboardChartRefreshDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan CombatLootAssociationWindow = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan PendingPlayerKillLifetime = TimeSpan.FromSeconds(30);
    private const int DashboardContentRankingLimit = 8;

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly object _syncRoot = new();
    private readonly StatisticSessionStorage _sessionStorage = new();
    private readonly List<PendingPlayerKill> _pendingPlayerKills = [];
    private readonly Dictionary<Guid, long> _dirtySessionVersions = [];
    private readonly SemaphoreSlim _sessionPersistenceSemaphore = new(1, 1);
    private readonly Dispatcher _uiDispatcher;
    private readonly DispatcherTimer _dashboardChartRefreshTimer;
    private readonly Dictionary<ValueType, LineSeries<ObservablePoint>> _dashboardChartSeries = [];
    private readonly Dictionary<ValueType, ObservableCollection<ObservablePoint>> _dashboardChartPoints = [];

    private int _isDashboardChartRefreshSchedulingPending;
    private DashboardUpdateScope _pendingDashboardUpdateScopes = DashboardUpdateScope.All;
    private DashboardStatistics _dashboardStatistics = new();
    private DashboardStatisticsAggregator _statisticsAggregator = new(new DashboardStatistics());

    public StatisticController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        _uiDispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        _dashboardChartRefreshTimer = new DispatcherTimer(
            DashboardChartRefreshDelay,
            DispatcherPriority.Background,
            OnDashboardChartRefreshTimerTick,
            _uiDispatcher);
        _dashboardChartRefreshTimer.Stop();
    }

    #region Dashboard

    public bool HasActiveSession
    {
        get
        {
            lock (_syncRoot)
            {
                return _dashboardStatistics.GetActiveSession() != null;
            }
        }
    }

    public void AddValue(ValueType valueType, double gainedValue, CityFaction cityFaction = CityFaction.Unknown)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var updateScope = ResolveDashboardUpdateScope(valueType);
        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Statistics value discarded because no active session exists. ValueType={ValueType}",
                    valueType);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = valueType,
                Value = gainedValue,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = cityFaction
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkDashboardDirtyInternal(updateScope);
            MarkSessionDirtyInternal(session.Id);
        }

        if (updateScope != DashboardUpdateScope.None)
        {
            UpdateDailyChart();
        }

        if (valueType == ValueType.RepairCosts)
        {
            UpdateRepairCostsUi();
        }
    }

    public void AddItemQualityReroll(
        double costs,
        IReadOnlyDictionary<ItemQuality, int> improvedItemCounts,
        IReadOnlyDictionary<ItemQuality, int> sourceItemCounts)
    {
        if (!double.IsFinite(costs)
            || costs < 0
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var resultItemCounts = improvedItemCounts
            .Where(itemCount => itemCount.Key != ItemQuality.Unknown && itemCount.Value > 0)
            .ToArray();
        var attemptedItemCounts = sourceItemCounts
            .Where(itemCount => itemCount.Key is >= ItemQuality.Normal and < ItemQuality.Masterpiece
                                && itemCount.Value > 0)
            .ToArray();
        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var clusterMode = ClusterController.CurrentCluster.ClusterMode;

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Statistics value discarded because no active session exists. ValueType={ValueType}",
                    ValueType.ItemQualityRerollCosts);
                return;
            }

            var entries = new List<StatisticEntry>(resultItemCounts.Length + attemptedItemCounts.Length + 1)
            {
                new()
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.ItemQualityRerollCosts,
                    Value = costs,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode,
                    ItemQuality = ItemQuality.Unknown
                }
            };

            foreach (var itemCount in resultItemCounts)
            {
                entries.Add(new StatisticEntry
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.ItemQualityRerollResult,
                    Value = 0,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode,
                    ItemQuality = itemCount.Key,
                    ItemQuantity = itemCount.Value
                });
            }

            foreach (var itemCount in attemptedItemCounts)
            {
                entries.Add(new StatisticEntry
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.ItemQualityRerollAttempt,
                    Value = 0,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode,
                    ItemQuality = itemCount.Key,
                    ItemQuantity = itemCount.Value
                });
            }

            foreach (var entry in entries)
            {
                _dashboardStatistics.Add(entry);
                _statisticsAggregator.Add(entry);
            }

            MarkDashboardDirtyInternal(DashboardUpdateScope.Economy);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void AddAwakenedWeaponAction(double costs, bool traitUpgraded, bool traitUpgradeProcced)
    {
        if (!double.IsFinite(costs)
            || costs <= 0
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var clusterMode = ClusterController.CurrentCluster.ClusterMode;

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Statistics value discarded because no active session exists. ValueType={ValueType}",
                    ValueType.AwakenedWeaponCosts);
                return;
            }

            var entries = new List<StatisticEntry>
            {
                new()
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.AwakenedWeaponCosts,
                    Value = costs,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode
                }
            };

            if (traitUpgraded)
            {
                entries.Add(new StatisticEntry
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.AwakenedWeaponTraitUpgrade,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode,
                    ItemQuantity = 1
                });
            }

            if (traitUpgradeProcced)
            {
                entries.Add(new StatisticEntry
                {
                    SessionId = session.Id,
                    OccurredAtUtc = nowUtc,
                    ValueType = ValueType.AwakenedWeaponTraitUpgradeProc,
                    MapType = mapType,
                    DungeonMode = dungeonMode,
                    ClusterMode = clusterMode,
                    ItemQuantity = 1
                });
            }

            foreach (var entry in entries)
            {
                _dashboardStatistics.Add(entry);
                _statisticsAggregator.Add(entry);
            }

            MarkDashboardDirtyInternal(DashboardUpdateScope.Economy);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void AddLootValue(int itemIndex, int quantity, double unitValue)
    {
        if (itemIndex <= 0
            || quantity <= 0
            || !double.IsFinite(unitValue))
        {
            return;
        }

        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var lootAreaIndex = ResolveLootAreaIndex(mapType, dungeonMode);
        var lootAreaClusterType = string.IsNullOrWhiteSpace(lootAreaIndex)
            ? ClusterType.Unknown
            : WorldData.GetClusterTypeByIndex(lootAreaIndex);
        var totalValue = Math.Max(0, unitValue) * quantity;

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug("Statistics value discarded because no active session exists. ValueType={ValueType}", ValueType.LootValue);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = ValueType.LootValue,
                Value = totalValue,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = CityFaction.Unknown,
                ItemIndex = itemIndex,
                ItemQuantity = quantity,
                LootAreaIndex = lootAreaIndex,
                LootAreaClusterType = lootAreaClusterType,
                LootAreaEnteredAtUtc = ClusterController.CurrentCluster.Entered
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkDashboardDirtyInternal(DashboardUpdateScope.Loot);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void TrackPlayerKillCandidate(
        string killedPlayerName,
        long killedPlayerObjectId)
    {
        if (string.IsNullOrWhiteSpace(killedPlayerName)
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var entityController = _trackingController.EntityController;
        var nowUtc = DateTime.UtcNow;
        var pendingPlayerKill = new PendingPlayerKill(
            killedPlayerObjectId,
            killedPlayerName,
            CreateCombatPlayerSnapshot(
                entityController.LocalUserData.Username,
                entityController.GetLastLocalCharacterEquipment()),
            CreateCombatPlayerSnapshot(
                killedPlayerName,
                entityController.GetLastKnownCharacterEquipment(killedPlayerObjectId)),
            nowUtc);

        lock (_syncRoot)
        {
            RemoveExpiredPendingPlayerKillsInternal(nowUtc);
            _pendingPlayerKills.RemoveAll(candidate => candidate.Matches(killedPlayerObjectId, killedPlayerName));
            _pendingPlayerKills.Add(pendingPlayerKill);
        }
    }

    public void ResolvePlayerCombatResult(
        long diedPlayerObjectId,
        string diedPlayerName,
        long killerObjectId,
        string killerPlayerName,
        bool isLethal)
    {
        var entityController = _trackingController.EntityController;
        var localPlayerName = entityController.LocalUserData.Username;
        var pendingPlayerKill = TakePendingPlayerKill(diedPlayerObjectId, diedPlayerName, DateTime.UtcNow);
        var combatResult = PlayerCombatResultResolver.Resolve(
            diedPlayerObjectId,
            diedPlayerName,
            killerObjectId,
            killerPlayerName,
            isLethal,
            entityController.LocalUserData.UserObjectId,
            localPlayerName);

        if (combatResult == PlayerCombatResult.None)
        {
            return;
        }

        var valueType = combatResult switch
        {
            PlayerCombatResult.Kill => ValueType.PlayerKill,
            PlayerCombatResult.Death => ValueType.PlayerDeath,
            PlayerCombatResult.Knockout => ValueType.PlayerKnockout,
            PlayerCombatResult.KnockedOut => ValueType.PlayerKnockedOut,
            _ => throw new ArgumentOutOfRangeException(nameof(combatResult), combatResult, null)
        };

        if (combatResult is PlayerCombatResult.Kill or PlayerCombatResult.Knockout)
        {
            AddPlayerCombatEvent(
                valueType,
                diedPlayerName,
                pendingPlayerKill?.Killer ?? CreateCombatPlayerSnapshot(
                    localPlayerName,
                    entityController.GetLastLocalCharacterEquipment()),
                pendingPlayerKill?.Victim ?? CreateCombatPlayerSnapshot(
                    diedPlayerName,
                    entityController.GetLastKnownCharacterEquipment(diedPlayerObjectId)));
            return;
        }

        AddPlayerCombatEvent(
            valueType,
            killerPlayerName,
            CreateCombatPlayerSnapshot(
                killerPlayerName,
                killerObjectId > 0
                    ? entityController.GetLastKnownCharacterEquipment(killerObjectId)
                    : entityController.GetLastKnownCharacterEquipment(killerPlayerName)),
            CreateCombatPlayerSnapshot(
                localPlayerName,
                entityController.GetLastLocalCharacterEquipment()));
    }

    private PendingPlayerKill TakePendingPlayerKill(
        long killedPlayerObjectId,
        string killedPlayerName,
        DateTime nowUtc)
    {
        lock (_syncRoot)
        {
            RemoveExpiredPendingPlayerKillsInternal(nowUtc);
            var candidateIndex = _pendingPlayerKills.FindLastIndex(
                candidate => candidate.Matches(killedPlayerObjectId, killedPlayerName));
            if (candidateIndex < 0)
            {
                return null;
            }

            var candidate = _pendingPlayerKills[candidateIndex];
            _pendingPlayerKills.RemoveAt(candidateIndex);
            return candidate;
        }
    }

    private void RemoveExpiredPendingPlayerKillsInternal(DateTime nowUtc)
    {
        _pendingPlayerKills.RemoveAll(
            candidate => nowUtc - candidate.OccurredAtUtc > PendingPlayerKillLifetime);
    }

    public void AddMobKill(string mobUniqueName)
    {
        if (string.IsNullOrWhiteSpace(mobUniqueName)
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Mob kill discarded because no active session exists. MobUniqueName={MobUniqueName}",
                    mobUniqueName);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = ValueType.MobKill,
                Value = 1,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                MobUniqueName = mobUniqueName
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkDashboardDirtyInternal(DashboardUpdateScope.Mobs);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    private void AddPlayerCombatEvent(
        ValueType valueType,
        string opponentName,
        CombatPlayerSnapshot killer,
        CombatPlayerSnapshot victim)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var combatAreaIndex = ResolveLootAreaIndex(mapType, dungeonMode);
        var combatAreaClusterType = mapType == MapType.CorruptedDungeon
            ? ClusterType.Corrupted
            : string.IsNullOrWhiteSpace(combatAreaIndex)
                ? ClusterType.Unknown
                : WorldData.GetClusterTypeByIndex(combatAreaIndex);
        var nowUtc = DateTime.UtcNow;

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Combat event discarded because no active session exists. ValueType={ValueType}",
                    valueType);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = valueType,
                Value = 1,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CombatAreaIndex = combatAreaIndex,
                CombatAreaClusterType = combatAreaClusterType,
                CombatOpponentName = opponentName ?? string.Empty,
                CombatKiller = killer,
                CombatVictim = victim
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkDashboardDirtyInternal(DashboardUpdateScope.Combat);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    private static CombatPlayerSnapshot CreateCombatPlayerSnapshot(
        string playerName,
        CharacterEquipment equipment)
    {
        var itemIndexes = equipment?.GetEquippedItemIndexes().ToList() ?? [];
        var estimatedEquipmentValue = itemIndexes
            .Select(itemIndex => ItemController.GetItemByIndex(itemIndex))
            .Where(item => item != null)
            .Sum(item => Math.Max(item.AverageEstMarketValue, 0L));

        return new CombatPlayerSnapshot
        {
            Name = playerName ?? string.Empty,
            EquipmentItemIndexes = itemIndexes,
            EstimatedEquipmentValue = estimatedEquipmentValue
        };
    }

    public void AddCombatLootValue(string lootedFromName, double value)
    {
        if (string.IsNullOrWhiteSpace(lootedFromName)
            || !double.IsFinite(value)
            || value <= 0
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var wasUpdated = false;
        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                return;
            }

            wasUpdated = _dashboardStatistics.TryAddCombatLootValue(
                session.Id,
                lootedFromName,
                value,
                DateTime.UtcNow.Subtract(CombatLootAssociationWindow));
            if (wasUpdated)
            {
                MarkDashboardDirtyInternal(DashboardUpdateScope.Combat);
                MarkSessionDirtyInternal(session.Id);
            }
        }

        if (wasUpdated)
        {
            UpdateDailyChart();
        }
    }

    public void AddLootedChest(TreasureRarity treasureRarity)
    {
        if (treasureRarity == TreasureRarity.Unknown
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var lootAreaIndex = ResolveLootAreaIndex(mapType, dungeonMode);
        var lootAreaClusterType = string.IsNullOrWhiteSpace(lootAreaIndex)
            ? ClusterType.Unknown
            : WorldData.GetClusterTypeByIndex(lootAreaIndex);

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug("Statistics value discarded because no active session exists. ValueType={ValueType}", ValueType.LootedChest);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = ValueType.LootedChest,
                Value = 1,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = CityFaction.Unknown,
                LootAreaIndex = lootAreaIndex,
                LootAreaClusterType = lootAreaClusterType,
                TreasureRarity = treasureRarity
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkDashboardDirtyInternal(DashboardUpdateScope.LootedChests);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void StartSession(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName) || !AppDataPaths.IsUserDataAvailable)
        {
            Log.Warning("Statistics session was not started because login metadata is incomplete. Character={Character}, Server={Server}", characterName, AppDataPaths.ActiveUserDataServerLocation);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var isNewSession = false;

        lock (_syncRoot)
        {
            var previousSessionId = _dashboardStatistics.GetActiveSession()?.Id;
            var session = _dashboardStatistics.StartSession(
                characterName,
                AppDataPaths.ActiveUserDataServerLocation,
                nowUtc);
            isNewSession = previousSessionId != session.Id;

            if (isNewSession)
            {
                MarkSessionDirtyInternal(session.Id);
            }
        }

        if (isNewSession)
        {
            _trackingController.LiveStatsTracker?.Reset();
            _trackingController.LiveStatsTracker?.Start();
            Log.Information("Statistics session started. Character={Character}, Server={Server}", characterName, AppDataPaths.ActiveUserDataServerLocation);
        }

        RefreshDashboardSessionFilters();
    }

    public bool EndSession(DateTime endedAtUtc)
    {
        bool wasEnded;
        lock (_syncRoot)
        {
            var activeSessionId = _dashboardStatistics.GetActiveSession()?.Id;
            wasEnded = _dashboardStatistics.EndActiveSession(endedAtUtc);

            if (wasEnded && activeSessionId.HasValue)
            {
                MarkSessionDirtyInternal(activeSessionId.Value);
            }
        }

        if (!wasEnded)
        {
            return false;
        }

        _trackingController.LiveStatsTracker?.Stop();
        RefreshDashboardSessionFilters();
        UpdateDailyChart(true);
        Log.Information("Statistics session ended");
        return true;
    }

    public async System.Threading.Tasks.Task<bool> ResetSessionAsync()
    {
        Guid sessionId;
        string characterName;
        lock (_syncRoot)
        {
            var activeSession = _dashboardStatistics.GetActiveSession();
            if (activeSession == null)
            {
                return false;
            }

            sessionId = activeSession.Id;
            characterName = activeSession.CharacterName;
        }

        if (!await RemoveSessionAsync(sessionId, true))
        {
            return false;
        }

        _trackingController.LiveStatsTracker?.Stop();
        StartSession(characterName);
        UpdateRepairCostsUi();
        UpdateDailyChart(true);
        Log.Information("Statistics session reset");
        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            return false;
        }

        if (!await RemoveSessionAsync(sessionId, false))
        {
            return false;
        }

        RefreshDashboardSessionFilters();
        UpdateRepairCostsUi();
        UpdateDailyChart(true);
        Log.Information("Statistics session deleted. SessionId={SessionId}", sessionId);
        return true;
    }

    private async System.Threading.Tasks.Task<bool> RemoveSessionAsync(
        Guid sessionId,
        bool canRemoveActiveSession)
    {
        await _sessionPersistenceSemaphore.WaitAsync();

        try
        {
            lock (_syncRoot)
            {
                var sessionExists = _dashboardStatistics
                    .CreateSessionSnapshot()
                    .Any(x => x.Id == sessionId);
                var isActiveSession = _dashboardStatistics.GetActiveSession()?.Id == sessionId;
                if (!sessionExists || (isActiveSession && !canRemoveActiveSession))
                {
                    return false;
                }
            }

            if (!_sessionStorage.DeleteSession(sessionId))
            {
                return false;
            }

            lock (_syncRoot)
            {
                if (!_dashboardStatistics.RemoveSession(sessionId))
                {
                    return false;
                }

                _dirtySessionVersions.Remove(sessionId);
                _statisticsAggregator = new DashboardStatisticsAggregator(_dashboardStatistics);
            }

            return true;
        }
        finally
        {
            _sessionPersistenceSemaphore.Release();
        }
    }

    public void UpdateDailyChart(bool forceUpdate = false)
    {
        if (!forceUpdate)
        {
            ScheduleDashboardChartRefresh();
            return;
        }

        MarkDashboardDirty(DashboardUpdateScope.All);
        RefreshDashboard();
    }

    public void UpdateDashboardChartSeries()
    {
        MarkDashboardDirty(DashboardUpdateScope.Chart);
        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        if (!_uiDispatcher.CheckAccess())
        {
            _ = _uiDispatcher.InvokeAsync(
                RefreshDashboard,
                DispatcherPriority.Background);
            return;
        }

        _dashboardChartRefreshTimer.Stop();

        var selectedRange = _mainWindowViewModel.SelectedDashboardChartRange;
        if (selectedRange == null)
        {
            return;
        }

        var updateScopes = TakeDashboardUpdateScopes();
        if (updateScopes == DashboardUpdateScope.None)
        {
            return;
        }

        var chartBuckets = CreateChartBuckets(selectedRange);
        var currentRangeBucketStarts = chartBuckets.Select(x => x.Start).ToArray();
        var previousRangeBucketStarts = currentRangeBucketStarts
            .Select(x => AddBuckets(x, -selectedRange.BucketCount, selectedRange.Unit))
            .ToArray();

        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues = new Dictionary<ValueType, Dictionary<DateTime, double>>();
        var aggregationScopes = DashboardUpdateScope.Chart
                                | DashboardUpdateScope.Summary
                                | DashboardUpdateScope.Loot;
        if ((updateScopes & aggregationScopes) != 0)
        {
            var aggregationBucketStarts = currentRangeBucketStarts
                .Concat(previousRangeBucketStarts)
                .Distinct()
                .ToArray();
            aggregatedValues = _statisticsAggregator.AggregateChartValues(
                aggregationBucketStarts,
                selectedRange.Unit,
                _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId,
                _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType);
        }

        if ((updateScopes & DashboardUpdateScope.Summary) != 0)
        {
            UpdateDashboardSummary(selectedRange, chartBuckets, aggregatedValues);
        }

        if ((updateScopes & DashboardUpdateScope.Combat) != 0)
        {
            UpdateDashboardCombatStatistics(selectedRange);
        }

        if ((updateScopes & DashboardUpdateScope.Mobs) != 0)
        {
            UpdateDashboardMobStatistics(
                selectedRange,
                currentRangeBucketStarts,
                previousRangeBucketStarts);
        }

        if ((updateScopes & DashboardUpdateScope.Loot) != 0)
        {
            UpdateDashboardLootStatistics(
                selectedRange,
                currentRangeBucketStarts,
                previousRangeBucketStarts,
                aggregatedValues);
        }

        if ((updateScopes & DashboardUpdateScope.LootedChests) != 0)
        {
            UpdateDashboardLootedChestStatistics(
                selectedRange,
                currentRangeBucketStarts,
                previousRangeBucketStarts);
        }

        if ((updateScopes & DashboardUpdateScope.ContentRankings) != 0)
        {
            UpdateDashboardContentRankings(selectedRange, currentRangeBucketStarts);
        }

        if ((updateScopes & DashboardUpdateScope.Economy) != 0)
        {
            UpdateDashboardEconomyStatistics(
                selectedRange,
                currentRangeBucketStarts,
                previousRangeBucketStarts,
                chartBuckets[0].Start,
                DateTime.UtcNow);
        }

        if ((updateScopes & DashboardUpdateScope.Chart) != 0)
        {
            UpdateDashboardChart(chartBuckets, aggregatedValues);
        }
    }

    private void UpdateDashboardChart(
        IReadOnlyList<ChartBucket> chartBuckets,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues)
    {
        var labels = chartBuckets.Select(x => x.Label).ToArray();
        var xAxes = _mainWindowViewModel.XAxesDashboardHourValues;
        if (xAxes is not { Length: 1 })
        {
            xAxes =
            [
                new Axis
                {
                    LabelsRotation = 15,
                    Labels = labels
                }
            ];
            _mainWindowViewModel.XAxesDashboardHourValues = xAxes;
        }
        else
        {
            xAxes[0].LabelsRotation = 15;
            xAxes[0].Labels = labels;
        }

        var selectedSeriesFilters = (_mainWindowViewModel.DashboardChartSeriesFilters ?? [])
            .Where(filter => filter.IsSelected)
            .ToArray();
        var visibleSeries = new List<ISeries>(selectedSeriesFilters.Length);

        foreach (var selectedSeriesFilter in selectedSeriesFilters)
        {
            if (!_dashboardChartSeries.TryGetValue(selectedSeriesFilter.ValueType, out var series))
            {
                var points = new ObservableCollection<ObservablePoint>();
                series = new LineSeries<ObservablePoint>
                {
                    Values = points,
                    Fill = GetValueTypeBrush(selectedSeriesFilter.ValueType, true),
                    Stroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                    GeometryStroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                    GeometryFill = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                    GeometrySize = 5,
                    YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
                };
                _dashboardChartSeries[selectedSeriesFilter.ValueType] = series;
                _dashboardChartPoints[selectedSeriesFilter.ValueType] = points;
            }

            series.Name = selectedSeriesFilter.Name;
            var seriesPoints = _dashboardChartPoints[selectedSeriesFilter.ValueType];
            while (seriesPoints.Count > chartBuckets.Count)
            {
                seriesPoints.RemoveAt(seriesPoints.Count - 1);
            }

            while (seriesPoints.Count < chartBuckets.Count)
            {
                seriesPoints.Add(new ObservablePoint());
            }

            var valuesLookup = aggregatedValues.GetValueOrDefault(selectedSeriesFilter.ValueType) ?? [];
            for (var i = 0; i < chartBuckets.Count; i++)
            {
                var value = valuesLookup.GetValueOrDefault(chartBuckets[i].Start);
                var point = seriesPoints[i];
                if (point.X != i)
                {
                    point.X = i;
                }

                if (point.Y != value)
                {
                    point.Y = value;
                }
            }

            visibleSeries.Add(series);
        }

        var targetSeries = _mainWindowViewModel.SeriesDashboardHourValues;
        if (targetSeries == null)
        {
            targetSeries = [];
            _mainWindowViewModel.SeriesDashboardHourValues = targetSeries;
        }

        ReplaceDashboardItems(
            targetSeries,
            visibleSeries);
    }

    private void ScheduleDashboardChartRefresh()
    {
        if (_uiDispatcher.CheckAccess())
        {
            StartDashboardChartRefreshTimer();
            return;
        }

        if (Interlocked.Exchange(ref _isDashboardChartRefreshSchedulingPending, 1) == 1)
        {
            return;
        }

        _ = _uiDispatcher.InvokeAsync(() =>
        {
            Interlocked.Exchange(ref _isDashboardChartRefreshSchedulingPending, 0);
            StartDashboardChartRefreshTimer();
        }, DispatcherPriority.Background);
    }

    private void StartDashboardChartRefreshTimer()
    {
        if (!_dashboardChartRefreshTimer.IsEnabled)
        {
            _dashboardChartRefreshTimer.Start();
        }
    }

    private void OnDashboardChartRefreshTimerTick(object sender, EventArgs e)
    {
        _dashboardChartRefreshTimer.Stop();
        RefreshDashboard();
    }

    public void UpdateDashboardSessionTime(DateTime nowUtc)
    {
        var selectedRange = _mainWindowViewModel.SelectedDashboardChartRange;
        if (selectedRange == null)
        {
            return;
        }

        var currentPeriodStart = AlignToBucketStart(nowUtc.ToLocalTime(), selectedRange.Unit);
        var rangeStart = AddBuckets(currentPeriodStart, -(selectedRange.BucketCount - 1), selectedRange.Unit);

        UpdateDashboardSessionTime(selectedRange, rangeStart, nowUtc);
    }

    private void UpdateDashboardSummary(
        DashboardChartRangeOption selectedRange,
        IReadOnlyList<ChartBucket> chartBuckets,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues)
    {
        var currentRangeBucketStarts = chartBuckets.Select(x => x.Start).ToHashSet();
        var previousRangeBucketStarts = currentRangeBucketStarts
            .Select(x => AddBuckets(x, -selectedRange.BucketCount, selectedRange.Unit))
            .ToHashSet();

        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FameSummary,
            aggregatedValues,
            ValueType.Fame,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.ReSpecSummary,
            aggregatedValues,
            ValueType.ReSpec,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.SilverSummary,
            aggregatedValues,
            ValueType.Silver,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.MightSummary,
            aggregatedValues,
            ValueType.Might,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FavorSummary,
            aggregatedValues,
            ValueType.Favor,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardFactionSummary(
            selectedRange,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryValuesPerHour(GetRangeHours(selectedRange));

        _mainWindowViewModel.DashboardBindings.SummaryComparisonText = selectedRange.Unit switch
        {
            DashboardChartRangeUnit.Minute => DashboardBindings.TranslationVsPreviousMinutes,
            DashboardChartRangeUnit.Hour when selectedRange.BucketCount == 1 => DashboardBindings.TranslationVsPreviousHour,
            DashboardChartRangeUnit.Hour => DashboardBindings.TranslationVsPreviousHours,
            DashboardChartRangeUnit.Day when selectedRange.BucketCount == 1 => DashboardBindings.TranslationVsPreviousDay,
            DashboardChartRangeUnit.Day => DashboardBindings.TranslationVsPreviousDays,
            _ => DashboardBindings.TranslationVsPreviousDay
        };

        UpdateDashboardSessionTime(selectedRange, chartBuckets[0].Start, DateTime.UtcNow);
    }

    private void UpdateDashboardFactionSummary(
        DashboardChartRangeOption selectedRange,
        IReadOnlySet<DateTime> currentRangeBucketStarts,
        IReadOnlySet<DateTime> previousRangeBucketStarts)
    {
        var factionValues = _statisticsAggregator.AggregateChartValues(
            currentRangeBucketStarts.Concat(previousRangeBucketStarts).ToArray(),
            selectedRange.Unit,
            _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId,
            _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType,
            _mainWindowViewModel.DashboardBindings.SelectedFactionOption.Faction);

        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FactionPointsSummary,
            factionValues,
            ValueType.FactionPoints,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FactionStandingSummary,
            factionValues,
            ValueType.FactionStanding,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
    }

    private void UpdateDashboardEconomyStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts,
        DateTime currentRangeStart,
        DateTime nowUtc)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var currentValues = _statisticsAggregator.AggregateEconomyValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId);
        var previousValues = _statisticsAggregator.AggregateEconomyValues(
            previousRangeBucketStarts,
            selectedRange.Unit,
            sessionId);
        var sessionCount = CountFilteredSessions(currentRangeStart, nowUtc, sessionId);
        var bindings = _mainWindowViewModel.DashboardBindings;

        bindings.EconomyReSpecSummary.Update(
            currentValues.ReSpec,
            currentValues.ReSpec,
            previousValues.ReSpec);
        bindings.RepairCostsSummary.Update(
            currentValues.RepairCosts,
            currentValues.RepairCosts,
            previousValues.RepairCosts);
        bindings.ItemQualityRerollCostsSummary.Update(
            currentValues.ItemQualityRerollCosts,
            currentValues.ItemQualityRerollCosts,
            previousValues.ItemQualityRerollCosts);
        bindings.AwakenedWeaponCostsSummary.Update(
            currentValues.AwakenedWeaponCosts,
            currentValues.AwakenedWeaponCosts,
            previousValues.AwakenedWeaponCosts);
        bindings.AwakenedWeaponTraitUpgradeCount = currentValues.AwakenedWeaponTraitUpgradeCount;
        bindings.AwakenedWeaponTraitUpgradeProcCount = currentValues.AwakenedWeaponTraitUpgradeProcCount;
        bindings.ReSpecSilverCost = currentValues.ReSpecSilverCost;
        bindings.AverageReSpecSilverCostPerSession = sessionCount > 0
            ? currentValues.ReSpecSilverCost / sessionCount
            : 0;
        bindings.SpentReSpec = currentValues.SpentReSpec;
        bindings.SpentReSpecVisibility = currentValues.SpentReSpec > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        bindings.ReSpecDetailColumnCount = currentValues.SpentReSpec > 0
            ? 3
            : 2;
        bindings.AverageRepairCostPerSession = sessionCount > 0
            ? currentValues.RepairCosts / sessionCount
            : 0;
        bindings.HighestRepairCost = currentValues.HighestRepairCost;
        bindings.GoodItemQualityRerollCount = currentValues.GoodItemCount;
        bindings.OutstandingItemQualityRerollCount = currentValues.OutstandingItemCount;
        bindings.ExcellentItemQualityRerollCount = currentValues.ExcellentItemCount;
        bindings.MasterpieceItemQualityRerollCount = currentValues.MasterpieceItemCount;
        bindings.TotalItemQualityRerollCount =
            (long) currentValues.GoodItemCount
            + currentValues.OutstandingItemCount
            + currentValues.ExcellentItemCount
            + currentValues.MasterpieceItemCount;
        bindings.GoodItemQualityRerollPercentage = CalculateItemQualityRerollPercentage(
            currentValues.GoodItemSuccessfulRerollCount,
            currentValues.GoodItemEligibleRerollCount);
        bindings.OutstandingItemQualityRerollPercentage = CalculateItemQualityRerollPercentage(
            currentValues.OutstandingItemSuccessfulRerollCount,
            currentValues.OutstandingItemEligibleRerollCount);
        bindings.ExcellentItemQualityRerollPercentage = CalculateItemQualityRerollPercentage(
            currentValues.ExcellentItemSuccessfulRerollCount,
            currentValues.ExcellentItemEligibleRerollCount);
        bindings.MasterpieceItemQualityRerollPercentage = CalculateItemQualityRerollPercentage(
            currentValues.MasterpieceItemSuccessfulRerollCount,
            currentValues.MasterpieceItemEligibleRerollCount);
    }

    private static double CalculateItemQualityRerollPercentage(int successfulItemCount, int eligibleItemCount)
    {
        return eligibleItemCount > 0
            ? successfulItemCount * 100d / eligibleItemCount
            : 0;
    }

    private void UpdateDashboardMobStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var currentEntries = _statisticsAggregator.GetMobKillEntries(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var previousEntries = _statisticsAggregator.GetMobKillEntries(
            previousRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);

        _mainWindowViewModel.DashboardBindings.Mobs.UpdateStatistics(
            currentEntries,
            previousEntries,
            GetRangeHours(selectedRange));
    }

    private void UpdateDashboardCombatStatistics(DashboardChartRangeOption selectedRange)
    {
        var rangeEndUtc = DateTime.UtcNow;
        var rangeStartUtc = AddBuckets(rangeEndUtc, -selectedRange.BucketCount, selectedRange.Unit);
        var entries = _statisticsAggregator.GetCombatEntries(
            rangeStartUtc,
            rangeEndUtc,
            _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId,
            _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType);
        var kills = entries.Where(entry => entry.ValueType == ValueType.PlayerKill).ToArray();
        var deaths = entries.Where(entry => entry.ValueType == ValueType.PlayerDeath).ToArray();
        var knockouts = entries.Where(entry => entry.ValueType == ValueType.PlayerKnockout).ToArray();
        var knockedOut = entries.Where(entry => entry.ValueType == ValueType.PlayerKnockedOut).ToArray();
        var combatStatistics = _mainWindowViewModel.DashboardBindings.CombatStatistics;
        var characterNamesBySession = _dashboardStatistics.CreateSessionSnapshot()
            .ToDictionary(session => session.Id, session => session.CharacterName ?? string.Empty);

        combatStatistics.KillCount = kills.LongLength;
        combatStatistics.DeathCount = deaths.LongLength;
        combatStatistics.KnockoutCount = knockouts.LongLength;
        combatStatistics.KnockedOutCount = knockedOut.LongLength;
        combatStatistics.KillDeathRatio = deaths.Length > 0
            ? (double) kills.Length / deaths.Length
            : kills.Length;
        combatStatistics.TotalKillLootValue = kills.Sum(entry => entry.CombatLootValue);
        combatStatistics.TotalDeathLootValue = deaths.Sum(ResolveCombatEstimatedValue);

        ReplaceDashboardItems(
            combatStatistics.TopKillLocations,
            CreateCombatLocations(kills, ValueType.PlayerKill));
        ReplaceDashboardItems(
            combatStatistics.TopDeathLocations,
            CreateCombatLocations(deaths, ValueType.PlayerDeath));
        combatStatistics.ReplaceRecentEvents(
            entries
                .OrderByDescending(entry => entry.OccurredAtUtc)
                .Select(entry =>
                {
                    var isPositiveResult = entry.ValueType is ValueType.PlayerKill or ValueType.PlayerKnockout;
                    var opponentName = string.IsNullOrWhiteSpace(entry.CombatOpponentName)
                        ? "\u2014"
                        : entry.CombatOpponentName;
                    var localPlayerName = characterNamesBySession.TryGetValue(entry.SessionId, out var characterName)
                        ? characterName
                        : _trackingController.EntityController.LocalUserData.Username ?? string.Empty;
                    var killer = CreateDashboardCombatPlayerItem(
                        entry.CombatKiller,
                        isPositiveResult ? localPlayerName : opponentName);
                    var victim = CreateDashboardCombatPlayerItem(
                        entry.CombatVictim,
                        isPositiveResult ? opponentName : localPlayerName,
                        entry.CombatVictim == null ? entry.CombatLootValue : 0);

                    return new DashboardCombatEventItem(
                        FormatRelativeTime(entry.OccurredAtUtc),
                        ResolveCombatResultName(entry.ValueType),
                        isPositiveResult,
                        ResolveCombatAreaName(entry),
                        opponentName,
                        killer,
                        victim);
                })
                .ToArray());
    }

    private static DashboardCombatPlayerItem CreateDashboardCombatPlayerItem(
        CombatPlayerSnapshot snapshot,
        string fallbackName,
        double fallbackEstimatedValue = 0)
    {
        var equipment = (snapshot?.EquipmentItemIndexes ?? [])
            .Select(itemIndex => ItemController.GetItemByIndex(itemIndex))
            .Where(item => item != null)
            .ToArray();
        var playerName = string.IsNullOrWhiteSpace(snapshot?.Name)
            ? fallbackName
            : snapshot.Name;
        var estimatedValue = snapshot == null
            ? fallbackEstimatedValue
            : snapshot.EstimatedEquipmentValue;

        return new DashboardCombatPlayerItem(
            string.IsNullOrWhiteSpace(playerName) ? "\u2014" : playerName,
            equipment,
            double.IsFinite(estimatedValue) ? Math.Max(estimatedValue, 0) : 0);
    }

    private static IReadOnlyCollection<DashboardCombatLocationItem> CreateCombatLocations(
        IReadOnlyCollection<StatisticEntry> entries,
        ValueType valueType)
    {
        var locations = entries
            .Where(entry => entry.ValueType == valueType)
            .GroupBy(entry => new
            {
                entry.MapType,
                entry.DungeonMode,
                AreaIndex = entry.MapType == MapType.Arena
                    ? string.Empty
                    : entry.CombatAreaIndex ?? string.Empty,
                entry.CombatAreaClusterType
            })
            .Select(group => new
            {
                FirstEntry = group.First(),
                Count = group.LongCount(),
                EstimatedLootValue = group.Sum(ResolveCombatEstimatedValue)
            })
            .OrderByDescending(location => location.Count)
            .ThenBy(location => ResolveCombatAreaName(location.FirstEntry), StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
        var totalCount = locations.Sum(location => location.Count);
        var maximumCount = locations.Select(location => location.Count).DefaultIfEmpty(0).Max();

        return locations
            .Select(location => new DashboardCombatLocationItem(
                ResolveCombatAreaName(location.FirstEntry),
                location.Count,
                totalCount > 0 ? location.Count * 100d / totalCount : 0,
                maximumCount > 0 ? location.Count * 100d / maximumCount : 0,
                location.EstimatedLootValue,
                location.FirstEntry.CombatAreaClusterType))
            .ToArray();
    }

    internal static double ResolveCombatEstimatedValue(StatisticEntry entry)
    {
        var estimatedValue = entry.ValueType == ValueType.PlayerDeath
            ? entry.CombatVictim?.EstimatedEquipmentValue ?? 0
            : entry.CombatLootValue;

        return double.IsFinite(estimatedValue) ? Math.Max(estimatedValue, 0) : 0;
    }

    private static string ResolveCombatResultName(ValueType valueType)
    {
        return valueType switch
        {
            ValueType.PlayerKill => TranslateCombatText("KILL", "T\u00F6tung", "Kill"),
            ValueType.PlayerDeath => TranslateCombatText("DEATH", "Tod", "Death"),
            ValueType.PlayerKnockout => TranslateCombatText("KNOCKOUT", "Knockout", "Knockout"),
            ValueType.PlayerKnockedOut => TranslateCombatText("KNOCKED_OUT", "Ausgeknockt", "Knocked out"),
            _ => string.Empty
        };
    }

    private static string ResolveCombatAreaName(StatisticEntry entry)
    {
        var areaName = ResolveLootAreaName(entry.DungeonMode, entry.CombatAreaIndex ?? string.Empty);
        return string.IsNullOrWhiteSpace(areaName)
            ? LocalizationController.Translation("UNKNOWN")
            : areaName;
    }

    private static string FormatRelativeTime(DateTime occurredAtUtc)
    {
        var elapsed = DateTime.UtcNow - occurredAtUtc;
        if (elapsed < TimeSpan.FromMinutes(1))
        {
            return TranslateCombatText("JUST_NOW", "Gerade eben", "Just now");
        }

        if (elapsed < TimeSpan.FromHours(1))
        {
            return TranslateRelativeTime("MINUTES_AGO", Math.Max(1, (int) elapsed.TotalMinutes), "vor {value} Min.", "{value} min ago");
        }

        if (elapsed < TimeSpan.FromDays(1))
        {
            return TranslateRelativeTime("HOURS_AGO", Math.Max(1, (int) elapsed.TotalHours), "vor {value} Std.", "{value} h ago");
        }

        return TranslateRelativeTime("DAYS_AGO", Math.Max(1, (int) elapsed.TotalDays), "vor {value} T.", "{value} d ago");
    }

    private static string TranslateRelativeTime(
        string translationKey,
        int value,
        string germanText,
        string englishText)
    {
        var formattedValue = value.ToString(CultureInfo.CurrentCulture);
        var translation = LocalizationController.Translation(
            translationKey,
            ["value"],
            [formattedValue]);
        if (!string.Equals(translation, translationKey, StringComparison.Ordinal))
        {
            return translation;
        }

        return TranslateCombatText(translationKey, germanText, englishText)
            .Replace("{value}", formattedValue, StringComparison.Ordinal);
    }

    private static string TranslateCombatText(string translationKey, string germanText, string englishText)
    {
        var translation = LocalizationController.Translation(translationKey);
        if (!string.Equals(translation, translationKey, StringComparison.Ordinal))
        {
            return translation;
        }

        return CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase)
            ? germanText
            : englishText;
    }

    private void UpdateDashboardLootStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var entries = _statisticsAggregator.GetLootEntries(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var values = aggregatedValues.GetValueOrDefault(ValueType.LootValue) ?? [];
        var currentValue = values
            .Where(x => currentRangeBucketStarts.Contains(x.Key))
            .Sum(x => x.Value);
        var previousValue = values
            .Where(x => previousRangeBucketStarts.Contains(x.Key))
            .Sum(x => x.Value);
        var lootStatistics = _mainWindowViewModel.DashboardBindings.LootStatistics;
        var rangeHours = GetRangeHours(selectedRange);

        lootStatistics.TotalValueSummary.Update(
            currentValue,
            currentValue,
            previousValue);
        lootStatistics.AverageValue = entries.Count > 0
            ? currentValue / entries.Count
            : 0;
        lootStatistics.LootPerHour = rangeHours > 0
            ? currentValue / rangeHours
            : 0;

        var valueClassItemCounts = new long[4];
        var valueClassTotals = new double[4];
        var tierItemCounts = new long[5];
        var enchantmentItemCounts = new long[5];

        var lootItems = new List<DashboardLootItem>(entries.Count);
        foreach (var entry in entries)
        {
            var unitValue = entry.ItemQuantity > 0 ? entry.Value / entry.ItemQuantity : 0;
            var valueClassIndex = GetValueClassIndex(unitValue);
            valueClassItemCounts[valueClassIndex] += entry.ItemQuantity;
            valueClassTotals[valueClassIndex] += entry.Value;

            var item = ItemController.GetItemByIndex(entry.ItemIndex);
            if (item == null)
            {
                continue;
            }

            if (item.Tier is >= 4 and <= 8)
            {
                tierItemCounts[item.Tier - 4] += entry.ItemQuantity;
            }

            if (item.Level is >= 0 and <= 4)
            {
                enchantmentItemCounts[item.Level] += entry.ItemQuantity;
            }

            lootItems.Add(new DashboardLootItem(
                item,
                entry.ItemQuantity,
                entry.Value,
                entry.OccurredAtUtc));
        }

        ReplaceDashboardItems(
            lootStatistics.RecentItems,
            lootItems
                .OrderByDescending(x => x.LootedAtLocal)
                .Take(10));
        ReplaceDashboardItems(
            lootStatistics.MostValuableItems,
            lootItems
                .OrderByDescending(x => x.UnitValue)
                .ThenByDescending(x => x.LootedAtLocal)
                .Take(10)
                .Select(x => new DashboardLootItem(
                    x.Item,
                    x.Quantity,
                    x.TotalValue,
                    x.LootedAtLocal,
                    displayUnitValue: true)));
        ReplaceDashboardItems(
            lootStatistics.ValueDistribution,
            CreateValueDistribution(valueClassItemCounts, valueClassTotals, currentValue));
        ReplaceDashboardItems(
            lootStatistics.TierDistribution,
            CreateCountDistribution(["T4", "T5", "T6", "T7", "T8"], tierItemCounts));
        ReplaceDashboardItems(
            lootStatistics.EnchantmentDistribution,
            CreateCountDistribution([".0", ".1", ".2", ".3", ".4"], enchantmentItemCounts));
        ReplaceDashboardItems(
            lootStatistics.TopAreas,
            CreateTopLootAreas(entries, rangeHours, currentValue));
    }

    private void UpdateDashboardLootedChestStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var currentEntries = _statisticsAggregator.GetLootedChestEntries(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var previousEntries = _statisticsAggregator.GetLootedChestEntries(
            previousRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var currentEntriesByContent = currentEntries
            .GroupBy(ResolveLootedChestContentType)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var previousTotalsByContent = previousEntries
            .GroupBy(ResolveLootedChestContentType)
            .ToDictionary(group => group.Key, group => group.Count());

        foreach (var contentStatistics in _mainWindowViewModel.DashboardBindings.LootedChests.ContentStatistics)
        {
            var contentEntries = currentEntriesByContent.GetValueOrDefault(contentStatistics.ContentType) ?? [];
            UpdateDashboardLootedChestContentStatistics(
                contentStatistics,
                contentEntries,
                previousTotalsByContent.GetValueOrDefault(contentStatistics.ContentType));
        }
    }

    private static void UpdateDashboardLootedChestContentStatistics(
        DashboardLootedChestContentStatistics contentStatistics,
        IReadOnlyCollection<StatisticEntry> currentEntries,
        int previousTotal)
    {
        var common = 0;
        var uncommon = 0;
        var rare = 0;
        var legendary = 0;

        foreach (var entry in currentEntries)
        {
            switch (entry.TreasureRarity)
            {
                case TreasureRarity.Common:
                    common++;
                    break;
                case TreasureRarity.Uncommon:
                    uncommon++;
                    break;
                case TreasureRarity.Rare:
                    rare++;
                    break;
                case TreasureRarity.Legendary:
                    legendary++;
                    break;
            }
        }

        var total = currentEntries.Count;
        var mapCount = currentEntries
            .Select(CreateLootedChestAreaKey)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var averagePerMap = mapCount > 0
            ? (double) total / mapCount
            : 0;

        contentStatistics.Update(
            total,
            previousTotal,
            common,
            uncommon,
            rare,
            legendary,
            averagePerMap);
    }

    private static DashboardContentType ResolveLootedChestContentType(StatisticEntry entry)
    {
        return DashboardContentTypeResolver.Resolve(
            entry.MapType,
            entry.DungeonMode,
            entry.ClusterMode);
    }

    private static string CreateLootedChestAreaKey(StatisticEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.LootAreaIndex))
        {
            return $"map:{entry.LootAreaIndex}";
        }

        return entry.DungeonMode != DungeonMode.Unknown
            ? $"content:{entry.DungeonMode}"
            : $"content:{entry.MapType}:{entry.ClusterMode}";
    }
    private static void ReplaceDashboardItems<T>(
        ObservableCollection<T> target,
        IEnumerable<T> items)
    {
        var newItems = items as IReadOnlyList<T> ?? items.ToArray();
        var sharedItemCount = Math.Min(target.Count, newItems.Count);

        for (var i = 0; i < sharedItemCount; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(target[i], newItems[i]))
            {
                target[i] = newItems[i];
            }
        }

        while (target.Count > newItems.Count)
        {
            target.RemoveAt(target.Count - 1);
        }

        for (var i = target.Count; i < newItems.Count; i++)
        {
            target.Add(newItems[i]);
        }
    }

    private static IReadOnlyCollection<DashboardLootAreaItem> CreateTopLootAreas(
        IReadOnlyCollection<StatisticEntry> entries,
        double rangeHours,
        double totalLootValue)
    {
        return entries
            .Where(HasKnownLootArea)
            .GroupBy(GetLootAreaKey)
            .Select(group => new
            {
                group.Key,
                ItemCount = group.Sum(entry => (long) entry.ItemQuantity),
                TotalValue = group.Sum(entry => entry.Value),
                ClusterType = group.First().LootAreaClusterType,
                VisitCount = group.Select(GetLootAreaVisitKey).Distinct().LongCount()
            })
            .OrderByDescending(area => area.TotalValue)
            .ThenBy(area => area.Key.AreaIndex, StringComparer.OrdinalIgnoreCase)
            .ThenBy(area => area.Key.DungeonMode)
            .Take(5)
            .Select(area => new DashboardLootAreaItem(
                ResolveLootAreaName(area.Key.DungeonMode, area.Key.AreaIndex),
                area.ItemCount,
                area.TotalValue,
                rangeHours > 0 ? area.TotalValue / rangeHours : 0,
                area.VisitCount > 0 ? area.TotalValue / area.VisitCount : 0,
                area.VisitCount,
                CalculateSharePercentage(area.TotalValue, totalLootValue),
                area.ClusterType,
                area.Key.DungeonMode))
            .ToArray();
    }

    private static (Guid SessionId, DateTime EnteredAtUtc) GetLootAreaVisitKey(StatisticEntry entry)
    {
        return (entry.SessionId, entry.LootAreaEnteredAtUtc);
    }

    private static bool HasKnownLootArea(StatisticEntry entry)
    {
        return IsStandaloneLootArea(entry.DungeonMode)
               || !string.IsNullOrWhiteSpace(entry.LootAreaIndex);
    }

    private static (DungeonMode DungeonMode, string AreaIndex) GetLootAreaKey(StatisticEntry entry)
    {
        return IsStandaloneLootArea(entry.DungeonMode)
            ? (entry.DungeonMode, string.Empty)
            : (DungeonMode.Unknown, entry.LootAreaIndex ?? string.Empty);
    }

    private static string ResolveLootAreaName(DungeonMode dungeonMode, string areaIndex)
    {
        if (!IsStandaloneLootArea(dungeonMode))
        {
            var mapName = WorldData.GetUniqueNameOrDefault(areaIndex);
            return string.IsNullOrWhiteSpace(mapName) ? areaIndex : mapName;
        }

        var translationKey = dungeonMode switch
        {
            DungeonMode.HellGate => "HELLGATE",
            DungeonMode.Corrupted => "CORRUPTED",
            DungeonMode.Expedition => "EXPEDITION",
            DungeonMode.Mists => "MISTS",
            DungeonMode.MistsDungeon => "MISTS_DUNGEON",
            DungeonMode.AbyssalDepths => "ABYSSALDEPTHS",
            DungeonMode.DragonArea => "DRAGONAREA",
            _ => "UNKNOWN"
        };

        return LocalizationController.Translation(translationKey);
    }

    private int CountFilteredSessions(DateTime localRangeStart, DateTime nowUtc, Guid? selectedSessionId)
    {
        List<StatisticSession> sessions;
        lock (_syncRoot)
        {
            sessions = _dashboardStatistics.CreateSessionSnapshot();
        }

        var rangeStartUtc = localRangeStart.ToUniversalTime();
        return sessions.Count(session =>
            (!selectedSessionId.HasValue || session.Id == selectedSessionId.Value)
            && session.StartedAtUtc < nowUtc
            && (session.EndedAtUtc ?? nowUtc) > rangeStartUtc);
    }

    private void UpdateDashboardContentRankings(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var fameValues = _statisticsAggregator.AggregateContentValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            ValueType.Fame,
            contentType);
        var silverValues = _statisticsAggregator.AggregateContentValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            ValueType.Silver,
            contentType);

        UpdateDashboardContentRanking(
            _mainWindowViewModel.DashboardBindings.FameContentRanking,
            fameValues,
            value => _mainWindowViewModel.DashboardBindings.TotalFameByContent = value);
        UpdateDashboardContentRanking(
            _mainWindowViewModel.DashboardBindings.SilverContentRanking,
            silverValues,
            value => _mainWindowViewModel.DashboardBindings.TotalSilverByContent = value);
    }

    private static void UpdateDashboardContentRanking(
        ObservableCollection<DashboardContentRankingItem> ranking,
        IReadOnlyDictionary<(MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> contentValues,
        Action<double> updateTotal)
    {
        var valuesByContent = contentValues
            .Where(x => x.Value > 0)
            .GroupBy(x => DashboardContentTypeResolver.Resolve(
                x.Key.MapType,
                x.Key.DungeonMode,
                x.Key.ClusterMode))
            .ToDictionary(x => x.Key, x => x.Sum(value => value.Value));
        var total = valuesByContent.Values.Sum();
        var topValues = valuesByContent
            .OrderByDescending(x => x.Value)
            .Take(DashboardContentRankingLimit)
            .ToList();
        var highestValue = topValues.FirstOrDefault().Value;

        var rankingItems = topValues
            .Select(item =>
            {
                var sharePercentage = total > 0 ? item.Value / total * 100 : 0;
                var barPercentage = highestValue > 0 ? item.Value / highestValue * 100 : 0;
                return new DashboardContentRankingItem(
                    LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(item.Key)),
                    item.Value,
                    sharePercentage,
                    barPercentage,
                    ResolveContentBrush(item.Key));
            })
            .ToArray();

        updateTotal(total);
        ReplaceDashboardItems(ranking, rankingItems);
    }

    private static Brush ResolveContentBrush(DashboardContentType contentType)
    {
        var resourceKey = DashboardContentTypeResolver.GetBrushResourceKey(contentType);
        return Application.Current?.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    private static void UpdateDashboardSummaryMetric(
        DashboardSummaryMetric metric,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues,
        ValueType valueType,
        IReadOnlySet<DateTime> currentRangeBucketStarts,
        IReadOnlySet<DateTime> previousRangeBucketStarts)
    {
        var values = aggregatedValues.GetValueOrDefault(valueType) ?? [];
        var currentRangeValue = values.Where(x => currentRangeBucketStarts.Contains(x.Key)).Sum(x => x.Value);
        var previousRangeValue = values.Where(x => previousRangeBucketStarts.Contains(x.Key)).Sum(x => x.Value);

        metric.Update(currentRangeValue, currentRangeValue, previousRangeValue);
    }

    private void UpdateDashboardSessionTime(
        DashboardChartRangeOption selectedRange,
        DateTime rangeStart,
        DateTime nowUtc)
    {
        List<StatisticSession> sessions;
        lock (_syncRoot)
        {
            sessions = _dashboardStatistics.CreateSessionSnapshot();
        }

        var selectedSessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var selectedSessions = selectedSessionId.HasValue
            ? sessions.Where(x => x.Id == selectedSessionId.Value).ToArray()
            : sessions.ToArray();
        var previousRangeStart = AddBuckets(rangeStart, -selectedRange.BucketCount, selectedRange.Unit);

        var currentRangeSeconds = SumSessionDurationSeconds(selectedSessions, rangeStart.ToUniversalTime(), nowUtc, nowUtc);
        var previousRangeSeconds = SumSessionDurationSeconds(
            selectedSessions,
            previousRangeStart.ToUniversalTime(),
            rangeStart.ToUniversalTime(),
            nowUtc);

        _mainWindowViewModel.DashboardBindings.SessionTimeSummary.Update(
            currentRangeSeconds,
            currentRangeSeconds,
            previousRangeSeconds);
    }

    private void UpdateDashboardSummaryValuesPerHour(double rangeHours)
    {
        var bindings = _mainWindowViewModel.DashboardBindings;

        UpdateValuePerHour(bindings.FameSummary, rangeHours);
        UpdateValuePerHour(bindings.ReSpecSummary, rangeHours);
        UpdateValuePerHour(bindings.SilverSummary, rangeHours);
        UpdateValuePerHour(bindings.MightSummary, rangeHours);
        UpdateValuePerHour(bindings.FavorSummary, rangeHours);
        UpdateValuePerHour(bindings.FactionPointsSummary, rangeHours);
        UpdateValuePerHour(bindings.FactionStandingSummary, rangeHours);
    }

    private static void UpdateValuePerHour(DashboardSummaryMetric metric, double rangeHours)
    {
        var valuePerHour = rangeHours > 0
            ? metric.Value / rangeHours
            : 0;
        metric.UpdateValuePerHour(valuePerHour);
    }

    private static double SumSessionDurationSeconds(
        IEnumerable<StatisticSession> sessions,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        DateTime nowUtc)
    {
        return sessions.Sum(session =>
        {
            var sessionEndUtc = session.EndedAtUtc ?? nowUtc;
            var overlapStartUtc = session.StartedAtUtc > periodStartUtc ? session.StartedAtUtc : periodStartUtc;
            var overlapEndUtc = sessionEndUtc < periodEndUtc ? sessionEndUtc : periodEndUtc;
            return overlapEndUtc > overlapStartUtc
                ? (overlapEndUtc - overlapStartUtc).TotalSeconds
                : 0;
        });
    }

    private static DateTime AlignToBucketStart(DateTime localDateTime, DashboardChartRangeUnit unit)
    {
        return unit switch
        {
            DashboardChartRangeUnit.Minute => new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, 0),
            DashboardChartRangeUnit.Hour => new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, 0, 0),
            DashboardChartRangeUnit.Day => localDateTime.Date,
            _ => localDateTime.Date
        };
    }

    private static DateTime AddBuckets(DateTime bucketStart, int bucketCount, DashboardChartRangeUnit unit)
    {
        return unit switch
        {
            DashboardChartRangeUnit.Minute => bucketStart.AddMinutes(bucketCount),
            DashboardChartRangeUnit.Hour => bucketStart.AddHours(bucketCount),
            DashboardChartRangeUnit.Day => bucketStart.AddDays(bucketCount),
            _ => bucketStart.AddDays(bucketCount)
        };
    }

    private static DashboardUpdateScope ResolveDashboardUpdateScope(ValueType valueType)
    {
        return valueType switch
        {
            ValueType.Fame or ValueType.Silver => DashboardUpdateScope.Chart
                                                 | DashboardUpdateScope.Summary
                                                 | DashboardUpdateScope.ContentRankings,
            ValueType.ReSpec => DashboardUpdateScope.Chart
                                | DashboardUpdateScope.Summary
                                | DashboardUpdateScope.Economy,
            ValueType.FactionStanding
                or ValueType.FactionPoints
                or ValueType.Might
                or ValueType.Favor => DashboardUpdateScope.Chart | DashboardUpdateScope.Summary,
            ValueType.PaidSilverForReSpec
                or ValueType.RepairCosts
                or ValueType.ItemQualityRerollCosts
                or ValueType.ItemQualityRerollResult
                or ValueType.ItemQualityRerollAttempt
                or ValueType.AwakenedWeaponCosts
                or ValueType.AwakenedWeaponTraitUpgrade
                or ValueType.AwakenedWeaponTraitUpgradeProc => DashboardUpdateScope.Economy,
            ValueType.LootValue => DashboardUpdateScope.Loot,
            ValueType.PlayerKill
                or ValueType.PlayerDeath
                or ValueType.PlayerKnockout
                or ValueType.PlayerKnockedOut => DashboardUpdateScope.Combat,
            ValueType.MobKill => DashboardUpdateScope.Mobs,
            ValueType.LootedChest => DashboardUpdateScope.LootedChests,
            _ => DashboardUpdateScope.None
        };
    }

    private void MarkDashboardDirty(DashboardUpdateScope updateScopes)
    {
        lock (_syncRoot)
        {
            MarkDashboardDirtyInternal(updateScopes);
        }
    }

    private void MarkDashboardDirtyInternal(DashboardUpdateScope updateScopes)
    {
        _pendingDashboardUpdateScopes |= updateScopes;
    }

    private DashboardUpdateScope TakeDashboardUpdateScopes()
    {
        lock (_syncRoot)
        {
            var updateScopes = _pendingDashboardUpdateScopes;
            _pendingDashboardUpdateScopes = DashboardUpdateScope.None;
            return updateScopes;
        }
    }

    private void MarkSessionDirtyInternal(Guid sessionId)
    {
        _dirtySessionVersions[sessionId] =
            _dirtySessionVersions.GetValueOrDefault(sessionId) + 1;
    }

    public DungeonMode ResolveDungeonMode(MapType mapType)
    {
        if (mapType != MapType.RandomDungeon)
        {
            return mapType switch
            {
                MapType.HellGate => DungeonMode.HellGate,
                MapType.CorruptedDungeon => DungeonMode.Corrupted,
                MapType.Expedition => DungeonMode.Expedition,
                MapType.Mists => DungeonMode.Mists,
                MapType.MistsDungeon => DungeonMode.MistsDungeon,
                MapType.AbyssalDepths => DungeonMode.AbyssalDepths,
                MapType.DragonArea => DungeonMode.DragonArea,
                _ => DungeonMode.Unknown
            };
        }

        var currentDungeonMode = _trackingController.DungeonController.GetCurrentDungeonMode();
        if (currentDungeonMode is DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon)
        {
            return currentDungeonMode;
        }

        var detectedDungeonMode = DungeonData.GetDungeonMode(
            ClusterController.CurrentCluster.SourceClusterIndex,
            ClusterController.CurrentCluster.Index,
            ClusterController.CurrentCluster.UniqueName,
            ClusterController.CurrentCluster.UniqueClusterName);

        return detectedDungeonMode is DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon
            ? detectedDungeonMode
            : DungeonMode.Unknown;
    }

    private static string ResolveLootAreaIndex(MapType mapType, DungeonMode dungeonMode)
    {
        if (IsStandaloneLootArea(dungeonMode))
        {
            return string.Empty;
        }

        var currentCluster = ClusterController.CurrentCluster;
        if (mapType == MapType.RandomDungeon
            && !string.IsNullOrWhiteSpace(currentCluster.SourceClusterIndex))
        {
            return currentCluster.SourceClusterIndex;
        }

        return currentCluster.Index ?? string.Empty;
    }

    private static bool IsStandaloneLootArea(DungeonMode dungeonMode)
    {
        return dungeonMode is DungeonMode.HellGate
            or DungeonMode.Corrupted
            or DungeonMode.Expedition
            or DungeonMode.Mists
            or DungeonMode.MistsDungeon
            or DungeonMode.AbyssalDepths
            or DungeonMode.DragonArea;
    }

    private static List<ChartBucket> CreateChartBuckets(DashboardChartRangeOption selectedRange)
    {
        var buckets = new List<ChartBucket>(selectedRange.BucketCount);
        var currentBucketStart = AlignToBucketStart(DateTime.Now, selectedRange.Unit);

        for (var i = selectedRange.BucketCount - 1; i >= 0; i--)
        {
            var start = AddBuckets(currentBucketStart, -i, selectedRange.Unit);
            var label = selectedRange.Unit switch
            {
                DashboardChartRangeUnit.Minute => start.ToString("HH:mm", CultureInfo.CurrentCulture),
                DashboardChartRangeUnit.Hour => start.ToString("dd.MM HH:mm", CultureInfo.CurrentCulture),
                DashboardChartRangeUnit.Day => start.ToString("d", CultureInfo.CurrentCulture),
                _ => start.ToString("d", CultureInfo.CurrentCulture)
            };

            buckets.Add(new ChartBucket(start, label));
        }

        return buckets;
    }

    public static SolidColorPaint GetValueTypeBrush(ValueType valueType, bool transparent)
    {
        try
        {
            if (transparent)
            {
                var transparentBrush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}.Transparent"];
                return new SolidColorPaint
                {
                    Color = new SKColor(transparentBrush.Color.R, transparentBrush.Color.G, transparentBrush.Color.B, transparentBrush.Color.A)
                };
            }

            var brush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
            return new SolidColorPaint
            {
                Color = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            };
        }
        catch
        {
            return new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0, 0)
            };
        }
    }

    private sealed class ChartBucket
    {
        public ChartBucket(DateTime start, string label)
        {
            Start = start;
            Label = label;
        }

        public DateTime Start { get; }
        public string Label { get; }
    }

    #endregion

    #region Kill / Death infos

    public void SetKillsDeathsValues()
    {
        _mainWindowViewModel.DashboardBindings.KillsToday = _trackingController.EntityController.LocalUserData.KillsToday;
        _mainWindowViewModel.DashboardBindings.SoloKillsToday = _trackingController.EntityController.LocalUserData.SoloKillsToday;
        _mainWindowViewModel.DashboardBindings.DeathsToday = _trackingController.EntityController.LocalUserData.DeathsToday;
        _mainWindowViewModel.DashboardBindings.KillsThisWeek = _trackingController.EntityController.LocalUserData.KillsWeek;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisWeek = _trackingController.EntityController.LocalUserData.SoloKillsWeek;
        _mainWindowViewModel.DashboardBindings.DeathsThisWeek = _trackingController.EntityController.LocalUserData.DeathsWeek;
        _mainWindowViewModel.DashboardBindings.KillsThisMonth = _trackingController.EntityController.LocalUserData.KillsMonth;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisMonth = _trackingController.EntityController.LocalUserData.SoloKillsMonth;
        _mainWindowViewModel.DashboardBindings.DeathsThisMonth = _trackingController.EntityController.LocalUserData.DeathsMonth;

        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenKilling = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenKilling;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerOfTheKilledEnemies = _trackingController.EntityController.LocalUserData.AverageItemPowerOfTheKilledEnemies;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenDying = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenDying;
        _mainWindowViewModel.DashboardBindings.LastUpdate = _trackingController.EntityController.LocalUserData.LastUpdate;
    }

    #endregion

    #region Repair costs stats

    public void UpdateRepairCostsUi()
    {
        var now = DateTime.Now;
        var endExclusive = now.AddTicks(1);
        var statisticsAggregator = _statisticsAggregator;

        _mainWindowViewModel.DashboardBindings.RepairCostsToday = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.Date, endExclusive)).IntegerValue;
        _mainWindowViewModel.DashboardBindings.RepairCostsLast7Days = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.AddDays(-7), endExclusive)).IntegerValue;
        _mainWindowViewModel.DashboardBindings.RepairCostsLast30Days = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.AddDays(-30), endExclusive)).IntegerValue;
    }

    #endregion

    #region Load / Save local file data

    public async System.Threading.Tasks.Task LoadFromFileAsync()
    {
        var loadedStatistics = await _sessionStorage.LoadAsync(DateTime.UtcNow);

        lock (_syncRoot)
        {
            _dashboardStatistics = loadedStatistics;
            _statisticsAggregator = new DashboardStatisticsAggregator(loadedStatistics);
            _dirtySessionVersions.Clear();
        }

        if (_mainWindowViewModel.MainStatusBindings.IsInGame)
        {
            StartSession(_trackingController.EntityController.LocalUserData.Username ?? string.Empty);
        }
        else
        {
            RefreshDashboardSessionFilters();
        }

        UpdateRepairCostsUi();
        UpdateDailyChart(true);
    }

    public async System.Threading.Tasks.Task SaveInFileAsync()
    {
        await _sessionPersistenceSemaphore.WaitAsync();

        try
        {
            DashboardStatistics statisticsSnapshot;
            Dictionary<Guid, long> dirtySessionVersions;
            lock (_syncRoot)
            {
                statisticsSnapshot = _dashboardStatistics.CreateSnapshot();
                dirtySessionVersions = new Dictionary<Guid, long>(_dirtySessionVersions);
            }

            if (dirtySessionVersions.Count == 0)
            {
                return;
            }

            var wasSaved = await _sessionStorage.SaveSessionsAsync(
                statisticsSnapshot,
                dirtySessionVersions.Keys.ToArray());
            if (!wasSaved)
            {
                Log.Warning("Statistics session save was incomplete. Sessions={SessionCount}", dirtySessionVersions.Count);
                return;
            }

            lock (_syncRoot)
            {
                foreach (var savedSession in dirtySessionVersions)
                {
                    if (_dirtySessionVersions.TryGetValue(savedSession.Key, out var currentVersion)
                        && currentVersion == savedSession.Value)
                    {
                        _dirtySessionVersions.Remove(savedSession.Key);
                    }
                }
            }

            Log.Information("Statistics sessions saved. Sessions={SessionCount}", dirtySessionVersions.Count);
        }
        finally
        {
            _sessionPersistenceSemaphore.Release();
        }
    }

    private void RefreshDashboardSessionFilters()
    {
        IReadOnlyCollection<StatisticSession> sessionsSnapshot;
        lock (_syncRoot)
        {
            sessionsSnapshot = _dashboardStatistics.CreateSessionSnapshot();
        }

        void ApplyFilters()
        {
            var selectedSessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId
                                    ?? SettingsController.CurrentSettings.SelectedDashboardSessionId;
            var filters = new List<DashboardSessionFilterOption>
            {
                new(null, LocalizationController.Translation("ALL_SESSIONS"))
            };

            filters.AddRange(sessionsSnapshot
                .OrderByDescending(x => x.StartedAtUtc)
                .Select(x => new DashboardSessionFilterOption(
                    x.Id,
                    CreateSessionFilterName(x),
                    x.EndedAtUtc.HasValue)));

            _mainWindowViewModel.DashboardSessionFilters = new ObservableCollection<DashboardSessionFilterOption>(filters);
            _mainWindowViewModel.SelectedDashboardSessionFilter = filters
                .FirstOrDefault(x => x.SessionId == selectedSessionId)
                ?? filters[0];
            SettingsController.CurrentSettings.SelectedDashboardSessionId = _mainWindowViewModel.SelectedDashboardSessionFilter.SessionId;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            ApplyFilters();
            return;
        }

        _ = dispatcher.InvokeAsync(ApplyFilters);
    }

    private static string CreateSessionFilterName(StatisticSession session)
    {
        var activeMarker = session.EndedAtUtc.HasValue ? string.Empty : "* ";
        var characterName = string.IsNullOrWhiteSpace(session.CharacterName) ? "?" : session.CharacterName;
        return $"{activeMarker}{session.StartedAtUtc.ToLocalTime():g} | {characterName} | {session.ServerLocation}";
    }

    #endregion
}
