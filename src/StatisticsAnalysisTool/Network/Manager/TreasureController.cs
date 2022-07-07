using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StatisticsAnalysisTool.Network.Manager;

public class TreasureController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ObservableCollection<TemporaryTreasure> _temporaryTreasures = new();
    private readonly ObservableCollection<Treasure> _treasures = new();

    public TreasureController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void AddTreasure(int objectId, string uniqueName, string uniqueNameWithLocation)
    {
        if (_temporaryTreasures.All(x => x.ObjectId != objectId))
        {
            _temporaryTreasures.Add(new TemporaryTreasure() { ObjectId = objectId, UniqueName = uniqueName, UniqueNameWithLocation = uniqueNameWithLocation });
        }
    }

    public void UpdateTreasure(int objectId, Guid openedBy)
    {
        var temporaryTreasure = _temporaryTreasures?.FirstOrDefault(x => x?.ObjectId == objectId && x.AlreadyScanned == false);
        if (temporaryTreasure == null)
        {
            return;
        }

        _treasures.Add(new Treasure()
        {
            OpenedBy = openedBy,
            TreasureRarity = GetRarity(temporaryTreasure.UniqueName),
            TreasureType = GetType(temporaryTreasure.UniqueName)
        });

        temporaryTreasure.AlreadyScanned = true;
    }

    public void RemoveTemporaryTreasures()
    {
        _temporaryTreasures.Clear();
    }

    private static TreasureRarity GetRarity(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureRarity.Unknown;
        }

        if (Regex.IsMatch(value, "\\w*_STANDARD\\b"))
        {
            return TreasureRarity.Standard;
        }
        if (Regex.IsMatch(value, "\\w*_UNCOMMON\\b"))
        {
            return TreasureRarity.Uncommon;
        }

        if (Regex.IsMatch(value, "\\w*_RARE\\b"))
        {
            return TreasureRarity.Rare;
        }

        if (Regex.IsMatch(value, "\\w*_LEGENDARY\\b"))
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

        if (Regex.IsMatch(value, "\\bNORMAL|\\bCHEST|\\bBOOKCHEST"))
        {
            return TreasureType.RandomDungeon;
        }

        return TreasureType.Unknown;
    }
}


//BOOKCHEST_STANDARD
//CHEST_STANDARD
//NORMAL_STANDARD
//AVALON_STANDARD_STANDARD
//AVALON_SMALL_STANDARD_STANDARD
//AVALON_VETERAN_STANDARD
//AVALON_SMALL_VETERAN_STANDARD
//AVALON_ELITE_STANDARD
//AVALON_SMALL_ELITE_STANDARD
//STATIC_UNDEAD_MINIBOSS_STANDARD
//STATIC_MORGANA_MINIBOSS_STANDARD
//STATIC_KEEPER_MINIBOSS_STANDARD
//STATIC_UNDEAD_BOSS_STANDARD
//STATIC_MORGANA_BOSS_STANDARD
//STATIC_KEEPER_BOSS_STANDARD
//STATIC_UNDEAD_POI_STANDARD
//STATIC_MORGANA_POI_STANDARD
//STATIC_KEEPER_POI_STANDARD
//STATIC_UNDEAD_CHAMPION_STANDARD
//STATIC_MORGANA_CHAMPION_STANDARD
//STATIC_KEEPER_CHAMPION_STANDARD
//TREASURE_SOLO_STANDARD
//TREASURE_VETERAN_STANDARD
//TREASURE_ELITE_STANDARD

//BOOKCHEST_UNCOMMON
//CHEST_UNCOMMON
//NORMAL_UNCOMMON
//CHEST_BOSS_UNCOMMON
//AVALON_STANDARD_UNCOMMON
//AVALON_SMALL_STANDARD_UNCOMMON
//AVALON_VETERAN_UNCOMMON
//AVALON_SMALL_VETERAN_UNCOMMON
//AVALON_ELITE_UNCOMMON
//AVALON_SMALL_ELITE_UNCOMMON
//STATIC_UNDEAD_MINIBOSS_UNCOMMON
//STATIC_MORGANA_MINIBOSS_UNCOMMON
//STATIC_KEEPER_MINIBOSS_UNCOMMON
//STATIC_UNDEAD_BOSS_UNCOMMON
//STATIC_MORGANA_BOSS_UNCOMMON
//STATIC_KEEPER_BOSS_UNCOMMON
//STATIC_UNDEAD_POI_UNCOMMON
//STATIC_MORGANA_POI_UNCOMMON
//STATIC_KEEPER_POI_UNCOMMON
//STATIC_UNDEAD_CHAMPION_UNCOMMON
//STATIC_MORGANA_CHAMPION_UNCOMMON
//STATIC_KEEPER_CHAMPION_UNCOMMON
//TREASURE_SOLO_UNCOMMON
//TREASURE_VETERAN_UNCOMMON
//TREASURE_ELITE_UNCOMMON

//BOOKCHEST_RARE
//CHEST_RARE
//NORMAL_RARE
//CHEST_BOSS_RARE
//AVALON_STANDARD_RARE
//AVALON_SMALL_STANDARD_RARE
//AVALON_VETERAN_RARE
//AVALON_SMALL_VETERAN_RARE
//AVALON_ELITE_RARE
//AVALON_SMALL_ELITE_RARE
//STATIC_UNDEAD_MINIBOSS_RARE
//STATIC_MORGANA_MINIBOSS_RARE
//STATIC_KEEPER_MINIBOSS_RARE
//STATIC_UNDEAD_BOSS_RARE
//STATIC_MORGANA_BOSS_RARE
//STATIC_KEEPER_BOSS_RARE
//STATIC_UNDEAD_POI_RARE
//STATIC_MORGANA_POI_RARE
//STATIC_KEEPER_POI_RARE
//STATIC_UNDEAD_CHAMPION_RARE
//STATIC_MORGANA_CHAMPION_RARE
//STATIC_KEEPER_CHAMPION_RARE
//TREASURE_SOLO_RARE
//TREASURE_VETERAN_RARE
//TREASURE_ELITE_RARE

//BOOKCHEST_LEGENDARY
//CHEST_LEGENDARY
//NORMAL_LEGENDARY
//CHEST_BOSS_LEGENDARY
//AVALON_STANDARD_LEGENDARY
//AVALON_SMALL_STANDARD_LEGENDARY
//AVALON_VETERAN_LEGENDARY
//AVALON_SMALL_VETERAN_LEGENDARY
//AVALON_ELITE_LEGENDARY
//AVALON_SMALL_ELITE_LEGENDARY
//STATIC_UNDEAD_MINIBOSS_LEGENDARY
//STATIC_MORGANA_MINIBOSS_LEGENDARY
//STATIC_KEEPER_MINIBOSS_LEGENDARY
//STATIC_UNDEAD_BOSS_LEGENDARY
//STATIC_MORGANA_BOSS_LEGENDARY
//STATIC_KEEPER_BOSS_LEGENDARY
//STATIC_UNDEAD_POI_LEGENDARY
//STATIC_MORGANA_POI_LEGENDARY
//STATIC_KEEPER_POI_LEGENDARY
//STATIC_UNDEAD_CHAMPION_LEGENDARY
//STATIC_MORGANA_CHAMPION_LEGENDARY
//STATIC_KEEPER_CHAMPION_LEGENDARY
//TREASURE_SOLO_LEGENDARY
//TREASURE_VETERAN_LEGENDARY
//TREASURE_ELITE_LEGENDARY