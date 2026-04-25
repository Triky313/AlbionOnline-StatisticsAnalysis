using StatisticsAnalysisTool.Abstractions;
using System;
using System.Runtime.Versioning;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public static class PacketProviderFactory
{
    [SupportedOSPlatform("windows")]
    public static PacketProvider Create(IPhotonReceiver photonReceiver, NetworkCaptureOptions options)
    {
        ArgumentNullException.ThrowIfNull(photonReceiver);
        ArgumentNullException.ThrowIfNull(options);

        return options.PacketProvider switch
        {
            PacketProviderKind.Npcap => new LibpcapPacketProvider(photonReceiver, options),
            PacketProviderKind.Sockets => new SocketsPacketProvider(photonReceiver, options),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.PacketProvider, "Unsupported packet provider.")
        };
    }
}
