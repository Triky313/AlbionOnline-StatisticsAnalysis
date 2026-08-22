using FontAwesome5;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DashboardBindings : BaseViewModel
{
    private DashboardFactionOption _selectedFactionOption;

    public DashboardBindings()
    {
        _selectedFactionOption = FactionOptions.FirstOrDefault(x => x.Faction == SettingsController.CurrentSettings.SelectedDashboardFaction)
                                 ?? FactionOptions[0];

        RepairCostsChestVisibility = Settings.Default.IsContainerRepairCostsVisible ? Visibility.Visible : Visibility.Collapsed;

        KillDeathStatsVisibility = SettingsController.CurrentSettings.IsKillDeathStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        KillDeathStatsToggleIcon = SettingsController.CurrentSettings.IsKillDeathStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        TopKillLocationsVisibility = SettingsController.CurrentSettings.IsTopKillLocationsVisible ? Visibility.Visible : Visibility.Collapsed;
        TopKillLocationsToggleIcon = SettingsController.CurrentSettings.IsTopKillLocationsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        TopDeathLocationsVisibility = SettingsController.CurrentSettings.IsTopDeathLocationsVisible ? Visibility.Visible : Visibility.Collapsed;
        TopDeathLocationsToggleIcon = SettingsController.CurrentSettings.IsTopDeathLocationsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        RecentKillsDeathsVisibility = SettingsController.CurrentSettings.IsRecentKillsDeathsVisible ? Visibility.Visible : Visibility.Collapsed;
        RecentKillsDeathsToggleIcon = SettingsController.CurrentSettings.IsRecentKillsDeathsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        FactionSummaryVisibility = SettingsController.CurrentSettings.IsFactionSummaryVisible ? Visibility.Visible : Visibility.Collapsed;
        FactionSummaryToggleIcon = SettingsController.CurrentSettings.IsFactionSummaryVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        var isContentRankingVisible = SettingsController.CurrentSettings.IsFameContentRankingVisible
                                      || SettingsController.CurrentSettings.IsSilverContentRankingVisible;
        ContentRankingVisibility = isContentRankingVisible ? Visibility.Visible : Visibility.Collapsed;
        ContentRankingToggleIcon = isContentRankingVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        LootedChestsStatsVisibility = SettingsController.CurrentSettings.IsLootedChestsStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        LootedChestsStatsToggleIcon = SettingsController.CurrentSettings.IsLootedChestsStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        LootStatsVisibility = SettingsController.CurrentSettings.IsLootStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        LootStatsToggleIcon = SettingsController.CurrentSettings.IsLootStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        ReSpecStatsVisibility = SettingsController.CurrentSettings.IsReSpecStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        ReSpecStatsToggleIcon = SettingsController.CurrentSettings.IsReSpecStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        RepairCostsStatsVisibility = SettingsController.CurrentSettings.IsRepairCostsStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        RepairCostsStatsToggleIcon = SettingsController.CurrentSettings.IsRepairCostsStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        ActivityChartVisibility = SettingsController.CurrentSettings.IsActivityChartVisible ? Visibility.Visible : Visibility.Collapsed;
        ActivityChartToggleIcon = SettingsController.CurrentSettings.IsActivityChartVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;
    }

    #region Summary

    public DashboardSummaryMetric FameSummary { get; } = new();
    public DashboardSummaryMetric ReSpecSummary { get; } = new();
    public DashboardSummaryMetric SilverSummary { get; } = new();
    public DashboardSummaryMetric MightSummary { get; } = new();
    public DashboardSummaryMetric FavorSummary { get; } = new();
    public DashboardSummaryMetric SessionTimeSummary { get; } = new();
    public DashboardSummaryMetric FactionPointsSummary { get; } = new();
    public DashboardSummaryMetric FactionStandingSummary { get; } = new();
    public DashboardSummaryMetric EconomyReSpecSummary { get; } = new();
    public DashboardSummaryMetric RepairCostsSummary { get; } = new();
    public DashboardSummaryMetric ItemQualityRerollCostsSummary { get; } = new();
    public DashboardSummaryMetric AwakenedWeaponCostsSummary { get; } = new();
    public DashboardMobsBindings Mobs { get; } = new();

    public IReadOnlyList<DashboardFactionOption> FactionOptions { get; } =
    [
        new(CityFaction.Caerleon, "Caerleon", "caerleon"),
        new(CityFaction.FortSterling, "Fort Sterling", "fortsterling"),
        new(CityFaction.Thetford, "Thetford", "thetford"),
        new(CityFaction.Lymhurst, "Lymhurst", "lymhurst"),
        new(CityFaction.Bridgewatch, "Bridgewatch", "bridgewatch"),
        new(CityFaction.Martlock, "Martlock", "martlock")
    ];

    public DashboardFactionOption SelectedFactionOption
    {
        get => _selectedFactionOption;
        set
        {
            if (value == null || ReferenceEquals(_selectedFactionOption, value))
            {
                return;
            }

            _selectedFactionOption = value;
            SettingsController.CurrentSettings.SelectedDashboardFaction = value.Faction;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardContentRankingItem> FameContentRanking { get; } = [];
    public ObservableCollection<DashboardContentRankingItem> SilverContentRanking { get; } = [];

    public double TotalFameByContent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double TotalSilverByContent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string SummaryComparisonText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = TranslationVsPreviousHour;

    #endregion

    #region Toggle

    public Visibility FactionSummaryVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsFactionSummaryVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon FactionSummaryToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility KillDeathStatsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsKillDeathStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon KillDeathStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility TopKillLocationsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsTopKillLocationsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon TopKillLocationsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility TopDeathLocationsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsTopDeathLocationsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon TopDeathLocationsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility RecentKillsDeathsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsRecentKillsDeathsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon RecentKillsDeathsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility ContentRankingVisibility
    {
        get;
        set
        {
            field = value;
            var isVisible = value == Visibility.Visible;
            SettingsController.CurrentSettings.IsFameContentRankingVisible = isVisible;
            SettingsController.CurrentSettings.IsSilverContentRankingVisible = isVisible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon ContentRankingToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility LootedChestsStatsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsLootedChestsStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon LootedChestsStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility LootStatsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsLootStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon LootStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility LootValueDistributionVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public EFontAwesomeIcon LootValueDistributionToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = EFontAwesomeIcon.Solid_Minus;

    public Visibility LootTierEnchantmentVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public EFontAwesomeIcon LootTierEnchantmentToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = EFontAwesomeIcon.Solid_Minus;

    public Visibility TopLootAreasVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public EFontAwesomeIcon TopLootAreasToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = EFontAwesomeIcon.Solid_Minus;

    public Visibility ReSpecStatsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsReSpecStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon ReSpecStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility RepairCostsStatsVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsRepairCostsStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon RepairCostsStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility ItemQualityRerollStatsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public EFontAwesomeIcon ItemQualityRerollStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = EFontAwesomeIcon.Solid_Minus;

    public Visibility AwakenedWeaponStatsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public EFontAwesomeIcon AwakenedWeaponStatsToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = EFontAwesomeIcon.Solid_Minus;

    public Visibility ActivityChartVisibility
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsActivityChartVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon ActivityChartToggleIcon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Fame / Respec / Silver / Might / Faction

    public double GetHighestValue()
    {
        var values = new List<double>()
            {
                TotalGainedFameInSession,
                TotalGainedSilverInSession,
                TotalGainedReSpecPointsInSession,
                TotalGainedMightInSession,
                TotalGainedFavorInSession
            };

        return values.Max<double>();
    }

    public void Reset()
    {
        HighestValue = 0;

        FamePerHour = 0;
        SilverPerHour = 0;
        ReSpecPointsPerHour = 0;
        MightPerHour = 0;
        FavorPerHour = 0;

        TotalGainedFameInSession = 0;
        TotalGainedSilverInSession = 0;
        TotalGainedReSpecPointsInSession = 0;
        TotalGainedMightInSession = 0;
        TotalGainedFavorInSession = 0;
        Mobs.ResetStatistics();
    }

    #region Per hour values

    public double HighestValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FamePerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double SilverPerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsPerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MightPerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FavorPerHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double SilverCostForReSpecHour
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Percent values

    public double FameInPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double SilverInPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsInPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MightInPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FavorInPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Total values

    public double TotalGainedFameInSession
    {
        get;
        set
        {
            field = value;
            HighestValue = GetHighestValue();
            FameInPercent = field / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedSilverInSession
    {
        get;
        set
        {
            field = value;
            HighestValue = GetHighestValue();
            SilverInPercent = field / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedReSpecPointsInSession
    {
        get;
        set
        {
            field = value;
            HighestValue = GetHighestValue();
            ReSpecPointsInPercent = field / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedMightInSession
    {
        get;
        set
        {
            field = value;
            HighestValue = GetHighestValue();
            MightInPercent = field / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedFavorInSession
    {
        get;
        set
        {
            field = value;
            HighestValue = GetHighestValue();
            FavorInPercent = field / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalSilverCostForReSpecInSession
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Kill / Death stats

    public string KillsDeathsText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = TranslationKillsDeaths;

    public int SoloKillsToday
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int SoloKillsThisWeek
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int SoloKillsThisMonth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int KillsToday
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int KillsThisWeek
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int KillsThisMonth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int DeathsToday
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int DeathsThisWeek
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int DeathsThisMonth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerWhenKilling
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerOfTheKilledEnemies
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerWhenDying
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DateTime? LastUpdate
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Chest stats

    public LootedChests LootedChests
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public DashboardLootStatistics LootStatistics { get; } = new();
    public DashboardCombatStatistics CombatStatistics { get; } = new();

    #endregion

    #region Repair costs

    public double ReSpecSilverCost
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageReSpecSilverCostPerSession
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double SpentReSpec
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility SpentReSpecVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public int ReSpecDetailColumnCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 2;

    public int GoodItemQualityRerollCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int AwakenedWeaponTraitUpgradeCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int AwakenedWeaponTraitUpgradeProcCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int OutstandingItemQualityRerollCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int ExcellentItemQualityRerollCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int MasterpieceItemQualityRerollCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long TotalItemQualityRerollCount
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double GoodItemQualityRerollPercentage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double OutstandingItemQualityRerollPercentage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ExcellentItemQualityRerollPercentage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double MasterpieceItemQualityRerollPercentage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AverageRepairCostPerSession
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double HighestRepairCost
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsToday
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsLast7Days
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsLast30Days
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsChest
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility RepairCostsChestVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationDashboardTitle => $"{LocalizationController.Translation("DASHBOARD")}";
    public static string TranslationCombatTitle => LocalizationController.Translation("COMBAT");
    public static string TranslationEconomyTitle => $"{LocalizationController.Translation("ECONOMY")}";
    public static string TranslationDeleteSession => LocalizationController.Translation("DELETE_SESSION");
    public static string TranslationFame => LocalizationController.Translation("FAME");
    public static string TranslationSilver => LocalizationController.Translation("SILVER");
    public static string TranslationReSpec => LocalizationController.Translation("RESPEC");
    public static string TranslationFaction => LocalizationController.Translation("FACTION");
    public static string TranslationMight => LocalizationController.Translation("MIGHT");
    public static string TranslationFavor => LocalizationController.Translation("FAVOR");
    public static string TranslationSessionTime => LocalizationController.Translation("SESSION_TIME");
    public static string TranslationTimeRange => LocalizationController.Translation("TIME_RANGE");
    public static string TranslationContent => LocalizationController.Translation("CONTENT");
    public static string TranslationSession => LocalizationController.Translation("SESSION");
    public static string TranslationResetSession => LocalizationController.Translation("RESET_SESSION");
    public static string TranslationResetSessionOnMapChange => LocalizationController.Translation("RESET_SESSION_ON_MAP_CHANGE");
    public static string TranslationTopFameSources => LocalizationController.Translation("TOP_FAME_SOURCES");
    public static string TranslationTopSilverSources => LocalizationController.Translation("TOP_SILVER_SOURCES");
    public static string TranslationTotalFame => LocalizationController.Translation("TOTAL_FAME");
    public static string TranslationTotalSilver => LocalizationController.Translation("TOTAL_SILVER");
    public static string TranslationVsPreviousMinutes => LocalizationController.Translation("VS_PREVIOUS_MINUTES");
    public static string TranslationVsPreviousHour => LocalizationController.Translation("VS_PREVIOUS_HOUR");
    public static string TranslationVsPreviousHours => LocalizationController.Translation("VS_PREVIOUS_HOURS");
    public static string TranslationVsPreviousDay => LocalizationController.Translation("VS_PREVIOUS_DAY");
    public static string TranslationVsPreviousDays => LocalizationController.Translation("VS_PREVIOUS_DAYS");
    public static string TranslationToday => LocalizationController.Translation("TODAY").ToLower();
    public static string TranslationWeek => LocalizationController.Translation("WEEK").ToLower();
    public static string TranslationMonth => LocalizationController.Translation("MONTH").ToLower();
    public static string TranslationKills => LocalizationController.Translation("KILLS");
    public static string TranslationSoloKills => LocalizationController.Translation("SOLO_KILLS");
    public static string TranslationDeaths => LocalizationController.Translation("DEATHS");
    public static string TranslationKnockouts => GetCombatTranslation("KNOCKOUTS", "Knockouts", "Knockouts");
    public static string TranslationKnockedOut => GetCombatTranslation("KNOCKED_OUT", "Ausgeknockt", "Knocked out");
    public static string TranslationKillDeathRatio => GetCombatTranslation("KILL_DEATH_RATIO", "K/D-Verh\u00E4ltnis", "K/D ratio");
    public static string TranslationPlayerCorpseLoot => GetCombatTranslation("PLAYER_CORPSE_LOOT", "Loot aus Spielerleichen", "Player corpse loot");
    public static string TranslationTopKillLocations => GetCombatTranslation("TOP_KILL_LOCATIONS", "Top-T\u00F6tungsorte", "Top kill locations");
    public static string TranslationTopDeathLocations => GetCombatTranslation("TOP_DEATH_LOCATIONS", "Top-Todesorte", "Top death locations");
    public static string TranslationRecentKillsDeaths => GetCombatTranslation("RECENT_KILLS_DEATHS", "Letzte Kampfergebnisse", "Recent combat results");
    public static string TranslationTime => LocalizationController.Translation("TIME");
    public static string TranslationResult => GetCombatTranslation("RESULT", "Ergebnis", "Result");
    public static string TranslationMap => LocalizationController.Translation("MAP");
    public static string TranslationTargetKiller => GetCombatTranslation("TARGET_KILLER", "Ziel / Killer", "Target / Killer");
    public static string TranslationEstimatedValueShort => GetCombatTranslation("ESTIMATED_VALUE_SHORT", "Gesch. Wert", "Est. value");
    public static string TranslationTotal => LocalizationController.Translation("TOTAL");
    public static string TranslationLastUpdate => LocalizationController.Translation("LAST_UPDATE");
    public static string TranslationDataFromAlbionOnlineServers => LocalizationController.Translation("DATA_FROM_ALBION_ONLINE_SERVERS");
    public static string TranslationAverageItemPowerWhenKilling => LocalizationController.Translation("AVERAGE_ITEM_POWER_WHEN_KILLING");
    public static string TranslationAverageItemPowerOfTheKilledEnemies => LocalizationController.Translation("AVERAGE_ITEM_POWER_OF_THE_KILLED_ENEMIES");
    public static string TranslationAverageItemPowerWhenDying => LocalizationController.Translation("AVERAGE_ITEM_POWER_WHEN_DYING");
    public static string TranslationPaidSilverForReSpecThisSession => LocalizationController.Translation("PAID_SILVER_FOR_RESPEC_THIS_SESSION");
    public static string TranslationPaidSilverForReSpecPerHour => LocalizationController.Translation("PAID_SILVER_FOR_RESPEC_PER_HOUR");
    public static string TranslationRepairCostsToday => LocalizationController.Translation("REPAIR_COSTS_TODAY");
    public static string TranslationRepairCostsLast7Days => LocalizationController.Translation("REPAIR_COSTS_LAST_7_DAYS");
    public static string TranslationRepairCostsLast30Days => LocalizationController.Translation("REPAIR_COSTS_LAST_30_DAYS");
    public static string TranslationKillsDeaths => LocalizationController.Translation("KILLS_DEATHS");
    public static string TranslationKillsDeathsLoading => LocalizationController.Translation("KILLS_DEATHS_LOADING");
    public static string TranslationLootedChests => LocalizationController.Translation("LOOTED_CHESTS");

    public static string TranslationLoot => LocalizationController.Translation("LOOT");
    public static string TranslationImportantLoot => LocalizationController.Translation("IMPORTANT_LOOT");
    public static string TranslationRecentLootItems => LocalizationController.Translation("RECENT_LOOT_ITEMS");
    public static string TranslationMostValuableLoot => LocalizationController.Translation("MOST_VALUABLE_LOOT");
    public static string TranslationTotalLootValue => LocalizationController.Translation("TOTAL_LOOT_VALUE");
    public static string TranslationAverageLootValue => LocalizationController.Translation("AVERAGE_LOOT_VALUE");
    public static string TranslationTopLootAreas => LocalizationController.Translation("TOP_LOOT_AREAS");
    public static string TranslationLootValueDistribution => LocalizationController.Translation("LOOT_VALUE_DISTRIBUTION");
    public static string TranslationLootByTierAndEnchantment => LocalizationController.Translation("LOOT_BY_TIER_AND_ENCHANTMENT");
    public static string TranslationValueClass => LocalizationController.Translation("VALUE_CLASS");
    public static string TranslationItems => LocalizationController.Translation("ITEMS");
    public static string TranslationShareOfTotalValue => LocalizationController.Translation("SHARE_OF_TOTAL_VALUE");
    public static string TranslationTierDistribution => LocalizationController.Translation("TIER_DISTRIBUTION");
    public static string TranslationEnchantment => LocalizationController.Translation("ENCHANTMENT");
    public static string TranslationMapArea => LocalizationController.Translation("MAP_AREA");
    public static string TranslationLootValue => LocalizationController.Translation("LOOT_VALUE");
    public static string TranslationLootPerHour => LocalizationController.Translation("LOOT_PER_HOUR_SHORT");
    public static string TranslationLootPerMap => LocalizationController.Translation("LOOT_PER_MAP");
    public static string TranslationVisits => LocalizationController.Translation("VISITS");
    public static string TranslationShare => LocalizationController.Translation("SHARE");
    public static string TranslationMaps => LocalizationController.Translation("MAPS");
    public static string TranslationItemQuantity => LocalizationController.Translation("ITEM_QUANTITY");
    public static string TranslationRepairCosts => LocalizationController.Translation("REPAIR_COSTS");
    public static string TranslationActivityChart => LocalizationController.Translation("HISTORY");
    public static string TranslationFactionPoints => LocalizationController.Translation("FACTION_POINTS");
    public static string TranslationSilverSpentOnReSpec => LocalizationController.Translation("SILVER_SPENT_ON_RESPEC");
    public static string TranslationAverageReSpecSilverCostPerSession => LocalizationController.Translation("AVERAGE_RESPEC_SILVER_COST_PER_SESSION");
    public static string TranslationSpentReSpec => LocalizationController.Translation("SPENT_RESPEC");
    public static string TranslationRefineItemQuality => LocalizationController.Translation("REFINE_ITEM_QUALITY");
    public static string TranslationSilverSpentOnRefiningItemQuality => LocalizationController.Translation("SILVER_SPENT_ON_REFINING_ITEM_QUALITY");
    public static string TranslationItemsUpgraded => LocalizationController.Translation("ITEMS_UPGRADED");
    public static string TranslationAwakenWeapon => LocalizationController.Translation("AWAKEN_WEAPON");
    public static string TranslationSilverSpentOnAwakenedWeapons => LocalizationController.Translation("SILVER_SPENT_ON_AWAKENED_WEAPONS");
    public static string TranslationTraitsUpgraded => LocalizationController.Translation("TRAITS_UPGRADED");
    public static string TranslationTraitUpgradeProcs => LocalizationController.Translation("TRAIT_UPGRADE_PROCS");
    public static string TranslationGood => LocalizationController.Translation("GOOD");
    public static string TranslationOutstanding => LocalizationController.Translation("OUTSTANDING");
    public static string TranslationExcellent => LocalizationController.Translation("EXCELLENT");
    public static string TranslationMasterpiece => LocalizationController.Translation("MASTERPIECE");
    public static string TranslationAverageRepairCostPerSession => LocalizationController.Translation("AVERAGE_REPAIR_COST_PER_SESSION");
    public static string TranslationHighestRepairCost => LocalizationController.Translation("HIGHEST_REPAIR_COST");
    public static string TranslationFactionStanding => LocalizationController.Translation("FACTION_STANDING");
    private static string GetCombatTranslation(string key, string germanText, string englishText)
    {
        var translation = LocalizationController.Translation(key);
        if (!string.Equals(translation, key, StringComparison.Ordinal))
        {
            return translation;
        }

        var cultureName = SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag ?? string.Empty;
        return cultureName.StartsWith("de", StringComparison.OrdinalIgnoreCase)
            ? germanText
            : englishText;
    }
}
