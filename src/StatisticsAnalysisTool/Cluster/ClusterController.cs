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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Cluster;

public sealed class ClusterController
{
    private const int MaxEnteredCluster = 1000;

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

    public void ChangeClusterInformation(MapType mapType, Guid? mapGuid, string clusterIndex, string instanceName, string worldMapDataType, byte[] dungeonInformation, string mainClusterIndex, Tier mistsDungeonTier)
    {
        CurrentCluster.ClusterInfoFullyAvailable = false;
        CurrentCluster.SetClusterInfo(mapType, mapGuid, clusterIndex, instanceName, worldMapDataType, dungeonInformation, mainClusterIndex, mistsDungeonTier);
    }

    public void SetJoinClusterInformation(string index, string mainClusterIndex, Guid? mapGuid, MapType mapType)
    {
        CurrentCluster.SetJoinClusterInfo(index, mainClusterIndex, mapGuid, mapType);
        CurrentCluster.Entered = DateTime.UtcNow;
        CurrentCluster.ClusterInfoFullyAvailable = true;

        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            OnChangeCluster?.Invoke(CurrentCluster);
        }

        Debug.Print($"[StateHandler] Changed cluster to: Index: '{CurrentCluster.Index}' UniqueName: '{CurrentCluster.UniqueName}' ClusterType: '{CurrentCluster.ClusterMode}' MapType: '{CurrentCluster.MapType}'");
    }

    public void SetAndResetValues(ClusterInfo currentCluster)
    {
        _trackingController.TradeController.ResetCraftingBuildingInfo();
        _mainWindowViewModel.DamageMeterBindings.GetSnapshot(_mainWindowViewModel.DamageMeterBindings.IsSnapshotAfterMapChangeActive);
        _trackingController.CombatController.ResetDamageMeterByClusterChange();
        _trackingController.VaultController.ResetDiscoveredItems();
        _trackingController.VaultController.ResetInternalVaultContainer();
        _trackingController.VaultController.ResetCurrentInternalVault();
        _trackingController.TreasureController.RemoveTemporaryTreasures();
        _trackingController.TreasureController.UpdateLootedChestsDashboardUi();
        _trackingController.LootController.ResetLocalPlayerDiscoveredLoot();
        _trackingController.LootController.ResetIdentifiedBodies();
        _ = _trackingController.TradeController.RemoveTradesByDaysInSettingsAsync();
        _ = _trackingController.GatheringController.SetGatheredResourcesClosedAsync();
        _trackingController.PartyController.UpdateIsPlayerInspectedToFalse();
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
        while (_mainWindowViewModel?.EnteredCluster?.Count > MaxEnteredCluster)
        {
            _mainWindowViewModel?.EnteredCluster?.RemoveAt(_mainWindowViewModel.EnteredCluster.Count - 1);
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
        if (_mainWindowViewModel?.EnteredCluster is null)
        {
            return;
        }

        var currentHistoryEntry = _mainWindowViewModel.EnteredCluster.FirstOrDefault(x => x.Guid == CurrentCluster.Guid && x.MapType == MapType.RandomDungeon);
        currentHistoryEntry?.SetRandomDungeonTrackingInfo(randomDungeonTier, randomDungeonLevel);
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
            _mainWindowViewModel.EnteredCluster = new ObservableCollection<ClusterInfo>(enteredClusters);
        });

        if (clusterInfoDtos.Count > MaxEnteredCluster)
        {
            await FileController.SaveAsync(trimmedClusterInfoDtos, GetMapHistoryFilePath());
            Log.Information("Map history trimmed to {MaxEnteredCluster} entries while loading", MaxEnteredCluster);
        }
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));

        var clusterHistoryToSave = _mainWindowViewModel.EnteredCluster
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
            _mainWindowViewModel.EnteredCluster.Clear();
        });

        await SaveInFileAsync();
        Log.Information("Map history cleared");
    }

    private static string GetMapHistoryFilePath()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.MapHistoryFileName);
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