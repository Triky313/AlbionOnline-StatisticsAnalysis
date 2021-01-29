namespace StatisticsAnalysisTool.Common
{
    public static class Converter
    {
        public static string GoldToDollar(ulong itemSilverPrice, int currentGoldPrice)
        {
            if (itemSilverPrice == 0 || currentGoldPrice == 0)
            {
                return 0.ToString();
            }

            // 750 Gold - 4,95 USD
            // 21.000 Gold - 99,95 USD

            double minReceivedGold = 750;
            double maxReceivedGold = 21000;

            double minGoldPriceInCent = 4.95;
            double maxGoldPriceInCent = 99.95;

            double minOneGoldInCent = minGoldPriceInCent / minReceivedGold;
            double maxOneGoldInCent = maxGoldPriceInCent / maxReceivedGold;

            var itemPriceInGold = itemSilverPrice / (ulong)currentGoldPrice;

            var maxPrice = minOneGoldInCent * itemPriceInGold;
            var minPrice = maxOneGoldInCent * itemPriceInGold;

            return $"{minPrice:0.00} - {maxPrice:0.00} $";
        }

        public static bool ParseToDouble(string value, out double outValue)
        {
            if (double.TryParse(value, out double newValue))
            {
                outValue = newValue / 10000;
                return true;
            }

            outValue = 0;
            return false;
        }
    }
}