using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Cluster;

public sealed class ClusterController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
{
    private const int MaxEnteredCluster = 1000;

    public static ClusterInfo CurrentCluster { get; } = new();

    public void RegisterEvents()
    {
        OnChangeCluster += UpdateClusterTracking;
        OnChangeCluster += SetAndResetValues;
        OnChangeCluster += UpdateUserInfoUi;
        OnChangeCluster += SaveUserData;
    }

    public void UnregisterEvents()
    {
        OnChangeCluster -= UpdateClusterTracking;
        OnChangeCluster -= SetAndResetValues;
        OnChangeCluster -= UpdateUserInfoUi;
        OnChangeCluster -= SaveUserData;
    }

    public event Action<ClusterInfo> OnChangeCluster;

    public void ChangeClusterInformation(MapType mapType, Guid? mapGuid, string clusterIndex, string instanceName, string worldMapDataType, byte[] dungeonInformation, string mainClusterIndex)
    {
        CurrentCluster.ClusterInfoFullyAvailable = false;
        CurrentCluster.SetClusterInfo(mapType, mapGuid, clusterIndex, instanceName, worldMapDataType, dungeonInformation, mainClusterIndex);
    }

    public void SetJoinClusterInformation(string index, string sourceClusterIndex, Guid? mapGuid, MapType mapType)
    {
        CurrentCluster.SetJoinClusterInfo(index, sourceClusterIndex, mapGuid, mapType);
        CurrentCluster.Entered = DateTime.UtcNow;
        CurrentCluster.ClusterInfoFullyAvailable = true;

        if (trackingController.IsTrackingAllowedByMainCharacter())
        {
            OnChangeCluster?.Invoke(CurrentCluster);
        }

        Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterMode}' MapType: '{CurrentCluster.MapType}'");
    }

    public void SetAndResetValues(ClusterInfo currentCluster)
    {
        trackingController.TradeController.ResetCraftingBuildingInfo();
        mainWindowViewModel.DamageMeterBindings.GetSnapshot(mainWindowViewModel.DamageMeterBindings.IsSnapshotAfterMapChangeActive);
        trackingController.CombatController.ResetDamageMeterByClusterChange();
        trackingController.VaultController.ResetDiscoveredItems();
        trackingController.VaultController.ResetInternalVaultContainer();
        trackingController.VaultController.ResetCurrentInternalVault();
        trackingController.TreasureController.RemoveTemporaryTreasures();
        trackingController.TreasureController.UpdateLootedChestsDashboardUi();
        trackingController.LootController.ResetLocalPlayerDiscoveredLoot();
        trackingController.LootController.ResetIdentifiedBodies();
        _ = trackingController.TradeController.RemoveTradesByDaysInSettingsAsync();
        _ = trackingController.GatheringController.SetGatheredResourcesClosedAsync();
        trackingController.PartyController.UpdateIsPlayerInspectedToFalse();
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
            mainWindowViewModel.EnteredCluster.Insert(0, newCluster);
            RemovesClusterIfMoreThanLimit();
        });
    }

    private void RemovesClusterIfMoreThanLimit()
    {
        while (mainWindowViewModel?.EnteredCluster?.Count > MaxEnteredCluster)
        {
            mainWindowViewModel?.EnteredCluster?.RemoveAt(mainWindowViewModel.EnteredCluster.Count - 1);
        }
    }

    public void UpdateCurrentMapHistoryRandomDungeonInformation(Tier randomDungeonTier, int randomDungeonLevel)
    {
        if (CurrentCluster.MapType != MapType.RandomDungeon)
        {
            return;
        }

        CurrentCluster.SetRandomDungeonTrackingInfo(randomDungeonTier, randomDungeonLevel);

        if (Application.Current.Dispatcher.CheckAccess())
        {
            UpdateCurrentMapHistoryRandomDungeonInformationOnUiThread(randomDungeonTier, randomDungeonLevel);
            return;
        }

        _ = Application.Current.Dispatcher.InvokeAsync(() => UpdateCurrentMapHistoryRandomDungeonInformationOnUiThread(randomDungeonTier, randomDungeonLevel));
    }

    private void UpdateCurrentMapHistoryRandomDungeonInformationOnUiThread(Tier randomDungeonTier, int randomDungeonLevel)
    {
        if (mainWindowViewModel?.EnteredCluster is null)
        {
            return;
        }

        var currentHistoryEntry = mainWindowViewModel.EnteredCluster.FirstOrDefault(x => x.Guid == CurrentCluster.Guid && x.MapType == MapType.RandomDungeon);
        currentHistoryEntry?.SetRandomDungeonTrackingInfo(randomDungeonTier, randomDungeonLevel);
    }

    #endregion

    #region Ui

    public void UpdateUserInfoUi(ClusterInfo currentCluster)
    {
        mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.Tier = currentCluster.TierString;
        mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.ClusterMode = currentCluster.ClusterMode;
        mainWindowViewModel.UserTrackingBindings.CurrentMapInfoBinding.ComposingMapInfoString(currentCluster);
    }

    #endregion

    #region Save UserData

    private static void SaveUserData(ClusterInfo currentCluster)
    {
        RateLimitedAction.Run(CriticalData.Save);
    }

    #endregion

    #region Save / Load data

    public async Task LoadMapHistoryFromFileAsync()
    {
        var clusterInfoDtos = await FileController.LoadAsync<List<ClusterInfoDto>>(GetMapHistoryFilePath()) ?? [];
        var trimmedClusterInfoDtos = clusterInfoDtos
            .Take(MaxEnteredCluster)
            .ToList();
        var enteredClusters = trimmedClusterInfoDtos
            .Select(ClusterInfoMapping.Mapping)
            .ToList();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.EnteredCluster = new ObservableCollection<ClusterInfo>(enteredClusters);
        });

        if (clusterInfoDtos.Count > MaxEnteredCluster)
        {
            await FileController.SaveAsync(trimmedClusterInfoDtos, GetMapHistoryFilePath());
            Log.Information("Map history trimmed to {MaxEnteredCluster} entries while loading", MaxEnteredCluster);
        }
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(AppDataPaths.UserDataDirectory);

        var clusterHistoryToSave = mainWindowViewModel.EnteredCluster
            .Take(MaxEnteredCluster)
            .Select(ClusterInfoMapping.Mapping)
            .ToList();

        await FileController.SaveAsync(clusterHistoryToSave, GetMapHistoryFilePath());
        Log.Information("Map history saved");
    }

    public async Task ClearMapHistoryAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            mainWindowViewModel.EnteredCluster.Clear();
        });

        await SaveInFileAsync();
        Log.Information("Map history cleared");
    }

    private static string GetMapHistoryFilePath()
    {
        return AppDataPaths.UserDataFile(Settings.Default.MapHistoryFileName);
    }

    #endregion
}