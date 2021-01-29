using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Common
{
    public class TrackingController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MainWindow _mainWindow;
        private Guid? _lastGuid;
        private Guid? _currentGuid;

        private const int _maxNotifications = 50;

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

        public void SetFameOfCurrentDungeon(double value)
        {
            _mainWindowViewModel.CurrentDungeonFame = value.ToString("N0", LanguageController.CurrentCultureInfo);
        }
        
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

        public void AddDungeon(MapType mapType, Guid? mapGuid)
        {
            LeaveDungeonCheck(mapType);

            if (mapType != MapType.RandomDungeon || mapGuid == null)
            {
                _currentGuid = null;
                _lastGuid = null;
                return;
            }

            try
            {
                _currentGuid = (Guid)mapGuid;

                if (_lastGuid != null && !_mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains((Guid)_currentGuid)))
                {
                    if (_mainWindow.Dispatcher.CheckAccess())
                    {
                        var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains((Guid)_lastGuid));
                        dun.MapsGuid.Add((Guid)_currentGuid);
                    }
                    else
                    {
                        _mainWindow.Dispatcher.Invoke(delegate
                        {
                            var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains((Guid)_lastGuid));
                            dun.MapsGuid.Add((Guid)_currentGuid);
                        });
                    }

                    _lastGuid = _currentGuid;
                    return;
                }

                if (_lastGuid == null && !_mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains((Guid) mapGuid)))
                {
                    if (_mainWindow.Dispatcher.CheckAccess())
                    {
                        _mainWindowViewModel.TrackingDungeons.Insert(0, new DungeonNotificationFragment((Guid)_currentGuid, _mainWindowViewModel.TrackingDungeons.Count + 1));
                    }
                    else
                    {
                        _mainWindow.Dispatcher.Invoke(delegate
                        {
                            _mainWindowViewModel.TrackingDungeons.Insert(0, new DungeonNotificationFragment((Guid)_currentGuid, _mainWindowViewModel.TrackingDungeons.Count + 1));
                        });
                    }

                    _lastGuid = mapGuid;
                    return;
                }

                _lastGuid = _currentGuid;
            }
            catch
            {
                _currentGuid = null;
            }
        }

        private void LeaveDungeonCheck(MapType mapType)
        {
            if (_lastGuid != null && _mainWindowViewModel.TrackingDungeons.Any(x => x.MapsGuid.Contains((Guid)_lastGuid)) && mapType != MapType.RandomDungeon)
            {
                var dun = _mainWindowViewModel.TrackingDungeons?.First(x => x.MapsGuid.Contains((Guid)_lastGuid));
                dun.DungeonEnd = DateTime.UtcNow;
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
                        SetFameOfCurrentDungeon(dun.Fame);
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
                            SetFameOfCurrentDungeon(dun.Fame);
                        }
                    });
                }
            }
            catch
            {
                // ignored
            }
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