namespace StatisticsAnalysisTool.Network.PacketProviders;

public sealed class ConfiguredNetworkDevice
{
    public string Identifier { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsSelected { get; init; } = true;
}
