using StatisticsAnalysisTool.Common;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models
{
    public class ItemWithMarketInformation
    {
        private readonly Item _item;

        public ItemWithMarketInformation(Item item)
        {
            _item = item;
        }

        public string LocalizationNameVariable => _item.LocalizationNameVariable;
        public string LocalizationDescriptionVariable => _item.LocalizationDescriptionVariable;
        public Item.LocalizedNamesStruct? LocalizedNames => _item.LocalizedNames;
        public int Index => _item.Index;
        public string UniqueName => _item.UniqueName;
        public string LocalizedName(string language = null) => _item.LocalizedName(language);
        public string LocalizedNameAndEnglish => _item.LocalizedNameAndEnglish;
        public BitmapImage Icon => _item.Icon;

        public decimal PriceAvgCaerleon => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.Caerleon).Result;
        public decimal PriceAvgBridgewatch => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.Bridgewatch).Result;
        public decimal PriceAvgFortSterling => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.FortSterling).Result;
        public decimal PriceAvgLymhurst => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.Lymhurst).Result;
        public decimal PriceAvgMartlock => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.Martlock).Result;
        public decimal PriceAvgThetford => ApiController.GetMarketStatAvgPriceFromJsonAsync(_item.UniqueName, Location.Thetford).Result;

    }
}
