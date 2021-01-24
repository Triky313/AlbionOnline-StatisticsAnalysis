namespace StatisticsAnalysisTool.Network
{
    public interface ICountUpTimer
    {
        void Add(double value);

        void Start();

        void Stop();

        void Reset();
    }
}