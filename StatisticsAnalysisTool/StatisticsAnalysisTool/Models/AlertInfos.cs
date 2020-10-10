namespace StatisticsAnalysisTool.Models
{
    public class AlertInfos
    {
        public AlertInfos(Item item, MarketResponse marketResponse)
        {
            Item = item;
            MarketResponse = marketResponse;
        }

        public Item Item { get; }
        public MarketResponse MarketResponse { get; }
    }
}