namespace StatisticsAnalysisTool.Models
{
    using System;

    public class MarketHistoryChartModel
    {
        public string Location { get; set; }

        public string ItemId { get; set; }

        public int Quality { get; set; }

        public int ItemCount { get; set; }

        public int AveragePrice { get; set; }

        public DateTime Timestamp { get; set; }
    }
}