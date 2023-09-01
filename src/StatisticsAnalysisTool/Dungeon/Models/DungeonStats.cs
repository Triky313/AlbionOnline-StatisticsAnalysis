using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DungeonStats : BaseViewModel
{
    private StatsMists _statsMists = new();
    private StatsSolo _statsSolo = new();
    private StatsStandard _statsStandard = new();

    public void Set(IEnumerable<DungeonBaseFragment> dungeons)
    {
        Set(dungeons.ToList());
    }

    public void Set(List<DungeonBaseFragment> dungeons)
    {
        UpdateMistsStats(dungeons);
        UpdateSoloStats(dungeons);
        UpdateStandardStats(dungeons);
    }

    public void UpdateMistsStats(List<DungeonBaseFragment> dungeons)
    {
        var mists = dungeons?.Where(x => x is MistsFragment).Cast<MistsFragment>().ToList() ?? new List<MistsFragment>();

        StatsMists.RunTimeTotal = mists.Sum(x => x.TotalRunTimeInSeconds);

        StatsMists.Entered = mists.Count;

        StatsMists.EnteredCommon = mists.Count(x => x.Rarity == MistsRarity.Common);
        StatsMists.EnteredUncommon = mists.Count(x => x.Rarity == MistsRarity.Uncommon);
        StatsMists.EnteredRare = mists.Count(x => x.Rarity == MistsRarity.Rare);
        StatsMists.EnteredEpic = mists.Count(x => x.Rarity == MistsRarity.Epic);
        StatsMists.EnteredLegendary = mists.Count(x => x.Rarity == MistsRarity.Legendary);

        StatsMists.Fame = mists.Sum(x => x.Fame);
        StatsMists.ReSpec = mists.Sum(x => x.ReSpec);
        StatsMists.Silver = mists.Sum(x => x.Silver);
        StatsMists.Might = mists.Sum(x => x.Might);
        StatsMists.Favor = mists.Sum(x => x.Favor);

        StatsMists.LootInSilver = mists.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsMists.MostValuableLoot = mists.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateSoloStats(List<DungeonBaseFragment> dungeons)
    {
        var soloDun = dungeons?.Where(x => x is RandomDungeonFragment { Mode: DungeonMode.Solo }).Cast<RandomDungeonFragment>().ToList() ?? new List<RandomDungeonFragment>();

        StatsSolo.RunTimeTotal = soloDun.Sum(x => x.TotalRunTimeInSeconds);

        StatsSolo.Entered = soloDun.Count;
        
        StatsSolo.EnteredUncommon = soloDun.Count(x => x.Level == 1);
        StatsSolo.EnteredRare = soloDun.Count(x => x.Level == 2);
        StatsSolo.EnteredEpic = soloDun.Count(x => x.Level == 3);
        StatsSolo.EnteredLegendary = soloDun.Count(x => x.Level == 4);

        StatsSolo.OpenedStandardChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest));
        StatsSolo.OpenedUncommonChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest));
        StatsSolo.OpenedRareChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest));
        StatsSolo.OpenedLegendaryChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest));

        StatsSolo.OpenedStandardBookChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest));
        StatsSolo.OpenedUncommonBookChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest));
        StatsSolo.OpenedRareBookChests = soloDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest));

        StatsSolo.Fame = soloDun.Sum(x => x.Fame);
        StatsSolo.ReSpec = soloDun.Sum(x => x.ReSpec);
        StatsSolo.Silver = soloDun.Sum(x => x.Silver);
        StatsSolo.Might = soloDun.Sum(x => x.Might);
        StatsSolo.Favor = soloDun.Sum(x => x.Favor);

        StatsSolo.LootInSilver = soloDun.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsSolo.MostValuableLoot = soloDun.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateStandardStats(List<DungeonBaseFragment> dungeons)
    {
        var stdDun = dungeons?.Where(x => x is RandomDungeonFragment { Mode: DungeonMode.Standard }).Cast<RandomDungeonFragment>().ToList() ?? new List<RandomDungeonFragment>();

        StatsStandard.RunTimeTotal = stdDun.Sum(x => x.TotalRunTimeInSeconds);

        StatsStandard.Entered = stdDun.Count;

        StatsStandard.EnteredUncommon = stdDun.Count(x => x.Level == 1);
        StatsStandard.EnteredRare = stdDun.Count(x => x.Level == 2);
        StatsStandard.EnteredEpic = stdDun.Count(x => x.Level == 3);
        StatsStandard.EnteredLegendary = stdDun.Count(x => x.Level == 4);

        StatsStandard.OpenedStandardChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest));
        StatsStandard.OpenedUncommonChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest));
        StatsStandard.OpenedRareChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest));
        StatsStandard.OpenedLegendaryChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest));

        StatsStandard.OpenedStandardBookChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest));
        StatsStandard.OpenedUncommonBookChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest));
        StatsStandard.OpenedRareBookChests = stdDun.Sum(x => x.Events.Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest));

        StatsStandard.Fame = stdDun.Sum(x => x.Fame);
        StatsStandard.ReSpec = stdDun.Sum(x => x.ReSpec);
        StatsStandard.Silver = stdDun.Sum(x => x.Silver);
        StatsStandard.Might = stdDun.Sum(x => x.Might);
        StatsStandard.Favor = stdDun.Sum(x => x.Favor);

        StatsStandard.LootInSilver = stdDun.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsStandard.MostValuableLoot = stdDun.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public StatsMists StatsMists
    {
        get => _statsMists;
        set
        {
            _statsMists = value;
            OnPropertyChanged();
        }
    }

    public StatsSolo StatsSolo
    {
        get => _statsSolo;
        set
        {
            _statsSolo = value;
            OnPropertyChanged();
        }
    }

    public StatsStandard StatsStandard
    {
        get => _statsStandard;
        set
        {
            _statsStandard = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationFame => LanguageController.Translation("FAME");
    public static string TranslationReSpec => LanguageController.Translation("RESPEC");
    public static string TranslationSilver => LanguageController.Translation("SILVER");
    public static string TranslationMight => LanguageController.Translation("MIGHT");
    public static string TranslationFavor => LanguageController.Translation("FAVOR");
    public static string TranslationLootInSilver => LanguageController.Translation("LOOT_IN_SILVER");
}