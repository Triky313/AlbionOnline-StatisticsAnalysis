using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Cluster;

public interface IClusterController
{
    void RegisterEvents();
    void UnregisterEvents();
    event Action<ClusterInfo> OnChangeCluster;
    void ChangeClusterInformation(MapType mapType, Guid? mapGuid, string clusterIndex, string instanceName,
        string worldMapDataType, byte[] dungeonInformation, string mainClusterIndex, Tier mistsDungeonTier);
    void SetJoinClusterInformation(string index, string mainClusterIndex);
    void SetAndResetValues(ClusterInfo currentCluster);

    void UpdateUserInfoUi(ClusterInfo currentCluster);
}