using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class CombatDamageTime
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan CombatTime => (StartTime != null && EndTime != null) ? (DateTime)EndTime - (DateTime)StartTime : new TimeSpan(0);
        public long Damage { get; set; } = 0;
    }
}