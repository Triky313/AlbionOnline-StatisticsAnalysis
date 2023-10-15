using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.ViewModels;

public class GuildViewModel : BaseViewModel
{

    public string TranslationSiphonedEnergy => LanguageController.Translation("SIPHONED_ENERGY");
    public string TranslationDeleteSelectedEntries => LanguageController.Translation("DELETE_SELECTED_ENTRIES");
    public string TranslationSelectDeselectAll => LanguageController.Translation("SELECT_DESELECT_ALL");
    public string TranslationLastUpdate => LanguageController.Translation("LAST_UPDATE");
}