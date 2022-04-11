using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class TrackingController : ITrackingController
    {
        private const int MaxNotifications = 4000;
        private const int MaxEnteredCluster = 500;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _lastClusterHash;
        public CountUpTimer CountUpTimer;
        public CombatController CombatController;
        public DungeonController DungeonController;
        public EntityController EntityController;
        public LootController LootController;
        public StatisticController StatisticController;
        public MailController MailController;
        private readonly List<NotificationType> _notificationTypesFilters = new();

        public TrackingController(MainWindowViewModel mainWindowViewModel, MainWindow mainWindow)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _mainWindow = mainWindow;
            EntityController = new EntityController(this, mainWindowViewModel);
            DungeonController = new DungeonController(this, mainWindowViewModel);
            CombatController = new CombatController(this, _mainWindow, mainWindowViewModel);
            LootController = new LootController(this, mainWindowViewModel);
            StatisticController = new StatisticController(this, mainWindowViewModel);
            MailController = new MailController(this, mainWindowViewModel);
            CountUpTimer = new CountUpTimer(this, mainWindowViewModel);
        }

        public ClusterInfo CurrentCluster { get; private set; }

        #region Trigger events

        public void RegisterEvents()
        {
            OnChangeCluster += UpdateClusterTracking;
        }

        public void UnregisterEvents()
        {
            OnChangeCluster -= UpdateClusterTracking;
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

            if (IsTrackingAllowedByMainCharacter())
            {
                OnChangeCluster?.Invoke(CurrentCluster);
            }

            StatisticController.SetKillsDeathsValues();
        }

        private bool TryChangeCluster(string index, string mapName)
        {
            var newClusterHash = index + mapName;

            if (_lastClusterHash == newClusterHash)
            {
                return false;
            }

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

        private void RemovesClusterIfMoreThanLimit()
        {
            foreach (var cluster in _mainWindowViewModel.EnteredCluster.OrderBy(x => x.Entered))
            {
                if (_mainWindowViewModel.EnteredCluster?.Count <= MaxEnteredCluster)
                {
                    break;
                }

                _ = _mainWindowViewModel.EnteredCluster.Remove(cluster);
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

        public async Task AddNotificationAsync(TrackingNotification item)
        {
            item.SetType();

            if (!IsTrackingAllowedByMainCharacter() && item.Type == NotificationType.Fame || !IsTrackingAllowedByMainCharacter() && item.Type == NotificationType.Silver 
                || !IsTrackingAllowedByMainCharacter() && item.Type == NotificationType.Faction)
            {
                return;
            }

            if (_mainWindowViewModel?.TrackingNotifications == null)
            {
                return;
            }

            if (!_mainWindowViewModel.IsTrackingFame && item.Type == NotificationType.Fame)
            {
                return;
            }

            if (!_mainWindowViewModel.IsTrackingSilver && item.Type == NotificationType.Silver)
            {
                return;
            }

            if (!_mainWindowViewModel.IsTrackingMobLoot && item.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: true })
            {
                return;
            }

            SetNotificationFilteredVisibility(item);

            await Application.Current.Dispatcher.InvokeAsync(delegate
            {
                _mainWindowViewModel.TrackingNotifications.Insert(0, item);
            });

            await RemovesUnnecessaryNotificationsAsync();
            await SetNotificationTypesAsync();
        }

        public async Task RemovesUnnecessaryNotificationsAsync()
        {
            if (!IsRemovesUnnecessaryNotificationsActiveAllowed())
            {
                return;
            }

            _isRemovesUnnecessaryNotificationsActive = true;

            var numberToBeRemoved = _mainWindowViewModel.TrackingNotifications.Count - MaxNotifications;
            if (numberToBeRemoved > 0)
            {
                await foreach (var notification in _mainWindowViewModel.TrackingNotifications.OrderBy(x => x.DateTime).ToList().Take(numberToBeRemoved).ToAsyncEnumerable())
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _ = _mainWindowViewModel.TrackingNotifications.Remove(notification);
                    });
                }
            }

            _isRemovesUnnecessaryNotificationsActive = false;
        }

        public async Task ClearNotificationsAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel.TrackingNotifications.Clear();
            });
        }

        public async Task NotificationUiFilteringAsync(string text = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    await _mainWindowViewModel?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                    {
                        d.Visibility = Visibility.Collapsed;
                    })!;

                    await _mainWindowViewModel?.TrackingNotifications?.ToAsyncEnumerable().Where(x =>
                        (_notificationTypesFilters?.Contains(x.Type) ?? true)
                        &&
                        (
                            x.Fragment is OtherGrabbedLootNotificationFragment fragment &&
                            (fragment.Looter.ToLower().Contains(text.ToLower())
                             || fragment.LocalizedName.ToLower().Contains(text.ToLower())
                             || fragment.LootedPlayer.ToLower().Contains(text.ToLower())
                            )
                            ||
                            x.Fragment is KillNotificationFragment killFragment &&
                            (killFragment.Died.ToLower().Contains(text.ToLower())
                             || killFragment.KilledBy.ToLower().Contains(text.ToLower())
                            )
                        )
                        && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                    ).ForEachAsync(d =>
                    {
                        d.Visibility = Visibility.Visible;
                    })!;
                }
                else
                {
                    await _mainWindowViewModel?.TrackingNotifications?.ToAsyncEnumerable().ForEachAsync(d =>
                    {
                        d.Visibility = Visibility.Collapsed;
                    })!;

                    await _mainWindowViewModel?.TrackingNotifications?.Where(x =>
                        (_notificationTypesFilters?.Contains(x.Type) ?? false)
                        && (IsLootFromMobShown || x.Fragment is OtherGrabbedLootNotificationFragment { IsLootedPlayerMob: false } or not OtherGrabbedLootNotificationFragment)
                    ).ToAsyncEnumerable().ForEachAsync(d =>
                    {
                        d.Visibility = Visibility.Visible;
                    })!;
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void SetNotificationFilteredVisibility(TrackingNotification trackingNotification)
        {
            trackingNotification.Visibility = IsNotificationFiltered(trackingNotification) ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool IsNotificationFiltered(TrackingNotification trackingNotification)
        {
            return !_notificationTypesFilters?.Exists(x => x == trackingNotification.Type) ?? false;
        }

        public void AddFilterType(NotificationType notificationType)
        {
            if (!_notificationTypesFilters.Exists(x => x == notificationType))
            {
                _notificationTypesFilters.Add(notificationType);
            }
        }

        public void RemoveFilterType(NotificationType notificationType)
        {
            if (_notificationTypesFilters.Exists(x => x == notificationType))
            {
                _ = _notificationTypesFilters.Remove(notificationType);
            }
        }

        public bool IsLootFromMobShown { get; set; }

        public async Task SetNotificationTypesAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await foreach (var notification in _mainWindowViewModel.TrackingNotifications.ToAsyncEnumerable())
                {
                    notification.SetType();
                }
            });
        }

        private static bool _isRemovesUnnecessaryNotificationsActive;
        private DateTime _lastRemovesUnnecessaryNotifications;

        private bool IsRemovesUnnecessaryNotificationsActiveAllowed(int waitTimeInSeconds = 1)
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastRemovesUnnecessaryNotifications);
            if (difference.Seconds >= waitTimeInSeconds && !_isRemovesUnnecessaryNotificationsActive)
            {
                _lastRemovesUnnecessaryNotifications = currentDateTime;
                return true;
            }

            return false;
        }

        #endregion

        #region Specific character name tracking
        
        public bool IsTrackingAllowedByMainCharacter()
        {
            var localEntity = EntityController.GetLocalEntity();

            if (localEntity?.Value?.Name == null || string.IsNullOrEmpty(SettingsController.CurrentSettings.MainTrackingCharacterName))
            {
                return true;
            }

            if (localEntity.Value.Value.Name == SettingsController.CurrentSettings.MainTrackingCharacterName)
            {
                return true;
            }

            if (localEntity.Value.Value.Name != SettingsController.CurrentSettings.MainTrackingCharacterName)
            {
                return false;
            }

            return true;
        }
        
        #endregion
    }
}