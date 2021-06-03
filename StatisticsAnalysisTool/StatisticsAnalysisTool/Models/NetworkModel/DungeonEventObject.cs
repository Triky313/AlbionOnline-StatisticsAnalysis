using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public abstract class DungeonEventObject
    {
        public int Id { get; set; }
        public DateTime Opened { get; set; }
        public bool IsOpen { get; set; }
        public string UniqueName { get; set; }
    }
}