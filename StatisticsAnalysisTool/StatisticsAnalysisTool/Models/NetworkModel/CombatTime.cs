using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class CombatTime
    {
        public CombatTime(DateTime startTime)
        {
            StartTime = startTime;
        }

        public DateTime StartTime { get; }
        public DateTime? EndTime { get; set; }
        public TimeSpan CombatTimeSpan => (EndTime != null) ? (DateTime)EndTime - (DateTime)StartTime : new TimeSpan(0);
    }
}