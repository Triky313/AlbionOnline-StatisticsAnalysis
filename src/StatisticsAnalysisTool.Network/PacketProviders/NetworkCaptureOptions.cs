using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public sealed class NetworkCaptureOptions
{
    public PacketProviderKind PacketProvider { get; init; } = PacketProviderKind.Npcap;

    public string PacketFilter { get; init; } = string.Empty;

    public int LegacyNetworkDeviceIndex { get; init; } = -1;

    public IReadOnlyList<ConfiguredNetworkDevice> NetworkDevices { get; init; } = [];
}
