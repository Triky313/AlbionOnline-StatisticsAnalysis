using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class TrackingController
    {
        private const int _maxNotifications = 1000;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _lastClusterHash;
        public CountUpTimer CountUpTimer;
        public CombatController CombatController;
        public DungeonController DungeonController;
        public EntityController EntityController;
        public LootController LootController;
        private readonly List<NotificationType> _notificationTypeFilters = new List<NotificationType>();
        private readonly List<TrackingNotification> _notifications = new List<TrackingNotification>();

        public TrackingController(MainWindowViewModel mainWindowViewModel, MainWindow mainWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindow = mainWindow;
            EntityController = new EntityController(_mainWindow, mainWindowViewModel);
            DungeonController = new DungeonController(this, mainWindowViewModel);
            CombatController = new CombatController(this, _mainWindow, mainWindowViewModel);
            LootController = new LootController(this);
            CountUpTimer = new CountUpTimer(mainWindowViewModel);
        }

        public ClusterInfo CurrentCluster { get; private set; }

        #region Trigger events

        public void RegisterEvents()
        {
            EntityController.OnHealthUpdate += DamageMeterUpdate;
        }

        public void UnregisterEvents()
        {
            EntityController.OnHealthUpdate -= DamageMeterUpdate;
        }

        public void DamageMeterUpdate(long objectId, GameTimeStamp timeStamp, double healthChange, double newHealthValue, EffectType effectType,
            EffectOrigin effectOrigin, long causerId, int causingSpellType)
        {
            CombatController.AddDamageAsync(causerId, healthChange);
        }

        public event Action<ClusterInfo> OnChangeCluster;

        #endregion

        public bool ExistIndispensableInfos => CurrentCluster != null && EntityController.ExistLocalEntity();

        #region Cluster

        public void SetNewCluster(MapType mapType, Guid? mapGuid, string clusterIndex, string mainClusterIndex)
        {
            CurrentCluster = WorldData.GetClusterInfoByIndex(clusterIndex, mainClusterIndex, mapType, mapGuid);

            if (!TryChangeCluster(CurrentCluster.Index, CurrentCluster.UniqueName))
            {
                return;
            }

            if (_mainWindowViewModel.IsDamageMeterResetByMapChangeActive)
            {
                CombatController.ResetDamageMeter();
            }

            Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterType}' MapType: '{CurrentCluster.MapType}'");
            ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod().DeclaringType,
                $"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterType}' MapType: '{CurrentCluster.MapType}'",
                ConsoleManager.EventMapChangeColor);

            OnChangeCluster?.Invoke(CurrentCluster);
        }

        private bool TryChangeCluster(string index, string mapName)
        {
            var newClusterHash = index + mapName;

            if (_lastClusterHash == newClusterHash) return false;

            _lastClusterHash = newClusterHash;
            return true;
        }

        #endregion

        #region Tracking Controller Helper

        public bool IsMainWindowNull()
        {
            if (_mainWindow != null) return false;

            Log.Error($"{MethodBase.GetCurrentMethod().DeclaringType}: _mainWindow is null.");
            return true;
        }

        #endregion

        #region Notifications

        public void AddNotification(TrackingNotification item)
        {
            if (IsMainWindowNull() || _mainWindowViewModel.TrackingNotifications == null || _notifications == null)
            {
                return;
            }

            _notifications.Insert(0, item);

            Application.Current.Dispatcher.Invoke(delegate
            {
                _mainWindowViewModel.TrackingNotifications.Insert(0, item);
            });

            RemovesUnnecessaryNotifications();
        }

        public void RemovesUnnecessaryNotifications()
        {
            if (IsMainWindowNull() || _notifications == null) return;

            try
            {
                while (true)
                {
                    if (_notifications?.Count <= _maxNotifications) break;

                    var dateTime = GetLowestDate(_notifications);
                    if (dateTime != null)
                    {
                        var removableItem = _notifications?.FirstOrDefault(x => x.DateTime == dateTime);
                        if (removableItem != null)
                        {
                            if (_mainWindow.Dispatcher.CheckAccess())
                            {
                                _notifications.Remove(removableItem);
                            }
                            else
                            {
                                _mainWindow.Dispatcher.Invoke(delegate { _notifications.Remove(removableItem); });
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

        public void FilterNotification()
        {
            var filteredNotifications = _notifications?
                .Where(x => _notificationTypeFilters.Contains(x.Type))
                .ToList();

            if (filteredNotifications == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(delegate
            {
                _mainWindowViewModel.TrackingNotifications = new ObservableCollection<TrackingNotification>(filteredNotifications.ToList());
                _mainWindowViewModel?.TrackingNotifications?
                    .OrderByReference(filteredNotifications.OrderByDescending(x => x.DateTime)
                        .ToList());
            });
        }
        private static DateTime? GetLowestDate(List<TrackingNotification> items)
        {
            if (items.IsNullOrEmpty()) return null;

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

        public void AddFilterType(NotificationType notificationType)
        {
            if (!_notificationTypeFilters.Exists(x => x == notificationType))
            {
                _notificationTypeFilters.Add(notificationType);
            }
        }

        public void RemoveFilterType(NotificationType notificationType)
        {
            if (_notificationTypeFilters.Exists(x => x == notificationType))
            {
                _notificationTypeFilters.Remove(notificationType);
            }
        }

        #endregion
    }
}