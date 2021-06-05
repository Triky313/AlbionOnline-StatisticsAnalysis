using log4net;
using Newtonsoft.Json;
using PcapDotNet.Base;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class DungeonController
    {
        private const int _maxDungeons = 9999;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;
        private Guid? _currentGuid;
        private Guid? _lastGuid;
        private List<DungeonObject> _dungeons = new List<DungeonObject>();

        public DungeonController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public LocalUserData LocalUserData { get; set; }

        public void AddDungeon(MapType mapType, Guid? mapGuid, string mainMapIndex)
        {
            LeaveDungeonCheck(mapType);
            IsDungeonDoneCheck(mapType);

            if (!IsClusterADungeonCluster(_dungeons, mapType, mapGuid))
            {
                SetOrUpdateDungeonDataToUi();
                return;
            }

            if (mapGuid == null)
            {
                SetOrUpdateDungeonDataToUi();
                return;
            }

            _currentGuid = (Guid)mapGuid;
            var currentGuid = (Guid)_currentGuid;

            try
            {
                AddDungeonRunIfNextMap(currentGuid);
                SetNewStartTimeWhenOneMoreTimeEnter(currentGuid);

                if (_lastGuid != null && !_dungeons.Any(x => x.GuidList.Contains(currentGuid)))
                {
                    AddMapToExistDungeon(_dungeons, currentGuid, (Guid) _lastGuid);
                    _lastGuid = currentGuid;

                    RemoveDungeonsAfterCertainNumber(_dungeons, _maxDungeons);
                    SetCurrentDungeonActive(_dungeons, currentGuid);
                    SetDungeonInformation(currentGuid, mapType);
                    SetOrUpdateDungeonDataToUi();
                    return;
                }

                if (_lastGuid == null && !_mainWindowViewModel.TrackingDungeons.Any(x => x.GuidList.Contains((Guid) mapGuid)) 
                    || (!_mainWindowViewModel.TrackingDungeons.Any(x => x.GuidList.Contains((Guid)mapGuid))))
                {
                    _dungeons.Insert(0, CreateNewDungeon(mainMapIndex, currentGuid));

                    _lastGuid = mapGuid;

                    RemoveDungeonsAfterCertainNumber(_dungeons, _maxDungeons);
                    SetCurrentDungeonActive(_dungeons, currentGuid);
                    SetDungeonInformation(currentGuid, mapType);
                    SetOrUpdateDungeonDataToUi();
                    return;
                }

                SetCurrentDungeonActive(_dungeons, currentGuid);
                _lastGuid = currentGuid;

                SetOrUpdateDungeonDataToUi();
            }
            catch
            {
                _currentGuid = null;
            }
        }

        public void ResetDungeons()
        {
            _dungeons.Clear();
            _mainWindow.Dispatcher?.Invoke(() =>
            {
                _mainWindowViewModel?.TrackingDungeons?.Clear();
            });
        }

        public void SetDungeonChestOpen(int id)
        {
            if (_currentGuid != null)
                try
                {
                    var dun = GetCurrentDungeon((Guid) _currentGuid);
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
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
                }

            SetDungeonStatsDay();
            SetDungeonStatsTotal();
        }

        public void SetDungeonEventObjectInformation(int id, string uniqueName)
        {
            if (_currentGuid != null && uniqueName != null)
            {
                try
                {
                    var dun = GetCurrentDungeon((Guid) _currentGuid);
                    if (dun == null || _currentGuid == null || dun.DungeonEventObjects?.Any(x => x.Id == id) == true)
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
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
                }
            }
        }

        private DungeonObject CreateNewDungeon(string mainMapIndex, Guid guid)
        {
            var dungeon = new DungeonObject()
            {
                MainMapIndex = mainMapIndex,
                EnterDungeonFirstTime = DateTime.UtcNow
            };
            
            dungeon.GuidList.Add(guid);
            dungeon.AddStartTime(DateTime.UtcNow);

            if (dungeon.Mode == DungeonMode.Unknown)
            {
                dungeon.Mode = DungeonObjectData.GetDungeonMode(mainMapIndex);
            }

            return dungeon;
        }

        private DungeonObject GetCurrentDungeon(Guid guid)
        {
            return _dungeons.FirstOrDefault(x => x.GuidList.Contains(guid));
        }

        private void SetDungeonInformation(Guid guid, MapType mapType)
        {
            var dun = GetCurrentDungeon(guid);
            if (dun == null)
            {
                return;
            }

            switch (mapType)
            {
                case MapType.CorruptedDungeon:
                    dun.Faction = Faction.Corrupted;
                    dun.Mode = DungeonMode.Corrupted;
                    return;
                case MapType.HellGate:
                    dun.Faction = Faction.HellGate;
                    dun.Mode = DungeonMode.HellGate;
                    return;
                case MapType.Expedition:
                    dun.Mode = DungeonMode.Expedition;
                    return;
                default:
                    return;
            }
        }

        private int GetChests(DateTime? chestIsNewerAsDateTime, ChestRarity rarity)
        {
            var dungeons = _dungeons.Where(x => x.EnterDungeonFirstTime > chestIsNewerAsDateTime || chestIsNewerAsDateTime == null);
            return dungeons.Select(dun => dun.DungeonEventObjects.Where(x => x.Rarity == rarity)).Select(filteredChests => filteredChests.Count()).Sum();
        }

        public int GetDungeonsCount(DateTime dungeonIsNewerAsDateTime)
        {
            return _dungeons.Count(x => x.EnterDungeonFirstTime > dungeonIsNewerAsDateTime);
        }

        public void SetDungeonStatsDay()
        {
            _mainWindowViewModel.DungeonStatsDay.OpenedStandardChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Standard);
            _mainWindowViewModel.DungeonStatsDay.OpenedUncommonChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Uncommon);
            _mainWindowViewModel.DungeonStatsDay.OpenedRareChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Rare);
            _mainWindowViewModel.DungeonStatsDay.OpenedLegendaryChests = GetChests(DateTime.UtcNow.AddDays(-1), ChestRarity.Legendary);
        }

        public void SetDungeonStatsTotal()
        {
            _mainWindowViewModel.DungeonStatsTotal.OpenedStandardChests = GetChests(null, ChestRarity.Standard);
            _mainWindowViewModel.DungeonStatsTotal.OpenedUncommonChests = GetChests(null, ChestRarity.Uncommon);
            _mainWindowViewModel.DungeonStatsTotal.OpenedRareChests = GetChests(null, ChestRarity.Rare);
            _mainWindowViewModel.DungeonStatsTotal.OpenedLegendaryChests = GetChests(null, ChestRarity.Legendary);
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
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
                }
            }
        }

        private void RemoveDungeonsAfterCertainNumber(List<DungeonObject> dungeons, int amount)
        {
            if (_trackingController.IsMainWindowNull() || dungeons == null) return;

            try
            {
                while (true)
                {
                    if (dungeons.Count <= amount) break;

                    var dateTime = GetLowestDate(dungeons);
                    if (dateTime != null)
                    {
                        var removableItem = dungeons.FirstOrDefault(x => x.EnterDungeonFirstTime == dateTime);
                        if (removableItem != null)
                        {
                            dungeons.Remove(removableItem);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }

        private void SetCurrentDungeonActive(List<DungeonObject> dungeons, Guid? guid)
        {
            if (_dungeons.Count <= 0)
            {
                return;
            }

            dungeons.Where(x => x.Status != DungeonStatus.Done).ToList().ForEach(x => x.Status = DungeonStatus.Done);

            if (guid != null)
            {
                var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid)guid));
                if (dun != null)
                {
                    dun.Status = DungeonStatus.Active;
                }
            }
        }

        private bool IsClusterADungeonCluster(List<DungeonObject> dungeons, MapType mapType, Guid? mapGuid)
        {
            if ((mapType != MapType.RandomDungeon && mapType != MapType.CorruptedDungeon && mapType != MapType.HellGate && mapType != MapType.Expedition) || mapGuid == null)
            {
                if (_lastGuid != null)
                {
                    SetCurrentDungeonActive(dungeons, null);
                }

                _currentGuid = null;
                _lastGuid = null;
                return false;
            }

            return true;
        }

        private void SetBestDungeonTime(ObservableCollection<DungeonNotificationFragment> dungeons)
        {
            if (dungeons?.Any(x => x.Status == DungeonStatus.Done && x.DungeonChests.Any(y => y?.IsBossChest == true)) == true)
            {
                dungeons.Where(x => x?.IsBestTime == true).ToList().ForEach(x => x.IsBestTime = false);
                var min = dungeons.Where(x => x?.DungeonChests.Any(y => y.IsBossChest) == true).Select(x => x.TotalRunTime).Min();
                var bestTimeDungeon = dungeons.FirstOrDefault(x => x.TotalRunTime == min);

                if (bestTimeDungeon != null)
                {
                    bestTimeDungeon.IsBestTime = true;
                }
            }
        }

        private void CalculateBestDungeonValues(ObservableCollection<DungeonNotificationFragment> dungeons)
        {
            try
            {
                dungeons.Where(x =>
                    x?.IsBestFame == true ||
                    x?.IsBestReSpec == true ||
                    x?.IsBestSilver == true ||
                    x?.IsBestFactionFlags == true ||
                    x?.IsBestFactionCoins == true ||
                    x?.IsBestFamePerHour == true ||
                    x?.IsBestReSpecPerHour == true ||
                    x?.IsBestSilverPerHour == true || 
                    x?.IsBestFactionFlagsPerHour == true || 
                    x?.IsBestFactionCoinsPerHour == true
                ).ToList().ForEach(x =>
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
                });

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.Fame > 0))
                {
                    var highestFame = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.Fame > 0).Select(x => x.Fame).Max();
                    var bestDungeonFame = dungeons.FirstOrDefault(x => x.Fame.CompareTo(highestFame) == 0);

                    if (bestDungeonFame != null)
                    {
                        bestDungeonFame.IsBestFame = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.ReSpec > 0))
                {
                    var highestReSpec = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.ReSpec > 0).Select(x => x.ReSpec).Max();
                    var bestDungeonReSpec = dungeons.FirstOrDefault(x => x.ReSpec.CompareTo(highestReSpec) == 0);

                    if (bestDungeonReSpec != null)
                    {
                        bestDungeonReSpec.IsBestReSpec = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.Silver > 0))
                {
                    var highestSilver = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.Silver > 0).Select(x => x.Silver).Max();
                    var bestDungeonSilver = dungeons.FirstOrDefault(x => x.Silver.CompareTo(highestSilver) == 0);

                    if (bestDungeonSilver != null)
                    {
                        bestDungeonSilver.IsBestSilver = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.FactionFlags > 0))
                {
                    var highestFactionFlags = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionFlags > 0).Select(x => x.FactionFlags).Max();
                    var bestDungeonFlags = dungeons.FirstOrDefault(x => x.FactionFlags.CompareTo(highestFactionFlags) == 0);

                    if (bestDungeonFlags != null)
                    {
                        bestDungeonFlags.IsBestFactionFlags = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.FactionCoins > 0))
                {
                    var highestFactionCoins = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionCoins > 0).Select(x => x.FactionCoins).Max();
                    var bestDungeonCoins = dungeons.FirstOrDefault(x => x.FactionCoins.CompareTo(highestFactionCoins) == 0);

                    if (bestDungeonCoins != null)
                    {
                        bestDungeonCoins.IsBestFactionCoins = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.FamePerHour > 0))
                {
                    var highestFamePerHour = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FamePerHour > 0).Select(x => x.FamePerHour).Max();
                    var bestDungeonFamePerHour = dungeons.FirstOrDefault(x => x.FamePerHour.CompareTo(highestFamePerHour) == 0);

                    if (bestDungeonFamePerHour != null)
                    {
                        bestDungeonFamePerHour.IsBestFamePerHour = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.ReSpecPerHour > 0))
                {
                    var highestReSpecPerHour = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.ReSpecPerHour > 0).Select(x => x.ReSpecPerHour).Max();
                    var bestDungeonReSpecPerHour = dungeons.FirstOrDefault(x => x.ReSpecPerHour.CompareTo(highestReSpecPerHour) == 0);

                    if (bestDungeonReSpecPerHour != null)
                    {
                        bestDungeonReSpecPerHour.IsBestReSpecPerHour = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.SilverPerHour > 0))
                {
                    var highestSilverPerHour = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.SilverPerHour > 0).Select(x => x.SilverPerHour).Max();
                    var bestDungeonSilverPerHour = dungeons.FirstOrDefault(x => x.SilverPerHour.CompareTo(highestSilverPerHour) == 0);

                    if (bestDungeonSilverPerHour != null)
                    {
                        bestDungeonSilverPerHour.IsBestSilverPerHour = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.FactionFlagsPerHour > 0))
                {
                    var highestFactionFlagsPerHour = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionFlagsPerHour > 0).Select(x => x.FactionFlagsPerHour).Max();
                    var bestDungeonFactionFlagsPerHour = dungeons.FirstOrDefault(x => x.FactionFlagsPerHour.CompareTo(highestFactionFlagsPerHour) == 0);

                    if (bestDungeonFactionFlagsPerHour != null)
                    {
                        bestDungeonFactionFlagsPerHour.IsBestFactionFlagsPerHour = true;
                    }
                }

                if (dungeons.Any(x => x?.Status == DungeonStatus.Done && x.FactionCoinsPerHour > 0))
                {
                    var highestFactionCoinsPerHour = dungeons.Where(x => x?.Status == DungeonStatus.Done && x.FactionCoinsPerHour > 0).Select(x => x.FactionCoinsPerHour).Max();
                    var bestDungeonFactionCoinsPerHour = dungeons.FirstOrDefault(x => x.FactionCoinsPerHour.CompareTo(highestFactionCoinsPerHour) == 0);

                    if (bestDungeonFactionCoinsPerHour != null)
                    {
                        bestDungeonFactionCoinsPerHour.IsBestFactionCoinsPerHour = true;
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }

        private void AddDungeonRunIfNextMap(Guid currentGuid)
        {
            if (_lastGuid != null && _dungeons.Any(x => x.GuidList.Contains(currentGuid) && x.GuidList.Contains((Guid) _lastGuid)))
            {
                var dun = _dungeons?.First(x => x.GuidList.Contains(currentGuid));
                dun?.AddEndTime(DateTime.UtcNow);
            }
        }

        public void SetOrUpdateDungeonDataToUi()
        {
            _mainWindowViewModel.DungeonStatsDay.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddDays(-1));
            _mainWindowViewModel.DungeonStatsTotal.EnteredDungeon = GetDungeonsCount(DateTime.UtcNow.AddYears(-10));

            var counter = 0;
            foreach (var dungeon in _dungeons.Where(x => x.GuidList.Count > 0).OrderBy(x => x.EnterDungeonFirstTime))
            {
                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    var uiDungeon = _mainWindowViewModel?.TrackingDungeons?.FirstOrDefault(
                        x => x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime == dungeon.EnterDungeonFirstTime);
                    if (uiDungeon != null)
                    {
                        uiDungeon.SetValues(dungeon);
                        uiDungeon.DungeonNumber = ++counter;
                    }
                    else
                    {
                        var dunFragment = new DungeonNotificationFragment(++counter, dungeon.GuidList, dungeon.MainMapIndex, dungeon.EnterDungeonFirstTime);
                        dunFragment.SetValues(dungeon);
                        _mainWindowViewModel?.TrackingDungeons?.Add(dunFragment);
                    }
                });
            }

            _mainWindow.Dispatcher?.Invoke(() =>
            {
                SetBestDungeonTime(_mainWindowViewModel?.TrackingDungeons);
                CalculateBestDungeonValues(_mainWindowViewModel?.TrackingDungeons);
                _mainWindowViewModel?.TrackingDungeons?.OrderByReference(_mainWindowViewModel?.TrackingDungeons?.OrderByDescending(x => x.DungeonNumber).ToList());
            });
        }

        public void UpdateDungeonDataUi(DungeonObject dungeon)
        {
            _mainWindow.Dispatcher?.Invoke(() =>
            {
                var uiDungeon = _mainWindowViewModel?.TrackingDungeons?.FirstOrDefault(
                    x => x.GuidList.Contains(dungeon.GuidList.FirstOrDefault()) && x.EnterDungeonFirstTime.Equals(dungeon.EnterDungeonFirstTime));
                uiDungeon?.SetValues(dungeon);
            });
        }

        private void AddMapToExistDungeon(List<DungeonObject> dungeons, Guid currentGuid, Guid lastGuid)
        {
            var dun = dungeons?.First(x => x.GuidList.Contains(lastGuid));
            dun?.GuidList.Add(currentGuid);
        }

        private void SetNewStartTimeWhenOneMoreTimeEnter(Guid currentGuid)
        {
            if (_dungeons.Any(x => x.GuidList.Contains(currentGuid)))
            {
                var dun = _dungeons?.First(x => x.GuidList.Contains(currentGuid));
                dun?.AddStartTime(DateTime.UtcNow);
            }
        }

        private void LeaveDungeonCheck(MapType mapType)
        {
            if (_lastGuid != null && _dungeons.Any(x => x.GuidList.Contains((Guid) _lastGuid)) && mapType != MapType.RandomDungeon)
            {
                var dun = _dungeons?.First(x => x.GuidList.Contains((Guid) _lastGuid));
                dun?.AddEndTime(DateTime.UtcNow);
            }
        }

        private void IsDungeonDoneCheck(MapType mapType)
        {
            if (_lastGuid != null && _currentGuid == null && _dungeons.Any(x => x.GuidList.Contains((Guid)_lastGuid))
                                  && (mapType != MapType.RandomDungeon && mapType != MapType.CorruptedDungeon && mapType != MapType.HellGate && mapType != MapType.Expedition))
            {
                var dun = _dungeons?.FirstOrDefault(x => x.GuidList.Contains((Guid) _lastGuid) && x.DungeonEventObjects.Any(y => y.IsBossChest));
                if (dun != null)
                {
                    dun.Status = DungeonStatus.Done;
                }
            }
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
            if (items.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                return items.Select(x => x.EnterDungeonFirstTime).Min();
            }
            catch (ArgumentNullException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
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
                    var dungeons = JsonConvert.DeserializeObject<List<DungeonObject>>(localItemString) ?? new List<DungeonObject>();
                    _dungeons = dungeons;
                    return;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
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
                var toSaveDungeons = _dungeons.Where(x => x != null && x.Status == DungeonStatus.Done);
                var fileString = JsonConvert.SerializeObject(toSaveDungeons);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }
    }
}