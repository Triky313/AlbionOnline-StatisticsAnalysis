using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public sealed class PlayerInformationServerOption
{
    public PlayerInformationServerOption(ServerLocation serverLocation, string displayName)
    {
        ServerLocation = serverLocation;
        DisplayName = displayName;
    }

    public ServerLocation ServerLocation { get; }

    public string DisplayName { get; }
}