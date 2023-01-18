namespace StatisticsAnalysisTool.Network;

public abstract class BaseEvent
{
    protected BaseEvent(IReadOnlyDictionary<byte, object> parameters) { }
}