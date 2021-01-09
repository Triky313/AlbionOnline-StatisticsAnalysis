namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class TrackingNotification
    {
        public TrackingNotification(string text)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}