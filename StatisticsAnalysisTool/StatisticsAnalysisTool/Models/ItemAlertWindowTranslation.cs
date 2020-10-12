using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class ItemAlertWindowTranslation
    {
        private readonly Item _item;

        public ItemAlertWindowTranslation(Item item)
        {
            _item = item;
        }

        public string Title => $"{LanguageController.Translation("ITEM_PRICE_UNDERCUT_FOR")}: {_item.LocalizedName}";
        public string ThePriceOf => $"{LanguageController.Translation("THE_PRICE_OF")}";
        public string In => $"{LanguageController.Translation("IN")}";
        public string HasBeenUndercut => $"{LanguageController.Translation("HAS_BEEN_UNDERCUT")}";
    }
}