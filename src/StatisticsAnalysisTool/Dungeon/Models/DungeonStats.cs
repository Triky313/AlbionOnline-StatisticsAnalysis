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
    private StatsMistsDungeon _statsMistsDungeon = new ();
    private StatsExpedition _statsExpedition = new ();
    private StatsCorrupted _statsCorrupted = new();
    private StatsHellGate _statsHellGate = new();
    private StatsAvalonian _statsAvalonian = new();

    public void Set(IEnumerable<DungeonBaseFragment> dungeons)
    {
        Set(dungeons.ToList());
    }

    public void Set(List<DungeonBaseFragment> dungeons)
    {
        UpdateMistsStats(dungeons);
        UpdateMistsDungeonStats(dungeons);
        UpdateSoloStats(dungeons);
        UpdateStandardStats(dungeons);
        UpdateAvalonianStats(dungeons);
        UpdateExpeditionStats(dungeons);
        UpdateCorruptedStats(dungeons);
        UpdateHellGateStats(dungeons);
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

    public void UpdateMistsDungeonStats(List<DungeonBaseFragment> dungeons)
    {
        var mistsDungeons = dungeons?.Where(x => x is MistsDungeonFragment).Cast<MistsDungeonFragment>().ToList() ?? new List<MistsDungeonFragment>();

        StatsMistsDungeon.RunTimeTotal = mistsDungeons.Sum(x => x.TotalRunTimeInSeconds);

        StatsMistsDungeon.Entered = mistsDungeons.Count;

        StatsMistsDungeon.Fame = mistsDungeons.Sum(x => x.Fame);
        StatsMistsDungeon.ReSpec = mistsDungeons.Sum(x => x.ReSpec);
        StatsMistsDungeon.Silver = mistsDungeons.Sum(x => x.Silver);
        StatsMistsDungeon.Might = mistsDungeons.Sum(x => x.Might);
        StatsMistsDungeon.Favor = mistsDungeons.Sum(x => x.Favor);

        StatsMistsDungeon.LootInSilver = mistsDungeons.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsMistsDungeon.MostValuableLoot = mistsDungeons.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateSoloStats(List<DungeonBaseFragment> dungeons)
    {
        var soloDun = dungeons?.Where(x => x is RandomDungeonFragment { Mode: DungeonMode.Solo }).Cast<RandomDungeonFragment>().ToList() ?? new List<RandomDungeonFragment>();

        StatsSolo.RunTimeTotal = soloDun.Sum(x => x.TotalRunTimeInSeconds);

        StatsSolo.Entered = soloDun.Count;
        
        StatsSolo.EnteredCommon = soloDun.Count(x => x.Level == 0);
        StatsSolo.EnteredUncommon = soloDun.Count(x => x.Level == 1);
        StatsSolo.EnteredRare = soloDun.Count(x => x.Level == 2);
        StatsSolo.EnteredEpic = soloDun.Count(x => x.Level == 3);
        StatsSolo.EnteredLegendary = soloDun.Count(x => x.Level == 4);

        StatsSolo.OpenedStandardChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsSolo.OpenedUncommonChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsSolo.OpenedRareChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsSolo.OpenedLegendaryChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));

        StatsSolo.OpenedStandardBookChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsSolo.OpenedUncommonBookChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsSolo.OpenedRareBookChests = soloDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));

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

        StatsStandard.EnteredCommon = stdDun.Count(x => x.Level == 0);
        StatsStandard.EnteredUncommon = stdDun.Count(x => x.Level == 1);
        StatsStandard.EnteredRare = stdDun.Count(x => x.Level == 2);
        StatsStandard.EnteredEpic = stdDun.Count(x => x.Level == 3);
        StatsStandard.EnteredLegendary = stdDun.Count(x => x.Level == 4);

        StatsStandard.OpenedStandardChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsStandard.OpenedUncommonChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsStandard.OpenedRareChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsStandard.OpenedLegendaryChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));

        StatsStandard.OpenedStandardBookChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsStandard.OpenedUncommonBookChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsStandard.OpenedRareBookChests = stdDun.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));

        StatsStandard.Fame = stdDun.Sum(x => x.Fame);
        StatsStandard.ReSpec = stdDun.Sum(x => x.ReSpec);
        StatsStandard.Silver = stdDun.Sum(x => x.Silver);
        StatsStandard.Might = stdDun.Sum(x => x.Might);
        StatsStandard.Favor = stdDun.Sum(x => x.Favor);

        StatsStandard.LootInSilver = stdDun.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsStandard.MostValuableLoot = stdDun.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateAvalonianStats(List<DungeonBaseFragment> dungeons)
    {
        var ava = dungeons?.Where(x => x is RandomDungeonFragment { Mode: DungeonMode.Avalon }).Cast<RandomDungeonFragment>().ToList() ?? new List<RandomDungeonFragment>();

        StatsAvalonian.RunTimeTotal = ava.Sum(x => x.TotalRunTimeInSeconds);

        StatsAvalonian.Entered = ava.Count;

        StatsAvalonian.EnteredCommon = ava.Count(x => x.Level == 0);
        StatsAvalonian.EnteredUncommon = ava.Count(x => x.Level == 1);
        StatsAvalonian.EnteredRare = ava.Count(x => x.Level == 2);
        StatsAvalonian.EnteredEpic = ava.Count(x => x.Level == 3);
        StatsAvalonian.EnteredLegendary = ava.Count(x => x.Level == 4);

        StatsAvalonian.OpenedStandardChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsAvalonian.OpenedUncommonChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsAvalonian.OpenedRareChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsAvalonian.OpenedLegendaryChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));

        StatsAvalonian.OpenedStandardBookChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsAvalonian.OpenedUncommonBookChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsAvalonian.OpenedRareBookChests = ava.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));

        StatsAvalonian.Fame = ava.Sum(x => x.Fame);
        StatsAvalonian.ReSpec = ava.Sum(x => x.ReSpec);
        StatsAvalonian.Silver = ava.Sum(x => x.Silver);
        StatsAvalonian.Might = ava.Sum(x => x.Might);
        StatsAvalonian.Favor = ava.Sum(x => x.Favor);

        StatsAvalonian.LootInSilver = ava.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsAvalonian.MostValuableLoot = ava.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateExpeditionStats(List<DungeonBaseFragment> dungeons)
    {
        var hce = dungeons?.Where(x => x is ExpeditionFragment).Cast<ExpeditionFragment>().ToList() ?? new List<ExpeditionFragment>();

        StatsExpedition.RunTimeTotal = hce.Sum(x => x.TotalRunTimeInSeconds);

        StatsExpedition.Entered = hce.Count;

        StatsExpedition.Fame = hce.Sum(x => x.Fame);
        StatsExpedition.ReSpec = hce.Sum(x => x.ReSpec);
        StatsExpedition.Silver = hce.Sum(x => x.Silver);

        StatsExpedition.LootInSilver = hce.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsExpedition.MostValuableLoot = hce.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateCorruptedStats(List<DungeonBaseFragment> dungeons)
    {
        var corrupted = dungeons?.Where(x => x is CorruptedFragment).Cast<CorruptedFragment>().ToList() ?? new List<CorruptedFragment>();

        StatsCorrupted.RunTimeTotal = corrupted.Sum(x => x.TotalRunTimeInSeconds);

        StatsCorrupted.Entered = corrupted.Count;

        StatsCorrupted.OpenedStandardChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsCorrupted.OpenedUncommonChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsCorrupted.OpenedRareChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsCorrupted.OpenedLegendaryChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));

        StatsCorrupted.OpenedStandardBookChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsCorrupted.OpenedUncommonBookChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsCorrupted.OpenedRareBookChests = corrupted.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));

        StatsCorrupted.Fame = corrupted.Sum(x => x.Fame);
        StatsCorrupted.ReSpec = corrupted.Sum(x => x.ReSpec);
        StatsCorrupted.Silver = corrupted.Sum(x => x.Silver);
        StatsCorrupted.Might = corrupted.Sum(x => x.Might);
        StatsCorrupted.Favor = corrupted.Sum(x => x.Favor);

        StatsCorrupted.LootInSilver = corrupted.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsCorrupted.MostValuableLoot = corrupted.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
    }

    public void UpdateHellGateStats(List<DungeonBaseFragment> dungeons)
    {
        var hellGate = dungeons?.Where(x => x is HellGateFragment).Cast<HellGateFragment>().ToList() ?? new List<HellGateFragment>();

        StatsHellGate.RunTimeTotal = hellGate.Sum(x => x.TotalRunTimeInSeconds);

        StatsHellGate.Entered = hellGate.Count;

        StatsHellGate.OpenedStandardChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsHellGate.OpenedUncommonChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsHellGate.OpenedRareChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));
        StatsHellGate.OpenedLegendaryChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Legendary && eventObject.Type == EventType.Chest && eventObject.Status == ChestStatus.Open));

        StatsHellGate.OpenedStandardBookChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Common && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsHellGate.OpenedUncommonBookChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Uncommon && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));
        StatsHellGate.OpenedRareBookChests = hellGate.Sum(x => x.Events
            .Count(eventObject => eventObject.Rarity == TreasureRarity.Rare && eventObject.Type == EventType.BookChest && eventObject.Status == ChestStatus.Open));

        StatsHellGate.Fame = hellGate.Sum(x => x.Fame);
        StatsHellGate.ReSpec = hellGate.Sum(x => x.ReSpec);
        StatsHellGate.Silver = hellGate.Sum(x => x.Silver);
        StatsHellGate.Might = hellGate.Sum(x => x.Might);
        StatsHellGate.Favor = hellGate.Sum(x => x.Favor);

        StatsHellGate.Kills = hellGate.Count(x => x.KillStatus == KillStatus.OpponentDead);
        StatsHellGate.Deaths = hellGate.Count(x => x.KillStatus == KillStatus.LocalPlayerDead);
        StatsHellGate.Fights = hellGate.Count(x => x.KillStatus is KillStatus.LocalPlayerDead or KillStatus.OpponentDead);

        StatsHellGate.LootInSilver = hellGate.SelectMany(x => x.Loot).Sum(x => FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        StatsHellGate.MostValuableLoot = hellGate.SelectMany(x => x.Loot).MaxBy(x => x?.EstimatedMarketValueInternal);
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

    public StatsMistsDungeon StatsMistsDungeon
    {
        get => _statsMistsDungeon;
        set
        {
            _statsMistsDungeon = value;
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

    public StatsAvalonian StatsAvalonian
    {
        get => _statsAvalonian;
        set
        {
            _statsAvalonian = value;
            OnPropertyChanged();
        }
    }

    public StatsExpedition StatsExpedition
    {
        get => _statsExpedition;
        set
        {
            _statsExpedition = value;
            OnPropertyChanged();
        }
    }

    public StatsCorrupted StatsCorrupted
    {
        get => _statsCorrupted;
        set
        {
            _statsCorrupted = value;
            OnPropertyChanged();
        }
    }

    public StatsHellGate StatsHellGate
    {
        get => _statsHellGate;
        set
        {
            _statsHellGate = value;
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