using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class FactionValuePerHour
    {
        public DateTime DateTime { get; set; }
        public CityFaction CityFaction { get; set; }
        public double Value { get; set; }
    }
}