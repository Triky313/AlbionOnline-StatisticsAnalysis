using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class Dungeon
    {
        public Dungeon(Guid firstMap)
        {
            FirstMap = firstMap;
            MapsGuid.Add(firstMap);
            FirstJoin = DateTime.UtcNow;
        }

        public DateTime FirstJoin { get; }
        public List<Guid> MapsGuid { get; set; }
        public Guid FirstMap { get; set; }
        public double Fame { get; set; }
        public double ReSpec { get; set; }
        public double Silver { get; set; }
    }
}