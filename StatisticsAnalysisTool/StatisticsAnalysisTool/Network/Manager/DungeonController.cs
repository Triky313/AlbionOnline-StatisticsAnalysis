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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;
// ReSharper disable PossibleMultipleEnumeration

namespace StatisticsAnalysisTool.Network.Manager
{
    public class DungeonController
    {
        private const int _maxDungeons = 9999;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;
        private Guid? _currentGuid;
        private Guid? _lastMapGuid;
        private List<DungeonObject> _dungeons = new();

        public DungeonController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public LocalUserData LocalUserData { get; set; }

        public async Task AddDungeonAsync(MapType mapType, Guid? mapGuid, string mainMapIndex)
        {
            UpdateDungeonSaveTimerUi();

            _currentGuid = mapGuid;

            // Last map is a dungeon, add new map
            if (IsDungeonCluster(mapType, mapGuid) && ExistDungeon(_lastMapGuid) && mapType != MapType.CorruptedDungeon)
            {
                if (AddClusterToExistDungeon(_dungeons, mapGuid, _lastMapGuid, out var currentDungeon))
                {
                    currentDungeon.AddTimer(DateTime.UtcNow);
                }
            }
            // Add new dungeon
            else if (mapGuid != null && IsDungeonCluster(mapType, mapGuid) && !ExistDungeon(_lastMapGuid))
            {
                UpdateDungeonSaveTimerUi(mapType);

                _dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

                var newDungeon = new DungeonObject(mainMapIndex, (Guid)mapGuid, DungeonStatus.Active);
                SetDungeonInformation(newDungeon, mapType);

                _dungeons.Insert(0, newDungeon);
            }
            // Make last dungeon done
            else if (mapGuid == null && ExistDungeon(_lastMapGuid))
            {
                var lastDungeon = GetDungeon(_lastMapGuid);
                lastDungeon.EndTimer();
                lastDungeon.Status = DungeonStatus.Done;
            }

            RemoveDungeonsAfterCertainNumber(_dungeons, _maxDungeons);

            _lastMapGuid = mapGuid;

            await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);

            SetDungeonStatsDayUi();
            SetDungeonStatsTotalUi();
        }

        private void UpdateDungeonSaveTimerUi(MapType mapType = MapType.Unknown)
        {
            if (mapType == MapType.RandomDungeon)
            {
                _mainWindowViewModel.DungeonCloseTimer = new DungeonCloseTimer
                {
                    IsVisible = Visibility.Visible
                };
            }
            else
            {
                _mainWindowViewModel.DungeonCloseTimer = new DungeonCloseTimer
                {
                    IsVisible = Visibility.Collapsed
                };
            }
        }

        public async void ResetDungeons()
        {
            _dungeons.Clear();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel?.TrackingDungeons?.Clear();
            });
        }

        public void SetDungeonChestOpen(int id)
        {
            if (_currentGuid != null)
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

            if (dialogResult is true)
            {
                _dungeons.Remove(dungeon);
                await SetOrUpdateDungeonsDataUiAsync().ConfigureAwait(false);
            }
        }
        
        private DungeonObject GetDungeon(Guid? guid)
        {
            return guid == null ? null : _dungeons.FirstOrDefault(x => x.GuidList.Contains((Guid)guid));
        }
        
        private static void SetDungeonInformation(DungeonObject dungeon, MapType mapType)
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

        private int GetChests(DateTime? chestIsNewerAsDateTime, ChestRarity rarity)
        {
            var dungeons = _dungeons.Where(x => (x.EnterDungeonFirstTime > chestIsNewerAsDateTime || chestIsNewerAsDateTime == null)
                                                && ((_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters != null
                                                     && _mainWindowViewModel.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                                    || _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters == null));

            return dungeons.Select(dun => dun.DungeonEventObjects.Where(x => x.Rarity == rarity)).Select(filteredChests => filteredChests.Count()).Sum();
        }

        private double GetFame(DateTime? dateTime)
        {
            return _dungeons.Where(x => (x.EnterDungeonFirstTime > dateTime || dateTime == null)
                                        && ((_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters != null
                                             && _mainWindowViewModel.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                            || _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters == null)).Select(x => x.Fame).Sum();
        }

        private double GetReSpec(DateTime? dateTime)
        {
            return _dungeons.Where(x => x.EnterDungeonFirstTime > dateTime || dateTime == null
                && ((_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters != null
                     && _mainWindowViewModel.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                    || _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters == null)).Select(x => x.ReSpec).Sum();
        }

        private double GetSilver(DateTime? dateTime)
        {
            return _dungeons.Where(x => x.EnterDungeonFirstTime > dateTime || dateTime == null
                && ((_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters != null
                     && _mainWindowViewModel.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                    || _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters == null)).Select(x => x.Silver).Sum();
        }

        public int GetDungeonsCount(DateTime dungeonIsNewerAsDateTime)
        {
            return _dungeons.Count(x => x.EnterDungeonFirstTime > dungeonIsNewerAsDateTime
                                        && ((_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters != null
                                             && _mainWindowViewModel.DungeonStatsFilter.DungeonModeFilters.Contains(x.Mode))
                                            || _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters == null));
        }

        public void SetDungeonStatsDayUi()
        {
            _mainWindowViewModel.DungeonStatsDay.TranslationTitle = LanguageController.Translation("DAY");

            _mainWindowViewModel.DungeonStatsDay.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Standard);
            _mainWindowViewModel.DungeonStatsDay.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Uncommon);
            _mainWindowViewModel.DungeonStatsDay.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Rare);
            _mainWindowViewModel.DungeonStatsDay.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Legendary);

            _mainWindowViewModel.DungeonStatsDay.Fame = GetFame(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonStatsDay.ReSpec = GetReSpec(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonStatsDay.Silver = GetSilver(DateTime.UtcNow.AddDays(-1));
        }

        public void SetDungeonStatsTotalUi()
        {
            _mainWindowViewModel.DungeonStatsTotal.TranslationTitle = LanguageController.Translation("TOTAL");

            _mainWindowViewModel.DungeonStatsTotal.OpenedStandardChests = GetChests(null, ChestRarity.Standard);
            _mainWindowViewModel.DungeonStatsTotal.OpenedUncommonChests = GetChests(null, ChestRarity.Uncommon);
            _mainWindowViewModel.DungeonStatsTotal.OpenedRareChests = GetChests(null, ChestRarity.Rare);
            _mainWindowViewModel.DungeonStatsTotal.OpenedLegendaryChests = GetChests(null, ChestRarity.Legendary);

            _mainWindowViewModel.DungeonStatsTotal.Fame = GetFame(null);
            _mainWindowViewModel.DungeonStatsTotal.ReSpec = GetReSpec(null);
            _mainWindowViewModel.DungeonStatsTotal.Silver = GetSilver(null);
        }

        public void SetDiedIfInDungeon(DiedObject dieObject)
        {
            if (_currentGuid != null && LocalUserData.Username != null && dieObject.DiedName == LocalUserData.Username)
            {
                try
                {
                    var item = _dungeons.First(x => x.GuidList.Contains((Guid)_currentGuid) && x.EnterDungeonFirstTime > DateTime.UtcNow.AddDays(-1));
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
        
        private bool IsDungeonCluster(MapType mapType, Guid? mapGuid)
        {
            return mapType is MapType.RandomDungeon or MapType.CorruptedDungeon or MapType.HellGate or MapType.Expedition && mapGuid != null;
        }
        
        private async Task SetBestDungeonTimeAsync(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
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

        private async Task ResetAllBestValuesAsync(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
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
            })).ConfigureAwait(false);
        }

        private async Task CalculateBestDungeonValues(IAsyncEnumerable<DungeonNotificationFragment> dungeons)
        {
            if (await dungeons.CountAsync() <= 0)
            {
                return;
            }

            await ResetAllBestValuesAsync(dungeons);

            var highestFame = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.Fame > 0).Select(x => x.Fame).MaxAsync();
            var bestDungeonFame = await dungeons.FirstOrDefaultAsync(x => x.Fame.CompareTo(highestFame) == 0);

            if (bestDungeonFame != null)
            {
                bestDungeonFame.IsBestFame = true;
            }

            var highestReSpec = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.ReSpec > 0).Select(x => x.ReSpec).MaxAsync();
            var bestDungeonReSpec = await dungeons.FirstOrDefaultAsync(x => x.ReSpec.CompareTo(highestReSpec) == 0);

            if (bestDungeonReSpec != null)
            {
                bestDungeonReSpec.IsBestReSpec = true;
            }

            var highestSilver = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.Silver > 0).Select(x => x.Silver).MaxAsync();
            var bestDungeonSilver = await dungeons.FirstOrDefaultAsync(x => x.Silver.CompareTo(highestSilver) == 0);

            if (bestDungeonSilver != null)
            {
                bestDungeonSilver.IsBestSilver = true;
            }

            var highestFactionFlags = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionFlags > 0).Select(x => x.FactionFlags).MaxAsync();
            var bestDungeonFlags = await dungeons.FirstOrDefaultAsync(x => x.FactionFlags.CompareTo(highestFactionFlags) == 0);

            if (bestDungeonFlags != null)
            {
                bestDungeonFlags.IsBestFactionFlags = true;
            }

            var highestFactionCoins = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionCoins > 0).Select(x => x.FactionCoins).MaxAsync();
            var bestDungeonCoins = await dungeons.FirstOrDefaultAsync(x => x.FactionCoins.CompareTo(highestFactionCoins) == 0);

            if (bestDungeonCoins != null)
            {
                bestDungeonCoins.IsBestFactionCoins = true;
            }

            var highestFamePerHour = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FamePerHour > 0).Select(x => x.FamePerHour).MaxAsync();
            var bestDungeonFamePerHour = await dungeons.FirstOrDefaultAsync(x => x.FamePerHour.CompareTo(highestFamePerHour) == 0);

            if (bestDungeonFamePerHour != null)
            {
                bestDungeonFamePerHour.IsBestFamePerHour = true;
            }

            var highestReSpecPerHour = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.ReSpecPerHour > 0).Select(x => x.ReSpecPerHour).MaxAsync();
            var bestDungeonReSpecPerHour = await dungeons.FirstOrDefaultAsync(x => x.ReSpecPerHour.CompareTo(highestReSpecPerHour) == 0);

            if (bestDungeonReSpecPerHour != null)
            {
                bestDungeonReSpecPerHour.IsBestReSpecPerHour = true;
            }

            var highestSilverPerHour = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.SilverPerHour > 0).Select(x => x.SilverPerHour).MaxAsync();
            var bestDungeonSilverPerHour = await dungeons.FirstOrDefaultAsync(x => x.SilverPerHour.CompareTo(highestSilverPerHour) == 0);

            if (bestDungeonSilverPerHour != null)
            {
                bestDungeonSilverPerHour.IsBestSilverPerHour = true;
            }

            var highestFactionFlagsPerHour = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionFlagsPerHour > 0).Select(x => x.FactionFlagsPerHour).MaxAsync();
            var bestDungeonFactionFlagsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionFlagsPerHour.CompareTo(highestFactionFlagsPerHour) == 0);

            if (bestDungeonFactionFlagsPerHour != null)
            {
                bestDungeonFactionFlagsPerHour.IsBestFactionFlagsPerHour = true;
            }

            var highestFactionCoinsPerHour = await dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionCoinsPerHour > 0).Select(x => x.FactionCoinsPerHour).MaxAsync();
            var bestDungeonFactionCoinsPerHour = await dungeons.FirstOrDefaultAsync(x => x.FactionCoinsPerHour.CompareTo(highestFactionCoinsPerHour) == 0);

            if (bestDungeonFactionCoinsPerHour != null)
            {
                bestDungeonFactionCoinsPerHour.IsBestFactionCoinsPerHour = true;
            }
        }
        
        public async Task SetOrUpdateDungeonsDataUiAsync()
        {
            _mainWindowViewModel.DungeonStatsDay.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonStatsTotal.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddYears(-10));

            var orderedDungeon = _dungeons.OrderBy(x => x.EnterDungeonFirstTime).ToList();
            foreach (var dungeonObject in orderedDungeon)
            {
                var dungeonFragment = _mainWindowViewModel.TrackingDungeons.FirstOrDefault(x => x.DungeonHash == dungeonObject.DungeonHash);
                if (dungeonFragment != null && IsDungeonDifferenceToAnother(dungeonObject, dungeonFragment))
                {
                    dungeonFragment.SetValues(dungeonObject);
                    dungeonFragment.DungeonNumber = orderedDungeon.IndexOf(dungeonObject);
                }
                else if(dungeonFragment == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var index = orderedDungeon.IndexOf(dungeonObject);
                        var dunFragment = new DungeonNotificationFragment(index, dungeonObject.GuidList, dungeonObject.MainMapIndex, dungeonObject.EnterDungeonFirstTime);
                        dunFragment.SetValues(dungeonObject);
                        _mainWindowViewModel?.TrackingDungeons.Insert(index, dunFragment);
                    });
                }
            }
            
            await RemoveLeftOverDungeonNotificationFragments().ConfigureAwait(false);
            await SetBestDungeonTimeAsync(_mainWindowViewModel?.TrackingDungeons.ToAsyncEnumerable());
            await CalculateBestDungeonValues(_mainWindowViewModel?.TrackingDungeons.ToAsyncEnumerable());
            await DungeonUiFilteringAsync();

            SetDungeonStatsDayUi();
            SetDungeonStatsTotalUi();
        }

        private static bool IsDungeonDifferenceToAnother(DungeonObject dungeonObject, DungeonNotificationFragment dungeonNotificationFragment)
        {
            if (dungeonObject.TotalRunTimeInSeconds != dungeonNotificationFragment.TotalRunTimeInSeconds 
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
                || dungeonObject.CityFaction != dungeonNotificationFragment.CityFaction)
            {
                return true;
            }

            return false;
        }

        private async Task DungeonUiFilteringAsync()
        {
            await _mainWindowViewModel?.TrackingDungeons?.Where(x => !_mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters?.Contains(x.Mode) ?? x.Status != DungeonStatus.Active)
                // ReSharper disable once ConstantConditionalAccessQualifier
                ?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Collapsed;
                });
            
            await _mainWindowViewModel?.TrackingDungeons?
                .Where(x => _mainWindowViewModel?.DungeonStatsFilter?.DungeonModeFilters.Contains(x.Mode) ?? x.Status == DungeonStatus.Active)
                // ReSharper disable once ConstantConditionalAccessQualifier
                ?.ToAsyncEnumerable().ForEachAsync(d =>
                {
                    d.Visibility = Visibility.Visible;
                });
        }
        
        private async Task RemoveLeftOverDungeonNotificationFragments()
        {
            await foreach (var dungeonFragment in _mainWindowViewModel?.TrackingDungeons?.ToAsyncEnumerable().ConfigureAwait(false) ?? new ConfiguredCancelableAsyncEnumerable<DungeonNotificationFragment>())
            {
                var dungeonObjectFound = _dungeons.Select(x => x.DungeonHash).Contains(dungeonFragment.DungeonHash);
                if (dungeonObjectFound)
                {
                    continue;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindowViewModel?.TrackingDungeons?.Remove(dungeonFragment);
                });
            }
        }

        public async void RemoveDungeonByHashAsync(IEnumerable<string> dungeonHash)
        {
            _dungeons.RemoveAll(x => dungeonHash.Contains(x.DungeonHash));

            foreach (var dungeonFragment in _mainWindowViewModel?.TrackingDungeons?.ToList() ?? new List<DungeonNotificationFragment>())
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _mainWindowViewModel?.TrackingDungeons?.Remove(dungeonFragment);
                });
            }

            await SetOrUpdateDungeonsDataUiAsync();
        }

        public void UpdateDungeonDataUi(DungeonObject dungeon)
        {
            if (dungeon == null)
            {
                return;
            }

            var uiDungeon = _mainWindowViewModel?.TrackingDungeons?.FirstOrDefault(x =>
                x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime.Equals(dungeon.EnterDungeonFirstTime));

            uiDungeon?.SetValues(dungeon);
        }

        private bool AddClusterToExistDungeon(List<DungeonObject> dungeons, Guid? currentGuid, Guid? lastGuid, out DungeonObject dungeon)
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
                var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid)_currentGuid) && x.Status == DungeonStatus.Active);
                dun?.Add(value, valueType, cityFaction);

                UpdateDungeonDataUi(dun);
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

        public void LoadDungeonFromFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    var dungeons = JsonSerializer.Deserialize<List<DungeonObject>>(localItemString) ?? new List<DungeonObject>();
                    _dungeons = dungeons;
                    return;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    _dungeons = new List<DungeonObject>();
                    return;
                }
            }

            _dungeons = new List<DungeonObject>();
        }

        public void SaveDungeonsInFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}";

            try
            {
                var toSaveDungeons = _dungeons.Where(x => x is { Status: DungeonStatus.Done });
                var fileString = JsonSerializer.Serialize(toSaveDungeons);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}