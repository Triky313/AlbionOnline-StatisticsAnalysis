using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class ClusterController
    {
        private const int MaxEnteredCluster = 500;

        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public ClusterInfo CurrentCluster { get; } = new();

        public ClusterController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void RegisterEvents()
        {
            OnChangeCluster += UpdateClusterTracking;
            OnChangeCluster += SetAndResetValues;
        }

        public void UnregisterEvents()
        {
            OnChangeCluster -= UpdateClusterTracking;
            OnChangeCluster -= SetAndResetValues;
        }

        public event Action<ClusterInfo> OnChangeCluster;

        public void SetAndResetValues(ClusterInfo currentCluster)
        {
            _trackingController.CombatController.ResetDamageMeterByClusterChange();
            _trackingController.StatisticController.SetKillsDeathsValues();
            _trackingController.VaultController.ResetDiscoveredItems();
            _trackingController.VaultController.ResetVaultContainer();
            _trackingController.VaultController.ResetCurrentVaultInfo();
        }

        public void ChangeClusterInformation(MapType mapType, Guid? mapGuid, string clusterIndex, string islandName, string worldMapDataType, byte[] dungeonInformation)
        {
            CurrentCluster.Entered = DateTime.UtcNow;
            CurrentCluster.MapType = mapType;
            CurrentCluster.Guid = mapGuid;
            CurrentCluster.Index = clusterIndex;
            CurrentCluster.IslandName = islandName;
            CurrentCluster.WorldMapDataType = worldMapDataType;
            CurrentCluster.DungeonInformation = dungeonInformation;

            if (_trackingController.IsTrackingAllowedByMainCharacter())
            {
                OnChangeCluster?.Invoke(CurrentCluster);
            }

            Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterType}' MapType: '{CurrentCluster.MapType}'");
            ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType,
                $"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterType}' MapType: '{CurrentCluster.MapType}'",
                ConsoleManager.EventMapChangeColor);
        }

        public void SetJoinClusterInformation(string index, string mainClusterIndex)
        {
            CurrentCluster.MainClusterIndex = mainClusterIndex;
            CurrentCluster.UniqueName = WorldData.GetUniqueNameOrDefault(index);
            CurrentCluster.Type = WorldData.GetTypeByIndex(index) ?? WorldData.GetTypeByIndex(mainClusterIndex) ?? string.Empty;
            CurrentCluster.File = WorldData.GetFileByIndex(index) ?? WorldData.GetFileByIndex(mainClusterIndex) ?? string.Empty;
        }

        #region Cluster history

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
    }
}