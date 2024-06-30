using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class ItemAlertWindowTranslation
{
    public string Title => $"{LocalizationController.Translation("ITEM_PRICE_UNDERCUT")}";
    public string ThePriceOf => $"{LocalizationController.Translation("THE_PRICE_OF")}";
    public string In => $"{LocalizationController.Translation("IN")}";
    public string HasBeenUndercut => $"{LocalizationController.Translation("HAS_BEEN_UNDERCUT")}";
}