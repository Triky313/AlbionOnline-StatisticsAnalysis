namespace StatisticsAnalysisTool.Network.Notification
{
    public class FameStatFragment : LineFragment
    {
        public FameStatFragment(double zone, double premium, double satchel)
        {
            Zone = zone;
            Premium = premium;
            Satchel = satchel;
        }

        public double Zone { get; }
        public double Premium { get; }
        public double Satchel { get; }
    }
}