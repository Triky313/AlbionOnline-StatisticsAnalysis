using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class KillNotificationFragment(string died, string diedPlayerGuild, string diedPlayerAlliance,
    string killedBy, string killedByGuild, string killedByAlliance, string valueText) : LineFragment
{
    public string Died { get; } = died;
    public string DiedPlayerGuild { get; } = diedPlayerGuild;
    public string DiedPlayerAlliance { get; } = diedPlayerAlliance;
    public string DiedPlayerAffiliations { get; } = PlayerAffiliationFormatter.Format(diedPlayerGuild, diedPlayerAlliance);
    public string KilledBy { get; } = killedBy;
    public string KilledByGuild { get; } = killedByGuild;
    public string KilledByAlliance { get; } = killedByAlliance;
    public string KilledByAffiliations { get; } = PlayerAffiliationFormatter.Format(killedByGuild, killedByAlliance);

    [JsonIgnore]
    public bool IsKilledByAffiliationsEmpty => string.IsNullOrEmpty(KilledByAffiliations);
    [JsonIgnore]
    public bool IsDiedPlayerAffiliationsEmpty => string.IsNullOrEmpty(DiedPlayerAffiliations);
    public string ValueText { get; } = valueText;
}