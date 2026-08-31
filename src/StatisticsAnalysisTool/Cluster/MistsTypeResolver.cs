using System;

namespace StatisticsAnalysisTool.Cluster;

public static class MistsTypeResolver
{
    public static MistsType FromUniqueName(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return MistsType.Unknown;
        }

        if (uniqueName.StartsWith("MISTS_SOLO_", StringComparison.Ordinal))
        {
            return MistsType.Solo;
        }

        return uniqueName.StartsWith("MISTS_DUO_", StringComparison.Ordinal)
            ? MistsType.Duo
            : MistsType.Unknown;
    }
}