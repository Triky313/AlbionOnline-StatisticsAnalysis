using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class ItemAlertWindowTranslation
{
    public string PriceAlertTitle => LocalizationController.Translation("ITEM_PRICE_UNDERCUT");
    public string AvailabilityAlertTitle => LocalizationController.Translation("ITEM_MARKET_OFFER_FOUND");
    public string BlackMarketBuyOrderAlertTitle => LocalizationController.Translation("BLACK_MARKET_BUY_ORDER_ALERT_TITLE");
    public string ThePriceOf => $"{LocalizationController.Translation("THE_PRICE_OF")}";
    public string AMarketOfferFor => LocalizationController.Translation("A_MARKET_OFFER_FOR");
    public string HighestBlackMarketBuyOrderFor => LocalizationController.Translation("HIGHEST_BLACK_MARKET_BUY_ORDER_FOR");
    public string In => $"{LocalizationController.Translation("IN")}";
    public string HasBeenUndercut => $"{LocalizationController.Translation("HAS_BEEN_UNDERCUT")}";
    public string WasFound => LocalizationController.Translation("WAS_FOUND");
    public string ReachedMinimumPrice => LocalizationController.Translation("REACHED_MINIMUM_PRICE");
}