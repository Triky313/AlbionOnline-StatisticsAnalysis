namespace StatisticsAnalysisTool.Cluster;

public static class ClusterInfoMapping
{
    public static ClusterInfoDto Mapping(ClusterInfo clusterInfo)
    {
        return new ClusterInfoDto
        {
            Entered = clusterInfo.Entered,
            MapType = clusterInfo.MapType,
            Guid = clusterInfo.Guid,
            Index = clusterInfo.Index,
            InstanceName = clusterInfo.InstanceName,
            WorldMapDataType = clusterInfo.WorldMapDataType,
            DungeonInformation = clusterInfo.DungeonInformation,
            MistsDungeonTier = clusterInfo.MistsDungeonTier,
            RandomDungeonTier = clusterInfo.RandomDungeonTier,
            RandomDungeonLevel = clusterInfo.RandomDungeonLevel,
            MainClusterIndex = clusterInfo.MainClusterIndex,
            MapHistoryNote = clusterInfo.MapHistoryNote
        };
    }

    public static ClusterInfo Mapping(ClusterInfoDto clusterInfoDto)
    {
        var clusterInfo = new ClusterInfo();

        clusterInfo.SetClusterInfo(
            clusterInfoDto.MapType,
            clusterInfoDto.Guid,
            clusterInfoDto.Index,
            clusterInfoDto.InstanceName,
            clusterInfoDto.WorldMapDataType,
            clusterInfoDto.DungeonInformation,
            clusterInfoDto.MainClusterIndex,
            clusterInfoDto.MistsDungeonTier);

        clusterInfo.Entered = clusterInfoDto.Entered;
        clusterInfo.MapHistoryNote = clusterInfoDto.MapHistoryNote ?? string.Empty;
        clusterInfo.SetJoinClusterInfo(clusterInfoDto.Index, clusterInfoDto.MainClusterIndex, clusterInfoDto.Guid, clusterInfoDto.MapType);
        clusterInfo.SetRandomDungeonTrackingInfo(clusterInfoDto.RandomDungeonTier, clusterInfoDto.RandomDungeonLevel);
        clusterInfo.ClusterInfoFullyAvailable = true;

        return clusterInfo;
    }
}
