namespace StatisticsAnalysisTool.Network.Notification
{
    public class TextFragment : LineFragment
    {
        public TextFragment(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }
}