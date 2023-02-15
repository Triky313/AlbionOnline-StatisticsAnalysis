using log4net;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
// ReSharper disable PossibleMultipleEnumeration

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonController
{
    private const int MaxDungeons = 9999;
    private const int NumberOfDungeonsUntilSaved = 1;

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly TrackingController _trackingController;
    private Guid? _currentGuid;
    private Guid? _lastMapGuid;
    private ObservableRangeCollection<DungeonObject> _dungeons = new();
    private int _addDungeonCounter;
    private readonly List<DiscoveredItem> _discoveredLoot = new();
    private List<Guid> _lastGuidWithRecognizedLevel = new();

    public DungeonController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
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
        if (IsDungeonCluster(mapType, mapGuid) && ExistDungeon(_lastMapGuid) && mapType != MapType.CorruptedDungeon && mapType != MapType.HellGate)
        {
            if (AddClusterToExistDungeon(_dungeons, mapGuid, _lastMapGuid, out var currentDungeon))
            {
                currentDungeon.AddTimer(DateTime.UtcNow);
            }
        }
        // Add new dungeon
        else if (IsDungeonCluster(mapType, mapGuid) && !ExistDungeon(_lastMapGuid) && !ExistDungeon(_currentGuid) || IsDungeonCluster(mapType, mapGuid) && mapType is MapType.CorruptedDungeon or MapType.HellGate)
        {
            UpdateDungeonSaveTimerUi(mapType);

            if (mapType is MapType.CorruptedDungeon or MapType.HellGate)
            {
                var lastDungeon = GetDungeon(_lastMapGuid);
                lastDungeon?.EndTimer();
            }

            _dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

            var newDungeon = new DungeonObject(ClusterController.CurrentCluster.MainClusterIndex,
                mapGuid ?? new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), DungeonStatus.Active);
            SetDungeonMapType(newDungeon, mapType);

            _dungeons.Insert(0, newDungeon);
        }
        // Activate exist dungeon again
        else if (IsDungeonCluster(mapType, mapGuid) && !ExistDungeon(_lastMapGuid) && ExistDungeon(_currentGuid) || IsDungeonCluster(mapType, mapGuid) && mapType is MapType.CorruptedDungeon or MapType.HellGate)
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
            lastDungeon.EndTimer();
            lastDungeon.Status = DungeonStatus.Done;
            await SaveInFileAfterExceedingLimit(NumberOfDungeonsUntilSaved);
            _lastGuidWithRecognizedLevel = new List<Guid>();
        }

        RemoveDungeonsAfterCertainNumber(_dungeons, MaxDungeons);

        _lastMapGuid = mapGuid;

        await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);

        UpdateDungeonStatsUi();
        UpdateDungeonChestsUi();
    }

    public void ResetDungeons()
    {
        _dungeons.Clear();
        Application.Current.Dispatcher.Invoke(() => { _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Clear(); });
    }

    public void ResetDungeonsByDateAscending(DateTime date)
    {
        var dungeonsToDelete = _dungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? new List<DungeonObject>())
        {
            _dungeons?.Remove(dungeonObject);
        }

        var trackingDungeonsToDelete = _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Where(x => x.EnterDungeonFirstTime >= date).ToList();
        foreach (var dungeonObject in trackingDungeonsToDelete ?? new List<DungeonNotificationFragment>())
        {
            _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Remove(dungeonObject);
        }
    }

    public void DeleteDungeonsWithZeroFame()
    {
        var dungeonsToDelete = _dungeons?.Where(x => x.Fame <= 0).ToList();
        foreach (var dungeonObject in dungeonsToDelete ?? new List<DungeonObject>())
        {
            _dungeons?.Remove(dungeonObject);
        }

        var trackingDungeonsToDelete = _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Where(x => x.Fame <= 0).ToList();
        foreach (var dungeonObject in trackingDungeonsToDelete ?? new List<DungeonNotificationFragment>())
        {
            _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Remove(dungeonObject);
        }
    }

    public async void RemoveDungeonAsync(string dungeonHash)
    {
        var dungeon = _dungeons.FirstOrDefault(x => x.DungeonHash.Contains(dungeonHash));

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

        _ = _dungeons.Remove(dungeon);
        await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);
    }

    private void RemoveDungeonsAfterCertainNumber(ObservableRangeCollection<DungeonObject> dungeons, int dungeonLimit)
    {
        try
        {
            var toDelete = dungeons?.Count - dungeonLimit;

            if (toDelete <= 0)
            {
                return;
            }

            for (var i = toDelete; i <= 0; i--)
            {
                var dateTime = GetLowestDate(dungeons);
                if (dateTime != null)
                {
                    var removableItem = dungeons.FirstOrDefault(x => x.EnterDungeonFirstTime == dateTime);
                    dungeons.Remove(removableItem);
                }
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static bool IsDungeonDifferenceToAnother(DungeonObject dungeonObject, DungeonNotificationFragment dungeonNotificationFragment)
    {
        return dungeonObject.TotalRunTimeInSeconds != dungeonNotificationFragment.TotalRunTimeInSeconds
               || !dungeonObject.GuidList.SequenceEqual(dungeonNotificationFragment.GuidList)
               || dungeonObject.DungeonEventObjects.Count != dungeonNotificationFragment.DungeonChestsFragments.Count
               || dungeonObject.Status != dungeonNotificationFragment.Status
               || Math.Abs(dungeonObject.Fame - dungeonNotificationFragment.Fame) > 0.0d
               || Math.Abs(dungeonObject.ReSpec - dungeonNotificationFragment.ReSpec) > 0.0d
               || Math.Abs(dungeonObject.Silver - dungeonNotificationFragment.Silver) > 0.0d
               || Math.Abs(dungeonObject.FactionCoins - dungeonNotificationFragment.FactionCoins) > 0.0d
               || Math.Abs(dungeonObject.FactionFlags - dungeonNotificationFragment.FactionFlags) > 0.0d
               || dungeonObject.DiedInDungeon != dungeonNotificationFragment.DiedInDungeon
               || dungeonObject.Faction != dungeonNotificationFragment.Faction
               || dungeonObject.Mode != dungeonNotificationFragment.Mode
               || dungeonObject.CityFaction != dungeonNotificationFragment.CityFaction;
    }

    private async Task RemoveLeftOverDungeonNotificationFragments()
    {
        await foreach (var dungeonFragment in _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.ToAsyncEnumerable().ConfigureAwait(false) ?? new ConfiguredCancelableAsyncEnumerable<DungeonNotificationFragment>())
        {
            var dungeonObjectFound = _dungeons.Select(x => x.DungeonHash).Contains(dungeonFragment.DungeonHash);
            if (dungeonObjectFound)
            {
                continue;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Remove(dungeonFragment);
            });
        }
    }

    public async Task RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash)
    {
        foreach (var dungeons in _dungeons)
        {
            if (dungeonHash.Contains(dungeons.DungeonHash))
            {
                _dungeons.Remove(dungeons);
            }
        }

        foreach (var dungeonFragment in _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.ToList() ?? new List<DungeonNotificationFragment>())
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Remove(dungeonFragment);
            });
        }

        await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);
    }

    private static bool AddClusterToExistDungeon(IEnumerable<DungeonObject> dungeons, Guid? currentGuid, Guid? lastGuid, out DungeonObject dungeon)
    {
        if (currentGuid != null && lastGuid != null && dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) != true)
        {
            var dun = dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid) lastGuid));
            dun?.GuidList.Add((Guid) currentGuid);

            dungeon = dun;

            return dungeons?.Any(x => x.GuidList.Contains((Guid) currentGuid)) ?? false;
        }

        dungeon = null;
        return false;
    }

    public static DateTime? GetLowestDate(IEnumerable<DungeonObject> items)
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
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            return null;
        }
    }

    private async Task DungeonUiFilteringAsync()
    {
        var dungeonStatsFilter = _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter;
        var modeFilter = dungeonStatsFilter?.DungeonModeFilters;
        var tierFilter = dungeonStatsFilter?.TierFilters;
        var levelFilter = dungeonStatsFilter?.LevelFilters;

        await _mainWindowViewModel?.DungeonBindings?.TrackingDungeons
            ?.Where(x =>
                !((modeFilter?.Contains(x.Mode) ?? false)
                  && (tierFilter?.Contains(x.Tier) ?? false)
                  && (levelFilter?.Contains((ItemLevel) x.Level) ?? x.Status != DungeonStatus.Active))
            )
            .ToAsyncEnumerable()
            .ForEachAsync(d =>
            {
                d.Visibility = Visibility.Collapsed;
            })!;

        await _mainWindowViewModel?.DungeonBindings?.TrackingDungeons
            ?.Where(x =>
            {
                if (x.Status == DungeonStatus.Active)
                {
                    return true;
                }

                return (modeFilter?.Contains(x.Mode) ?? false)
                       && (tierFilter?.Contains(x.Tier) ?? false)
                       && (levelFilter?.Contains((ItemLevel) x.Level) ?? false);
            })
            .ToAsyncEnumerable()
            .ForEachAsync(d =>
            {
                d.Visibility = Visibility.Visible;
            })!;
    }

    public async Task SetOrUpdateDungeonsDataUiAsync()
    {
        var orderedDungeon = _dungeons.OrderBy(x => x.EnterDungeonFirstTime).ToList();
        foreach (var dungeonObject in orderedDungeon)
        {
            var dungeonFragment = _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.FirstOrDefault(x => x.DungeonHash == dungeonObject.DungeonHash);
            if (dungeonFragment != null && IsDungeonDifferenceToAnother(dungeonObject, dungeonFragment))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    dungeonFragment.SetValues(dungeonObject);
                    dungeonFragment.DungeonNumber = orderedDungeon.IndexOf(dungeonObject);
                });
            }
            else if (dungeonFragment == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var index = orderedDungeon.IndexOf(dungeonObject);
                    var dunFragment = new DungeonNotificationFragment(index, dungeonObject.GuidList, dungeonObject.MainMapIndex, dungeonObject.EnterDungeonFirstTime);
                    dunFragment.SetValues(dungeonObject);
                    _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Insert(index, dunFragment);
                });
            }
        }

        await RemoveLeftOverDungeonNotificationFragments().ConfigureAwait(false);
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await SetBestDungeonTimeAsync(_mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.ToAsyncEnumerable());
            await CalculateBestDungeonValues(_mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.ToAsyncEnumerable());
        });

        await DungeonUiFilteringAsync();

        UpdateDungeonStatsUi();
        UpdateDungeonChestsUi();
    }

    private void UpdateDungeonDataUi(DungeonObject dungeon)
    {
        if (dungeon == null)
        {
            return;
        }

        var uiDungeon = GetCurrentUiDungeon(dungeon);
        uiDungeon?.SetValues(dungeon);
    }

    private DungeonNotificationFragment GetCurrentUiDungeon(DungeonObject dungeon)
    {
        return _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.FirstOrDefault(x =>
            x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime.Equals(dungeon.EnterDungeonFirstTime));
    }

    #region Dungeon object

    public void SetDungeonChestOpen(int id)
    {
        if (_currentGuid != null)
        {
            try
            {
                var dun = GetDungeon((Guid) _currentGuid);
                var chest = dun?.DungeonEventObjects?.FirstOrDefault(x => x.Id == id);

                if (chest == null)
                {
                    return;
                }

                chest.IsOpen = true;
                chest.Opened = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        UpdateDungeonChestsUi();
    }

    private DungeonObject GetDungeon(Guid? guid)
    {
        return guid == null ? null : _dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid) guid));
    }

    public async Task SetDungeonEventObjectInformationAsync(int id, string uniqueName)
    {
        if (_currentGuid != null && uniqueName != null)
        {
            try
            {
                var dun = GetDungeon((Guid) _currentGuid);
                if (dun == null || dun.DungeonEventObjects?.Any(x => x.Id == id) == true)
                {
                    return;
                }

                var eventObject = new DungeonEventObject()
                {
                    UniqueName = uniqueName,
                    IsBossChest = DungeonObjectData.IsBossChest(uniqueName),
                    Id = id
                };

                dun.DungeonEventObjects?.Add(eventObject);

                dun.Faction = DungeonObjectData.GetFaction(uniqueName);

                if (dun.Mode == DungeonMode.Unknown)
                {
                    dun.Mode = DungeonObjectData.GetDungeonMode(uniqueName);
                }

                await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }

    private static void SetDungeonMapType(DungeonObject dungeon, MapType mapType)
    {
        switch (mapType)
        {
            case MapType.CorruptedDungeon:
                dungeon.Faction = Faction.Corrupted;
                dungeon.Mode = DungeonMode.Corrupted;
                return;
            case MapType.HellGate:
                dungeon.Faction = Faction.HellGate;
                dungeon.Mode = DungeonMode.HellGate;
                return;
            case MapType.Expedition:
                dungeon.Mode = DungeonMode.Expedition;
                return;
            case MapType.RandomDungeon:
                break;
            case MapType.Island:
                break;
            case MapType.Hideout:
                break;
            case MapType.Arena:
                break;
            case MapType.Mists:
                break;
            case MapType.Unknown:
            default:
                return;
        }
    }

    public void AddValueToDungeon(double value, ValueType valueType, CityFaction cityFaction = CityFaction.Unknown)
    {
        try
        {
            lock (_dungeons)
            {
                var dun = _dungeons?.FirstOrDefault(x => _currentGuid != null && x.GuidList.Contains((Guid) _currentGuid) && x.Status == DungeonStatus.Active);
                dun?.Add(value, valueType, cityFaction);

                UpdateDungeonDataUi(dun);
            }
        }
        catch
        {
            // ignored
        }
    }

    public void SetDiedIfInDungeon(DiedObject dieObject)
    {
        if (_currentGuid != null && _trackingController.EntityController.LocalUserData.Username != null && dieObject.DiedName == _trackingController.EntityController.LocalUserData.Username)
        {
            try
            {
                var item = _dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid) _currentGuid) && x.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-1));

                if (item == null)
                {
                    return;
                }

                item.DiedName = dieObject.DiedName;
                item.KilledBy = dieObject.KilledBy;
                item.DiedInDungeon = true;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }

    #endregion

    #region Best values

    private static async Task CalculateBestDungeonValues(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
    {
        if (await dungeons.CountAsync() <= 0)
        {
            return;
        }

        await ResetAllBestValuesAsync(dungeons);

        var highestFame = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Fame: > 0 }).Select(x => x?.Fame).MaxAsync();
        var bestDungeonFame = await dungeons.FirstOrDefaultAsync(x => x.Fame.CompareTo(highestFame) == 0);

        if (bestDungeonFame != null)
        {
            bestDungeonFame.IsBestFame = true;
        }

        var highestReSpec = await dungeons.Where(x => x is { Status: DungeonStatus.Done, ReSpec: > 0 }).Select(x => x?.ReSpec).MaxAsync();
        var bestDungeonReSpec = await dungeons.FirstOrDefaultAsync(x => x.ReSpec.CompareTo(highestReSpec) == 0);

        if (bestDungeonReSpec != null)
        {
            bestDungeonReSpec.IsBestReSpec = true;
        }

        var highestSilver = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Silver: > 0 }).Select(x => x?.Silver).MaxAsync();
        var bestDungeonSilver = await dungeons.FirstOrDefaultAsync(x => x.Silver.CompareTo(highestSilver) == 0);

        if (bestDungeonSilver != null)
        {
            bestDungeonSilver.IsBestSilver = true;
        }

        var highestFactionFlags = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionFlags: > 0 }).Select(x => x?.FactionFlags).MaxAsync();
        var bestDungeonFlags = await dungeons.FirstOrDefaultAsync(x => x.FactionFlags.CompareTo(highestFactionFlags) == 0);

        if (bestDungeonFlags != null)
        {
            bestDungeonFlags.IsBestFactionFlags = true;
        }

        var highestFactionCoins = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionCoins: > 0 }).Select(x => x?.FactionCoins).MaxAsync();
        var bestDungeonCoins = await dungeons.FirstOrDefaultAsync(x => x.FactionCoins.CompareTo(highestFactionCoins) == 0);

        if (bestDungeonCoins != null)
        {
            bestDungeonCoins.IsBestFactionCoins = true;
        }

        var highestMight = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Might: > 0 }).Select(x => x?.Might).MaxAsync();
        var bestDungeonMight = await dungeons.FirstOrDefaultAsync(x => x.Might.CompareTo(highestMight) == 0);

        if (bestDungeonMight != null)
        {
            bestDungeonMight.IsBestMight = true;
        }

        var highestFavor = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Favor: > 0 }).Select(x => x?.Favor).MaxAsync();
        var bestDungeonFavor = await dungeons.FirstOrDefaultAsync(x => x.Favor.CompareTo(highestFavor) == 0);

        if (bestDungeonFavor != null)
        {
            bestDungeonFavor.IsBestMightPerHour = true;
        }

        var highestFamePerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FamePerHour: > 0 }).Select(x => x?.FamePerHour).MaxAsync();
        var bestDungeonFamePerHour = await dungeons.FirstOrDefaultAsync(x => x.FamePerHour.CompareTo(highestFamePerHour) == 0);

        if (bestDungeonFamePerHour != null)
        {
            bestDungeonFamePerHour.IsBestFamePerHour = true;
        }

        var highestReSpecPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, ReSpecPerHour: > 0 }).Select(x => x?.ReSpecPerHour).MaxAsync();
        var bestDungeonReSpecPerHour = await dungeons.FirstOrDefaultAsync(x => x.ReSpecPerHour.CompareTo(highestReSpecPerHour) == 0);

        if (bestDungeonReSpecPerHour != null)
        {
            bestDungeonReSpecPerHour.IsBestReSpecPerHour = true;
        }

        var highestSilverPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, SilverPerHour: > 0 }).Select(x => x?.SilverPerHour).MaxAsync();
        var bestDungeonSilverPerHour = await dungeons.FirstOrDefaultAsync(x => x.SilverPerHour.CompareTo(highestSilverPerHour) == 0);

        if (bestDungeonSilverPerHour != null)
        {
            bestDungeonSilverPerHour.IsBestSilverPerHour = true;
        }

        var highestFactionFlagsPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionFlagsPerHour: > 0 }).Select(x => x?.FactionFlagsPerHour).MaxAsync();
        var bestDungeonFactionFlagsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionFlagsPerHour.CompareTo(highestFactionFlagsPerHour) == 0);

        if (bestDungeonFactionFlagsPerHour != null)
        {
            bestDungeonFactionFlagsPerHour.IsBestFactionFlagsPerHour = true;
        }

        var highestFactionCoinsPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionCoinsPerHour: > 0 }).Select(x => x?.FactionCoinsPerHour).MaxAsync();
        var bestDungeonFactionCoinsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionCoinsPerHour.CompareTo(highestFactionCoinsPerHour) == 0);

        if (bestDungeonFactionCoinsPerHour != null)
        {
            bestDungeonFactionCoinsPerHour.IsBestFactionCoinsPerHour = true;
        }

        var highestMightPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, MightPerHour: > 0 }).Select(x => x?.MightPerHour).MaxAsync();
        var bestDungeonMightPerHour = await dungeons.FirstOrDefaultAsync(x => x.MightPerHour.CompareTo(highestMightPerHour) == 0);

        if (bestDungeonMightPerHour != null)
        {
            bestDungeonMightPerHour.IsBestMightPerHour = true;
        }

        var highestFavorPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FavorPerHour: > 0 }).Select(x => x?.FavorPerHour).MaxAsync();
        var bestDungeonFavorPerHour = await dungeons.FirstOrDefaultAsync(x => x.FavorPerHour.CompareTo(highestFavorPerHour) == 0);

        if (bestDungeonFavorPerHour != null)
        {
            bestDungeonFavorPerHour.IsBestFavorPerHour = true;
        }
    }

    private static async Task SetBestDungeonTimeAsync(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
    {
        if (await dungeons.CountAsync() <= 0)
        {
            return;
        }

        try
        {
            await dungeons.Where(x => x?.IsBestTime == true).ForEachAsync(x => x.IsBestTime = false).ConfigureAwait(false);
            var bestTime = await dungeons.Where(x => x?.DungeonChestsFragments?.Any(y => y?.IsBossChest ?? false) == true).MinAsync(x => x?.TotalRunTimeInSeconds).ConfigureAwait(false);
            var bestTimeDungeon = await dungeons.FirstOrDefaultAsync(x => x.TotalRunTimeInSeconds == bestTime);

            if (bestTimeDungeon != null)
            {
                bestTimeDungeon.IsBestTime = true;
            }
        }
        catch
        {
            // ignore
        }
    }

    private static async Task ResetAllBestValuesAsync(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
    {
        // ReSharper disable once PossibleNullReferenceException
        await (dungeons?.ForEachAsync(x =>
        {
            x.IsBestFame = false;
            x.IsBestReSpec = false;
            x.IsBestSilver = false;
            x.IsBestFactionFlags = false;
            x.IsBestFactionCoins = false;
            x.IsBestFamePerHour = false;
            x.IsBestReSpecPerHour = false;
            x.IsBestSilverPerHour = false;
            x.IsBestFactionFlagsPerHour = false;
            x.IsBestFactionCoinsPerHour = false;
            x.IsBestMight = false;
            x.IsBestMightPerHour = false;
            x.IsBestFavor = false;
            x.IsBestFavorPerHour = false;
        })).ConfigureAwait(false);
    }

    #endregion

    #region Stats

    public void UpdateDungeonStatsUi()
    {
        var nowMinus1days = DateTime.UtcNow.AddDays(-1);
        var nowMinus7days = DateTime.UtcNow.AddDays(-7);
        var nowMinus30days = DateTime.UtcNow.AddDays(-30);
        var nowMinus365days = DateTime.UtcNow.AddDays(-365);
        var nowMinus10Years = DateTime.UtcNow.AddYears(-10);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.EnteredDungeon = GetDungeonsCount(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.EnteredDungeon = GetDungeonsCount(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.EnteredDungeon = GetDungeonsCount(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.EnteredDungeon = GetDungeonsCount(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.EnteredDungeon = GetDungeonsCount(nowMinus10Years);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.DungeonRunTimeTotal = GetDungeonsRunTime(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.DungeonRunTimeTotal = GetDungeonsRunTime(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.DungeonRunTimeTotal = GetDungeonsRunTime(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.DungeonRunTimeTotal = GetDungeonsRunTime(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.DungeonRunTimeTotal = GetDungeonsRunTime(nowMinus10Years);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Fame = GetFame(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.ReSpec = GetReSpec(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Silver = GetSilver(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Might = GetMight(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Favor = GetFavor(nowMinus1days);

        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.Fame = GetFame(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.ReSpec = GetReSpec(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.Silver = GetSilver(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.Might = GetMight(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.Favor = GetFavor(nowMinus7days);

        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.Fame = GetFame(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.ReSpec = GetReSpec(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.Silver = GetSilver(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.Might = GetMight(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.Favor = GetFavor(nowMinus30days);

        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.Fame = GetFame(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.ReSpec = GetReSpec(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.Silver = GetSilver(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.Might = GetMight(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.Favor = GetFavor(nowMinus365days);

        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Fame = GetFame(null);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.ReSpec = GetReSpec(null);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Silver = GetSilver(null);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Might = GetMight(null);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Favor = GetFavor(null);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.BestLootedItem = GetBestLootedItem(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.BestLootedItem = GetBestLootedItem(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.BestLootedItem = GetBestLootedItem(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.BestLootedItem = GetBestLootedItem(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.BestLootedItem = GetBestLootedItem(nowMinus10Years);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.LootInSilver = GetLootInSilver(nowMinus1days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.LootInSilver = GetLootInSilver(nowMinus7days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.LootInSilver = GetLootInSilver(nowMinus30days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.LootInSilver = GetLootInSilver(nowMinus365days);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.LootInSilver = GetLootInSilver(nowMinus10Years);
    }

    public void SetDungeonStatsUi()
    {
        if (_mainWindowViewModel.DungeonBindings.DungeonStatsSelection?.EnteredDungeon == null)
        {
            _mainWindowViewModel.DungeonBindings.DungeonStatsSelection = _mainWindowViewModel.DungeonBindings.DungeonStatsDay;
        }
    }

    public void UpdateDungeonChestsUi()
    {
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Standard);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Standard, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Uncommon, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Rare, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Legendary, DungeonEventObjectType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Standard);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Standard, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Uncommon, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Rare, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Legendary, DungeonEventObjectType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Standard);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Standard, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Uncommon, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Rare, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Legendary, DungeonEventObjectType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Standard);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Standard, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Uncommon, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Rare, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Legendary, DungeonEventObjectType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedStandardChests = GetChests(null, TreasureRarity.Standard);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedUncommonChests = GetChests(null, TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedRareChests = GetChests(null, TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedLegendaryChests = GetChests(null, TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedStandardBookChests = GetChests(null, TreasureRarity.Standard, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedUncommonBookChests = GetChests(null, TreasureRarity.Uncommon, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedRareBookChests = GetChests(null, TreasureRarity.Rare, DungeonEventObjectType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedLegendaryBookChests = GetChests(null, TreasureRarity.Legendary, DungeonEventObjectType.BookChest);
    }

    private int GetChests(DateTime? chestIsNewerAsDateTime, TreasureRarity rarity, DungeonEventObjectType dungeonEventObjectType = DungeonEventObjectType.Chest)
    {
        var dungeonStatsFilter = _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter;

        var dungeonsWithEnterDungeonFiltering = _dungeons
            .Where(x => x.EnterDungeonFirstTime > chestIsNewerAsDateTime || chestIsNewerAsDateTime == null);

        var dungeonWithDungeonModeFilters = dungeonsWithEnterDungeonFiltering
            .Where(x => dungeonStatsFilter?.DungeonModeFilters != null && dungeonStatsFilter.DungeonModeFilters.Contains(x.Mode) || dungeonStatsFilter?.DungeonModeFilters == null);

        var dungeonWithTierFilters = dungeonWithDungeonModeFilters
            .Where(x => dungeonStatsFilter?.TierFilters != null && dungeonStatsFilter.TierFilters.Contains(x.Tier) || dungeonStatsFilter?.TierFilters == null);

        var dungeonWithLevelFilters = dungeonWithTierFilters
            .Where(x => dungeonStatsFilter?.LevelFilters != null && dungeonStatsFilter.LevelFilters.Contains((ItemLevel) x.Level) || dungeonStatsFilter?.LevelFilters == null);

        return dungeonWithLevelFilters
            .Select(dun => dun.DungeonEventObjects.Where(x => x.Rarity == rarity && x.ObjectType == dungeonEventObjectType))
            .Select(filteredChests => filteredChests.Count()).Sum();
    }

    private double GetFame(DateTime? dateTime)
    {
        var dungeonFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(
                x =>
                    x.EnterDungeonFirstTime > dateTime && dungeonFilters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
                    || dateTime == null && (dungeonFilters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || dungeonFilters is not { Count: > 0 })
            )
            .Select(x => x.Fame).Sum();
    }

    private double GetReSpec(DateTime? dateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(
                x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
                     || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
            .Select(x => x.ReSpec).Sum();
    }

    private double GetSilver(DateTime? dateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(
                x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
                     || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
            .Select(x => x.Silver).Sum();
    }

    private double GetMight(DateTime? dateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(
                x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
                     || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
            .Select(x => x.Might).Sum();
    }

    private double GetFavor(DateTime? dateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(
                x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
                     || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
            .Select(x => x.Favor).Sum();
    }

    private int GetDungeonsCount(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Count(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level));
    }

    private int GetDungeonsRunTime(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        return _dungeons.Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level))
            .ToList()
            .Select(x => x.TotalRunTimeInSeconds)
            .Sum();
    }

    private DungeonLoot GetBestLootedItem(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        var filteredDungeons = _dungeons
            ?.Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)).ToList();
        var mostExpensiveLoot = filteredDungeons?.MaxBy(x => x.MostExpensiveLoot?.EstimatedMarketValueInternal);
        return mostExpensiveLoot?.MostExpensiveLoot;
    }

    private long GetLootInSilver(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        var filteredDungeons = _dungeons
            ?.Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)).ToList();
        return filteredDungeons?.Sum(x => x.TotalLootInSilver) ?? 0;
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
            && ClusterController.CurrentCluster.MapType != MapType.RandomDungeon)
        {
            return;
        }

        try
        {
            lock (_dungeons)
            {
                var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
                if (dun == null)
                {
                    return;
                }

                dun.SetLevel(MobsData.GetMobLevelByIndex((int) mobIndex, hitPointsMax));

                if (dun.Level > 0)
                {
                    _lastGuidWithRecognizedLevel = dun.GuidList;
                }

                UpdateDungeonDataUi(dun);
            }
        }
        catch
        {
            // ignored
        }
    }

    public void AddTierToCurrentDungeon(int? mobIndex)
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
            lock (_dungeons)
            {
                var mobTier = (Tier) MobsData.GetMobTierByIndex((int) mobIndex);
                var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains(currentGuid) && x.Status == DungeonStatus.Active);
                if (dun == null || dun.Tier >= mobTier)
                {
                    return;
                }

                dun.SetTier(mobTier);
                UpdateDungeonDataUi(dun);
            }
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

    public void AddNewLocalPlayerLootOnCurrentDungeon(int containerSlot, Guid containerGuid, Guid userInteractGuid)
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

        AddLocalPlayerLootedItemToCurrentDungeon(lootedItem);
    }

    private long GetItemObjectIdFromContainer(int containerSlot)
    {
        if (_currentItemContainer == null || _currentItemContainer?.SlotItemIds?.Count is null or <= 0 || _currentItemContainer?.SlotItemIds?.Count <= containerSlot)
        {
            return 0;
        }

        return _currentItemContainer!.SlotItemIds![containerSlot];
    }

    public void AddLocalPlayerLootedItemToCurrentDungeon(DiscoveredItem discoveredItem)
    {
        if (_currentGuid == null)
        {
            return;
        }

        try
        {
            lock (_dungeons)
            {
                var dun = GetDungeon((Guid) _currentGuid);
                if (dun == null)
                {
                    return;
                }

                var uniqueItemName = ItemController.GetUniqueNameByIndex(discoveredItem.ItemIndex);

                dun.DungeonLoot.Add(new DungeonLoot()
                {
                    EstimatedMarketValueInternal = discoveredItem.EstimatedMarketValueInternal,
                    Quantity = discoveredItem.Quantity,
                    UniqueName = uniqueItemName,
                    UtcDiscoveryTime = discoveredItem.UtcDiscoveryTime
                });

                UpdateDungeonDataUi(dun);
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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

    #region Helper methods

    private bool ExistDungeon(Guid? mapGuid)
    {
        return mapGuid != null && _dungeons.Any(x => x.GuidList.Contains((Guid) mapGuid));
    }

    private static bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
    {
        return mapGuid != null && mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition;
    }

    #endregion

    #region Load / Save file data

    public async Task LoadDungeonFromFileAsync()
    {
        _dungeons = await FileController.LoadAsync<ObservableRangeCollection<DungeonObject>>($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}");
    }

    public async Task SaveInFileAsync()
    {
        var toSaveDungeons = _dungeons.Where(x => x is { Status: DungeonStatus.Done }).ToList();
        await FileController.SaveAsync(toSaveDungeons, $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}");
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