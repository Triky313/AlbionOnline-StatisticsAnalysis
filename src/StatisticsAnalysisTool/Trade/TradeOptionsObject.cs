using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Trade;

public class TradeOptionsObject : BaseViewModel
{
    private bool _isTradeMonitoringActive = true;
    private bool _isPlayerTradeMonitoringActive = true;
    private List<DeleteTradesAfterDaysStruct> _deleteTradesOlderThanSpecifiedDays = new();
    private DeleteTradesAfterDaysStruct _damageMeterSortSelection;
    private bool _ignoreMailsWithZeroValues;
    private double _marketTaxRate;
    private double _marketTaxSetupRate;

    public TradeOptionsObject()
    {
        RefreshDeleteTradesOlderThanSpecifiedDays(SettingsController.CurrentSettings.DeleteTradesOlderThanSpecifiedDays);
        IsTradeMonitoringActive = SettingsController.CurrentSettings.IsTradeMonitoringActive;
        IsPlayerTradeMonitoringActive = SettingsController.CurrentSettings.IsPlayerTradeMonitoringActive;
        IgnoreMailsWithZeroValues = SettingsController.CurrentSettings.IgnoreMailsWithZeroValues;
        MarketTaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate;
        MarketTaxSetupRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxSetupRate;
    }

    public void RefreshLocalization()
    {
        RefreshDeleteTradesOlderThanSpecifiedDays(DeleteTradesOlderThanSpecifiedDaysSelection.Days);
        OnPropertyChanged(nameof(TranslationTradeMonitoringActive));
        OnPropertyChanged(nameof(TranslationPlayerTradeMonitoringActive));
        OnPropertyChanged(nameof(TranslationIgnoreMailsWithZeroValues));
        OnPropertyChanged(nameof(TranslationMarketTaxRate));
        OnPropertyChanged(nameof(TranslationMarketTaxSetupRate));
        OnPropertyChanged(nameof(TranslationSettings));
    }

    private void RefreshDeleteTradesOlderThanSpecifiedDays(int selectedDays)
    {
        var deleteTradesOlderThanSpecifiedDays = BuildDeleteTradesOlderThanSpecifiedDays();
        var neverDeleteObject = deleteTradesOlderThanSpecifiedDays.First();
        var deleteTradesAfterDaysSelection = deleteTradesOlderThanSpecifiedDays.FirstOrDefault(x => x.Days == selectedDays);

        DeleteTradesOlderThanSpecifiedDays = deleteTradesOlderThanSpecifiedDays;
        DeleteTradesOlderThanSpecifiedDaysSelection = deleteTradesAfterDaysSelection.Name == null ? neverDeleteObject : deleteTradesAfterDaysSelection;
    }

    private static List<DeleteTradesAfterDaysStruct> BuildDeleteTradesOlderThanSpecifiedDays()
    {
        return
        [
            new DeleteTradesAfterDaysStruct() { Days = 0, Name = LocalizationController.Translation("DELETE_TRADES_NEVER") },
            new DeleteTradesAfterDaysStruct() { Days = 7, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_7_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 14, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_14_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 30, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_30_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 60, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_60_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 90, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_90_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 180, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_180_DAYS") },
            new DeleteTradesAfterDaysStruct() { Days = 365, Name = LocalizationController.Translation("DELETE_TRADES_AFTER_365_DAYS") }
        ];
    }

    public bool IsTradeMonitoringActive
    {
        get => _isTradeMonitoringActive;
        set
        {
            _isTradeMonitoringActive = value;
            SettingsController.CurrentSettings.IsTradeMonitoringActive = _isTradeMonitoringActive;
            OnPropertyChanged();
        }
    }

    public bool IgnoreMailsWithZeroValues
    {
        get => _ignoreMailsWithZeroValues;
        set
        {
            _ignoreMailsWithZeroValues = value;
            SettingsController.CurrentSettings.IgnoreMailsWithZeroValues = _ignoreMailsWithZeroValues;
            OnPropertyChanged();
        }
    }

    public bool IsPlayerTradeMonitoringActive
    {
        get => _isPlayerTradeMonitoringActive;
        set
        {
            _isPlayerTradeMonitoringActive = value;
            SettingsController.CurrentSettings.IsPlayerTradeMonitoringActive = _isPlayerTradeMonitoringActive;
            OnPropertyChanged();
        }
    }

    public List<DeleteTradesAfterDaysStruct> DeleteTradesOlderThanSpecifiedDays
    {
        get => _deleteTradesOlderThanSpecifiedDays;
        set
        {
            _deleteTradesOlderThanSpecifiedDays = value;
            OnPropertyChanged();
        }
    }

    public DeleteTradesAfterDaysStruct DeleteTradesOlderThanSpecifiedDaysSelection
    {
        get => _damageMeterSortSelection;
        set
        {
            _damageMeterSortSelection = value;
            SettingsController.CurrentSettings.DeleteTradesOlderThanSpecifiedDays = _damageMeterSortSelection.Days;
            OnPropertyChanged();
        }
    }

    public double MarketTaxRate
    {
        get => _marketTaxRate;
        set
        {
            _marketTaxRate = value;
            SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate = _marketTaxRate;
            OnPropertyChanged();
        }
    }
    public double MarketTaxSetupRate
    {
        get => _marketTaxSetupRate;
        set
        {
            _marketTaxSetupRate = value;
            SettingsController.CurrentSettings.TradeMonitoringMarketTaxSetupRate = _marketTaxSetupRate;
            OnPropertyChanged();
        }
    }

    public static string TranslationTradeMonitoringActive => LocalizationController.Translation("TRADE_MONITORING_ACTIVE");
    public static string TranslationPlayerTradeMonitoringActive => LocalizationController.Translation("PLAYER_TRADE_MONITORING_ACTIVE");
    public static string TranslationIgnoreMailsWithZeroValues => LocalizationController.Translation("IGNORE_MAILS_WITH_ZERO_VALUES");
    public static string TranslationMarketTaxRate => LocalizationController.Translation("MARKET_TAX_RATE");
    public static string TranslationMarketTaxSetupRate => LocalizationController.Translation("MARKET_TAX_SETUP_RATE");
    public static string TranslationSettings => LocalizationController.Translation("SETTINGS");
}
