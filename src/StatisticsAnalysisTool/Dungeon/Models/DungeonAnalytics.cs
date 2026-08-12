using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonAnalytics : BaseViewModel
{
    public DungeonMetric TotalRuns { get; } = new();
    public DungeonMetric TotalFame { get; } = new(true);
    public DungeonMetric TotalReSpec { get; } = new(true);
    public DungeonMetric TotalMight { get; } = new(true);
    public DungeonMetric TotalFavor { get; } = new(true);
    public DungeonMetric TotalSilver { get; } = new(true);
    public DungeonMetric TotalLootValue { get; } = new(true);
    public DungeonMetric Deaths { get; } = new(false, true);
    public DungeonMetric AverageRunTime { get; } = new(false, true);

    public string ComparisonText
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string MostProfitableTierEnchantment
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "—";

    public double MostProfitableLootPerHour
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string MostPlayedTierEnchantment
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "—";

    public int MostPlayedTierRunCount
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Loot MostValuableLoot
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public string BestDungeonMap
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "—";

    public double BestDungeonMapAverageLoot
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasFastestRun
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FastestRunTimeInSeconds
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string FastestRunTierEnchantment
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "—";

    public int FastestRunPartySize
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageChestsPerRun
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<DungeonEfficiencyEntry> EfficiencyEntries
    {
        get;
        internal set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public void Update(
        IReadOnlyList<DungeonBaseFragment> currentDungeons,
        IReadOnlyList<DungeonBaseFragment> previousDungeons,
        string comparisonText,
        DungeonMode selectedMode)
    {
        ComparisonText = comparisonText;
        DungeonAnalyticsService.Populate(this, currentDungeons, previousDungeons, selectedMode);
    }

    public void RefreshLocalization()
    {
        OnPropertyChanged(null);
    }

    public static string TranslationOverview => LocalizationController.Translation("OVERVIEW");
    public static string TranslationDungeonOverview => LocalizationController.Translation("DUNGEON_OVERVIEW");
    public static string TranslationRunEfficiency => LocalizationController.Translation("RUN_EFFICIENCY");
    public static string TranslationTotalRuns => LocalizationController.Translation("TOTAL_RUNS");
    public static string TranslationTotalFame => LocalizationController.Translation("TOTAL_FAME");
    public static string TranslationTotalReSpec => LocalizationController.Translation("TOTAL_RESPEC");
    public static string TranslationTotalMight => LocalizationController.Translation("TOTAL_MIGHT");
    public static string TranslationTotalFavor => LocalizationController.Translation("TOTAL_FAVOR");
    public static string TranslationTotalSilver => LocalizationController.Translation("TOTAL_SILVER");
    public static string TranslationTotalLootValue => LocalizationController.Translation("TOTAL_LOOT_VALUE");
    public static string TranslationDungeonDeaths => LocalizationController.Translation("DUNGEON_DEATHS");
    public static string TranslationAverageRunTime => LocalizationController.Translation("AVERAGE_RUN_TIME");
    public static string TranslationMostProfitableTier => LocalizationController.Translation("MOST_PROFITABLE_TIER");
    public static string TranslationMostPlayedTier => LocalizationController.Translation("MOST_PLAYED_TIER");
    public static string TranslationMostValuableLoot => LocalizationController.Translation("MOST_VALUABLE_LOOT");
    public static string TranslationBestDungeonMap => LocalizationController.Translation("BEST_DUNGEON_MAP");
    public static string TranslationFastestRun => LocalizationController.Translation("FASTEST_RUN");
    public static string TranslationAverageChestsPerRun => LocalizationController.Translation("AVERAGE_CHESTS_PER_RUN");
    public static string TranslationAverageLootPerRun => LocalizationController.Translation("AVERAGE_LOOT_PER_RUN");
    public static string TranslationAverageFamePerRun => LocalizationController.Translation("AVERAGE_FAME_PER_RUN");
    public static string TranslationAverageDuration => LocalizationController.Translation("AVERAGE_DURATION");
    public static string TranslationPartySize => LocalizationController.Translation("PARTY_SIZE");
    public static string TranslationLootValuePerHour => LocalizationController.Translation("LOOT_VALUE_PER_HOUR");
    public static string TranslationRuns => LocalizationController.Translation("RUNS");
}