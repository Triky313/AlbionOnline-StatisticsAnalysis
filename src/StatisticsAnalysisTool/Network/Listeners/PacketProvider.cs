namespace StatisticsAnalysisTool.Network.Listeners;

public enum PacketProviderKind
{
    Sockets = 1,
    Npcap = 2,
}

public abstract class PacketProvider
{
    public abstract bool IsRunning { get; }

    public abstract void Start();
    public abstract void Stop();
}