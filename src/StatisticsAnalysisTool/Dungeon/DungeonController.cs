using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Loot = StatisticsAnalysisTool.Dungeon.Models.Loot;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
// ReSharper disable PossibleMultipleEnumeration

namespace StatisticsAnalysisTool.Dungeon;

public sealed class DungeonController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
{
    private const int NumberOfDungeonsUntilSaved = 1;
    private const int DungeonRetentionYears = 2;
    private const string AbyssalDepthsRewardChestName = "HD_DEMON_SOUL_REWARD";
    private const float MaxDungeonExitAssociationDistanceSquared = 25f;

    private readonly object _saveSnapshotLock = new();
    private List<DungeonDto> _preparedShutdownSaveSnapshot;
    private Guid? _currentGuid;
    private Guid? _lastMapGuid;
    private int _addDungeonCounter;
    private readonly List<DiscoveredItem> _discoveredLoot = [];
    private readonly Dictionary<long, DungeonLootSource> _lootSources = [];
    private readonly ConcurrentDictionary<int, (string UniqueName, TreasureRarity Rarity)> _pendingAbyssalDepthsChests = [];
    private readonly List<RandomDungeonExitInfo> _discoveredRandomDungeonExits = [];
    private int? _selectedRandomDungeonExitObjectId;

    private static bool IsTrackingActive => SettingsController.CurrentSettings.IsDungeonTrackingActive;

    public async Task ApplyTrackingStateAsync(bool isTrackingActive)
    {
        mainWindowViewModel.DungeonBindings.IsDungeonTrackingActive = isTrackingActive;

        if (!isTrackingActive)
        {
            await CompleteActiveDungeonsAsync();
        }

        ResetTrackingContext();
    }

    private void ResetTrackingContext()
    {
        _currentGuid = null;
        _lastMapGuid = null;
        _currentItemContainer = null;
        ResetLocalPlayerDiscoveredLoot();
        ClearRandomDungeonExits();
    }

    public async Task AddDungeonAsync(MapType mapType, Guid? mapGuid, string sourceClusterIndex, WorldPosition? sourceExitPosition)
    {
        if (!IsTrackingActive || !trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var dungeonGuid = GetDungeonTrackingGuid(mapType, mapGuid);
        _currentGuid = dungeonGuid;

        if (IsDungeonCluster(mapType, dungeonGuid))
        {
            if (TryAddClusterToExistingDungeon(mapType, dungeonGuid, _lastMapGuid, out var currentDungeon))
            {
                currentDungeon.AddTimer(DateTime.UtcNow);
            }
            else if (StartsNewDungeonRun(mapType) || !ExistDungeon(_currentGuid))
            {
                await AddNewDungeonAsync(mapType, dungeonGuid, sourceClusterIndex, sourceExitPosition);
            }
            else if (GetDungeon(_currentGuid) is { Status: not DungeonStatus.Active } existingDungeon)
            {
                existingDungeon.Status = DungeonStatus.Active;
                existingDungeon.AddTimer(DateTime.UtcNow);
            }
        }
        else if (ExistDungeon(_lastMapGuid))
        {
            ClearRandomDungeonExits();
            await CompleteActiveDungeonsAsync();
        }

        _lastMapGuid = dungeonGuid;
    }

    private async Task AddNewDungeonAsync(MapType mapType, Guid? dungeonGuid, string sourceClusterIndex, WorldPosition? sourceExitPosition)
    {
        await CompleteActiveDungeonsAsync();

        if (!IsTrackingActive)
        {
            return;
        }

        var mainMapIndex = GetMainMapIndex(mapType, sourceClusterIndex);
        var newDungeon = CreateNewDungeon(mapType, mainMapIndex, dungeonGuid);
        newDungeon.PartySize = Math.Max(1, mainWindowViewModel.PartyBindings.Party.Count);
        UpdateCurrentDungeonFromEntrance(newDungeon, sourceClusterIndex, sourceExitPosition);
        ClearRandomDungeonExits();
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            if (!IsTrackingActive)
            {
                return;
            }

            mainWindowViewModel.DungeonBindings.Dungeons.Insert(0, newDungeon);
        });
    }

    private async Task CompleteActiveDungeonsAsync()
    {
        var activeDungeons = mainWindowViewModel.DungeonBindings.Dungeons
            .Where(x => x.Status != DungeonStatus.Done)
            .ToList();
        if (activeDungeons.Count == 0)
        {
            return;
        }

        foreach (var dungeon in activeDungeons)
        {
            dungeon.EndTimer();
            dungeon.Status = DungeonStatus.Done;
        }

        await SaveInFileAfterExceedingLimit(NumberOfDungeonsUntilSaved);
    }

    private bool TryAddClusterToExistingDungeon(MapType mapType, Guid? currentGuid, Guid? lastGuid, out DungeonBaseFragment dungeon)
    {
        dungeon = null;
        if (StartsNewDungeonRun(mapType))
        {
            return false;
        }

        var lastDungeon = GetDungeon(lastGuid);
        if (lastDungeon?.MapType != mapType)
        {
            return false;
        }

        return AddClusterToExistDungeon(currentGuid, lastGuid, out dungeon);
    }

    private static bool StartsNewDungeonRun(MapType mapType)
    {
        return mapType is MapType.CorruptedDungeon
            or MapType.HellGate
            or MapType.Mists
            or MapType.MistsDungeon
            or MapType.DragonArea
            or MapType.StaticDungeon;
    }

    private static Guid? GetDungeonTrackingGuid(MapType mapType, Guid? mapGuid)
    {
        return mapType == MapType.StaticDungeon ? Guid.NewGuid() : mapGuid;
    }

    private static string GetMainMapIndex(MapType mapType, string sourceClusterIndex)
    {
        return mapType == MapType.StaticDungeon ? sourceClusterIndex : ClusterController.CurrentCluster.SourceClusterIndex;
    }

    private static DungeonBaseFragment CreateNewDungeon(MapType mapType, string mainMapIndex, Guid? guid)
    {
        if (guid == null)
        {
            return null;
        }

        DungeonBaseFragment newDungeon;
        switch (mapType)
        {
            case MapType.RandomDungeon:
                var dungeonMode = DungeonData.GetDungeonMode(
                    mainMapIndex,
                    ClusterController.CurrentCluster.SourceClusterIndex,
                    ClusterController.CurrentCluster.Index,
                    ClusterController.CurrentCluster.UniqueName,
                    ClusterController.CurrentCluster.UniqueClusterName);
                newDungeon = new RandomDungeonFragment((Guid) guid, mapType, dungeonMode, mainMapIndex);
                break;
            case MapType.CorruptedDungeon:
                newDungeon = new CorruptedFragment((Guid) guid, mapType, DungeonMode.Corrupted, mainMapIndex);
                break;
            case MapType.HellGate:
                newDungeon = new HellGateFragment((Guid) guid, mapType, DungeonMode.HellGate, mainMapIndex);
                break;
            case MapType.Expedition:
                newDungeon = new ExpeditionFragment((Guid) guid, mapType, DungeonMode.Expedition, mainMapIndex);
                break;
            case MapType.Mists:
                var tier = (Tier) Enum.ToObject(typeof(Tier), MistsData.GetTier(ClusterController.CurrentCluster.WorldMapDataType));
                newDungeon = new MistsFragment((Guid) guid, mapType, DungeonMode.Mists, mainMapIndex, ClusterController.CurrentCluster.MistsRarity, tier);
                break;
            case MapType.MistsDungeon:
                newDungeon = new MistsDungeonFragment((Guid) guid, mapType, DungeonMode.MistsDungeon, mainMapIndex, ClusterController.CurrentCluster.MistsDungeonTier);
                break;
            case MapType.AbyssalDepths:
                newDungeon = new AbyssalDepthsFragment((Guid) guid, mapType, DungeonMode.AbyssalDepths, mainMapIndex);
                break;
            case MapType.DragonArea:
                newDungeon = new DragonAreaFragment((Guid) guid, mapType, DungeonMode.DragonArea, mainMapIndex);
                break;
            case MapType.StaticDungeon:
                var faction = DungeonData.GetFaction(
                    ClusterController.CurrentCluster.Index,
                    ClusterController.CurrentCluster.File,
                    ClusterController.CurrentCluster.UniqueName);
                newDungeon = new StaticDungeonFragment((Guid) guid, mainMapIndex, faction, ClusterController.CurrentCluster.Tier);
                break;
            default:
                newDungeon = null;
                break;
        }

        return newDungeon;
    }

    public void ResetDungeons()
    {
        mainWindowViewModel.DungeonBindings.Dungeons.Clear();
        Application.Current.Dispatcher.Invoke(() => { mainWindowViewModel?.DungeonBindings?.Dungeons?.Clear(); });
    }

    public void ResetDungeonsByDateAscending(DateTime date)
    {
        var dungeonsToDelete = mainWindowViewModel.DungeonBindings.Dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? [])
        {
            mainWindowViewModel.DungeonBindings.Dungeons?.Remove(dungeonObject);
        }

        var trackingDungeonsToDelete = mainWindowViewModel?.DungeonBindings?.Dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in trackingDungeonsToDelete ?? [])
        {
            mainWindowViewModel?.DungeonBindings?.Dungeons?.Remove(dungeonObject);
        }
    }

    public void DeleteDungeonsWithZeroFame()
    {
        var dungeonsToDelete = mainWindowViewModel.DungeonBindings.Dungeons?.Where(x => x.Fame <= 0 && x.Status != DungeonStatus.Active).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? [])
        {
            mainWindowViewModel.DungeonBindings.Dungeons?.Remove(dungeonObject);
        }
    }

    public void RemoveDungeon(string dungeonHash)
    {
        var dungeon = mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.DungeonHash.Contains(dungeonHash));

        if (dungeon == null)
        {
            return;
        }

        var dialog = new DialogWindow(LocalizationController.Translation("REMOVE_DUNGEON"), LocalizationController.Translation("SURE_YOU_WANT_TO_REMOVE_DUNGEON"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is not true)
        {
            return;
        }

        _ = mainWindowViewModel.DungeonBindings.Dungeons.Remove(dungeon);
    }

    public async Task RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash)
    {
        await foreach (var dungeons in mainWindowViewModel.DungeonBindings.Dungeons.ToList().ToAsyncEnumerable())
        {
            if (dungeonHash.Contains(dungeons.DungeonHash))
            {
                mainWindowViewModel.DungeonBindings.Dungeons.Remove(dungeons);
            }
        }

        await SaveInFileAsync();
    }

    private bool AddClusterToExistDungeon(Guid? currentGuid, Guid? lastGuid, out DungeonBaseFragment dungeon)
    {
        if (currentGuid != null && lastGuid != null && mainWindowViewModel.DungeonBindings.Dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) != true)
        {
            var dun = mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid) lastGuid));
            dun?.GuidList.Add((Guid) currentGuid);

            dungeon = dun;

            return mainWindowViewModel.DungeonBindings.Dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) ?? false;
        }

        dungeon = null;
        return false;
    }

    #region Dungeon object

    public async Task<TreasureRarity> UpdateDungeonChestAsync(int id, List<Guid> allowedToOpen, bool isOpened, TreasureRarity rarity)
    {
        if (_currentGuid is not { } currentGuid)
        {
            return TreasureRarity.Unknown;
        }

        var isAbyssalDepths = GetCurrentDungeonMode() == DungeonMode.AbyssalDepths;
        if (isAbyssalDepths && !isOpened)
        {
            CacheAbyssalDepthsChestRarity(id, rarity);
            return TreasureRarity.Unknown;
        }

        if (!isOpened || !trackingController.EntityController.IsAnyEntityInParty(allowedToOpen))
        {
            return TreasureRarity.Unknown;
        }

        var openedChestName = string.Empty;
        var trackedRarity = TreasureRarity.Unknown;
        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dun = GetDungeon(currentGuid);
                var chest = dun?.Events?.FirstOrDefault(x => x?.Id == id);
                if (chest == null && dun?.Mode == DungeonMode.AbyssalDepths)
                {
                    var hasPendingChest = _pendingAbyssalDepthsChests.TryRemove(id, out var pendingChest);
                    var uniqueName = hasPendingChest ? pendingChest.UniqueName : AbyssalDepthsRewardChestName;
                    var openedRarity = rarity != TreasureRarity.Unknown ? rarity : hasPendingChest && pendingChest.Rarity != TreasureRarity.Unknown ? pendingChest.Rarity : TreasureRarity.Common;
                    chest = new PointOfInterest(id, uniqueName, openedRarity);
                    dun.Events?.Add(chest);
                    openedChestName = uniqueName;
                }

                if (chest == null)
                {
                    return;
                }

                var resolvedRarity = ResolveOpenedChestRarity(dun, chest.Rarity, rarity);
                if (resolvedRarity != TreasureRarity.Unknown)
                {
                    chest.Rarity = resolvedRarity;
                }

                chest.Status = ChestStatus.Open;
                chest.Opened = DateTime.UtcNow;
                trackedRarity = chest.Rarity;
            });

            if (!string.IsNullOrWhiteSpace(openedChestName))
            {
                SetLootSource(id, openedChestName, DungeonLootSourceType.Chest);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        return trackedRarity;
    }

    public async Task RegisterDungeonChestAsync(int id, string uniqueName, TreasureRarity networkRarity)
    {
        if (GetCurrentDungeonMode() == DungeonMode.AbyssalDepths)
        {
            if (id > 0 && !string.IsNullOrWhiteSpace(uniqueName))
            {
                _pendingAbyssalDepthsChests.AddOrUpdate(
                    id,
                    (uniqueName, TreasureRarity.Unknown),
                    (_, pendingChest) => (uniqueName, pendingChest.Rarity));
            }

            return;
        }

        var rarity = networkRarity != TreasureRarity.Unknown
            ? networkRarity
            : DungeonData.GetChestRarity(uniqueName);
        await SetDungeonEventInformationAsync(id, uniqueName, rarity);
    }

    private static TreasureRarity ResolveOpenedChestRarity(
        DungeonBaseFragment dungeon,
        TreasureRarity registeredRarity,
        TreasureRarity networkRarity)
    {
        if (networkRarity != TreasureRarity.Unknown)
        {
            return networkRarity;
        }

        if (registeredRarity != TreasureRarity.Unknown)
        {
            return registeredRarity;
        }

        return dungeon?.Mode == DungeonMode.DragonArea ? TreasureRarity.Common : TreasureRarity.Unknown;
    }

    private void CacheAbyssalDepthsChestRarity(int id, TreasureRarity rarity)
    {
        if (id <= 0 || rarity == TreasureRarity.Unknown)
        {
            return;
        }

        _pendingAbyssalDepthsChests.AddOrUpdate(
            id,
            (AbyssalDepthsRewardChestName, rarity),
            (_, pendingChest) => (pendingChest.UniqueName, rarity));
    }

    private DungeonBaseFragment GetDungeon(Guid? guid)
    {
        return !IsTrackingActive || guid == null ? null : mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid) guid));
    }

    public async Task SetDungeonEventInformationAsync(int id, string uniqueName, TreasureRarity rarity = TreasureRarity.Unknown)
    {
        if (_currentGuid == null || uniqueName == null)
        {
            return;
        }

        try
        {
            var dun = GetDungeon((Guid) _currentGuid);
            if (dun == null || dun.Events?.Any(x => x.Id == id) == true)
            {
                return;
            }

            var eventObject = new PointOfInterest(id, uniqueName, rarity);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                dun.Events?.Add(eventObject);
                UpdateDragonAreaPortalSize(dun, uniqueName);
            });

            if (dun.Faction == Faction.Unknown)
            {
                dun.Faction = DungeonData.GetFaction(uniqueName);
            }

            if (dun.Mode == DungeonMode.Unknown)
            {
                dun.Mode = DungeonData.GetDungeonMode(uniqueName);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public DungeonMode GetCurrentDungeonMode()
    {
        var dungeons = mainWindowViewModel.DungeonBindings.Dungeons;
        if (dungeons is null || _currentGuid is null)
        {
            return DungeonMode.Unknown;
        }

        lock (dungeons)
        {
            var currentDungeon = dungeons.FirstOrDefault(x =>
                x.GuidList.Contains(_currentGuid.Value)
                && x.Status == DungeonStatus.Active);
            return currentDungeon?.Mode ?? DungeonMode.Unknown;
        }
    }

    public void AddValueToDungeon(double value, ValueType valueType, CityFaction cityFaction = CityFaction.Unknown)
    {
        if (!IsTrackingActive)
        {
            return;
        }

        try
        {
            lock (mainWindowViewModel.DungeonBindings.Dungeons)
            {
                var dun = mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => _currentGuid != null && x.GuidList.Contains((Guid) _currentGuid) && x.Status == DungeonStatus.Active);

                switch (dun)
                {
                    case RandomDungeonFragment standardDun:
                        standardDun.Add(value, valueType, cityFaction);
                        break;
                    case HellGateFragment hellGate:
                        hellGate.Add(value, valueType);
                        break;
                    case CorruptedFragment corrupted:
                        corrupted.Add(value, valueType);
                        break;
                    case ExpeditionFragment expedition:
                        expedition.Add(value, valueType);
                        break;
                    case MistsFragment mists:
                        mists.Add(value, valueType);
                        break;
                    case MistsDungeonFragment mistsDungeon:
                        mistsDungeon.Add(value, valueType);
                        break;
                    case AbyssalDepthsFragment abyssalDepths:
                        abyssalDepths.Add(value, valueType);
                        break;
                    case DragonAreaFragment dragonArea:
                        dragonArea.Add(value, valueType);
                        break;
                    case StaticDungeonFragment staticDungeon:
                        staticDungeon.Add(value, valueType);
                        break;
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    public async Task SetDiedIfInDungeonAsync(DiedObject dieObject)
    {
        if (!IsTrackingActive
            || _currentGuid is not { } currentGuid
            || trackingController.EntityController.LocalUserData.Username is not { } username)
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            AddCombatEventIfInDungeon(currentGuid, username, dieObject);
            return;
        }

        await dispatcher.InvokeAsync(() => AddCombatEventIfInDungeon(currentGuid, username, dieObject));
    }

    private void AddCombatEventIfInDungeon(Guid currentGuid, string username, DiedObject dieObject)
    {
        if (!IsTrackingActive)
        {
            return;
        }

        var dungeon = mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.GuidList.Contains(currentGuid));

        if (dungeon is null)
        {
            return;
        }

        if (dieObject.DiedName == username)
        {
            dungeon.AddCombatEvent(KillStatus.LocalPlayerDead, dieObject.DiedName, dieObject.KilledBy);
        }
        else if (dieObject.KilledBy == username)
        {
            dungeon.AddCombatEvent(KillStatus.OpponentDead, dieObject.DiedName, dieObject.KilledBy);
        }
    }

    #endregion

    #region Level recognize

    public void AddRandomDungeonExit(RandomDungeonExitInfo exitInfo)
    {
        if (exitInfo is null || exitInfo.ObjectId <= 0 || exitInfo.SourceClusterIndex == null)
        {
            return;
        }

        lock (_discoveredRandomDungeonExits)
        {
            if (_discoveredRandomDungeonExits.Any(x => x.ObjectId == exitInfo.ObjectId))
            {
                return;
            }

            _discoveredRandomDungeonExits.Add(exitInfo);
        }
    }

    public void SelectRandomDungeonExit(int objectId)
    {
        lock (_discoveredRandomDungeonExits)
        {
            _selectedRandomDungeonExitObjectId = _discoveredRandomDungeonExits.Any(x => x.ObjectId == objectId) ? objectId : null;
        }
    }

    private void UpdateCurrentDungeonFromEntrance(DungeonBaseFragment dungeon, string sourceClusterIndex, WorldPosition? worldPosition)
    {
        if (dungeon is not (RandomDungeonFragment or MistsFragment or DragonAreaFragment))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sourceClusterIndex))
        {
            return;
        }

        lock (_discoveredRandomDungeonExits)
        {
            var discoveredRandomDungeonExit = FindDiscoveredRandomDungeonExit(sourceClusterIndex, worldPosition, dungeon);
            _selectedRandomDungeonExitObjectId = null;
            if (discoveredRandomDungeonExit is null)
            {
                return;
            }

            switch (dungeon)
            {
                case RandomDungeonFragment randomDungeon:
                    UpdateRandomDungeonFromEntrance(randomDungeon, discoveredRandomDungeonExit);
                    break;
                case MistsFragment mists:
                    UpdateMistsFromEntrance(mists, discoveredRandomDungeonExit);
                    break;
                case DragonAreaFragment dragonArea:
                    UpdateDragonAreaFromEntrance(dragonArea, discoveredRandomDungeonExit);
                    break;
            }
        }
    }

    private static void UpdateRandomDungeonFromEntrance(RandomDungeonFragment dungeon, RandomDungeonExitInfo exit)
    {
        dungeon.MobHitPointsFactor = DungeonData.GetDungeonMobHitPointsFactor(exit.DungeonType);
        dungeon.ZoneLootFactor = DungeonData.GetDungeonZoneLootFactor(exit.DungeonType);
        dungeon.TrySetTierFromEntrance(DungeonData.GetDungeonTierFromExit(exit.UniqueName));

        var dungeonMode = DungeonData.GetRandomDungeonModeFromExit(exit.UniqueName);
        if (dungeonMode != DungeonMode.Unknown)
        {
            dungeon.Mode = dungeonMode;
        }

        var faction = DungeonData.GetFaction(exit.UniqueName);
        if (faction != Faction.Unknown)
        {
            dungeon.Faction = faction;
        }

        if (exit.HasVisibleLevel)
        {
            dungeon.TrySetLevelFromEntrance(exit.Level);
        }
    }

    private static void UpdateMistsFromEntrance(MistsFragment mists, RandomDungeonExitInfo exit)
    {
        var rarity = exit.ResolvedMistsRarity;
        var mistsType = exit.ResolvedMistsType;
        if (mistsType != MistsType.Unknown)
        {
            mists.PortalType = mistsType;
        }

        if (rarity != MistsRarity.Unknown)
        {
            mists.Rarity = rarity;
        }
    }

    private static void UpdateDragonAreaFromEntrance(DragonAreaFragment dragonArea, RandomDungeonExitInfo exit)
    {
        if (exit.ResolvedDragonAreaPortalSize != DragonAreaPortalSize.Unknown)
        {
            dragonArea.PortalSize = exit.ResolvedDragonAreaPortalSize;
        }
    }

    private static void UpdateDragonAreaPortalSize(DungeonBaseFragment dungeon, string uniqueName)
    {
        if (dungeon is not DragonAreaFragment dragonArea)
        {
            return;
        }

        var portalSize = DragonAreaPortalSizeResolver.FromUniqueName(uniqueName);
        if (portalSize != DragonAreaPortalSize.Unknown)
        {
            dragonArea.PortalSize = portalSize;
        }
    }

    public void UpdateCurrentDungeonLevel(int? mobIndex, double hitPointsMax)
    {
        if (!IsTrackingActive || _currentGuid is not { } currentDungeonGuid || mobIndex is null)
        {
            return;
        }

        if (ClusterController.CurrentCluster.Guid != currentDungeonGuid)
        {
            return;
        }

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TryUpdateCurrentDungeonLevelFromMob(currentDungeonGuid, mobIndex.Value, hitPointsMax);
            });
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public void UpdateCurrentDungeonLevelFromLootChest(int objectId, double combinedLootFactor)
    {
        if (!IsTrackingActive || _currentGuid is not { } currentDungeonGuid)
        {
            return;
        }

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TryUpdateCurrentDungeonLevelFromLootChest(currentDungeonGuid, objectId, combinedLootFactor);
            });
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private RandomDungeonExitInfo FindDiscoveredRandomDungeonExit(string sourceClusterIndex, WorldPosition? worldPosition, DungeonBaseFragment dungeon)
    {
        if (_selectedRandomDungeonExitObjectId is { } selectedObjectId)
        {
            var selectedExit = _discoveredRandomDungeonExits.FirstOrDefault(x => x.ObjectId == selectedObjectId && x.SourceClusterIndex == sourceClusterIndex && IsCompatibleExit(x, dungeon));
            if (selectedExit is not null)
            {
                return selectedExit;
            }
        }

        if (worldPosition is not { } sourceWorldPosition)
        {
            return null;
        }

        RandomDungeonExitInfo closestExit = null;
        var closestDistanceSquared = MaxDungeonExitAssociationDistanceSquared;

        foreach (var exit in _discoveredRandomDungeonExits)
        {
            if (exit.SourceClusterIndex != sourceClusterIndex
                || exit.SourceExitPosition is not { } sourceExitPosition
                || !IsCompatibleExit(exit, dungeon))
            {
                continue;
            }

            var distanceSquared = GetDistanceSquared(sourceWorldPosition, sourceExitPosition);
            if (distanceSquared > closestDistanceSquared)
            {
                continue;
            }

            closestExit = exit;
            closestDistanceSquared = distanceSquared;
        }

        return closestExit;
    }

    private static bool IsCompatibleExit(RandomDungeonExitInfo exit, DungeonBaseFragment dungeon)
    {
        var isMistsExit = exit.UniqueName.StartsWith("MISTS_", StringComparison.Ordinal);
        var isDragonAreaExit = exit.ResolvedDragonAreaPortalSize != DragonAreaPortalSize.Unknown;
        return dungeon switch
        {
            MistsFragment => isMistsExit,
            DragonAreaFragment => isDragonAreaExit,
            RandomDungeonFragment => !isMistsExit && !isDragonAreaExit,
            _ => false
        };
    }

    private static float GetDistanceSquared(WorldPosition first, WorldPosition second)
    {
        var deltaX = first.X - second.X;
        var deltaY = first.Y - second.Y;
        return deltaX * deltaX + deltaY * deltaY;
    }

    private void TryUpdateCurrentDungeonLevelFromLootChest(Guid currentDungeonGuid, int objectId, double combinedLootFactor)
    {
        var activeDungeon = mainWindowViewModel.DungeonBindings.Dungeons?
            .FirstOrDefault(x => x.GuidList.Contains(currentDungeonGuid) && x.Status == DungeonStatus.Active);

        if (activeDungeon is not RandomDungeonFragment { IsLevelLockedFromEntrance: false } randomDungeon)
        {
            return;
        }

        var lootChest = randomDungeon.Events?.FirstOrDefault(x => x.Id == objectId);
        if (lootChest is null || !DungeonData.IsRandomDungeonLootChest(lootChest.UniqueName, randomDungeon.Mode))
        {
            return;
        }

        var level = DungeonData.GetDungeonLevelFromLootFactor(combinedLootFactor, randomDungeon.ZoneLootFactor);
        if (!randomDungeon.TrySetLevelFromLootFactor(level))
        {
            return;
        }

        UpdateCurrentMapHistoryRandomDungeonInformation(randomDungeon);
    }

    private void TryUpdateCurrentDungeonLevelFromMob(Guid currentDungeonGuid, int mobIndex, double hitPointsMax)
    {
        var activeDungeon = mainWindowViewModel.DungeonBindings.Dungeons?
            .FirstOrDefault(x => x.GuidList.Contains(currentDungeonGuid) && x.Status == DungeonStatus.Active);

        if (activeDungeon is not RandomDungeonFragment { IsLevelLockedFromEntrance: false } randomDungeon)
        {
            return;
        }

        var level = MobsData.GetRandomDungeonMobLevelByIndex(mobIndex, hitPointsMax, randomDungeon.MobHitPointsFactor);
        if (!randomDungeon.TrySetLevelFromMob(level))
        {
            return;
        }

        UpdateCurrentMapHistoryRandomDungeonInformation(randomDungeon);
    }

    public void ClearRandomDungeonExits()
    {
        lock (_discoveredRandomDungeonExits)
        {
            _discoveredRandomDungeonExits.Clear();
            _selectedRandomDungeonExitObjectId = null;
        }
    }

    #endregion

    #region Tier recognize

    public async Task AddTierToCurrentDungeonAsync(int? mobIndex)
    {
        if (!IsTrackingActive || _currentGuid is not { } currentGuid)
        {
            return;
        }

        if (mobIndex is null || ClusterController.CurrentCluster.Guid != currentGuid)
        {
            return;
        }

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dun = mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
                if (dun == null)
                {
                    return;
                }

                var mobTier = GetTierFromMob(dun.MapType, (int) mobIndex);
                if (mobTier == Tier.Unknown)
                {
                    return;
                }

                if (dun.MapType == MapType.RandomDungeon)
                {
                    var previousTier = dun.Tier;
                    SetRandomDungeonTier(dun, mobTier);
                    if (dun.Tier != previousTier && dun is RandomDungeonFragment randomDungeon)
                    {
                        UpdateCurrentMapHistoryRandomDungeonInformation(randomDungeon);
                    }

                    return;
                }

                dun.SetTier(mobTier);
            });
        }
        catch
        {
            // ignored
        }
    }

    private static Tier GetTierFromMob(MapType mapType, int mobIndex)
    {
        return mapType switch
        {
            MapType.RandomDungeon => (Tier) MobsData.GetRandomDungeonMobTierByIndex(mobIndex),
            _ => (Tier) MobsData.GetMobTierByIndex(mobIndex)
        };
    }

    private static void SetRandomDungeonTier(DungeonBaseFragment dungeon, Tier mobTier)
    {
        if (dungeon is RandomDungeonFragment randomDungeon)
        {
            randomDungeon.TrySetTierFromMob(mobTier);
        }
    }

    #endregion

    #region Dungeon loot tracking

    private ItemContainerObject _currentItemContainer;

    public void SetCurrentItemContainer(ItemContainerObject itemContainerObject)
    {
        _currentItemContainer = itemContainerObject;
    }

    public void SetLootSource(long objectId, string name, DungeonLootSourceType type)
    {
        if (objectId <= 0)
        {
            return;
        }

        _lootSources[objectId] = new DungeonLootSource
        {
            ObjectId = objectId,
            Name = name ?? string.Empty,
            Type = type
        };
    }


    public void AddDiscoveredItem(DiscoveredItem discoveredItem)
    {
        if (_discoveredLoot.Any(x => x?.ObjectId == discoveredItem?.ObjectId))
        {
            return;
        }

        if (_currentGuid == null)
        {
            return;
        }

        _discoveredLoot.Add(discoveredItem);
    }

    public async Task AddNewLocalPlayerLootOnCurrentDungeonAsync(int containerSlot, Guid containerGuid, Guid userInteractGuid)
    {
        if (!IsLocalPlayerCurrentContainerLoot(containerGuid, userInteractGuid))
        {
            return;
        }

        var itemObjectId = GetItemObjectIdFromContainer(containerSlot);
        var lootedItem = _discoveredLoot.FirstOrDefault(x => x.ObjectId == itemObjectId);

        if (lootedItem == null)
        {
            return;
        }

        await AddLocalPlayerLootedItemToCurrentDungeonAsync(lootedItem, GetCurrentLootSource());
    }

    public async Task AddNewLocalPlayerLootOnCurrentDungeonAsync(IReadOnlyCollection<long> itemObjectIds, Guid containerGuid, Guid userInteractGuid)
    {
        if (itemObjectIds?.Count <= 0 || !IsLocalPlayerCurrentContainerLoot(containerGuid, userInteractGuid))
        {
            return;
        }

        var currentContainerItemIds = GetCurrentContainerItemObjectIds();
        foreach (var itemObjectId in itemObjectIds.Distinct())
        {
            if (!currentContainerItemIds.Contains(itemObjectId))
            {
                continue;
            }

            var lootedItem = _discoveredLoot.FirstOrDefault(x => x.ObjectId == itemObjectId);
            if (lootedItem == null)
            {
                continue;
            }

            await AddLocalPlayerLootedItemToCurrentDungeonAsync(lootedItem, GetCurrentLootSource());
        }
    }

    private bool IsLocalPlayerCurrentContainerLoot(Guid containerGuid, Guid userInteractGuid)
    {
        if (trackingController.EntityController.LocalUserData.InteractGuid != userInteractGuid)
        {
            return false;
        }

        if (_currentItemContainer?.ContainerGuid != containerGuid)
        {
            return false;
        }

        return true;
    }

    private HashSet<long> GetCurrentContainerItemObjectIds()
    {
        if (_currentItemContainer?.SlotItemIds?.Count is null or <= 0)
        {
            return [];
        }

        return _currentItemContainer.SlotItemIds.ToHashSet();
    }

    private long GetItemObjectIdFromContainer(int containerSlot)
    {
        if (_currentItemContainer == null || _currentItemContainer?.SlotItemIds?.Count is null or <= 0 || _currentItemContainer?.SlotItemIds?.Count <= containerSlot)
        {
            return 0;
        }

        return _currentItemContainer!.SlotItemIds![containerSlot];
    }

    private DungeonLootSource GetCurrentLootSource()
    {
        if (_currentItemContainer == null)
        {
            return DungeonLootSource.Unknown;
        }

        return _currentItemContainer.ObjectId is { } objectId
               && _lootSources.TryGetValue(objectId, out var source)
            ? source
            : DungeonLootSource.Unknown;
    }

    public async Task AddLocalPlayerLootedItemToCurrentDungeonAsync(DiscoveredItem discoveredItem, DungeonLootSource source)
    {
        if (_currentGuid == null)
        {
            return;
        }

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var dun = GetDungeon((Guid) _currentGuid);
                if (dun == null)
                {
                    return;
                }

                var uniqueItemName = ItemController.GetUniqueNameByIndex(discoveredItem.ItemIndex);
                if (uniqueItemName.Contains("SILVERBAG"))
                {
                    return;
                }

                dun.Loot.Add(new Loot()
                {
                    EstimatedMarketValueInternal = discoveredItem.EstimatedMarketValueInternal,
                    Quantity = discoveredItem.Quantity,
                    UniqueName = uniqueItemName,
                    UtcDiscoveryTime = discoveredItem.UtcDiscoveryTime,
                    SourceObjectId = source.ObjectId,
                    SourceName = source.Name,
                    SourceType = source.Type
                });
            });
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public void ResetLocalPlayerDiscoveredLoot()
    {
        _discoveredLoot.Clear();
        _lootSources.Clear();
        _pendingAbyssalDepthsChests.Clear();
    }

    #endregion

    #region Expedition

    public async Task UpdateCheckPointAsync(CheckPoint checkPoint)
    {
        if (!IsTrackingActive || _currentGuid is not { } currentGuid)
        {
            return;
        }


        if (ClusterController.CurrentCluster.MapType != MapType.Expedition)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dun = mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
            if (dun is not ExpeditionFragment expedition)
            {
                return;
            }

            var foundCheckPoint = expedition.CheckPoints?.FirstOrDefault(x => x.Id == checkPoint.Id);
            if (foundCheckPoint is null)
            {
                expedition.CheckPoints?.Add(checkPoint);
            }
            else
            {
                foundCheckPoint.Status = checkPoint.Status;
            }

        });
    }

    #endregion

    #region Map history

    private void UpdateCurrentMapHistoryRandomDungeonInformation(RandomDungeonFragment randomDungeon)
    {
        trackingController.ClusterController.UpdateCurrentMapHistoryRandomDungeonInformation(randomDungeon.Tier, randomDungeon.Level);
    }

    #endregion

    #region Helper methods

    private bool ExistDungeon(Guid? mapGuid)
    {
        return mapGuid != null && mainWindowViewModel.DungeonBindings.Dungeons.Any(x => x.GuidList.Contains((Guid) mapGuid));
    }

    private static bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
    {
        return mapGuid != null && mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition or MapType.Mists or MapType.MistsDungeon or MapType.AbyssalDepths or MapType.DragonArea or MapType.StaticDungeon;
    }

    #endregion

    #region Load / Save file data

    public async Task LoadDungeonFromFileAsync()
    {
        var dungeons = await FileController.LoadAsync<List<DungeonDto>>(
            AppDataPaths.UserDataFile(Settings.Default.DungeonRunsFileName));

        var dungeonsToAdd = new List<DungeonBaseFragment>();
        var expirationThreshold = DateTime.UtcNow.AddYears(-DungeonRetentionYears);
        var expiredDungeonCount = 0;
        var invalidDungeonCount = 0;
        foreach (DungeonDto dungeonDto in dungeons)
        {
            if (dungeonDto.EnterDungeonFirstTime < expirationThreshold)
            {
                expiredDungeonCount++;
                continue;
            }

            if (!DungeonMapping.TryMapping(dungeonDto, out var dungeon))
            {
                invalidDungeonCount++;
                continue;
            }

            dungeonsToAdd.Add(dungeon);
        }

        if (invalidDungeonCount > 0)
        {
            Log.Warning("Skipped {invalidDungeonCount} invalid dungeon records while loading user data.", invalidDungeonCount);
        }

        mainWindowViewModel.DungeonBindings.Dungeons.Clear();
        mainWindowViewModel.DungeonBindings.Dungeons.AddRange(dungeonsToAdd.OrderBy(x => x?.EnterDungeonFirstTime).ToList());

        if (expiredDungeonCount > 0)
        {
            Log.Information("Deleted {expiredDungeonCount} dungeon records older than {retentionYears} years.", expiredDungeonCount, DungeonRetentionYears);
            await SaveInFileAsync();
        }
    }

    public async Task SaveInFileAsync()
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            return;
        }

        var toSaveDungeons = await CreateSaveSnapshotAsync();
        var path = AppDataPaths.UserDataFile(Settings.Default.DungeonRunsFileName);
        if (await FileController.SaveAsync(toSaveDungeons, path))
        {
            Log.Information("Dungeons saved. Count: {dungeonCount}", toSaveDungeons.Count);
        }
        else
        {
            Log.Warning("Dungeons could not be saved.");
        }
    }

    private async Task<List<DungeonDto>> CreateSaveSnapshotAsync()
    {
        var preparedSnapshot = GetPreparedShutdownSaveSnapshot();
        if (preparedSnapshot is not null)
        {
            return preparedSnapshot;
        }

        List<DungeonDto> snapshot = null;

        void CreateSnapshot()
        {
            snapshot = CreateSaveSnapshot();
        }

        if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            await dispatcher.InvokeAsync(CreateSnapshot);
        }
        else
        {
            CreateSnapshot();
        }

        return snapshot;
    }

    public void PrepareShutdownSaveSnapshot()
    {
        var snapshot = CreateSaveSnapshot();
        lock (_saveSnapshotLock)
        {
            _preparedShutdownSaveSnapshot = snapshot;
        }
    }

    private List<DungeonDto> GetPreparedShutdownSaveSnapshot()
    {
        lock (_saveSnapshotLock)
        {
            return _preparedShutdownSaveSnapshot;
        }
    }

    private List<DungeonDto> CreateSaveSnapshot()
    {
        return mainWindowViewModel.DungeonBindings.Dungeons
            .Where(x => x.Status == DungeonStatus.Done)
            .Select(DungeonMapping.Mapping)
            .ToList();
    }

    private async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_addDungeonCounter < limit)
        {
            return;
        }

        await SaveInFileAsync();
        _addDungeonCounter = 0;
    }

    #endregion
}