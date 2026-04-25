using StatisticsAnalysisTool.Network.PacketProviders;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Avalonia.Models;

public sealed class TrackingSettingsDocument
{
    public const string DefaultPacketFilter = "(ip or ip6) and (udp and (port 5055 or port 5056 or port 5058))";

    public PacketProviderKind PacketProvider { get; set; } = PacketProviderKind.Npcap;

    public string PacketFilter { get; set; } = DefaultPacketFilter;

    public int NetworkDevice { get; set; } = -1;

    public List<TrackingNetworkDeviceSettings> NetworkDevices { get; set; } = [];
}
