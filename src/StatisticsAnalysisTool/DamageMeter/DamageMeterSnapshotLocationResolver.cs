using StatisticsAnalysisTool.Cluster;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageMeterSnapshotLocationResolver
{
    public static string Resolve(ClusterInfo clusterInfo)
    {
        if (clusterInfo == null)
        {
            return string.Empty;
        }

        return clusterInfo.MapHistoryClipboardName ?? string.Empty;
    }
}