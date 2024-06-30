using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FontAwesome5;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DashboardBindings : BaseViewModel
{
    private double _famePerHour;
    private double _reSpecPointsPerHour;
    private double _silverPerHour;
    private double _mightPerHour;
    private double _favorPerHour;
    private double _silverCostForReSpecHour;
    private double _totalGainedFameInSessionInSession;
    private double _totalGainedReSpecPointsInSessionInSession;
    private double _totalGainedSilverInSessionInSession;
    private double _totalGainedMightInSession;
    private double _totalGainedFavorInSession;
    private double _totalSilverCostForReSpecInSession;
    private double _highestValue;
    private double _fameInPercent;
    private double _silverInPercent;
    private double _reSpecPointsInPercent;
    private double _mightInPercent;
    private double _favorInPercent;
    private int _killsToday;
    private int _deathsToday;
    private int _killsThisWeek;
    private int _deathsThisWeek;
    private DateTime? _lastUpdate;
    private double _averageItemPowerWhenKilling;
    private double _averageItemPowerOfTheKilledEnemies;
    private double _averageItemPowerWhenDying;
    private int _killsThisMonth;
    private int _deathsThisMonth;
    private int _soloKillsToday;
    private int _soloKillsThisWeek;
    private int _soloKillsThisMonth;
    private LootedChests _lootedChests = new();
    private long _repairCostsToday;
    private long _repairCostsLast7Days;
    private long _repairCostsLast30Days;
    private long _repairCostsChest;
    private Visibility _repairCostsChestVisibility;
    private Visibility _killDeathStatsVisibility;
    private EFontAwesomeIcon _killDeathStatsToggleIcon;
    private Visibility _lootedChestsStatsVisibility;
    private EFontAwesomeIcon _lootedChestsStatsToggleIcon;
    private Visibility _reSpecStatsVisibility;
    private EFontAwesomeIcon _reSpecStatsToggleIcon;
    private Visibility _repairCostsStatsVisibility;
    private EFontAwesomeIcon _repairCostsStatsToggleIcon;

    public DashboardBindings()
    {
        RepairCostsChestVisibility = Settings.Default.IsContainerRepairCostsVisible ? Visibility.Visible : Visibility.Collapsed;

        KillDeathStatsVisibility = SettingsController.CurrentSettings.IsKillDeathStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        KillDeathStatsToggleIcon = SettingsController.CurrentSettings.IsKillDeathStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        LootedChestsStatsVisibility = SettingsController.CurrentSettings.IsLootedChestsStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        LootedChestsStatsToggleIcon = SettingsController.CurrentSettings.IsLootedChestsStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        ReSpecStatsVisibility = SettingsController.CurrentSettings.IsReSpecStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        ReSpecStatsToggleIcon = SettingsController.CurrentSettings.IsReSpecStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;

        RepairCostsStatsVisibility = SettingsController.CurrentSettings.IsRepairCostsStatsVisible ? Visibility.Visible : Visibility.Collapsed;
        RepairCostsStatsToggleIcon = SettingsController.CurrentSettings.IsRepairCostsStatsVisible ? EFontAwesomeIcon.Solid_Minus : EFontAwesomeIcon.Solid_Plus;
    }

    #region Toggle

    public Visibility KillDeathStatsVisibility
    {
        get => _killDeathStatsVisibility;
        set
        {
            _killDeathStatsVisibility = value;
            SettingsController.CurrentSettings.IsKillDeathStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon KillDeathStatsToggleIcon
    {
        get => _killDeathStatsToggleIcon;
        set
        {
            _killDeathStatsToggleIcon = value;
            OnPropertyChanged();
        }
    }

    public Visibility LootedChestsStatsVisibility
    {
        get => _lootedChestsStatsVisibility;
        set
        {
            _lootedChestsStatsVisibility = value;
            SettingsController.CurrentSettings.IsLootedChestsStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon LootedChestsStatsToggleIcon
    {
        get => _lootedChestsStatsToggleIcon;
        set
        {
            _lootedChestsStatsToggleIcon = value;
            OnPropertyChanged();
        }
    }

    public Visibility ReSpecStatsVisibility
    {
        get => _reSpecStatsVisibility;
        set
        {
            _reSpecStatsVisibility = value;
            SettingsController.CurrentSettings.IsReSpecStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon ReSpecStatsToggleIcon
    {
        get => _reSpecStatsToggleIcon;
        set
        {
            _reSpecStatsToggleIcon = value;
            OnPropertyChanged();
        }
    }

    public Visibility RepairCostsStatsVisibility
    {
        get => _repairCostsStatsVisibility;
        set
        {
            _repairCostsStatsVisibility = value;
            SettingsController.CurrentSettings.IsRepairCostsStatsVisible = value == Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public EFontAwesomeIcon RepairCostsStatsToggleIcon
    {
        get => _repairCostsStatsToggleIcon;
        set
        {
            _repairCostsStatsToggleIcon = value;
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
    }

    #region Per hour values

    public double HighestValue
    {
        get => _highestValue;
        set
        {
            _highestValue = value;
            OnPropertyChanged();
        }
    }

    public double FamePerHour
    {
        get => _famePerHour;
        set
        {
            _famePerHour = value;
            OnPropertyChanged();
        }
    }

    public double SilverPerHour
    {
        get => _silverPerHour;
        set
        {
            _silverPerHour = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsPerHour
    {
        get => _reSpecPointsPerHour;
        set
        {
            _reSpecPointsPerHour = value;
            OnPropertyChanged();
        }
    }

    public double MightPerHour
    {
        get => _mightPerHour;
        set
        {
            _mightPerHour = value;
            OnPropertyChanged();
        }
    }

    public double FavorPerHour
    {
        get => _favorPerHour;
        set
        {
            _favorPerHour = value;
            OnPropertyChanged();
        }
    }

    public double SilverCostForReSpecHour
    {
        get => _silverCostForReSpecHour;
        set
        {
            _silverCostForReSpecHour = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Percent values

    public double FameInPercent
    {
        get => _fameInPercent;
        set
        {
            _fameInPercent = value;
            OnPropertyChanged();
        }
    }

    public double SilverInPercent
    {
        get => _silverInPercent;
        set
        {
            _silverInPercent = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPointsInPercent
    {
        get => _reSpecPointsInPercent;
        set
        {
            _reSpecPointsInPercent = value;
            OnPropertyChanged();
        }
    }

    public double MightInPercent
    {
        get => _mightInPercent;
        set
        {
            _mightInPercent = value;
            OnPropertyChanged();
        }
    }

    public double FavorInPercent
    {
        get => _favorInPercent;
        set
        {
            _favorInPercent = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Total values

    public double TotalGainedFameInSession
    {
        get => _totalGainedFameInSessionInSession;
        set
        {
            _totalGainedFameInSessionInSession = value;
            HighestValue = GetHighestValue();
            FameInPercent = _totalGainedFameInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedSilverInSession
    {
        get => _totalGainedSilverInSessionInSession;
        set
        {
            _totalGainedSilverInSessionInSession = value;
            HighestValue = GetHighestValue();
            SilverInPercent = _totalGainedSilverInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedReSpecPointsInSession
    {
        get => _totalGainedReSpecPointsInSessionInSession;
        set
        {
            _totalGainedReSpecPointsInSessionInSession = value;
            HighestValue = GetHighestValue();
            ReSpecPointsInPercent = _totalGainedReSpecPointsInSessionInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedMightInSession
    {
        get => _totalGainedMightInSession;
        set
        {
            _totalGainedMightInSession = value;
            HighestValue = GetHighestValue();
            MightInPercent = _totalGainedMightInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalGainedFavorInSession
    {
        get => _totalGainedFavorInSession;
        set
        {
            _totalGainedFavorInSession = value;
            HighestValue = GetHighestValue();
            FavorInPercent = _totalGainedFavorInSession / HighestValue * 100;
            OnPropertyChanged();
        }
    }

    public double TotalSilverCostForReSpecInSession
    {
        get => _totalSilverCostForReSpecInSession;
        set
        {
            _totalSilverCostForReSpecInSession = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #endregion

    #region Kill / Death stats

    public int SoloKillsToday
    {
        get => _soloKillsToday;
        set
        {
            _soloKillsToday = value;
            OnPropertyChanged();
        }
    }

    public int SoloKillsThisWeek
    {
        get => _soloKillsThisWeek;
        set
        {
            _soloKillsThisWeek = value;
            OnPropertyChanged();
        }
    }

    public int SoloKillsThisMonth
    {
        get => _soloKillsThisMonth;
        set
        {
            _soloKillsThisMonth = value;
            OnPropertyChanged();
        }
    }

    public int KillsToday
    {
        get => _killsToday;
        set
        {
            _killsToday = value;
            OnPropertyChanged();
        }
    }

    public int KillsThisWeek
    {
        get => _killsThisWeek;
        set
        {
            _killsThisWeek = value;
            OnPropertyChanged();
        }
    }

    public int KillsThisMonth
    {
        get => _killsThisMonth;
        set
        {
            _killsThisMonth = value;
            OnPropertyChanged();
        }
    }

    public int DeathsToday
    {
        get => _deathsToday;
        set
        {
            _deathsToday = value;
            OnPropertyChanged();
        }
    }

    public int DeathsThisWeek
    {
        get => _deathsThisWeek;
        set
        {
            _deathsThisWeek = value;
            OnPropertyChanged();
        }
    }

    public int DeathsThisMonth
    {
        get => _deathsThisMonth;
        set
        {
            _deathsThisMonth = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerWhenKilling
    {
        get => _averageItemPowerWhenKilling;
        set
        {
            _averageItemPowerWhenKilling = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerOfTheKilledEnemies
    {
        get => _averageItemPowerOfTheKilledEnemies;
        set
        {
            _averageItemPowerOfTheKilledEnemies = value;
            OnPropertyChanged();
        }
    }

    public double AverageItemPowerWhenDying
    {
        get => _averageItemPowerWhenDying;
        set
        {
            _averageItemPowerWhenDying = value;
            OnPropertyChanged();
        }
    }

    public DateTime? LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Chest stats

    public LootedChests LootedChests
    {
        get => _lootedChests;
        set
        {
            _lootedChests = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Repair costs

    public long RepairCostsToday
    {
        get => _repairCostsToday;
        set
        {
            _repairCostsToday = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsLast7Days
    {
        get => _repairCostsLast7Days;
        set
        {
            _repairCostsLast7Days = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsLast30Days
    {
        get => _repairCostsLast30Days;
        set
        {
            _repairCostsLast30Days = value;
            OnPropertyChanged();
        }
    }

    public long RepairCostsChest
    {
        get => _repairCostsChest;
        set
        {
            _repairCostsChest = value;
            OnPropertyChanged();
        }
    }

    public Visibility RepairCostsChestVisibility
    {
        get => _repairCostsChestVisibility;
        set
        {
            _repairCostsChestVisibility = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationTitle => $"{LocalizationController.Translation("DASHBOARD")}";
    public static string TranslationFame => LocalizationController.Translation("FAME");
    public static string TranslationSilver => LocalizationController.Translation("SILVER");
    public static string TranslationReSpec => LocalizationController.Translation("RESPEC");
    public static string TranslationFaction => LocalizationController.Translation("FACTION");
    public static string TranslationMight => LocalizationController.Translation("MIGHT");
    public static string TranslationFavor => LocalizationController.Translation("FAVOR");
    public static string TranslationResetTrackingCounter => LocalizationController.Translation("RESET_TRACKING_COUNTER");
    public static string TranslationToday => LocalizationController.Translation("TODAY").ToLower();
    public static string TranslationWeek => LocalizationController.Translation("WEEK").ToLower();
    public static string TranslationMonth => LocalizationController.Translation("MONTH").ToLower();
    public static string TranslationKills => LocalizationController.Translation("KILLS");
    public static string TranslationSoloKills => LocalizationController.Translation("SOLO_KILLS");
    public static string TranslationDeaths => LocalizationController.Translation("DEATHS");
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
    public static string TranslationLootedChests => LocalizationController.Translation("LOOTED_CHESTS");
    public static string TranslationRepairCosts => LocalizationController.Translation("REPAIR_COSTS");
}