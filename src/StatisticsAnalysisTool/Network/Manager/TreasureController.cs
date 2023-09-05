using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager;

public class TreasureController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ObservableCollection<TemporaryTreasure> _temporaryTreasures = new();
    private ObservableCollection<Treasure> _treasures = new ();

    public TreasureController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void RegisterEvents()
    {
        _treasures.CollectionChanged += UpdateLootedChestsDashboardUi;
    }

    public void UnregisterEvents()
    {
        _treasures.CollectionChanged -= UpdateLootedChestsDashboardUi;
    }

    public void AddTreasure(int objectId, string uniqueName, string uniqueNameWithLocation)
    {
        if (_temporaryTreasures.All(x => x.ObjectId != objectId))
        {
            _temporaryTreasures.Add(new TemporaryTreasure() { ObjectId = objectId, UniqueName = uniqueName, UniqueNameWithLocation = uniqueNameWithLocation });
        }
    }

    public void UpdateTreasure(int objectId, List<Guid> openedBy)
    {
        if (openedBy is not { Count: > 0 })
        {
            return;
        }

        var temporaryTreasure = _temporaryTreasures?.FirstOrDefault(x => x?.ObjectId == objectId && x.AlreadyScanned == false);
        if (temporaryTreasure == null)
        {
            return;
        }

        var treasure = new Treasure()
        {
            OpenedBy = openedBy,
            TreasureRarity = GetRarity(temporaryTreasure.UniqueName),
            TreasureType = GetTreasureType(temporaryTreasure.UniqueName)
        };

        _treasures.Add(treasure);
        temporaryTreasure.AlreadyScanned = true;
    }

    public void RemoveTemporaryTreasures()
    {
        _temporaryTreasures.Clear();
    }

    public void UpdateLootedChestsDashboardUi()
    {
        UpdateLootedChestsDashboardUi(null, null);
    }

    private void UpdateLootedChestsDashboardUi(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
        #region Avalonian roads

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadCommonWeek = GetStats(TreasureRarity.Common, TreasureType.Avalon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadCommonMonth = GetStats(TreasureRarity.Common, TreasureType.Avalon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadCommonYear = GetStats(TreasureRarity.Common, TreasureType.Avalon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.Avalon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.Avalon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.Avalon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.Avalon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.Avalon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadEpicYear = GetStats(TreasureRarity.Rare, TreasureType.Avalon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.Avalon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.Avalon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.Avalon, -365);

        #endregion

        #region Open world

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldCommonWeek = GetStats(TreasureRarity.Common, TreasureType.OpenWorld, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldCommonMonth = GetStats(TreasureRarity.Common, TreasureType.OpenWorld, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldCommonYear = GetStats(TreasureRarity.Common, TreasureType.OpenWorld, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.OpenWorld, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.OpenWorld, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.OpenWorld, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.OpenWorld, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.OpenWorld, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldEpicYear = GetStats(TreasureRarity.Rare, TreasureType.OpenWorld, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.OpenWorld, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.OpenWorld, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.OpenWorld, -365);

        #endregion

        #region Random group dungeons

        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonCommonWeek = GetStats(TreasureRarity.Common, TreasureType.RandomGroupDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonCommonMonth = GetStats(TreasureRarity.Common, TreasureType.RandomGroupDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonCommonYear = GetStats(TreasureRarity.Common, TreasureType.RandomGroupDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.RandomGroupDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.RandomGroupDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.RandomGroupDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.RandomGroupDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.RandomGroupDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonEpicYear = GetStats(TreasureRarity.Rare, TreasureType.RandomGroupDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.RandomGroupDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.RandomGroupDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomGroupDungeonLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.RandomGroupDungeon, -365);

        #endregion

        #region Random solo dungeons

        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonCommonWeek = GetStats(TreasureRarity.Common, TreasureType.RandomSoloDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonCommonMonth = GetStats(TreasureRarity.Common, TreasureType.RandomSoloDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonCommonYear = GetStats(TreasureRarity.Common, TreasureType.RandomSoloDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.RandomSoloDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.RandomSoloDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.RandomSoloDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.RandomSoloDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.RandomSoloDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonEpicYear = GetStats(TreasureRarity.Rare, TreasureType.RandomSoloDungeon, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.RandomSoloDungeon, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.RandomSoloDungeon, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.RandomSoloDungeonLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.RandomSoloDungeon, -365);

        #endregion

        #region HellGate

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateCommonWeek = GetStats(TreasureRarity.Common, TreasureType.HellGate, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateCommonMonth = GetStats(TreasureRarity.Common, TreasureType.HellGate, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateCommonYear = GetStats(TreasureRarity.Common, TreasureType.HellGate, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.HellGate, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.HellGate, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.HellGate, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.HellGate, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.HellGate, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateEpicYear = GetStats(TreasureRarity.Rare, TreasureType.HellGate, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.HellGate, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.HellGate, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.HellGate, -365);

        #endregion

        #region Mist

        _mainWindowViewModel.DashboardBindings.LootedChests.MistCommonWeek = GetStats(TreasureRarity.Common, TreasureType.Mist, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistCommonMonth = GetStats(TreasureRarity.Common, TreasureType.Mist, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistCommonYear = GetStats(TreasureRarity.Common, TreasureType.Mist, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistUncommonWeek = GetStats(TreasureRarity.Uncommon, TreasureType.Mist, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistUncommonMonth = GetStats(TreasureRarity.Uncommon, TreasureType.Mist, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistUncommonYear = GetStats(TreasureRarity.Uncommon, TreasureType.Mist, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistEpicWeek = GetStats(TreasureRarity.Rare, TreasureType.Mist, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistEpicMonth = GetStats(TreasureRarity.Rare, TreasureType.Mist, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistEpicYear = GetStats(TreasureRarity.Rare, TreasureType.Mist, -365);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistLegendaryWeek = GetStats(TreasureRarity.Legendary, TreasureType.Mist, -7);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistLegendaryMonth = GetStats(TreasureRarity.Legendary, TreasureType.Mist, -30);
        _mainWindowViewModel.DashboardBindings.LootedChests.MistLegendaryYear = GetStats(TreasureRarity.Legendary, TreasureType.Mist, -365);

        #endregion
    }

    #region Helper methods

    private int GetStats(TreasureRarity treasureRarity, TreasureType treasureType, int lastDays = -90)
    {
        if (_trackingController.EntityController.LocalUserData.Guid is not { } localPlayerGuid)
        {
            return 0;
        }

        return _treasures?.Count(x => x != null
                                           && x.OpenedBy.Contains(localPlayerGuid)
                                           && x.TreasureRarity == treasureRarity
                                           && x.TreasureType == treasureType
                                           && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(lastDays)) ?? 0;
    }

    private static TreasureRarity GetRarity(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureRarity.Unknown;
        }

        if (Regex.IsMatch(value, "\\w*_STANDARD\\b|\\w*_STANDARD_[T][4-8]|\\w*_STANDARD_STANDARD_[T][4-8]|STATIC_\\w*_POI"))
        {
            return TreasureRarity.Common;
        }
        if (Regex.IsMatch(value, "\\w*_UNCOMMON\\b|\\w*_UNCOMMON_[T][4-8]|\\w*_STANDARD_UNCOMMON_[T][4-8]|STATIC_\\w*_CHAMPION"))
        {
            return TreasureRarity.Uncommon;
        }

        if (Regex.IsMatch(value, "\\w*_RARE\\b|\\w*_RARE_[T][4-8]|\\w*_STANDARD_RARE_[T][4-8]|STATIC_\\w*_MINIBOSS"))
        {
            return TreasureRarity.Rare;
        }

        if (Regex.IsMatch(value, "\\w*_LEGENDARY\\b|\\w*_LEGENDARY_[T][4-8]|\\w*_STANDARD_LEGENDARY_[T][4-8]|STATIC_\\w*_BOSS"))
        {
            return TreasureRarity.Legendary;
        }

        return TreasureRarity.Unknown;
    }

    private static TreasureType GetTreasureType(string input)
    {
        var inputArray = input.Split("_");

        if (inputArray.Any(x => x == "MISTS"))
        {
            return TreasureType.Mist;
        }

        if (inputArray.Any(x => x == "TREASURE"))
        {
            return TreasureType.OpenWorld;
        }

        if (inputArray.Any(x => x == "STATIC"))
        {
            return TreasureType.StaticDungeon;
        }

        if (inputArray.Any(x => x == "AVALON"))
        {
            return TreasureType.Avalon;
        }

        if (inputArray.Any(x => x == "CORRUPTED"))
        {
            return TreasureType.Corrupted;
        }

        if (inputArray.Any(x => x == "HELL"))
        {
            return TreasureType.HellGate;
        }

        var pattern = "_VETERAN_CHEST_|[^SOLO]_CHEST_BOSS_HALLOWEEN_";
        if (Regex.IsMatch(input, pattern))
        {
            return TreasureType.RandomGroupDungeon;
        }

        pattern = "_SOLO_BOOKCHEST_|_SOLO_CHEST_";
        if (Regex.IsMatch(input, pattern))
        {
            return TreasureType.RandomSoloDungeon;
        }

        return TreasureType.Unknown;
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TreasureStatsFileName));
        _treasures = await FileController.LoadAsync<ObservableRangeCollection<Treasure>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.TreasureStatsFileName));
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_treasures, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.TreasureStatsFileName));
        Debug.Print("Treasure saved");
    }

    #endregion
}