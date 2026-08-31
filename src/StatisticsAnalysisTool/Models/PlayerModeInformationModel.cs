using System;

using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class PlayerModeInformationModel
{
    private const string AvatarAssetBaseUrl = "https://www.albiondatabase.com/icons/avatars/";

    public DateTime Timestamp { get; set; }
    public GameInfoSearchResponse GameInfoSearch { get; set; }
    public SearchPlayerResponse SearchPlayer { get; set; }
    public GameInfoPlayersResponse GameInfoPlayers { get; set; }
    public StatisticsAnalysisTool.Enumerations.ServerLocation ServerLocation { get; set; }
    public string ServerName { get; set; }
    public string TimestampText => Timestamp.CurrentDateTimeFormat();
    public bool HasData => !string.IsNullOrWhiteSpace(GameInfoPlayers?.Id);
    public string AvatarImageUrl => GetAvatarAssetUrl(GameInfoPlayers?.Avatar);
    public string AvatarRingImageUrl => GetAvatarAssetUrl(GameInfoPlayers?.AvatarRing);

    private static string GetAvatarAssetUrl(string identifier)
    {
        return string.IsNullOrWhiteSpace(identifier)
            ? string.Empty
            : $"{AvatarAssetBaseUrl}{Uri.EscapeDataString(identifier)}.png";
    }
}