using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models;

public class MailOptionsObject : INotifyPropertyChanged
{
    private bool _isMailMonitoringActive = true;
    private List<DeleteMailsAfterDaysStruct> _deleteMailsOlderThanSpecifiedDays = new();
    private DeleteMailsAfterDaysStruct _damageMeterSortSelection;
    private bool _ignoreMailsWithZeroValues;
    private double _marketTaxRate;
    private double _marketTaxSetupRate;

    public MailOptionsObject()
    {
        var neverDeleteObject = new DeleteMailsAfterDaysStruct() {Days = 0, Name = LanguageController.Translation("DELETE_MAILS_NEVER")};

        DeleteMailsOlderThanSpecifiedDays.Clear();
        DeleteMailsOlderThanSpecifiedDays.Add(neverDeleteObject);
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 7, Name = LanguageController.Translation("DELETE_MAILS_AFTER_7_DAYS") });
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 30, Name = LanguageController.Translation("DELETE_MAILS_AFTER_30_DAYS") });
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 60, Name = LanguageController.Translation("DELETE_MAILS_AFTER_60_DAYS") });
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 90, Name = LanguageController.Translation("DELETE_MAILS_AFTER_90_DAYS") });
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 180, Name = LanguageController.Translation("DELETE_MAILS_AFTER_180_DAYS") });
        DeleteMailsOlderThanSpecifiedDays.Add(new DeleteMailsAfterDaysStruct() { Days = 365, Name = LanguageController.Translation("DELETE_MAILS_AFTER_365_DAYS") });
        
        var deleteMailsAfterDaysSelection = DeleteMailsOlderThanSpecifiedDays.FirstOrDefault(x => x.Days == SettingsController.CurrentSettings.DeleteMailsOlderThanSpecifiedDays);
        DeleteMailsOlderThanSpecifiedDaysSelection = deleteMailsAfterDaysSelection.Name == null ? neverDeleteObject : deleteMailsAfterDaysSelection;

        IsMailMonitoringActive = SettingsController.CurrentSettings.IsMailMonitoringActive;
        IgnoreMailsWithZeroValues = SettingsController.CurrentSettings.IgnoreMailsWithZeroValues;
        MarketTaxRate = SettingsController.CurrentSettings.MailMonitoringMarketTaxRate;
        MarketTaxSetupRate = SettingsController.CurrentSettings.MailMonitoringMarketTaxSetupRate;
    }

    public bool IsMailMonitoringActive
    {
        get => _isMailMonitoringActive;
        set
        {
            _isMailMonitoringActive = value;
            SettingsController.CurrentSettings.IsMailMonitoringActive = _isMailMonitoringActive;
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

    public List<DeleteMailsAfterDaysStruct> DeleteMailsOlderThanSpecifiedDays
    {
        get => _deleteMailsOlderThanSpecifiedDays;
        set
        {
            _deleteMailsOlderThanSpecifiedDays = value;
            OnPropertyChanged();
        }
    }

    public DeleteMailsAfterDaysStruct DeleteMailsOlderThanSpecifiedDaysSelection
    {
        get => _damageMeterSortSelection;
        set
        {
            _damageMeterSortSelection = value;
            SettingsController.CurrentSettings.DeleteMailsOlderThanSpecifiedDays = _damageMeterSortSelection.Days;
            OnPropertyChanged();
        }
    }

    public double MarketTaxRate
    {
        get => _marketTaxRate;
        set
        {
            _marketTaxRate = value;
            SettingsController.CurrentSettings.MailMonitoringMarketTaxRate = _marketTaxRate;
            OnPropertyChanged();
        }
    }
    public double MarketTaxSetupRate
    {
        get => _marketTaxSetupRate;
        set
        {
            _marketTaxSetupRate = value;
            SettingsController.CurrentSettings.MailMonitoringMarketTaxSetupRate = _marketTaxSetupRate;
            OnPropertyChanged();
        }
    }

    public static string TranslationMailMonitoringActive => LanguageController.Translation("MAIL_MONITORING_ACTIVE");
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