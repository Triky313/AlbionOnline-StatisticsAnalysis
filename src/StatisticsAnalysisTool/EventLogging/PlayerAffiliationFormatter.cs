namespace StatisticsAnalysisTool.EventLogging;

internal static class PlayerAffiliationFormatter
{
    public static string Format(string guild, string alliance)
    {
        var normalizedGuild = guild?.Trim() ?? string.Empty;
        var normalizedAlliance = alliance?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(normalizedGuild))
        {
            return normalizedAlliance;
        }

        return string.IsNullOrEmpty(normalizedAlliance) ? normalizedGuild : $"{normalizedGuild}, {normalizedAlliance}";
    }
}