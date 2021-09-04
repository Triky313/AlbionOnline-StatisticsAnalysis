using log4net;
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

namespace StatisticsAnalysisTool.Network.Manager
{
    public class TrackingController : ITrackingController
    {
        private const int _maxNotifications = 1000;
        private const int _maxEnteredCluster = 500;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _lastClusterHash;
        public CountUpTimer CountUpTimer;
        public CombatController CombatController;
        public DungeonController DungeonController;
        public EntityController EntityController;
        public LootController LootController;
        private readonly List<NotificationType> _notificationTypeFilters = new();
        private readonly List<TrackingNotification> _notifications = new();

        public TrackingController(MainWindowViewModel mainWindowViewModel, MainWindow mainWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindow = mainWindow;
            EntityController = new EntityController(mainWindowViewModel);
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
            OnChangeCluster += UpdateClusterTracking;
        }

        public void UnregisterEvents()
        {
            EntityController.OnHealthUpdate -= DamageMeterUpdate;
            OnChangeCluster -= UpdateClusterTracking;
        }

        public void DamageMeterUpdate(long objectId, GameTimeStamp timeStamp, double healthChange, double newHealthValue, EffectType effectType,
            EffectOrigin effectOrigin, long causerId, int causingSpellType)
        {
            CombatController.AddDamageAsync(objectId, causerId, healthChange, newHealthValue);
        }

        public event Action<ClusterInfo> OnChangeCluster;

        #endregion

        public bool ExistIndispensableInfos => CurrentCluster != null && EntityController.ExistLocalEntity();

        #region Cluster

        public void SetNewCluster(MapType mapType, Guid? mapGuid, string clusterIndex, string mainClusterIndex)
        {
            CurrentCluster = WorldData.GetClusterInfoByIndex(clusterIndex, mainClusterIndex, mapType, mapGuid);
            CurrentCluster.Entered = DateTime.UtcNow;

            if (!TryChangeCluster(CurrentCluster.Index, CurrentCluster.UniqueName))
            {
                return;
            }

            if (_mainWindowViewModel.IsDamageMeterResetByMapChangeActive)
            {
                CombatController.ResetDamageMeter();
            }

            CombatController.LastPlayersHealth.Clear();

            Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterType}' MapType: '{CurrentCluster.MapType}'");
            ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType,
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

        private async void UpdateClusterTracking(ClusterInfo currentCluster)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel.EnteredCluster.Insert(0, currentCluster);
                RemovesClusterIfMoreThanLimit();
            });
        }
        
        public void RemovesClusterIfMoreThanLimit()
        {
            foreach (var cluster in _mainWindowViewModel.EnteredCluster.OrderBy(x => x.Entered))
            {
                if (_mainWindowViewModel.EnteredCluster?.Count <= _maxEnteredCluster)
                {
                    break;
                }

                _mainWindowViewModel.EnteredCluster.Remove(cluster);
            }
        }

        #endregion

        #region Tracking Controller Helper

        public bool IsMainWindowNull()
        {
            if (_mainWindow != null)
            {
                return false;
            }

            Log.Error($"{MethodBase.GetCurrentMethod()?.DeclaringType}: _mainWindow is null.");
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

            RemovesUnnecessaryNotifications();

            if (_notificationTypeFilters.Contains(item.Type))
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    _mainWindowViewModel.TrackingNotifications.Insert(0, item);
                });
            }
        }

        public void RemovesUnnecessaryNotifications()
        {

            foreach (var notification in _notifications.OrderBy(x => x.DateTime))
            {
                if (_notifications?.Count <= _maxNotifications)
                {
                    break;
                }

                _notifications.Remove(notification);
            }
        }

        public void ClearNotifications()
        {
            _notifications.Clear();

            Application.Current.Dispatcher.Invoke(delegate
            {
                _mainWindowViewModel.TrackingNotifications.Clear();
            });
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
                _mainWindowViewModel.TrackingNotifications = new ObservableCollection<TrackingNotification>(filteredNotifications);
                _mainWindowViewModel?.TrackingNotifications?
                    .OrderByReference(filteredNotifications.OrderByDescending(x => x.DateTime)
                        .ToList());
            });
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