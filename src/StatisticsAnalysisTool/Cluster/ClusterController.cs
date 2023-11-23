using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.PartyBuilder;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace StatisticsAnalysisTool.Cluster;

public sealed class ClusterController : IClusterController
{
    private const int MaxEnteredCluster = 500;

    private readonly ITrackingController _trackingController;
    private readonly ILootController _lootController;
    private readonly IPartyBuilderController _partyBuilderController;
    private readonly IStatisticController _statisticController;
    private readonly ITradeController _tradeController;
    private readonly IVaultController _vaultController;
    private readonly ITreasureController _treasureController;
    private readonly IGatheringController _gatheringController;
    private readonly ICombatController _combatController;
    private readonly MainWindowViewModelOld _mainWindowViewModel;

    public static ClusterInfo CurrentCluster { get; } = new();

    public ClusterController(ITrackingController trackingController,
        ILootController lootController,
        IPartyBuilderController partyBuilderController,
        IStatisticController statisticController,
        ITradeController tradeController,
        IVaultController vaultController,
        ITreasureController treasureController,
        IGatheringController gatheringController,
        ICombatController combatController,
        MainWindowViewModelOld mainWindowViewModel)
    {
        _trackingController = trackingController;
        _lootController = lootController;
        _partyBuilderController = partyBuilderController;
        _statisticController = statisticController;
        _tradeController = tradeController;
        _vaultController = vaultController;
        _treasureController = treasureController;
        _gatheringController = gatheringController;
        _combatController = combatController;
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
        _tradeController.ResetCraftingBuildingInfo();
        _mainWindowViewModel.DamageMeterBindings.GetSnapshot(_mainWindowViewModel.DamageMeterBindings.IsSnapshotAfterMapChangeActive);
        _combatController.ResetDamageMeterByClusterChange();
        _statisticController.SetKillsDeathsValues();
        _vaultController.ResetDiscoveredItems();
        _vaultController.ResetVaultContainer();
        _vaultController.ResetCurrentVaultInfo();
        _treasureController.RemoveTemporaryTreasures();
        _treasureController.UpdateLootedChestsDashboardUi();
        _lootController.ResetLocalPlayerDiscoveredLoot();
        _lootController.ResetIdentifiedBodies();
        _ = _tradeController.RemoveTradesByDaysInSettingsAsync();
        _ = _gatheringController.SetGatheredResourcesClosedAsync();
        _partyBuilderController.UpdateIsPlayerInspectedToFalse();
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