using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network;

public static class AlbionServerRegistry
{
    public static AlbionServerInfo Unknown { get; } = new()
    {
        ServerLocation = ServerLocation.Unknown,
        Name = "Unknown",
        IpPrefix = string.Empty
    };

    public static IReadOnlyCollection<AlbionServerInfo> Servers { get; } =
    [
        new()
        {
            ServerLocation = ServerLocation.America,
            Name = "Americas",
            IpPrefix = "5.188.125"
        },
        new()
        {
            ServerLocation = ServerLocation.Asia,
            Name = "Asia",
            IpPrefix = "5.45.187"
        },
        new()
        {
            ServerLocation = ServerLocation.Europe,
            Name = "Europe",
            IpPrefix = "193.169.238"
        }
    ];

    public static AlbionServerInfo GetBySourceIp(string sourceIp)
    {
        if (string.IsNullOrWhiteSpace(sourceIp))
        {
            return Unknown;
        }

        return Servers.FirstOrDefault(x => sourceIp.StartsWith(x.IpPrefix, StringComparison.Ordinal)) ?? Unknown;
    }
}