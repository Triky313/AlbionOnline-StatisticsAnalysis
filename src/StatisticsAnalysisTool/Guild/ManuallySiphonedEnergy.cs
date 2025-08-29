using Microsoft.Win32;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Guild;

public class ManuallySiphonedEnergy : BaseViewModel
{
    private string _characterName;
    private long _quantity;
    private EnergyOperator _selectedOperator = EnergyOperator.Deposit;

    public string CharacterName
    {
        get => _characterName;
        set
        {
            _characterName = value;
            OnPropertyChanged();
        }
    }

    public long Quantity
    {
        get => _quantity;
        set
        {
            if (value > 9_000_000_000_000)
            {
                _quantity = 9_000_000_000_000;
            }
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public EnergyOperator SelectedOperator
    {
        get => _selectedOperator;
        set
        {
            _selectedOperator = value;
            OnPropertyChanged();
        }
    }

    #region Commands

    private void AddManualEntry(object obj)
    {
        if (Quantity == 0)
        {
            return;
        }

        var dateTimeTicks = DateTime.UtcNow.Ticks;
        var trackingController = ServiceLocator.Resolve<TrackingController>();

        var isDeposit = SelectedOperator == EnergyOperator.Deposit;
        var value = FixPoint.FromFloatingPointValue(isDeposit ? Quantity : -Quantity);

        trackingController?.GuildController?.AddSiphonedEnergyEntry(CharacterName, value, dateTimeTicks, true);
    }

    private ICommand _addManualEntryCommand;

    public ICommand AddManualEntryCommand => _addManualEntryCommand ??= new CommandHandler(AddManualEntry, true);

    #endregion

    #region Export commands

    private static void ExportAsCsv(object obj)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"siphoned_energy_{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}utc",
            DefaultExt = ".csv",
            Filter = "CSV documents (.csv)|*.csv"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var trackingController = ServiceLocator.Resolve<TrackingController>();
                File.WriteAllText(dialog.FileName, trackingController?.GuildController?.GetSiphonedEnergyListAsCsv());
            }
            catch (Exception e)
            {
                DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    private ICommand _exportAsCsvCommand;

    public ICommand ExportAsCsvCommand => _exportAsCsvCommand ??= new CommandHandler(ExportAsCsv, true);

    #endregion

    public static string TranslationAddOrRemoveManually => LocalizationController.Translation("ADD_OR_REMOVE_MANUALLY");
    public static string TranslationCharacterName => LocalizationController.Translation("CHARACTER_NAME");
    public static string TranslationAddEntry => LocalizationController.Translation("ADD_ENTRY");
    public static string TranslationQuantity => LocalizationController.Translation("QUANTITY");
    public static string TranslationOperator => LocalizationController.Translation("OPERATOR");
    public static string TranslationDeposit => LocalizationController.Translation("DEPOSIT");
    public static string TranslationWithdraw => LocalizationController.Translation("WITHDRAW");
    public static string TranslationExportAsCsv => LocalizationController.Translation("EXPORT_AS_CSV");
    public static string TranslationExportSiphonedEnergyList => LocalizationController.Translation("EXPORT_SIPHONED_ENERGY_LIST");
}