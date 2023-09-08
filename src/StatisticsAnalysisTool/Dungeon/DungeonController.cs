using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Loot = StatisticsAnalysisTool.Dungeon.Models.Loot;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
// ReSharper disable PossibleMultipleEnumeration

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonController
{
    private const int MaxDungeons = 9999;
    private const int NumberOfDungeonsUntilSaved = 1;

    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private Guid? _currentGuid;
    private Guid? _lastMapGuid;
    private int _addDungeonCounter;
    private readonly List<DiscoveredItem> _discoveredLoot = new();
    private ObservableCollection<Guid> _lastGuidWithRecognizedLevel = new();

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
        _mainWindowViewModel?.DungeonBindings?.Stats.Set(_mainWindowViewModel?.DungeonBindings?.Dungeons);
    }

    public async Task AddDungeonAsync(MapType mapType, Guid? mapGuid)
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
            && mapType is not MapType.MistsDungeon)
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
                 && mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon))
        {
            UpdateDungeonSaveTimerUi(mapType);

            if (mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon)
            {
                var lastDungeon = GetDungeon(_lastMapGuid);
                lastDungeon?.EndTimer();
            }

            _mainWindowViewModel.DungeonBindings.Dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

            var newDungeon = CreateNewDungeon(mapType, ClusterController.CurrentCluster.MainClusterIndex, mapGuid);
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
                 && mapType is MapType.CorruptedDungeon or MapType.HellGate or MapType.Mists or MapType.MistsDungeon)
        {
            UpdateDungeonSaveTimerUi(mapType);

            var currentDungeon = GetDungeon(_currentGuid);
            currentDungeon.Status = DungeonStatus.Active;
            currentDungeon.AddTimer(DateTime.UtcNow);
        }
        // Make last dungeon done
        else if (mapGuid == null && ExistDungeon(_lastMapGuid))
        {
            var lastDungeon = GetDungeon(_lastMapGuid);
            lastDungeon.EndTimer();
            lastDungeon.Status = DungeonStatus.Done;
            await SaveInFileAfterExceedingLimit(NumberOfDungeonsUntilSaved);
            _lastGuidWithRecognizedLevel = new ObservableCollection<Guid>();
        }

        _lastMapGuid = mapGuid;

        await RemoveDungeonsAfterCertainNumberAsync(_mainWindowViewModel.DungeonBindings.Dungeons, MaxDungeons);
        await Application.Current.Dispatcher.InvokeAsync(_mainWindowViewModel.DungeonBindings.UpdateFilteredDungeonsAsync);
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
                var dungeonMode = DungeonData.GetDungeonMode(mainMapIndex);
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
        foreach (var dungeonObject in dungeonsToDelete ?? new List<DungeonBaseFragment>())
        {
            _mainWindowViewModel.DungeonBindings.Dungeons?.Remove(dungeonObject);
        }

        var trackingDungeonsToDelete = _mainWindowViewModel?.DungeonBindings?.Dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in trackingDungeonsToDelete ?? new List<DungeonBaseFragment>())
        {
            _mainWindowViewModel?.DungeonBindings?.Dungeons?.Remove(dungeonObject);
        }
    }

    public void DeleteDungeonsWithZeroFame()
    {
        var dungeonsToDelete = _mainWindowViewModel.DungeonBindings.Dungeons?.Where(x => x.Fame <= 0 && x.Status != DungeonStatus.Active).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? new List<DungeonBaseFragment>())
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

        var dialog = new DialogWindow(LanguageController.Translation("REMOVE_DUNGEON"), LanguageController.Translation("SURE_YOU_WANT_TO_REMOVE_DUNGEON"));
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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    #region Dungeon object

    public void SetDungeonChestOpen(int id)
    {
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
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
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

    #region Tier / Level recognize

    public void AddLevelToCurrentDungeon(int? mobIndex, double hitPointsMax)
    {
        if (_currentGuid is not { } currentGuid)
        {
            return;
        }

        if (_lastGuidWithRecognizedLevel.Contains(currentGuid))
        {
            return;
        }

        if (mobIndex is null || ClusterController.CurrentCluster.Guid != currentGuid)
        {
            return;
        }

        if (ClusterController.CurrentCluster.MapType != MapType.Expedition
            && ClusterController.CurrentCluster.MapType != MapType.CorruptedDungeon
            && ClusterController.CurrentCluster.MapType != MapType.HellGate
            && ClusterController.CurrentCluster.MapType != MapType.RandomDungeon
            && ClusterController.CurrentCluster.MapType != MapType.Mists
            && ClusterController.CurrentCluster.MapType != MapType.MistsDungeon)
        {
            return;
        }

        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
                if (dun is not RandomDungeonFragment randomDungeon)
                {
                    return;
                }

                randomDungeon.Level = randomDungeon.Level < 0 ? MobsData.GetMobLevelByIndex((int) mobIndex, hitPointsMax) : randomDungeon.Level;

                if (randomDungeon.Level > 0)
                {
                    _lastGuidWithRecognizedLevel = dun.GuidList;
                }
            });
        }
        catch
        {
            // ignored
        }
    }

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

        if (ClusterController.CurrentCluster.MapType != MapType.Expedition
            && ClusterController.CurrentCluster.MapType != MapType.CorruptedDungeon
            && ClusterController.CurrentCluster.MapType != MapType.HellGate
            && ClusterController.CurrentCluster.MapType != MapType.RandomDungeon)
        {
            return;
        }

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mobTier = (Tier) MobsData.GetMobTierByIndex((int) mobIndex);
                var dun = _mainWindowViewModel.DungeonBindings.Dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
                if (dun == null || dun.Tier >= mobTier)
                {
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

    #endregion

    #region Dungeon loot tracking

    private ItemContainerObject _currentItemContainer;

    public void SetCurrentItemContainer(ItemContainerObject itemContainerObject)
    {
        _currentItemContainer = itemContainerObject;
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
        if (_trackingController.EntityController.LocalUserData.InteractGuid != userInteractGuid)
        {
            return;
        }

        if (_currentItemContainer?.ContainerGuid != containerGuid)
        {
            return;
        }

        var itemObjectId = GetItemObjectIdFromContainer(containerSlot);
        var lootedItem = _discoveredLoot.FirstOrDefault(x => x.ObjectId == itemObjectId);

        if (lootedItem == null)
        {
            return;
        }

        await AddLocalPlayerLootedItemToCurrentDungeonAsync(lootedItem);
    }

    private long GetItemObjectIdFromContainer(int containerSlot)
    {
        if (_currentItemContainer == null || _currentItemContainer?.SlotItemIds?.Count is null or <= 0 || _currentItemContainer?.SlotItemIds?.Count <= containerSlot)
        {
            return 0;
        }

        return _currentItemContainer!.SlotItemIds![containerSlot];
    }

    public async Task AddLocalPlayerLootedItemToCurrentDungeonAsync(DiscoveredItem discoveredItem)
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

                dun.Loot.Add(new Loot()
                {
                    EstimatedMarketValueInternal = discoveredItem.EstimatedMarketValueInternal,
                    Quantity = discoveredItem.Quantity,
                    UniqueName = uniqueItemName,
                    UtcDiscoveryTime = discoveredItem.UtcDiscoveryTime
                });
                dun.UpdateTotalValue();
                dun.UpdateMostValuableLoot();
                dun.UpdateMostValuableLootVisibility();
            });
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    public void ResetLocalPlayerDiscoveredLoot()
    {
        _discoveredLoot.Clear();
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

    #region Helper methods

    private bool ExistDungeon(Guid? mapGuid)
    {
        return mapGuid != null && _mainWindowViewModel.DungeonBindings.Dungeons.Any(x => x.GuidList.Contains((Guid) mapGuid));
    }

    private static bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
    {
        return mapGuid != null && mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition or MapType.Mists or MapType.MistsDungeon;
    }

    #endregion

    #region Load / Save file data

    public async Task LoadDungeonFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.DungeonRunsFileName));
        var dungeons = await FileController.LoadAsync<List<DungeonDto>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DungeonRunsFileName));

        _mainWindowViewModel.DungeonBindings.Dungeons.AddRange(dungeons
            .Where(x => x.Mode != DungeonMode.Unknown).Select(DungeonMapping.Mapping)
            .OrderBy(x => x.EnterDungeonFirstTime)
            .ToList());
        _mainWindowViewModel.DungeonBindings.InitListCollectionView();
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        var toSaveDungeons = _mainWindowViewModel.DungeonBindings.Dungeons.Select(DungeonMapping.Mapping).ToList();
        await FileController.SaveAsync(toSaveDungeons, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.DungeonRunsFileName));
        Debug.Print("Dungeons saved");
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