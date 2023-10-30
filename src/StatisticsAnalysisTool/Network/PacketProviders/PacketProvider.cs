namespace StatisticsAnalysisTool.Network.PacketProviders;

public abstract class PacketProvider
{
    public abstract bool IsRunning { get; }

    public abstract void Start();
    public abstract void Stop();
}