using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.Cluster;

public class ClusterController
{
    private const int MaxEnteredCluster = 500;

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public static ClusterInfo CurrentCluster { get; } = new();

    public ClusterController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        CreateRandomClusterInfosForTracking(0);
    }

    public void RegisterEvents()
    {
        OnChangeCluster += UpdateClusterTracking;
        OnChangeCluster += SetAndResetValues;
        OnChangeCluster += UpdateUserInfoUi;
    }

    public void UnregisterEvents()
    {
        OnChangeCluster -= UpdateClusterTracking;
        OnChangeCluster -= SetAndResetValues;
        OnChangeCluster -= UpdateUserInfoUi;
    }

    public event Action<ClusterInfo> OnChangeCluster;

    public void ChangeClusterInformation(MapType mapType, Guid? mapGuid, string clusterIndex, string instanceName, string worldMapDataType, byte[] dungeonInformation, string mainClusterIndex, Tier mistsDungeonTier)
    {
        CurrentCluster.ClusterInfoFullyAvailable = false;
        CurrentCluster.SetClusterInfo(mapType, mapGuid, clusterIndex, instanceName, worldMapDataType, dungeonInformation, mainClusterIndex, mistsDungeonTier);
    }

    public void SetJoinClusterInformation(string index, string mainClusterIndex)
    {
        CurrentCluster.SetJoinClusterInfo(index, mainClusterIndex);
        CurrentCluster.ClusterInfoFullyAvailable = true;

        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            OnChangeCluster?.Invoke(CurrentCluster);
        }

        Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterMode}' MapType: '{CurrentCluster.MapType}'");
        ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType,
            $"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterMode}' MapType: '{CurrentCluster.MapType}'",
            ConsoleColorType.EventMapChangeColor);
    }

    public void SetAndResetValues(ClusterInfo currentCluster)
    {
        _mainWindowViewModel.DamageMeterBindings.GetSnapshot(_mainWindowViewModel.DamageMeterBindings.IsSnapshotAfterMapChangeActive);
        _trackingController.CombatController.ResetDamageMeterByClusterChange();
        _trackingController.StatisticController.SetKillsDeathsValues();
        _trackingController.VaultController.ResetDiscoveredItems();
        _trackingController.VaultController.ResetVaultContainer();
        _trackingController.VaultController.ResetCurrentVaultInfo();
        _trackingController.TreasureController.RemoveTemporaryTreasures();
        _trackingController.TreasureController.UpdateLootedChestsDashboardUi();
        _trackingController.LootController.ResetLocalPlayerDiscoveredLoot();
        _trackingController.LootController.ResetIdentifiedBodies();
        _ = _trackingController.TradeController.RemoveTradesByDaysInSettingsAsync();
        _ = _trackingController.GatheringController.SetGatheredResourcesClosedAsync();
        _trackingController.PartyBuilderController.UpdateIsPlayerInspectedToFalse();
    }

    public static string ComposingMapInfoString(string index, MapType mapType, string instanceName)
    {
        var currentMapName = WorldData.GetUniqueNameOrDefault(index);

        if (string.IsNullOrEmpty(currentMapName))
        {
            currentMapName = WorldData.GetMapNameByMapType(mapType);
        }

        string islandName = !string.IsNullOrEmpty(instanceName) ? $"({instanceName})" : string.Empty;

        return $"{currentMapName} {islandName}";
    }

    #region Cluster history

    private async void UpdateClusterTracking(ClusterInfo currentCluster)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var newCluster = new ClusterInfo(currentCluster);
            _mainWindowViewModel.EnteredCluster.Insert(0, newCluster);
            RemovesClusterIfMoreThanLimit();
        });
    }

    private void RemovesClusterIfMoreThanLimit()
    {
        if (_mainWindowViewModel?.EnteredCluster?.Count > MaxEnteredCluster)
        {
            _mainWindowViewModel?.EnteredCluster?.RemoveAt(_mainWindowViewModel.EnteredCluster.Count - 1);
        }
    }

    #endregion

    #region Ui

    public void UpdateUserInfoUi(ClusterInfo currentCluster)
    {
        _mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.Tier = currentCluster.TierString;
        _mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.ClusterMode = currentCluster.ClusterMode;
        _mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.ComposingMapInfoString(currentCluster);
    }

    #endregion

    #region Test methods

    private static readonly Random Random = new();

    private static T RandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T) v.GetValue(Random.Next(v.Length));
    }

    private void CreateRandomClusterInfosForTracking(int runs)
    {
        for (var i = 0; i < runs; i++)
        {
            var clusterInfo = new ClusterInfo();
            var value = RandomEnumValue<MapType>();

            clusterInfo.SetClusterInfo(value, Guid.NewGuid(), "3000", "Meine Super Insel", "@ISLAND", null, "4001", Tier.T7);

            UpdateClusterTracking(clusterInfo);
        }
    }

    #endregion
}