using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Network.Notification;

public class KillNotificationFragment : LineFragment
{
    public KillNotificationFragment(string died, string killedBy, string killedByGuild, string valueText)
    {
        Died = died;
        KilledBy = killedBy;
        KilledByGuild = killedByGuild;
        ValueText = valueText;
    }

    public string Died { get; }
    public string KilledBy { get; }
    public string KilledByGuild { get; }
    [JsonIgnore] 
    public bool IsKilledByGuildEmpty => string.IsNullOrEmpty(KilledByGuild);
    public string ValueText { get; }
}