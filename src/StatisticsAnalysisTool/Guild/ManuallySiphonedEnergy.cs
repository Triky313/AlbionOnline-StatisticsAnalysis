using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Guild;

public class ManuallySiphonedEnergy : BaseViewModel
{
    private string _characterName;
    private long _quantity;

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

    #region Commands

    public void AddManualEntry(object obj)
    {
        if (Quantity == 0)
        {
            return;
        }

        var dateTimeTicks = DateTime.UtcNow.Ticks;

        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.GuildController?.AddSiphonedEnergyEntry(CharacterName, FixPoint.FromFloatingPointValue(Quantity), dateTimeTicks, true);
    }

    private ICommand _addManualEntryCommand;

    public ICommand AddManualEntryCommand => _addManualEntryCommand ??= new CommandHandler(AddManualEntry, true);

    #endregion

    public static string TranslationAddOrRemoveManually => LanguageController.Translation("ADD_OR_REMOVE_MANUALLY");
    public static string TranslationCharacterName => LanguageController.Translation("CHARACTER_NAME");
    public static string TranslationAddManualEntry => LanguageController.Translation("ADD_MANUAL_ENTRY");
    public static string TranslationQuantity => LanguageController.Translation("QUANTITY");
}