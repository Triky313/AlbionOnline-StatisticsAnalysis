using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class ValuePerHour
    {
        public DateTime DateTime { get; set; }
        public CityFaction CityFaction { get; set; } = CityFaction.Unknown;
        public double Value { get; set; }
    }
}