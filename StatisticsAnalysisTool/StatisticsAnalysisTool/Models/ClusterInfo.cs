using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;

namespace StatisticsAnalysisTool.Models
{
    public class ClusterInfo
    {
        public string Index { get; set; }
        public string MainClusterIndex { get; set; }
        public Guid? Guid { get; set; }
        public string UniqueName { get; set; }
        public string Type { get; set; }
        public MapType MapType { get; set; }
        public ClusterType ClusterType => WorldData.GetClusterType(Type);
    }
}