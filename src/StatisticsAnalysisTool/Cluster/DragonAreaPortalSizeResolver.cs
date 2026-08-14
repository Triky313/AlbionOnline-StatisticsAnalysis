using System;

namespace StatisticsAnalysisTool.Cluster;

public static class DragonAreaPortalSizeResolver
{
    private const string DragonAreaPrefix = "DRAGON_AREA_";

    public static DragonAreaPortalSize FromUniqueName(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName)
            || !uniqueName.StartsWith(DragonAreaPrefix, StringComparison.Ordinal))
        {
            return DragonAreaPortalSize.Unknown;
        }

        if (uniqueName.Contains("_SMALL", StringComparison.Ordinal)
            || uniqueName.Contains("_CHEST_SOLO", StringComparison.Ordinal))
        {
            return DragonAreaPortalSize.Small;
        }

        if (uniqueName.Contains("_MEDIUM", StringComparison.Ordinal)
            || uniqueName.Contains("_CHEST_VETERAN", StringComparison.Ordinal))
        {
            return DragonAreaPortalSize.Medium;
        }

        if (uniqueName.Contains("_LARGE", StringComparison.Ordinal)
            || uniqueName.Contains("_CHEST_ELITE", StringComparison.Ordinal))
        {
            return DragonAreaPortalSize.Large;
        }

        return DragonAreaPortalSize.Unknown;
    }
}
