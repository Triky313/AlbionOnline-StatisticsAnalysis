namespace StatisticsAnalysisTool.Models;

public class AlertInfos
{
    public AlertInfos(Item item, MarketResponse marketResponse, global::StatisticsAnalysisTool.Alert.ItemAlertType alertType)
    {
        Item = item;
        MarketResponse = marketResponse;
        AlertType = alertType;
    }

    public Item Item { get; }
    public MarketResponse MarketResponse { get; }
    public global::StatisticsAnalysisTool.Alert.ItemAlertType AlertType { get; }
}