using System.Buffers;

namespace StatisticsAnalysisTool.Abstractions;

public interface IPhotonReceiver
{
    void ReceivePacket(byte[] payload);
    void ReceivePacket(ReadOnlySpan<byte> payload);
    void ReceivePacket(ReadOnlySequence<byte> payload);
}