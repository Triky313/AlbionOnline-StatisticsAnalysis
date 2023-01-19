namespace StatisticsAnalysisTool.Network;

public interface IPacketHandler
{
    Task HandleAsync(object request);
}