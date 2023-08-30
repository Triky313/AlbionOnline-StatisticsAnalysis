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

        RemoveDungeonsAfterCertainNumber(_mainWindowViewModel.DungeonBindings.Dungeons, MaxDungeons);

        _lastMapGuid = mapGuid;

        //UpdateDungeonStatsUi();
        //UpdateDungeonChestsUi();
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

    private static void RemoveDungeonsAfterCertainNumber(ICollection<DungeonBaseFragment> dungeons, int dungeonLimit)
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

    //private async Task DungeonUiFilteringAsync()
    //{
    //    var dungeonStatsFilter = _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter;
    //    var modeFilter = dungeonStatsFilter?.DungeonModeFilters;
    //    var tierFilter = dungeonStatsFilter?.TierFilters;
    //    var levelFilter = dungeonStatsFilter?.LevelFilters;

    //    await _mainWindowViewModel?.DungeonBindings?.Dungeons
    //        ?.Where(x =>
    //            !((modeFilter?.Contains(x.Mode) ?? false)
    //              && (tierFilter?.Contains(x.Tier) ?? false)
    //              && (levelFilter?.Contains((ItemLevel) x.Level) ?? x.Status != DungeonStatus.Active))
    //        )
    //        .ToAsyncEnumerable()
    //        .ForEachAsync(d =>
    //        {
    //            d.Visibility = Visibility.Collapsed;
    //        })!;

    //    await _mainWindowViewModel?.DungeonBindings?.Dungeons
    //        ?.Where(x =>
    //        {
    //            if (x.Status == DungeonStatus.Active)
    //            {
    //                return true;
    //            }

    //            if (((tierFilter?.Contains(Tier.Unknown) ?? false) && x.Tier is Tier.Unknown or > Tier.T8))
    //            {
    //                return true;
    //            }

    //            return (modeFilter?.Contains(x.Mode) ?? false)
    //                   && ((tierFilter?.Contains(x.Tier) ?? false))
    //                   && (levelFilter?.Contains((ItemLevel) x.Level) ?? false);
    //        })
    //        .ToAsyncEnumerable()
    //        .ForEachAsync(d =>
    //        {
    //            d.Visibility = Visibility.Visible;
    //        })!;
    //}

    //public async Task SetOrUpdateDungeonsDataUiAsync()
    //{
    //    var orderedDungeon = _mainWindowViewModel.DungeonBindings.Dungeons.OrderBy(x => x.EnterDungeonFirstTime).ToList();
    //    foreach (var dungeonObject in orderedDungeon)
    //    {
    //        var dungeonFragment = _mainWindowViewModel?.DungeonBindings?.Dungeons?.FirstOrDefault(x => x.DungeonHash == dungeonObject.DungeonHash);
    //        if (dungeonFragment != null && IsDungeonDifferenceToAnother(dungeonObject, dungeonFragment))
    //        {
    //            Application.Current.Dispatcher.Invoke(() =>
    //            {
    //                dungeonFragment.SetValues(dungeonObject);
    //                dungeonFragment.DungeonNumber = orderedDungeon.IndexOf(dungeonObject);
    //            });
    //        }
    //        else if (dungeonFragment == null)
    //        {
    //            Application.Current.Dispatcher.Invoke(() =>
    //            {
    //                var index = orderedDungeon.IndexOf(dungeonObject);
    //                var dunFragment = new DungeonNotificationFragment(index, dungeonObject.GuidList, dungeonObject.MainMapIndex, dungeonObject.EnterDungeonFirstTime);
    //                dunFragment.SetValues(dungeonObject);
    //                _mainWindowViewModel?.DungeonBindings?.Dungeons?.Insert(index, dunFragment);
    //            });
    //        }
    //    }

    //    await RemoveLeftOverDungeonNotificationFragments().ConfigureAwait(false);
    //    await Application.Current.Dispatcher.InvokeAsync(async () =>
    //    {
    //        await SetBestDungeonTimeAsync(_mainWindowViewModel.DungeonBindings.Dungeons?.ToAsyncEnumerable());
    //        await CalculateBestDungeonValues(_mainWindowViewModel.DungeonBindings.Dungeons?.ToAsyncEnumerable());
    //    });

    //    await DungeonUiFilteringAsync();

    //    UpdateDungeonStatsUi();
    //    UpdateDungeonChestsUi();
    //}

    //private void UpdateDungeonDataUi(DungeonObject dungeon)
    //{
    //    if (dungeon == null)
    //    {
    //        return;
    //    }

    //    var uiDungeon = GetCurrentUiDungeon(dungeon);
    //    uiDungeon?.SetValues(dungeon);
    //}

    //private DungeonNotificationFragment GetCurrentUiDungeon(DungeonObject dungeon)
    //{
    //    return _mainWindowViewModel?.DungeonBindings?.Dungeons?.FirstOrDefault(x =>
    //        x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime.Equals(dungeon.EnterDungeonFirstTime));
    //}

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

        UpdateDungeonChestsUi();
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

    #region Best values

    // TODO: Rework: Stats not in the dungeon fragment
    private static async Task CalculateBestDungeonValues(IAsyncEnumerable<DungeonBaseFragment> dungeons)
    {
        //if (await dungeons.CountAsync() <= 0)
        //{
        //    return;
        //}

        //var highestFame = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Fame: > 0 }).Select(x => x?.Fame).MaxAsync();
        //var bestDungeonFame = await dungeons.FirstOrDefaultAsync(x => x.Fame.CompareTo(highestFame) == 0);

        //if (bestDungeonFame != null)
        //{
        //    bestDungeonFame.IsBestFame = true;
        //}

        //var highestReSpec = await dungeons.Where(x => x is { Status: DungeonStatus.Done, ReSpec: > 0 }).Select(x => x?.ReSpec).MaxAsync();
        //var bestDungeonReSpec = await dungeons.FirstOrDefaultAsync(x => x.ReSpec.CompareTo(highestReSpec) == 0);

        //if (bestDungeonReSpec != null)
        //{
        //    bestDungeonReSpec.IsBestReSpec = true;
        //}

        //var highestSilver = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Silver: > 0 }).Select(x => x?.Silver).MaxAsync();
        //var bestDungeonSilver = await dungeons.FirstOrDefaultAsync(x => x.Silver.CompareTo(highestSilver) == 0);

        //if (bestDungeonSilver != null)
        //{
        //    bestDungeonSilver.IsBestSilver = true;
        //}

        //var highestFactionFlags = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionFlags: > 0 }).Select(x => x?.FactionFlags).MaxAsync();
        //var bestDungeonFlags = await dungeons.FirstOrDefaultAsync(x => x.FactionFlags.CompareTo(highestFactionFlags) == 0);

        //if (bestDungeonFlags != null)
        //{
        //    bestDungeonFlags.IsBestFactionFlags = true;
        //}

        //var highestFactionCoins = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionCoins: > 0 }).Select(x => x?.FactionCoins).MaxAsync();
        //var bestDungeonCoins = await dungeons.FirstOrDefaultAsync(x => x.FactionCoins.CompareTo(highestFactionCoins) == 0);

        //if (bestDungeonCoins != null)
        //{
        //    bestDungeonCoins.IsBestFactionCoins = true;
        //}

        //var highestMight = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Might: > 0 }).Select(x => x?.Might).MaxAsync();
        //var bestDungeonMight = await dungeons.FirstOrDefaultAsync(x => x.Might.CompareTo(highestMight) == 0);

        //if (bestDungeonMight != null)
        //{
        //    bestDungeonMight.IsBestMight = true;
        //}

        //var highestFavor = await dungeons.Where(x => x is { Status: DungeonStatus.Done, Favor: > 0 }).Select(x => x?.Favor).MaxAsync();
        //var bestDungeonFavor = await dungeons.FirstOrDefaultAsync(x => x.Favor.CompareTo(highestFavor) == 0);

        //if (bestDungeonFavor != null)
        //{
        //    bestDungeonFavor.IsBestMightPerHour = true;
        //}

        //var highestFamePerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FamePerHour: > 0 }).Select(x => x?.FamePerHour).MaxAsync();
        //var bestDungeonFamePerHour = await dungeons.FirstOrDefaultAsync(x => x.FamePerHour.CompareTo(highestFamePerHour) == 0);

        //if (bestDungeonFamePerHour != null)
        //{
        //    bestDungeonFamePerHour.IsBestFamePerHour = true;
        //}

        //var highestReSpecPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, ReSpecPerHour: > 0 }).Select(x => x?.ReSpecPerHour).MaxAsync();
        //var bestDungeonReSpecPerHour = await dungeons.FirstOrDefaultAsync(x => x.ReSpecPerHour.CompareTo(highestReSpecPerHour) == 0);

        //if (bestDungeonReSpecPerHour != null)
        //{
        //    bestDungeonReSpecPerHour.IsBestReSpecPerHour = true;
        //}

        //var highestSilverPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, SilverPerHour: > 0 }).Select(x => x?.SilverPerHour).MaxAsync();
        //var bestDungeonSilverPerHour = await dungeons.FirstOrDefaultAsync(x => x.SilverPerHour.CompareTo(highestSilverPerHour) == 0);

        //if (bestDungeonSilverPerHour != null)
        //{
        //    bestDungeonSilverPerHour.IsBestSilverPerHour = true;
        //}

        //var highestFactionFlagsPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionFlagsPerHour: > 0 }).Select(x => x?.FactionFlagsPerHour).MaxAsync();
        //var bestDungeonFactionFlagsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionFlagsPerHour.CompareTo(highestFactionFlagsPerHour) == 0);

        //if (bestDungeonFactionFlagsPerHour != null)
        //{
        //    bestDungeonFactionFlagsPerHour.IsBestFactionFlagsPerHour = true;
        //}

        //var highestFactionCoinsPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FactionCoinsPerHour: > 0 }).Select(x => x?.FactionCoinsPerHour).MaxAsync();
        //var bestDungeonFactionCoinsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionCoinsPerHour.CompareTo(highestFactionCoinsPerHour) == 0);

        //if (bestDungeonFactionCoinsPerHour != null)
        //{
        //    bestDungeonFactionCoinsPerHour.IsBestFactionCoinsPerHour = true;
        //}

        //var highestMightPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, MightPerHour: > 0 }).Select(x => x?.MightPerHour).MaxAsync();
        //var bestDungeonMightPerHour = await dungeons.FirstOrDefaultAsync(x => x.MightPerHour.CompareTo(highestMightPerHour) == 0);

        //if (bestDungeonMightPerHour != null)
        //{
        //    bestDungeonMightPerHour.IsBestMightPerHour = true;
        //}

        //var highestFavorPerHour = await dungeons.Where(x => x is { Status: DungeonStatus.Done, FavorPerHour: > 0 }).Select(x => x?.FavorPerHour).MaxAsync();
        //var bestDungeonFavorPerHour = await dungeons.FirstOrDefaultAsync(x => x.FavorPerHour.CompareTo(highestFavorPerHour) == 0);

        //if (bestDungeonFavorPerHour != null)
        //{
        //    bestDungeonFavorPerHour.IsBestFavorPerHour = true;
        //}
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
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Common);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Common, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Uncommon, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Rare, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Legendary, EventType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Common);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Common, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Uncommon, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Rare, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsWeek.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-7), TreasureRarity.Legendary, EventType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Common);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Common, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Uncommon, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Rare, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsMonth.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-30), TreasureRarity.Legendary, EventType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Common);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedStandardBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Common, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedUncommonBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Uncommon, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedRareBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Rare, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsYear.OpenedLegendaryBookChests = GetChests(DateTime.UtcNow.AddDays(-365), TreasureRarity.Legendary, EventType.BookChest);

        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedStandardChests = GetChests(null, TreasureRarity.Common);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedUncommonChests = GetChests(null, TreasureRarity.Uncommon);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedRareChests = GetChests(null, TreasureRarity.Rare);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedLegendaryChests = GetChests(null, TreasureRarity.Legendary);

        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedStandardBookChests = GetChests(null, TreasureRarity.Common, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedUncommonBookChests = GetChests(null, TreasureRarity.Uncommon, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedRareBookChests = GetChests(null, TreasureRarity.Rare, EventType.BookChest);
        _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedLegendaryBookChests = GetChests(null, TreasureRarity.Legendary, EventType.BookChest);
    }

    private int GetChests(DateTime? chestIsNewerAsDateTime, TreasureRarity rarity, EventType eventType = EventType.Chest)
    {
        var dungeonStatsFilter = _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter;

        var dungeonsWithEnterDungeonFiltering = _mainWindowViewModel.DungeonBindings.Dungeons
            .Where(x => x.EnterDungeonFirstTime > chestIsNewerAsDateTime || chestIsNewerAsDateTime == null);

        var dungeonWithDungeonModeFilters = dungeonsWithEnterDungeonFiltering
            .Where(x => dungeonStatsFilter?.DungeonModeFilters != null && dungeonStatsFilter.DungeonModeFilters.Contains(x.Mode) || dungeonStatsFilter?.DungeonModeFilters == null);

        var dungeonWithTierFilters = dungeonWithDungeonModeFilters
            .Where(x => dungeonStatsFilter?.TierFilters != null && dungeonStatsFilter.TierFilters.Contains(x.Tier) || dungeonStatsFilter?.TierFilters == null);

        //var dungeonWithLevelFilters = dungeonWithTierFilters
        //    .Where(x => dungeonStatsFilter?.LevelFilters != null && dungeonStatsFilter.LevelFilters.Contains((ItemLevel) x.Level) || dungeonStatsFilter?.LevelFilters == null);

        //return dungeonWithLevelFilters
        //    .Select(dun => dun.Events.Where(x => x.Rarity == rarity && x.Type == eventType))
        //    .Select(filteredChests => filteredChests.Count()).Sum();
        return 0;
    }

    private double GetFame(DateTime? dateTime)
    {
        //var dungeonFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons?.Where(
        //        x =>
        //            x?.EnterDungeonFirstTime > dateTime && dungeonFilters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
        //            || dateTime == null && (dungeonFilters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || dungeonFilters is not { Count: > 0 })
        //    )
        //    .Select(x => x.Fame).Sum() ?? 0;

        return 0;
    }

    private double GetReSpec(DateTime? dateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons?.Where(
        //        x => x?.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
        //             || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
        //    .Select(x => x.ReSpec).Sum() ?? 0;

        return 0;
    }

    private double GetSilver(DateTime? dateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons?.Where(
        //        x => x?.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
        //             || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
        //    .Select(x => x.Silver).Sum() ?? 0;

        return 0;
    }

    // TODO: Rework dungeon stats
    private double GetMight(DateTime? dateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons.Where(
        //        x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
        //             || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
        //    .Select(x => x.Might).Sum();

        return 0;
    }

    // TODO: Rework dungeon stats
    private double GetFavor(DateTime? dateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons.Where(
        //        x => x.EnterDungeonFirstTime > dateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)
        //             || dateTime == null && (filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level) || filters is not { Count: > 0 }))
        //    .Select(x => x.Favor).Sum();

        return 0;
    }

    private int GetDungeonsCount(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons
        //    .Count(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level));

        return 0;
    }

    private int GetDungeonsRunTime(DateTime dungeonIsNewerAsDateTime)
    {
        var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //return _mainWindowViewModel.DungeonBindings.Dungeons
        //    .Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level))
        //    .ToList()
        //    .Select(x => x.TotalRunTimeInSeconds)
        //    .Sum();

        return 0;
    }

    // TODO: Rework dungeon stats
    private Loot GetBestLootedItem(DateTime dungeonIsNewerAsDateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //var filteredDungeons = _mainWindowViewModel.DungeonBindings.Dungeons
        //    ?.Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)).ToList();
        //var mostExpensiveLoot = filteredDungeons?.MaxBy(x => x.MostExpensiveLoot?.EstimatedMarketValueInternal);
        //return mostExpensiveLoot?.MostExpensiveLoot;

        return new Loot();
    }

    // TODO: Rework dungeon stats
    private long GetLootInSilver(DateTime dungeonIsNewerAsDateTime)
    {
        //var filters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.DungeonModeFilters ?? new List<DungeonMode>();
        //var tierFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.TierFilters ?? new List<Tier>();
        //var levelFilters = _mainWindowViewModel.DungeonBindings.DungeonStatsFilter?.LevelFilters ?? new List<ItemLevel>();

        //var filteredDungeons = _mainWindowViewModel.DungeonBindings.Dungeons
        //    ?.Where(x => x?.EnterDungeonFirstTime > dungeonIsNewerAsDateTime && filters.Contains(x.Mode) && tierFilters.Contains(x.Tier) && levelFilters.Contains((ItemLevel) x.Level)).ToList();
        //return filteredDungeons?.Sum(x => x.TotalLootInSilver) ?? 0;

        return 0;
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
            lock (_mainWindowViewModel.DungeonBindings.Dungeons)
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
            }
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