namespace StatisticsAnalysisTool.Network;

public interface IPhotonReceiver
{
    void ReceivePacket(byte[] payload);
}