using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Common
{
    public class TrackingController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly MainWindow _mainWindow;
        private readonly List<Dungeon> _dungeons = new List<Dungeon>();
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
            _mainWindowViewModel.TotalPlayerFame = value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public void SetTotalPlayerSilver(double value)
        {
            _mainWindowViewModel.TotalPlayerSilver = value.ToString("N0", CultureInfo.CurrentCulture);
        }

        public void SetTotalPlayerReSpecPoints(double value)
        {
            _mainWindowViewModel.TotalPlayerReSpecPoints = value.ToString("N0", CultureInfo.CurrentCulture);
        }


        #endregion

        #region Notifications

        public void AddNotification(TrackingNotification item)
        {
            if (IsMainWindowNull() || _mainWindowViewModel.TrackingNotifications == null)
            {
                return;
            }

            var index = -1;
            var MostRecentDate = GetHighestDate(_mainWindowViewModel.TrackingNotifications);
            try
            {
                if (MostRecentDate != null)
                {
                    var bigger = _mainWindowViewModel.TrackingNotifications.First(x => x.DateTime == MostRecentDate);
                    index = _mainWindowViewModel.TrackingNotifications.IndexOf(bigger);
                }
            }
            catch
            {
                index = _mainWindowViewModel.TrackingNotifications.Count;
            }
            finally
            {
                if (index != -1)
                {
                    if (_mainWindow.Dispatcher.CheckAccess())
                    {
                        _mainWindowViewModel.TrackingNotifications.Insert(index, item);
                    }
                    else
                    {
                        _mainWindow.Dispatcher.Invoke(delegate
                        {
                            _mainWindowViewModel.TrackingNotifications.Insert(index, item);
                        });
                    }
                }
                else
                {
                    if (_mainWindow.Dispatcher.CheckAccess())
                    {
                        _mainWindowViewModel.TrackingNotifications.Add(item);
                    }
                    else
                    {
                        _mainWindow.Dispatcher.Invoke(delegate
                        {
                            _mainWindowViewModel.TrackingNotifications.Add(item);
                        });
                    }
                }
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

        public static DateTime? GetHighestDate(ObservableCollection<TrackingNotification> items)
        {
            if (items.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                var highestDate = items.Select(x => x.DateTime).Max();
                return highestDate;
            }
            catch (ArgumentNullException e)
            {
                Log.Error(nameof(GetHighestDate), e);
                return null;
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
                Log.Error(nameof(GetHighestDate), e);
                return null;
            }
        }


        #endregion

        #region Dungeon

        public void AddDungeon(MapType mapType, Guid? mapGuid)
        {
            if (mapType != MapType.RandomDungeon || mapGuid == null)
            {
                _currentGuid = null;
                _lastGuid = null;
                return;
            }

            try
            {
                _currentGuid = (Guid)mapGuid;

                if (_lastGuid != null && !_dungeons.Any(x => x.MapsGuid.Contains((Guid)_currentGuid)))
                {
                    var dun = _dungeons?.First(x => x.MapsGuid.Contains((Guid)_lastGuid));
                    dun.MapsGuid.Add((Guid)_currentGuid);

                    _lastGuid = _currentGuid;
                    return;
                }

                if (_lastGuid == null && !_dungeons.Any(x => x.MapsGuid.Contains((Guid) mapGuid)))
                {
                    _dungeons.Add(new Dungeon((Guid)_currentGuid));

                    _lastGuid = mapGuid;
                }
            }
            catch
            {
                _currentGuid = null;
            }
        }

        private double? _lastFameValue;

        public void AddFame(double value)
        {
            if (_lastFameValue == null || _currentGuid == null)
            {
                _lastFameValue = value;
                return;
            }

            var newValue = (double)(value - _lastFameValue);

            if (newValue == 0)
            {
                return;
            }

            try
            {
                var dun = _dungeons?.FirstOrDefault(x => x.MapsGuid.Contains((Guid)_currentGuid));
                if (dun != null)
                {
                    dun.Fame += newValue;
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