using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Loot = StatisticsAnalysisTool.Dungeon.Models.Loot;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
// ReSharper disable PossibleMultipleEnumeration

namespace StatisticsAnalysisTool.Dungeon;

public sealed class DungeonController
{
    private const int MaxDungeons = 9999;
    private const int NumberOfDungeonsUntilSaved = 1;
    private const int DungeonRetentionYears = 2;

    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private Guid? _currentGuid;
    private Guid? _lastMapGuid;
    private int _addDungeonCounter;
    private readonly List<DiscoveredItem> _discoveredLoot = [];
    private readonly Dictionary<long, DungeonLootSource> _lootSources = [];
    private readonly List<RandomDungeonExitInfo> _discoveredRandomDungeonExits = [];
    private int? _selectedRandomDungeonExitObjectId;

    public DungeonController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        if (_mainWindowViewModel?.DungeonBindings?.Dungeons != null)
        {
            _mainWindowViewModel.DungeonBindings.Dungeons.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _ = _mainWindowViewModel?.DungeonBindings?.UpdateFilteredDungeonsAsync();
    }

    public async Task AddDungeonAsync(MapType mapType, Guid? mapGuid, string sourceClusterIndex, WorldPosition? sourceExitPosition)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        UpdateDungeonSaveTimerUi();

        _currentGuid = mapGuid;

        // Last map is a dungeon, add new map
        if (IsDungeonCluster(mapType, mapGuid)
            && ExistDungeon(_lastMapGuid)
            && mapType is not MapType.CorruptedDungeon
            && mapType is not MapType.HellGate
            && mapType is not MapType.Mists
            && mapType is not MapType.MistsDungeon
            && mapType is not MapType.DragonArea)
        {
            if (AddClusterToExistDungeon(mapGuid, _lastMapGuid, out var currentDungeon))
            {
                currentDungeon.AddTimer(DateTime.UtcNow);
            }
        }
        // Add new dungeon
        else if (IsDungeonCluster(mapType, mapGuid)
                 && !ExistDungeon(_lastMapGuid)
                 && !ExistDungeon(_currentGuid)
                 || (IsDungeonCluster(mapType, mapGuid)
                 && mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon or MapType.AbyssalDepths or MapType.DragonArea))
        {
            UpdateDungeonSaveTimerUi(mapType);

            if (mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon or MapType.AbyssalDepths or MapType.DragonArea)
            {
                var lastDungeon = GetDungeon(_lastMapGuid);
                lastDungeon?.EndTimer();
            }

            _mainWindowViewModel.DungeonBindings.Dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

            var newDungeon = CreateNewDungeon(mapType, ClusterController.CurrentCluster.SourceClusterIndex, mapGuid);
            newDungeon.PartySize = Math.Max(1, _mainWindowViewModel.PartyBindings.Party.Count);
            UpdateCurrentDungeonLevel(newDungeon, sourceClusterIndex, sourceExitPosition);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel.DungeonBindings.Dungeons.Insert(0, newDungeon);
            });
        }
        // Activate exist dungeon again
        else if (IsDungeonCluster(mapType, mapGuid)
                 && !ExistDungeon(_lastMapGuid)
                 && ExistDungeon(_currentGuid)
                 || IsDungeonCluster(mapType, mapGuid)
                 && mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon or MapType.AbyssalDepths or MapType.DragonArea)
        {
            UpdateDungeonSaveTimerUi(mapType);

            var currentDungeon = GetDungeon(_currentGuid);
            currentDungeon.Status = DungeonStatus.Active;
            currentDungeon.AddTimer(DateTime.UtcNow);
        }
        // Make last dungeon done
        else if (mapGuid == null && ExistDungeon(_lastMapGuid))
        {
            ClearRandomDungeonExits();
            var lastDungeon = GetDungeon(_lastMapGuid);
            lastDungeon.EndTimer();
            lastDungeon.Status = DungeonStatus.Done;
            await SaveInFileAfterExceedingLimit(NumberOfDungeonsUntilSaved);
        }

        _lastMapGuid = mapGuid;

        await RemoveDungeonsAfterCertainNumberAsync(_mainWindowViewModel.DungeonBindings.Dungeons, MaxDungeons);
        await _mainWindowViewModel.DungeonBindings.UpdateFilteredDungeonsAsync();
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
            default:
                newDungeon = null;
                break;
        }

        return newDungeon;
    }

    public void ResetDungeons()
    {
        _mainWindowViewModel.DungeonBindings.Dungeons.Clear();
        Application.Current.Dispatcher.Invoke(() => { _mainWindowViewModel?.DungeonBindings?.Dungeons?.Clear(); });
    }

    public void ResetDungeonsByDateAscending(DateTime date)
    {
        var dungeonsToDelete = _mainWindowViewModel.DungeonBindings.Dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? [])
        {
            _mainWindowViewModel.DungeonBindings.Dungeons?.Remove(dungeonObject);
        }

        var trackingDungeonsToDelete = _mainWindowViewModel?.DungeonBindings?.Dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in trackingDungeonsToDelete ?? [])
        {
            _mainWindowViewModel?.DungeonBindings?.Dungeons?.Remove(dungeonObject);
        }
    }

    public void DeleteDungeonsWithZeroFame()
    {
        var dungeonsToDelete = _mainWindowViewModel.DungeonBindings.Dungeons?.Where(x => x.Fame <= 0 && x.Status != DungeonStatus.Active).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? [])
        {
            _mainWindowViewModel.DungeonBindings.Dungeons?.Remove(dungeonObject);
        }
    }

    public void RemoveDungeon(string dungeonHash)
    {
        var dungeon = _mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.DungeonHash.Contains(dungeonHash));

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

        _ = _mainWindowViewModel.DungeonBindings.Dungeons.Remove(dungeon);
    }

    private async Task RemoveDungeonsAfterCertainNumberAsync(ICollection<DungeonBaseFragment> dungeons, int dungeonLimit)
    {
        try
        {
            var toDelete = dungeons?.Count - dungeonLimit;

            if (toDelete <= 0)
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                for (var i = toDelete; i <= 0; i--)
                {
                    var dateTime = GetLowestDate(dungeons);
                    if (dateTime == null)
                    {
                        continue;
                    }

                    var removableItem = dungeons?.FirstOrDefault(x => x.EnterDungeonFirstTime == dateTime);
                    dungeons?.Remove(removableItem);
                }

                await _mainWindowViewModel.DungeonBindings.UpdateFilteredDungeonsAsync();
            });
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public async Task RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash)
    {
        await foreach (var dungeons in _mainWindowViewModel.DungeonBindings.Dungeons.ToList().ToAsyncEnumerable())
        {
            if (dungeonHash.Contains(dungeons.DungeonHash))
            {
                _mainWindowViewModel.DungeonBindings.Dungeons.Remove(dungeons);
            }
        }

        await SaveInFileAsync();
    }

    private bool AddClusterToExistDungeon(Guid? currentGuid, Guid? lastGuid, out DungeonBaseFragment dungeon)
    {
        if (currentGuid != null && lastGuid != null && _mainWindowViewModel.DungeonBindings.Dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) != true)
        {
            var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid) lastGuid));
            dun?.GuidList.Add((Guid) currentGuid);

            dungeon = dun;

            return _mainWindowViewModel.DungeonBindings.Dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) ?? false;
        }

        dungeon = null;
        return false;
    }

    public static DateTime? GetLowestDate(IEnumerable<DungeonBaseFragment> items)
    {
        if (items?.Count() <= 0)
        {
            return null;
        }

        try
        {
            return items?.Select(x => x.EnterDungeonFirstTime).Min();
        }
        catch (ArgumentNullException e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    #region Dungeon object

    public void SetDungeonChestOpen(int id, List<Guid> allowedToOpen)
    {
        if (!_trackingController.EntityController.IsAnyEntityInParty(allowedToOpen))
        {
            return;
        }

        if (_currentGuid != null)
        {
            try
            {
                var dun = GetDungeon((Guid) _currentGuid);
                var chest = dun?.Events?.FirstOrDefault(x => x?.Id == id);
                if (chest != null)
                {
                    chest.Status = ChestStatus.Open;
                    chest.Opened = DateTime.UtcNow;
                }
            }
            catch (Exception e)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    private DungeonBaseFragment GetDungeon(Guid? guid)
    {
        return guid == null ? null : _mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid) guid));
    }

    public async Task SetDungeonEventInformationAsync(int id, string uniqueName)
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

            var eventObject = new PointOfInterest(id, uniqueName);
            await Application.Current.Dispatcher.InvokeAsync(() => { dun.Events?.Add(eventObject); });

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
        var dungeons = _mainWindowViewModel.DungeonBindings.Dungeons;
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
        try
        {
            lock (_mainWindowViewModel.DungeonBindings.Dungeons)
            {
                var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => _currentGuid != null && x.GuidList.Contains((Guid) _currentGuid) && x.Status == DungeonStatus.Active);

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
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    public void SetDiedIfInDungeon(DiedObject dieObject)
    {
        if (_currentGuid == null || _trackingController.EntityController.LocalUserData.Username == null)
        {
            return;
        }

        var dungeon = _mainWindowViewModel.DungeonBindings.Dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid) _currentGuid));

        if (dungeon is null)
        {
            return;
        }

        if (dieObject.DiedName == _trackingController.EntityController.LocalUserData.Username)
        {
            dungeon.KillStatus = KillStatus.LocalPlayerDead;
        }
        else if (dieObject.KilledBy == _trackingController.EntityController.LocalUserData.Username)
        {
            dungeon.KillStatus = KillStatus.OpponentDead;
        }

        dungeon.DiedName = dieObject.DiedName;
        dungeon.KilledBy = dieObject.KilledBy;
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
            _selectedRandomDungeonExitObjectId = _discoveredRandomDungeonExits.Any(x => x.ObjectId == objectId)
                ? objectId
                : null;
        }
    }

    public void UpdateCurrentDungeonLevel(DungeonBaseFragment dungeon, string sourceClusterIndex, WorldPosition? worldPosition)
    {
        if (dungeon is not RandomDungeonFragment randomDungeon)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sourceClusterIndex))
        {
            return;
        }

        lock (_discoveredRandomDungeonExits)
        {
            var discoveredRandomDungeonExit = FindDiscoveredRandomDungeonExit(sourceClusterIndex, worldPosition);
            _selectedRandomDungeonExitObjectId = null;
            if (discoveredRandomDungeonExit is null)
            {
                return;
            }

            randomDungeon.MobHitPointsFactor = DungeonData.GetDungeonMobHitPointsFactor(discoveredRandomDungeonExit.DungeonType);
            randomDungeon.ZoneLootFactor = DungeonData.GetDungeonZoneLootFactor(discoveredRandomDungeonExit.DungeonType);
            randomDungeon.TrySetTierFromEntrance(DungeonData.GetDungeonTierFromExit(discoveredRandomDungeonExit.UniqueName));

            var dungeonMode = DungeonData.GetRandomDungeonModeFromExit(discoveredRandomDungeonExit.UniqueName);
            if (dungeonMode != DungeonMode.Unknown)
            {
                randomDungeon.Mode = dungeonMode;
            }

            var faction = DungeonData.GetFaction(discoveredRandomDungeonExit.UniqueName);
            if (faction != Faction.Unknown)
            {
                randomDungeon.Faction = faction;
            }

            if (discoveredRandomDungeonExit.HasVisibleLevel)
            {
                randomDungeon.TrySetLevelFromEntrance(discoveredRandomDungeonExit.Level);
            }
        }
    }

    public void UpdateCurrentDungeonLevel(int? mobIndex, double hitPointsMax)
    {
        if (_currentGuid is not { } currentDungeonGuid || mobIndex is null)
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
        if (_currentGuid is not { } currentDungeonGuid)
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

    private RandomDungeonExitInfo FindDiscoveredRandomDungeonExit(string sourceClusterIndex, WorldPosition? worldPosition)
    {
        if (_selectedRandomDungeonExitObjectId is { } selectedObjectId)
        {
            var selectedExit = _discoveredRandomDungeonExits.FirstOrDefault(x =>
                x.ObjectId == selectedObjectId && x.SourceClusterIndex == sourceClusterIndex);
            if (selectedExit is not null)
            {
                return selectedExit;
            }
        }

        if (worldPosition is not { } sourceWorldPosition)
        {
            return null;
        }

        return _discoveredRandomDungeonExits.FirstOrDefault(x =>
            x.SourceClusterIndex == sourceClusterIndex
            && x.SourceExitPosition is { } sourceExitPosition
            && sourceExitPosition.X.Equals(sourceWorldPosition.X)
            && sourceExitPosition.Y.Equals(sourceWorldPosition.Y));
    }

    private void TryUpdateCurrentDungeonLevelFromLootChest(Guid currentDungeonGuid, int objectId, double combinedLootFactor)
    {
        var activeDungeon = _mainWindowViewModel.DungeonBindings.Dungeons?
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
        var activeDungeon = _mainWindowViewModel.DungeonBindings.Dungeons?
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
        if (_currentGuid is not { } currentGuid)
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
                var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
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
        if (_trackingController.EntityController.LocalUserData.InteractGuid != userInteractGuid)
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
    }

    #endregion

    #region Dungeon timer

    private void UpdateDungeonSaveTimerUi(MapType mapType = MapType.Unknown)
    {
        _mainWindowViewModel.DungeonBindings.DungeonCloseTimer.Visibility = mapType == MapType.RandomDungeon ? Visibility.Visible : Visibility.Collapsed;
    }

    #endregion

    #region Expedition

    public async Task UpdateCheckPointAsync(CheckPoint checkPoint)
    {
        if (_currentGuid is not { } currentGuid)
        {
            return;
        }


        if (ClusterController.CurrentCluster.MapType != MapType.Expedition)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
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
        _trackingController.ClusterController.UpdateCurrentMapHistoryRandomDungeonInformation(randomDungeon.Tier, randomDungeon.Level);
    }

    #endregion

    #region Helper methods

    private bool ExistDungeon(Guid? mapGuid)
    {
        return mapGuid != null && _mainWindowViewModel.DungeonBindings.Dungeons.Any(x => x.GuidList.Contains((Guid) mapGuid));
    }

    private static bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
    {
        return mapGuid != null && mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition or MapType.Mists or MapType.MistsDungeon or MapType.AbyssalDepths or MapType.DragonArea;
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

        _mainWindowViewModel.DungeonBindings.Dungeons.Clear();
        _mainWindowViewModel.DungeonBindings.Dungeons.AddRange(dungeonsToAdd.OrderBy(x => x?.EnterDungeonFirstTime).ToList());
        _mainWindowViewModel.DungeonBindings.InitListCollectionView();

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

        var toSaveDungeons = _mainWindowViewModel.DungeonBindings.Dungeons.Select(DungeonMapping.Mapping).ToList();
        await FileController.SaveAsync(toSaveDungeons, AppDataPaths.UserDataFile(Settings.Default.DungeonRunsFileName));
        Log.Information("Dungeons saved");
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