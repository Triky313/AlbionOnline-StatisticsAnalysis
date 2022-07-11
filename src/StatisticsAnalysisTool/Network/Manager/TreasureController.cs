using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace StatisticsAnalysisTool.Network.Manager;

public class TreasureController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ObservableCollection<TemporaryTreasure> _temporaryTreasures = new();
    private readonly ObservableCollection<Treasure> _treasures = new();

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

        var test = new Treasure()
        {
            OpenedBy = openedBy,
            TreasureRarity = GetRarity(temporaryTreasure.UniqueName),
            TreasureType = GetType(temporaryTreasure.UniqueName)
        };

        _treasures.Add(test);
        temporaryTreasure.AlreadyScanned = true;
    }

    public void RemoveTemporaryTreasures()
    {
        _temporaryTreasures.Clear();
    }

    private void UpdateLootedChestsDashboardUi(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_trackingController.EntityController.LocalUserData.Guid is not { } localPlayerGuid)
        {
            return;
        }

        #region Avalonian roads


        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadCommonWeek = _treasures.Count(x => x != null 
                                                                     && x.OpenedBy.Contains(localPlayerGuid)
                                                                     && x.TreasureRarity == TreasureRarity.Standard
                                                                     && x.TreasureType == TreasureType.Avalon
                                                                     && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadCommonMonth = _treasures.Count(x => x != null
                                                                      && x.OpenedBy.Contains(localPlayerGuid)
                                                                      && x.TreasureRarity == TreasureRarity.Standard
                                                                      && x.TreasureType == TreasureType.Avalon
                                                                      && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadUncommonWeek = _treasures.Count(x => x != null
                                                                       && x.OpenedBy.Contains(localPlayerGuid)
                                                                       && x.TreasureRarity == TreasureRarity.Uncommon
                                                                       && x.TreasureType == TreasureType.Avalon
                                                                       && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadUncommonMonth = _treasures.Count(x => x != null
                                                                        && x.OpenedBy.Contains(localPlayerGuid)
                                                                        && x.TreasureRarity == TreasureRarity.Uncommon
                                                                        && x.TreasureType == TreasureType.Avalon
                                                                        && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadEpicWeek = _treasures.Count(x => x != null
                                                                   && x.OpenedBy.Contains(localPlayerGuid)
                                                                   && x.TreasureRarity == TreasureRarity.Rare
                                                                   && x.TreasureType == TreasureType.Avalon
                                                                   && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadEpicMonth = _treasures.Count(x => x != null
                                                                    && x.OpenedBy.Contains(localPlayerGuid)
                                                                    && x.TreasureRarity == TreasureRarity.Rare
                                                                    && x.TreasureType == TreasureType.Avalon
                                                                    && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadLegendaryWeek = _treasures.Count(x => x != null
                                                                        && x.OpenedBy.Contains(localPlayerGuid)
                                                                        && x.TreasureRarity == TreasureRarity.Legendary
                                                                        && x.TreasureType == TreasureType.Avalon
                                                                        && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.AvalonianRoadLegendaryMonth = _treasures.Count(x => x != null
                                                                         && x.OpenedBy.Contains(localPlayerGuid)
                                                                         && x.TreasureRarity == TreasureRarity.Legendary
                                                                         && x.TreasureType == TreasureType.Avalon
                                                                         && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));


        #endregion

        #region Open world

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldCommonWeek = _treasures.Count(x => x != null
                                                                 && x.OpenedBy.Contains(localPlayerGuid)
                                                                 && x.TreasureRarity == TreasureRarity.Standard
                                                                 && x.TreasureType == TreasureType.OpenWorld
                                                                 && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldCommonMonth = _treasures.Count(x => x != null
                                                                  && x.OpenedBy.Contains(localPlayerGuid)
                                                                  && x.TreasureRarity == TreasureRarity.Standard
                                                                  && x.TreasureType == TreasureType.OpenWorld
                                                                  && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldUncommonWeek = _treasures.Count(x => x != null
                                                                   && x.OpenedBy.Contains(localPlayerGuid)
                                                                   && x.TreasureRarity == TreasureRarity.Uncommon
                                                                   && x.TreasureType == TreasureType.OpenWorld
                                                                   && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldUncommonMonth = _treasures.Count(x => x != null
                                                                    && x.OpenedBy.Contains(localPlayerGuid)
                                                                    && x.TreasureRarity == TreasureRarity.Uncommon
                                                                    && x.TreasureType == TreasureType.OpenWorld
                                                                    && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldEpicWeek = _treasures.Count(x => x != null
                                                               && x.OpenedBy.Contains(localPlayerGuid)
                                                               && x.TreasureRarity == TreasureRarity.Rare
                                                               && x.TreasureType == TreasureType.OpenWorld
                                                               && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldEpicMonth = _treasures.Count(x => x != null
                                                                && x.OpenedBy.Contains(localPlayerGuid)
                                                                && x.TreasureRarity == TreasureRarity.Rare
                                                                && x.TreasureType == TreasureType.OpenWorld
                                                                && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldLegendaryWeek = _treasures.Count(x => x != null
                                                                    && x.OpenedBy.Contains(localPlayerGuid)
                                                                    && x.TreasureRarity == TreasureRarity.Legendary
                                                                    && x.TreasureType == TreasureType.OpenWorld
                                                                    && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.OpenWorldLegendaryMonth = _treasures.Count(x => x != null
                                                                     && x.OpenedBy.Contains(localPlayerGuid)
                                                                     && x.TreasureRarity == TreasureRarity.Legendary
                                                                     && x.TreasureType == TreasureType.OpenWorld
                                                                     && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        #endregion

        #region Static dungeons

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticCommonWeek = _treasures.Count(x => x != null
                                                              && x.OpenedBy.Contains(localPlayerGuid)
                                                              && x.TreasureRarity == TreasureRarity.Standard
                                                              && x.TreasureType == TreasureType.StaticDungeon
                                                              && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticCommonMonth = _treasures.Count(x => x != null
                                                               && x.OpenedBy.Contains(localPlayerGuid)
                                                               && x.TreasureRarity == TreasureRarity.Standard
                                                               && x.TreasureType == TreasureType.StaticDungeon
                                                               && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticUncommonWeek = _treasures.Count(x => x != null
                                                                && x.OpenedBy.Contains(localPlayerGuid)
                                                                && x.TreasureRarity == TreasureRarity.Uncommon
                                                                && x.TreasureType == TreasureType.StaticDungeon
                                                                && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticUncommonMonth = _treasures.Count(x => x != null
                                                                 && x.OpenedBy.Contains(localPlayerGuid)
                                                                 && x.TreasureRarity == TreasureRarity.Uncommon
                                                                 && x.TreasureType == TreasureType.StaticDungeon
                                                                 && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticEpicWeek = _treasures.Count(x => x != null
                                                            && x.OpenedBy.Contains(localPlayerGuid)
                                                            && x.TreasureRarity == TreasureRarity.Rare
                                                            && x.TreasureType == TreasureType.StaticDungeon
                                                            && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticEpicMonth = _treasures.Count(x => x != null
                                                             && x.OpenedBy.Contains(localPlayerGuid)
                                                             && x.TreasureRarity == TreasureRarity.Rare
                                                             && x.TreasureType == TreasureType.StaticDungeon
                                                             && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticLegendaryWeek = _treasures.Count(x => x != null
                                                                 && x.OpenedBy.Contains(localPlayerGuid)
                                                                 && x.TreasureRarity == TreasureRarity.Legendary
                                                                 && x.TreasureType == TreasureType.StaticDungeon
                                                                 && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.StaticLegendaryMonth = _treasures.Count(x => x != null
                                                                  && x.OpenedBy.Contains(localPlayerGuid)
                                                                  && x.TreasureRarity == TreasureRarity.Legendary
                                                                  && x.TreasureType == TreasureType.StaticDungeon
                                                                  && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        #endregion

        #region HellGate

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateCommonWeek = _treasures.Count(x => x != null
                                                                && x.OpenedBy.Contains(localPlayerGuid)
                                                                && x.TreasureRarity == TreasureRarity.Standard
                                                                && x.TreasureType == TreasureType.HellGate
                                                                && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateCommonMonth = _treasures.Count(x => x != null
                                                                 && x.OpenedBy.Contains(localPlayerGuid)
                                                                 && x.TreasureRarity == TreasureRarity.Standard
                                                                 && x.TreasureType == TreasureType.HellGate
                                                                 && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateUncommonWeek = _treasures.Count(x => x != null
                                                                  && x.OpenedBy.Contains(localPlayerGuid)
                                                                  && x.TreasureRarity == TreasureRarity.Uncommon
                                                                  && x.TreasureType == TreasureType.HellGate
                                                                  && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateUncommonMonth = _treasures.Count(x => x != null
                                                                   && x.OpenedBy.Contains(localPlayerGuid)
                                                                   && x.TreasureRarity == TreasureRarity.Uncommon
                                                                   && x.TreasureType == TreasureType.HellGate
                                                                   && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateEpicWeek = _treasures.Count(x => x != null
                                                              && x.OpenedBy.Contains(localPlayerGuid)
                                                              && x.TreasureRarity == TreasureRarity.Rare
                                                              && x.TreasureType == TreasureType.HellGate
                                                              && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateEpicMonth = _treasures.Count(x => x != null
                                                               && x.OpenedBy.Contains(localPlayerGuid)
                                                               && x.TreasureRarity == TreasureRarity.Rare
                                                               && x.TreasureType == TreasureType.HellGate
                                                               && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateLegendaryWeek = _treasures.Count(x => x != null
                                                                   && x.OpenedBy.Contains(localPlayerGuid)
                                                                   && x.TreasureRarity == TreasureRarity.Legendary
                                                                   && x.TreasureType == TreasureType.HellGate
                                                                   && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-7));

        _mainWindowViewModel.DashboardBindings.LootedChests.HellGateLegendaryMonth = _treasures.Count(x => x != null
                                                                    && x.OpenedBy.Contains(localPlayerGuid)
                                                                    && x.TreasureRarity == TreasureRarity.Legendary
                                                                    && x.TreasureType == TreasureType.HellGate
                                                                    && x.OpenedAt.Date > DateTime.UtcNow.Date.AddDays(-30));

        #endregion
    }

    private static TreasureRarity GetRarity(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureRarity.Unknown;
        }

        if (Regex.IsMatch(value, "\\w*_STANDARD\\b|\\w*_STANDARD_[T][4-8]|\\w*_STANDARD_STANDARD_[T][4-8]|STATIC_\\w*_POI"))
        {
            return TreasureRarity.Standard;
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

    private static TreasureType GetType(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureType.Unknown;
        }

        if (value.Contains("TREASURE"))
        {
            return TreasureType.OpenWorld;
        }

        if (value.Contains("STATIC"))
        {
            return TreasureType.StaticDungeon;
        }

        if (value.Contains("AVALON"))
        {
            return TreasureType.Avalon;
        }

        if (value.Contains("CORRUPTED"))
        {
            return TreasureType.Corrupted;
        }

        if (value.Contains("HELL"))
        {
            return TreasureType.HellGate;
        }

        if (Regex.IsMatch(value, "_VETERAN_CHEST_|[^SOLO]_CHEST_BOSS_HALLOWEEN_"))
        {
            return TreasureType.RandomGroupDungeon;
        }

        if (Regex.IsMatch(value, "_SOLO_BOOKCHEST_|_SOLO_CHEST_"))
        {
            return TreasureType.RandomSoloDungeon;
        }

        return TreasureType.Unknown;
    }
}