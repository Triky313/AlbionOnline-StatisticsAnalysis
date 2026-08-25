namespace StatisticsAnalysisTool.Network.PacketProviders;

public abstract class PacketProvider
{
    private int _hasReportedGameData;

    public event System.EventHandler GameDataDetected;

    public abstract bool IsRunning { get; }

    public abstract PacketProviderStartResult Start();
    public abstract void Stop();

    protected void ResetGameDataDetectedState()
    {
        System.Threading.Interlocked.Exchange(ref _hasReportedGameData, 0);
    }

    protected void ReportGameDataDetected()
    {
        if (System.Threading.Interlocked.Exchange(ref _hasReportedGameData, 1) == 1)
        {
            return;
        }

        GameDataDetected?.Invoke(this, System.EventArgs.Empty);
    }
}