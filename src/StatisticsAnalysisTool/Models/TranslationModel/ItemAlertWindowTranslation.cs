using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class ItemAlertWindowTranslation
{
    public string Title => $"{LanguageController.Translation("ITEM_PRICE_UNDERCUT")}";
    public string ThePriceOf => $"{LanguageController.Translation("THE_PRICE_OF")}";
    public string In => $"{LanguageController.Translation("IN")}";
    public string HasBeenUndercut => $"{LanguageController.Translation("HAS_BEEN_UNDERCUT")}";
}