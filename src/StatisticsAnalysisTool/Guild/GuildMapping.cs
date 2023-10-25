namespace StatisticsAnalysisTool.Guild;

public static class GuildMapping
{
    public static GuildDto Mapping(Guild guild)
    {
        return new GuildDto()
        {
            GuildId = guild.GuildId,
            SiphonedEnergy = guild.SiphonedEnergy
        };
    }

    public static Guild Mapping(GuildDto guild)
    {
        return new Guild()
        {
            GuildId = guild.GuildId,
            SiphonedEnergy = guild.SiphonedEnergy
        };
    }
}