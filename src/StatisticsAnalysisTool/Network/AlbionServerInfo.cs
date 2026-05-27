using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network;

public class AlbionServerInfo
{
    public required ServerLocation ServerLocation { get; init; }
    public required string Name { get; init; }
    public required string IpPrefix { get; init; }
}