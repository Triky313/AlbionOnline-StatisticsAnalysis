using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
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

namespace StatisticsAnalysisTool.Network.Manager
{
    public class DungeonController
    {
        private const int MaxDungeons = 9999;
        private const int NumberOfDungeonsUntilSaved = 2;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;
        private Guid? _currentGuid;
        private Guid? _lastMapGuid;
        private List<DungeonObject> _dungeons = new();
        private int _addDungeonCounter;

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
            else if ((IsDungeonCluster(mapType, mapGuid) && !ExistDungeon(_lastMapGuid) && !ExistDungeon(_currentGuid)) || (IsDungeonCluster(mapType, mapGuid) && mapType is MapType.CorruptedDungeon or MapType.HellGate))
            {
                UpdateDungeonSaveTimerUi(mapType);

                if (mapType is MapType.CorruptedDungeon or MapType.HellGate)
                {
                    var lastDungeon = GetDungeon(_lastMapGuid);
                    lastDungeon?.EndTimer();
                }

                _dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

                var newDungeon = new DungeonObject(ClusterController.CurrentCluster.MainClusterIndex,
                    mapGuid ?? new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0), DungeonStatus.Active, ClusterController.CurrentCluster.Tier);
                SetDungeonMapType(newDungeon, mapType);

                _dungeons.Insert(0, newDungeon);
            }
            // Activate exist dungeon again
            else if ((IsDungeonCluster(mapType, mapGuid) && !ExistDungeon(_lastMapGuid) && ExistDungeon(_currentGuid)) || (IsDungeonCluster(mapType, mapGuid) && mapType is MapType.CorruptedDungeon or MapType.HellGate))
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
            }

            RemoveDungeonsAfterCertainNumber(_dungeons, MaxDungeons);

            _lastMapGuid = mapGuid;

            await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);

            SetDungeonStatsDayUi();
            SetDungeonStatsTotalUi();
        }

        private void UpdateDungeonSaveTimerUi(MapType mapType = MapType.Unknown)
        {
            _mainWindowViewModel.DungeonBindings.DungeonCloseTimer.Visibility = mapType == MapType.RandomDungeon ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ResetDungeons()
        {
            _dungeons.Clear();

            Application.Current.Dispatcher.Invoke(() => { _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Clear(); });
        }

        public void SetDungeonChestOpen(int id)
        {
            if (_currentGuid != null)
            {
                try
                {
                    var dun = GetDungeon((Guid)_currentGuid);
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

            SetDungeonStatsDayUi();
            SetDungeonStatsTotalUi();
        }

        public async Task SetDungeonEventObjectInformationAsync(int id, string uniqueName)
        {
            if (_currentGuid != null && uniqueName != null)
            {
                try
                {
                    var dun = GetDungeon((Guid)_currentGuid);
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

        private DungeonObject GetDungeon(Guid? guid)
        {
            return guid == null ? null : _dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid)guid));
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
                case MapType.Unknown:
                    break;
                default:
                    return;
            }
        }

        private int GetChests(DateTime? chestIsNewerAsDateTime, TreasureRarity rarity)
        {
            var dungeons = _dungeons.Where(x => (x.EnterDungeonFirstTime > chestIsNewerAsDateTime || chestIsNewerAsDateTime == null)
                                                && ((_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters != null
                                                     && _mainWindowViewModel.DungeonBindings.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                                    || _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters == null));

            return dungeons.Select(dun => dun.DungeonEventObjects.Where(x => x.Rarity == rarity)).Select(filteredChests => filteredChests.Count()).Sum();
        }

        private double GetFame(DateTime? dateTime)
        {
            return _dungeons.Where(x => (x.EnterDungeonFirstTime > dateTime || dateTime == null)
                                        && ((_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters != null
                                             && _mainWindowViewModel.DungeonBindings.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                            || _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters == null)).Select(x => x.Fame).Sum();
        }

        private double GetReSpec(DateTime? dateTime)
        {
            return _dungeons.Where(x => x.EnterDungeonFirstTime > dateTime || (dateTime == null
                && ((_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters != null
                     && _mainWindowViewModel.DungeonBindings.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                    || _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters == null))).Select(x => x.ReSpec).Sum();
        }

        private double GetSilver(DateTime? dateTime)
        {
            return _dungeons.Where(x => x.EnterDungeonFirstTime > dateTime || (dateTime == null
                && ((_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters != null
                     && _mainWindowViewModel.DungeonBindings.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                    || _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters == null))).Select(x => x.Silver).Sum();
        }

        public int GetDungeonsCount(DateTime dungeonIsNewerAsDateTime)
        {
            return _dungeons.Count(x => x.EnterDungeonFirstTime > dungeonIsNewerAsDateTime
                                        && ((_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters != null
                                             && _mainWindowViewModel.DungeonBindings.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                            || _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters == null));
        }

        public void SetDungeonStatsDayUi()
        {
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.TranslationTitle = LanguageController.Translation("DAY");

            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Standard);
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Uncommon);
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Rare);
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-1), TreasureRarity.Legendary);

            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Fame = GetFame(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.ReSpec = GetReSpec(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.Silver = GetSilver(DateTime.UtcNow.AddDays(-1));
        }

        public void SetDungeonStatsTotalUi()
        {
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.TranslationTitle = LanguageController.Translation("TOTAL");

            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedStandardChests = GetChests(null, TreasureRarity.Standard);
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedUncommonChests = GetChests(null, TreasureRarity.Uncommon);
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedRareChests = GetChests(null, TreasureRarity.Rare);
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.OpenedLegendaryChests = GetChests(null, TreasureRarity.Legendary);

            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Fame = GetFame(null);
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.ReSpec = GetReSpec(null);
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.Silver = GetSilver(null);
        }

        public void SetDiedIfInDungeon(DiedObject dieObject)
        {
            if (_currentGuid != null && _trackingController.EntityController.LocalUserData.Username != null && dieObject.DiedName == _trackingController.EntityController.LocalUserData.Username)
            {
                try
                {
                    var item = _dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid)_currentGuid) && x.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-1));

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

        private void RemoveDungeonsAfterCertainNumber(List<DungeonObject> dungeons, int dungeonLimit)
        {
            if (_trackingController.IsMainWindowNull())
            {
                return;
            }

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

        private bool ExistDungeon(Guid? mapGuid)
        {
            return mapGuid != null && _dungeons.Any(x => x.GuidList.Contains((Guid)mapGuid));
        }

        private static bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
        {
            return mapGuid != null && mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition;
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
                var bestTime = await dungeons.Where(x => x?.DungeonChests?.Any(y => y?.IsBossChest ?? false) == true).MinAsync(x => x?.TotalRunTimeInSeconds).ConfigureAwait(false);
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

        private static async Task CalculateBestDungeonValues(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
        {
            if (await dungeons.CountAsync() <= 0)
            {
                return;
            }

            await ResetAllBestValuesAsync(dungeons);

            var highestFame = await dungeons.Where(x => x is {Status: DungeonStatus.Done, Fame: > 0}).Select(x => x?.Fame).MaxAsync();
            var bestDungeonFame = await dungeons.FirstOrDefaultAsync(x => x.Fame.CompareTo(highestFame) == 0);

            if (bestDungeonFame != null)
            {
                bestDungeonFame.IsBestFame = true;
            }

            var highestReSpec = await dungeons.Where(x => x is {Status: DungeonStatus.Done, ReSpec: > 0}).Select(x => x?.ReSpec).MaxAsync();
            var bestDungeonReSpec = await dungeons.FirstOrDefaultAsync(x => x.ReSpec.CompareTo(highestReSpec) == 0);

            if (bestDungeonReSpec != null)
            {
                bestDungeonReSpec.IsBestReSpec = true;
            }

            var highestSilver = await dungeons.Where(x => x is {Status: DungeonStatus.Done, Silver: > 0}).Select(x => x?.Silver).MaxAsync();
            var bestDungeonSilver = await dungeons.FirstOrDefaultAsync(x => x.Silver.CompareTo(highestSilver) == 0);

            if (bestDungeonSilver != null)
            {
                bestDungeonSilver.IsBestSilver = true;
            }

            var highestFactionFlags = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FactionFlags: > 0}).Select(x => x?.FactionFlags).MaxAsync();
            var bestDungeonFlags = await dungeons.FirstOrDefaultAsync(x => x.FactionFlags.CompareTo(highestFactionFlags) == 0);

            if (bestDungeonFlags != null)
            {
                bestDungeonFlags.IsBestFactionFlags = true;
            }

            var highestFactionCoins = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FactionCoins: > 0}).Select(x => x?.FactionCoins).MaxAsync();
            var bestDungeonCoins = await dungeons.FirstOrDefaultAsync(x => x.FactionCoins.CompareTo(highestFactionCoins) == 0);

            if (bestDungeonCoins != null)
            {
                bestDungeonCoins.IsBestFactionCoins = true;
            }

            var highestMight = await dungeons.Where(x => x is {Status: DungeonStatus.Done, Might: > 0}).Select(x => x?.Might).MaxAsync();
            var bestDungeonMight = await dungeons.FirstOrDefaultAsync(x => x.Might.CompareTo(highestMight) == 0);

            if (bestDungeonMight != null)
            {
                bestDungeonMight.IsBestMight = true;
            }

            var highestFavor = await dungeons.Where(x => x is {Status: DungeonStatus.Done, Favor: > 0}).Select(x => x?.Favor).MaxAsync();
            var bestDungeonFavor = await dungeons.FirstOrDefaultAsync(x => x.Favor.CompareTo(highestFavor) == 0);

            if (bestDungeonFavor != null)
            {
                bestDungeonFavor.IsBestMightPerHour = true;
            }

            var highestFamePerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FamePerHour: > 0}).Select(x => x?.FamePerHour).MaxAsync();
            var bestDungeonFamePerHour = await dungeons.FirstOrDefaultAsync(x => x.FamePerHour.CompareTo(highestFamePerHour) == 0);

            if (bestDungeonFamePerHour != null)
            {
                bestDungeonFamePerHour.IsBestFamePerHour = true;
            }

            var highestReSpecPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, ReSpecPerHour: > 0}).Select(x => x?.ReSpecPerHour).MaxAsync();
            var bestDungeonReSpecPerHour = await dungeons.FirstOrDefaultAsync(x => x.ReSpecPerHour.CompareTo(highestReSpecPerHour) == 0);

            if (bestDungeonReSpecPerHour != null)
            {
                bestDungeonReSpecPerHour.IsBestReSpecPerHour = true;
            }

            var highestSilverPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, SilverPerHour: > 0}).Select(x => x?.SilverPerHour).MaxAsync();
            var bestDungeonSilverPerHour = await dungeons.FirstOrDefaultAsync(x => x.SilverPerHour.CompareTo(highestSilverPerHour) == 0);

            if (bestDungeonSilverPerHour != null)
            {
                bestDungeonSilverPerHour.IsBestSilverPerHour = true;
            }

            var highestFactionFlagsPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FactionFlagsPerHour: > 0}).Select(x => x?.FactionFlagsPerHour).MaxAsync();
            var bestDungeonFactionFlagsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionFlagsPerHour.CompareTo(highestFactionFlagsPerHour) == 0);

            if (bestDungeonFactionFlagsPerHour != null)
            {
                bestDungeonFactionFlagsPerHour.IsBestFactionFlagsPerHour = true;
            }

            var highestFactionCoinsPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FactionCoinsPerHour: > 0}).Select(x => x?.FactionCoinsPerHour).MaxAsync();
            var bestDungeonFactionCoinsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionCoinsPerHour.CompareTo(highestFactionCoinsPerHour) == 0);

            if (bestDungeonFactionCoinsPerHour != null)
            {
                bestDungeonFactionCoinsPerHour.IsBestFactionCoinsPerHour = true;
            }

            var highestMightPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, MightPerHour: > 0}).Select(x => x?.MightPerHour).MaxAsync();
            var bestDungeonMightPerHour = await dungeons.FirstOrDefaultAsync(x => x.MightPerHour.CompareTo(highestMightPerHour) == 0);

            if (bestDungeonMightPerHour != null)
            {
                bestDungeonMightPerHour.IsBestMightPerHour = true;
            }

            var highestFavorPerHour = await dungeons.Where(x => x is {Status: DungeonStatus.Done, FavorPerHour: > 0}).Select(x => x?.FavorPerHour).MaxAsync();
            var bestDungeonFavorPerHour = await dungeons.FirstOrDefaultAsync(x => x.FavorPerHour.CompareTo(highestFavorPerHour) == 0);

            if (bestDungeonFavorPerHour != null)
            {
                bestDungeonFavorPerHour.IsBestFavorPerHour = true;
            }
        }

        public async Task SetOrUpdateDungeonsDataUiAsync()
        {
            _mainWindowViewModel.DungeonBindings.DungeonStatsDay.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonBindings.DungeonStatsTotal.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddYears(-10));

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

            SetDungeonStatsDayUi();
            SetDungeonStatsTotalUi();
        }

        private static bool IsDungeonDifferenceToAnother(DungeonObject dungeonObject, DungeonNotificationFragment dungeonNotificationFragment)
        {
            return dungeonObject.TotalRunTimeInSeconds != dungeonNotificationFragment.TotalRunTimeInSeconds
                   || !dungeonObject.GuidList.SequenceEqual(dungeonNotificationFragment.GuidList)
                   || dungeonObject.DungeonEventObjects.Count != dungeonNotificationFragment.DungeonChests.Count
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

        private async Task DungeonUiFilteringAsync()
        {
            await _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Where(x => !_mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters?.Contains(x.Mode) ?? x.Status != DungeonStatus.Active)
                // ReSharper disable once ConstantConditionalAccessQualifier
                ?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                });

            await _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Where(x => _mainWindowViewModel?.DungeonBindings?.DungeonStatsFilter?.DungeonModeFilters.Contains(x.Mode) ?? x.Status == DungeonStatus.Active)
                // ReSharper disable once ConstantConditionalAccessQualifier
                ?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                });
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

        public async void RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash)
        {
            _ = _dungeons.RemoveAll(x => dungeonHash.Contains(x.DungeonHash));

            foreach (var dungeonFragment in _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.ToList() ?? new List<DungeonNotificationFragment>())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.Remove(dungeonFragment);
                });
            }

            await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);
        }

        public void UpdateDungeonDataUi(DungeonObject dungeon)
        {
            if (dungeon == null)
            {
                return;
            }

            var uiDungeon = _mainWindowViewModel?.DungeonBindings?.TrackingDungeons?.FirstOrDefault(x =>
                x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime.Equals(dungeon.EnterDungeonFirstTime));

            uiDungeon?.SetValues(dungeon);
        }

        private static bool AddClusterToExistDungeon(List<DungeonObject> dungeons, Guid? currentGuid, Guid? lastGuid, out DungeonObject dungeon)
        {
            if (currentGuid != null && lastGuid != null && dungeons?.Any(x => x.GuidList.Contains((Guid)currentGuid)) != true)
            {
                var dun = dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid)lastGuid));
                dun?.GuidList.Add((Guid)currentGuid);

                dungeon = dun;

                return dungeons?.Any(x => x.GuidList.Contains((Guid)currentGuid)) ?? false;
            }

            dungeon = null;
            return false;
        }

        public void AddValueToDungeon(double value, ValueType valueType, CityFaction cityFaction = CityFaction.Unknown)
        {
            if (_currentGuid == null)
            {
                return;
            }

            try
            {
                lock (_dungeons)
                {
                    var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid)_currentGuid) && x.Status == DungeonStatus.Active);
                    dun?.Add(value, valueType, cityFaction);

                    UpdateDungeonDataUi(dun);
                }
            }
            catch
            {
                // ignored
            }
        }

        public static DateTime? GetLowestDate(List<DungeonObject> items)
        {
            if (items?.Count <= 0)
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

        #region Load / Save file data

        public async Task LoadDungeonFromFileAsync()
        {
            _dungeons = await FileController.LoadAsync<List<DungeonObject>>($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}");
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
}