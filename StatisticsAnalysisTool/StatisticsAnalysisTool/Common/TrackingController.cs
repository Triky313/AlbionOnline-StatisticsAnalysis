using log4net;
using Newtonsoft.Json;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StatisticsAnalysisTool.Common
{
    public class TrackingController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MainWindow _mainWindow;
        private Guid? _lastGuid;
        private Guid? _currentGuid;
        private string _lastMapNameBeforeDungeon;

        private const int _maxNotifications = 50;
        private const int _maxDungeons = 999;

        public TrackingController(MainWindowViewModel mainWindowViewModel, MainWindow mainWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindow = mainWindow;
        }

        #region Set values

        public void SetTotalPlayerFame(double value)
        {
            _mainWindowViewModel.TotalPlayerFame = value.ToString("N0", LanguageController.CurrentCultureInfo);
        }

        public void SetTotalPlayerSilver(double value)
        {
            _mainWindowViewModel.TotalPlayerSilver = value.ToString("N0", LanguageController.CurrentCultureInfo);
        }

        public void SetTotalPlayerReSpecPoints(double value)
        {
            _mainWindowViewModel.TotalPlayerReSpecPoints = value.ToString("N0", LanguageController.CurrentCultureInfo);
        }

        public string CurrentPlayerUsername { get; set; }

        #endregion

        #region Notifications

        public void AddNotification(TrackingNotification item)
        {
            if (IsMainWindowNull() || _mainWindowViewModel.TrackingNotifications == null)
            {
                return;
            }

            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindowViewModel.TrackingNotifications.Insert(0, item);
            }
            else
            {
                _mainWindow.Dispatcher.Invoke(delegate
                {
                    _mainWindowViewModel.TrackingNotifications.Insert(0, item);
                });
            }

            RemovesUnnecessaryNotifications();
        }

        public void RemovesUnnecessaryNotifications()
        {
            if (IsMainWindowNull() || _mainWindowViewModel.TrackingNotifications == null)
            {
                return;
            }

            try
            {
                while (true)
                {
                    if (_mainWindowViewModel.TrackingNotifications?.Count <= _maxNotifications)
                    {
                        break;
                    }

                    var dateTime = GetLowestDate(_mainWindowViewModel.TrackingNotifications);
                    if (dateTime != null)
                    {
                        var removableItem = _mainWindowViewModel.TrackingNotifications?.FirstOrDefault(x => x.DateTime == dateTime);
                        if (removableItem != null)
                        {
                            if (_mainWindow.Dispatcher.CheckAccess())
                            {
                                _mainWindowViewModel.TrackingNotifications.Remove(removableItem);
                            }
                            else
                            {
                                _mainWindow.Dispatcher.Invoke(delegate
                                {
                                    _mainWindowViewModel.TrackingNotifications.Remove(removableItem);
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(RemovesUnnecessaryNotifications), e);
            }
        }

        public static DateTime? GetLowestDate(ObservableCollection<TrackingNotification> items)
        {
            if (items.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                var lowestDate = items.Select(x => x.DateTime).Min();
                return lowestDate;
            }
            catch (ArgumentNullException e)
            {
                Log.Error(nameof(GetLowestDate), e);
                return null;
            }
        }
        
        #endregion

        #region Dungeon

        public void SaveDungeonsInFile(ObservableCollection<DungeonNotificationFragment> dungeons)
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}";

            try
            {
                var toSaveDungeons = dungeons.Where(x => x != null && x.DungeonStatus == DungeonStatus.Done && x.TotalTime.Ticks > 0);
                var fileString = JsonConvert.SerializeObject(toSaveDungeons);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.Error(nameof(SaveDungeonsInFile), e);
            }
        }

        public ObservableCollection<DungeonNotificationFragment> LoadDungeonFromFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.DungeonRunsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    var dungeons = JsonConvert.DeserializeObject<ObservableCollection<DungeonNotificationFragment>>(localItemString);
                    _mainWindowViewModel.EnteredDungeon = dungeons.Count;
                    return dungeons;
                }
                catch (Exception e)
                {
                    Log.Error(nameof(LoadDungeonFromFile), e);
                    return new ObservableCollection<DungeonNotificationFragment>();
                }
            }
            return new ObservableCollection<DungeonNotificationFragment>();
        }

        public void AddDungeon(MapType mapType, Guid? mapGuid, string uniqueMapName)
        {
            LeaveDungeonCheck(mapType);
            SetBestDungeonTime();
            SetBestDungeonFame();
            SetLastMapNameBeforeDungeon(mapType, uniqueMapName);

            if (mapType != MapType.RandomDungeon || mapGuid == null)
            {
                if (_lastGuid != null)
                {
                    SetCurrentDungeonActive((Guid)_lastGuid, true);
                }

                _currentGuid = null;
                _lastGuid = null;
                return;
            }

            try
            {
                _currentGuid = (Guid)mapGuid;
                var currentGuid = (Guid)_currentGuid;

                AddDungeonRunIfNextMap(currentGuid);
                SetNewStartTimeWhenOneMoreTimeEnter(currentGuid);

                if (_lastGuid != null && !_mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains(currentGuid)))
                {
                    AddMapToExistDungeon(currentGuid, (Guid)_lastGuid);

                    _lastGuid = currentGuid;
                    _mainWindowViewModel.EnteredDungeon = _mainWindowViewModel.TrackingDungeons.Count;

                    RemoveDungeonsAfterCertainNumber(_maxDungeons);
                    SetCurrentDungeonActive(currentGuid);
                    return;
                }

                if (_lastGuid == null && !_mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains((Guid)mapGuid)))
                {
                    if (_mainWindow.Dispatcher.CheckAccess())
                    {
                        _mainWindowViewModel.TrackingDungeons.Insert(
                            0, 
                            new DungeonNotificationFragment(currentGuid, _mainWindowViewModel.TrackingDungeons.Count + 1, _lastMapNameBeforeDungeon, DateTime.UtcNow, _mainWindowViewModel));
                    }
                    else
                    {
                        _mainWindow.Dispatcher.Invoke(delegate
                        {
                            _mainWindowViewModel.TrackingDungeons.Insert(
                                0, 
                                new DungeonNotificationFragment(currentGuid, _mainWindowViewModel.TrackingDungeons.Count + 1, _lastMapNameBeforeDungeon, DateTime.UtcNow, _mainWindowViewModel));
                        });
                    }

                    _lastGuid = mapGuid;
                    _mainWindowViewModel.EnteredDungeon = _mainWindowViewModel.TrackingDungeons.Count;

                    RemoveDungeonsAfterCertainNumber(_maxDungeons);
                    SetCurrentDungeonActive(currentGuid);
                    return;
                }

                SetCurrentDungeonActive(currentGuid);
                _lastGuid = currentGuid;
            }
            catch
            {
                _currentGuid = null;
            }
        }

        public void SetDiedIfInDungeon(DiedObject dieObject)
        {
            if (_currentGuid != null && CurrentPlayerUsername != null && dieObject.DiedName == CurrentPlayerUsername)
            {
                try
                {
                    var item = _mainWindowViewModel.TrackingDungeons.First(x => x.MapsGuid.Contains((Guid)_currentGuid) && x.StartDungeon > DateTime.UtcNow.AddDays(-1));
                    item.DiedMessage = $"{dieObject.DiedName} {LanguageController.Translation("KILLED_BY")} {dieObject.KilledBy}";
                    item.DiedInDungeon = true;
                }
                catch (Exception e)
                {
                    Log.Error(nameof(SetDiedIfInDungeon), e);
                }
            }
        }

        private double TotalFameByTime(FameCountMode mode)
        {
            switch (mode)
            {
                case FameCountMode.Hour:
                    return _mainWindowViewModel.TrackingDungeons.Where(x => x.StartDungeon > DateTime.UtcNow.AddHours(-1)).Sum(x => x.Fame);
                case FameCountMode.Day:
                    return _mainWindowViewModel.TrackingDungeons.Where(x => x.StartDungeon > DateTime.UtcNow.AddDays(-1)).Sum(x => x.Fame);
                case FameCountMode.Week:
                    return _mainWindowViewModel.TrackingDungeons.Where(x => x.StartDungeon > DateTime.UtcNow.AddDays(-7)).Sum(x => x.Fame);
                case FameCountMode.Total:
                    return _mainWindowViewModel.TrackingDungeons.Where(x => true).Sum(x => x.Fame);
                default:
                    return 0;
            }
        }

        private void RemoveDungeonsAfterCertainNumber(int amount)
        {
            if (IsMainWindowNull() || _mainWindowViewModel.TrackingDungeons == null)
            {
                return;
            }

            try
            {
                while (true)
                {
                    if (_mainWindowViewModel.TrackingNotifications?.Count <= amount)
                    {
                        break;
                    }

                    var dateTime = GetLowestDate(_mainWindowViewModel.TrackingDungeons);
                    if (dateTime != null)
                    {
                        var removableItem = _mainWindowViewModel.TrackingDungeons?.FirstOrDefault(x => x.StartDungeon == dateTime);
                        if (removableItem != null)
                        {
                            if (_mainWindow.Dispatcher.CheckAccess())
                            {
                                _mainWindowViewModel.TrackingDungeons.Remove(removableItem);
                            }
                            else
                            {
                                _mainWindow.Dispatcher.Invoke(delegate
                                {
                                    _mainWindowViewModel.TrackingDungeons.Remove(removableItem);
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(RemoveDungeonsAfterCertainNumber), e);
            }
        }

        private void SetLastMapNameBeforeDungeon(MapType mapType, string uniqueMapName)
        {
            if (mapType != MapType.HellGate && mapType != MapType.Island && mapType != MapType.CorruptedDungeon && mapType != MapType.RandomDungeon)
            {
                _lastMapNameBeforeDungeon = uniqueMapName;
            }
        }

        private void SetCurrentDungeonActive(Guid guid, bool allToFalse = false)
        {
            if (_mainWindowViewModel.TrackingDungeons.Count <= 0)
            {
                return;
            }

            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindowViewModel.TrackingDungeons.Where(x => x.DungeonStatus != DungeonStatus.Done).ToList().ForEach(x => x.DungeonStatus = DungeonStatus.Done);

                var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(guid));
                if (!allToFalse)
                {
                    dun.DungeonStatus = DungeonStatus.Active;
                }
            }
            else
            {
                _mainWindow.Dispatcher.Invoke(delegate
                {
                    _mainWindowViewModel.TrackingDungeons.Where(x => x.DungeonStatus != DungeonStatus.Done).ToList().ForEach(x => x.DungeonStatus = DungeonStatus.Done);

                    var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(guid));
                    if (!allToFalse)
                    {
                        dun.DungeonStatus = DungeonStatus.Active;
                    }
                });
            }
        }

        private void SetBestDungeonTime()
        {
            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindowViewModel.TrackingDungeons.Where(x => x?.IsBestTime == true).ToList().ForEach(x => x.IsBestTime = false);
                var highest = _mainWindowViewModel.TrackingDungeons.Select(x => x?.TotalTime).Min();
                var bestTimeDungeon = _mainWindowViewModel?.TrackingDungeons?.SingleOrDefault(x => x.TotalTime == highest);
                if (bestTimeDungeon != null)
                {
                    bestTimeDungeon.IsBestTime = true;
                }
            }
            else
            {
                _mainWindow.Dispatcher.Invoke(delegate
                {
                    _mainWindowViewModel.TrackingDungeons.Where(x => x?.IsBestTime == true).ToList().ForEach(x => x.IsBestTime = false);
                    var highest = _mainWindowViewModel.TrackingDungeons.Select(x => x?.TotalTime).Min();
                    var bestTimeDungeon = _mainWindowViewModel?.TrackingDungeons?.SingleOrDefault(x => x.TotalTime == highest);
                    if (bestTimeDungeon != null)
                    {
                        bestTimeDungeon.IsBestTime = true;
                    }
                });
            }
        }

        private void SetBestDungeonFame()
        {
            if (_mainWindow.Dispatcher.CheckAccess())
            {
                _mainWindowViewModel.TrackingDungeons.Where(x => x?.IsBestFame == true).ToList().ForEach(x => x.IsBestFame = false);
                var highest = _mainWindowViewModel.TrackingDungeons.Select(x => x?.Fame).Max() ?? -1;
                var bestDungeonFame = _mainWindowViewModel?.TrackingDungeons?.SingleOrDefault(x => x.Fame.CompareTo(highest) == 0);
                if (bestDungeonFame != null)
                {
                    bestDungeonFame.IsBestFame = true;
                }
            }
            else
            {
                _mainWindow.Dispatcher.Invoke(delegate
                {
                    _mainWindowViewModel.TrackingDungeons.Where(x => x?.IsBestFame == true).ToList().ForEach(x => x.IsBestFame = false);
                    var highest = _mainWindowViewModel.TrackingDungeons.Select(x => x?.Fame).Max();
                    var bestDungeonFame = _mainWindowViewModel?.TrackingDungeons?.SingleOrDefault(x => x.Fame.CompareTo(highest) == 0);
                    if (bestDungeonFame != null)
                    {
                        bestDungeonFame.IsBestFame = true;
                    }
                });
            }
        }

        private void AddDungeonRunIfNextMap(Guid currentGuid)
        {
            if (_lastGuid != null && _mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains(currentGuid) && x.MapsGuid.Contains((Guid)_lastGuid)))
            {
                var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(currentGuid));
                dun.AddDungeonRun(DateTime.UtcNow);
            }
        }
        
        private void SetNewStartTimeWhenOneMoreTimeEnter(Guid currentGuid)
        {
            if (_mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains(currentGuid)))
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                {
                    var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(currentGuid));
                    dun.EnterDungeonMap = DateTime.UtcNow;
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(delegate
                    {
                        var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(currentGuid));
                        dun.EnterDungeonMap = DateTime.UtcNow;
                    });
                }
            }
        }
        
        private void AddMapToExistDungeon(Guid currentGuid, Guid lastGuid)
        {
            if (_mainWindow.Dispatcher.CheckAccess())
            {
                var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(lastGuid));
                dun?.MapsGuid.Add(currentGuid);
            }
            else
            {
                _mainWindow.Dispatcher.Invoke(delegate
                {
                    var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains(lastGuid));
                    dun?.MapsGuid.Add(currentGuid);
                });
            }
        }

        private void LeaveDungeonCheck(MapType mapType)
        {
            if (_lastGuid != null && _mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains((Guid)_lastGuid)) && mapType != MapType.RandomDungeon)
            {
                var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains((Guid)_lastGuid));
                dun.AddDungeonRun(DateTime.UtcNow);
            }
        }

        public void AddFame(double value)
        {
            if (_currentGuid == null)
            {
                return;
            }

            try
            {
                if (_mainWindow.Dispatcher.CheckAccess())
                {
                    var dun = _mainWindowViewModel.TrackingDungeons?.FirstOrDefault(x => x.MapsGuid.Contains((Guid)_currentGuid));
                    if (dun != null)
                    {
                        dun.Fame += value;
                    }
                }
                else
                {
                    _mainWindow.Dispatcher.Invoke(delegate
                    {
                        var dun = _mainWindowViewModel.TrackingDungeons?.FirstOrDefault(x => x.MapsGuid.Contains((Guid)_currentGuid));
                        if (dun != null)
                        {
                            dun.Fame += value;
                        }
                    });
                }
            }
            catch
            {
                // ignored
            }
        }

        public static DateTime? GetLowestDate(ObservableCollection<DungeonNotificationFragment> items)
        {
            if (items.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                return items.Select(x => x.StartDungeon).Min();
            }
            catch (ArgumentNullException e)
            {
                Log.Error(nameof(GetLowestDate), e);
                return null;
            }
        }

        enum FameCountMode
        {
            Hour,
            Day,
            Week,
            Total
        }

        #endregion

        private bool IsMainWindowNull()
        {
            if (_mainWindow != null)
            {
                return false;
            }

            Log.Error($"{nameof(AddNotification)}: _mainWindow is null.");
            return true;
        }
    }
}