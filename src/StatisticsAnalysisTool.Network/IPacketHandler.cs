namespace StatisticsAnalysisTool.Network;

public interface IPacketHandler<in TPacket>
{
    int Code { get; }
    Task HandleAsync(TPacket packet);
}