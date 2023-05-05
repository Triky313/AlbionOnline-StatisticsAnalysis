using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models;

public class TradeOptionsObject : INotifyPropertyChanged
{
    private bool _isTradeMonitoringActive = true;
    private List<DeleteTradesAfterDaysStruct> _deleteTradesOlderThanSpecifiedDays = new();
    private DeleteTradesAfterDaysStruct _damageMeterSortSelection;
    private bool _ignoreMailsWithZeroValues;
    private double _marketTaxRate;
    private double _marketTaxSetupRate;

    public TradeOptionsObject()
    {
        var neverDeleteObject = new DeleteTradesAfterDaysStruct() { Days = 0, Name = LanguageController.Translation("DELETE_TRADES_NEVER") };

        DeleteTradesOlderThanSpecifiedDays.Clear();
        DeleteTradesOlderThanSpecifiedDays.Add(neverDeleteObject);
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 7, Name = LanguageController.Translation("DELETE_TRADES_AFTER_7_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 14, Name = LanguageController.Translation("DELETE_TRADES_AFTER_14_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 30, Name = LanguageController.Translation("DELETE_TRADES_AFTER_30_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 60, Name = LanguageController.Translation("DELETE_TRADES_AFTER_60_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 90, Name = LanguageController.Translation("DELETE_TRADES_AFTER_90_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 180, Name = LanguageController.Translation("DELETE_TRADES_AFTER_180_DAYS") });
        DeleteTradesOlderThanSpecifiedDays.Add(new DeleteTradesAfterDaysStruct() { Days = 365, Name = LanguageController.Translation("DELETE_TRADES_AFTER_365_DAYS") });
        
        var deleteTradesAfterDaysSelection = DeleteTradesOlderThanSpecifiedDays.FirstOrDefault(x => x.Days == SettingsController.CurrentSettings.DeleteTradesOlderThanSpecifiedDays);
        DeleteTradesOlderThanSpecifiedDaysSelection = deleteTradesAfterDaysSelection.Name == null ? neverDeleteObject : deleteTradesAfterDaysSelection;

        IsTradeMonitoringActive = SettingsController.CurrentSettings.IsTradeMonitoringActive;
        IgnoreMailsWithZeroValues = SettingsController.CurrentSettings.IgnoreMailsWithZeroValues;
        MarketTaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate;
        MarketTaxSetupRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxSetupRate;
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

    public static string TranslationTradeMonitoringActive => LanguageController.Translation("TRADE_MONITORING_ACTIVE");
    public static string TranslationIgnoreMailsWithZeroValues => LanguageController.Translation("IGNORE_MAILS_WITH_ZERO_VALUES");
    public static string TranslationMarketTaxRate => LanguageController.Translation("MARKET_TAX_RATE");
    public static string TranslationMarketTaxSetupRate => LanguageController.Translation("MARKET_TAX_SETUP_RATE");
    public static string TranslationSettings => LanguageController.Translation("SETTINGS");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}