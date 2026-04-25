namespace StatisticsAnalysisTool.Avalonia.Models;

public sealed class PacketActivitySnapshot
{
    public static PacketActivitySnapshot Empty { get; } = new();

    public long TotalPackets { get; init; }

    public long EventPackets { get; init; }

    public long RequestPackets { get; init; }

    public long ResponsePackets { get; init; }

    public string LastPacket { get; init; } = "No packets received.";
}
