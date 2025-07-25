using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class KillNotificationFragment(string died, string diedPlayerGuild, string killedBy, string killedByGuild, string valueText) : LineFragment
{
    public string Died { get; } = died;
    public string DiedPlayerGuild { get; } = diedPlayerGuild;
    public string KilledBy { get; } = killedBy;
    public string KilledByGuild { get; } = killedByGuild;

    [JsonIgnore]
    public bool IsKilledByGuildEmpty => string.IsNullOrEmpty(KilledByGuild);
    [JsonIgnore]
    public bool IsDiedPlayerGuildEmpty => string.IsNullOrEmpty(DiedPlayerGuild);
    public string ValueText { get; } = valueText;
}