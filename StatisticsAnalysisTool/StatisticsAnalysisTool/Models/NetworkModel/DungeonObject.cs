using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonObject
    {
        public List<TimeCollectObject> DungeonRunTimes { get; } = new List<TimeCollectObject>();
        public TimeSpan TotalRunTime { get; set; }
        public List<Guid> GuidList { get; set; }
        public DateTime EnterDungeonFirstTime { get; }
        public string MainMapIndex { get; set; }
        public List<DungeonChestObject> DungeonChests { get; set; } = new List<DungeonChestObject>();
        public DungeonStatus Status { get; set; }
        public bool IsBestTime { get; set; }
        public bool IsBestFame { get; set; }
        public bool IsBestReSpec { get; set; }
        public bool IsBestSilver { get; set; }
        public bool IsBestFamePerHour { get; set; }
        public bool IsBestReSpecPerHour { get; set; }
        public bool IsBestSilverPerHour { get; set; }
        public double Fame { get; set; }
        public double ReSpec { get; set; }
        public double Silver { get; set; }
        public string DiedName { get; set; }
        public string KilledBy { get; set; }
        public bool DiedInDungeon { get; set; }
        public Faction Faction { get; set; }
        public DungeonMode Mode { get; set; } = DungeonMode.Unknown;
        public string DungeonHash => $"{EnterDungeonFirstTime}{GuidList}";

        public double FamePerHour => Utilities.GetValuePerHourToDouble(Fame, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
        public double ReSpecPerHour => Utilities.GetValuePerHourToDouble(ReSpec, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);
        public double SilverPerHour => Utilities.GetValuePerHourToDouble(Silver, TotalRunTime.Ticks <= 0 ? DateTime.UtcNow - EnterDungeonFirstTime : TotalRunTime);

        private double? _lastReSpecValue;
        private double? _lastSilverValue;

        public DungeonObject(Guid firstGuid, string mainMapIndex)
        {
            GuidList = new List<Guid> { firstGuid };
            AddStartTime(DateTime.UtcNow);
            EnterDungeonFirstTime = DateTime.UtcNow;
            MainMapIndex = mainMapIndex;
        }

        public void Add(double value, ValueType type)
        {
            switch (type)
            {
                case ValueType.Fame:
                    Fame += value;
                    return;
                case ValueType.ReSpec:
                    ReSpec += AddValue(value, _lastReSpecValue, out _lastReSpecValue);
                    return;
                case ValueType.Silver:
                    Silver += AddValue(value, _lastSilverValue, out _lastSilverValue);
                    return;
            }
        }

        private double AddValue(double value, double? lastValue, out double? newLastValue)
        {
            if (lastValue == null)
            {
                newLastValue = value;
                return 0;
            }

            var newSilverValue = (double)(value - lastValue);

            if (newSilverValue == 0)
            {
                newLastValue = value;
                return 0;
            }

            newLastValue = value;

            return newSilverValue;
        }

        public void AddStartTime(DateTime time)
        {
            if (DungeonRunTimes.Any(x => x.EndTime == null))
            {
                var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
                if (dun != null)
                {
                    dun.EndTime = time;
                    DungeonRunTimes.Add(new TimeCollectObject(time));
                }
            }
            else
            {
                DungeonRunTimes.Add(new TimeCollectObject(time));
            }

            SetCombatTimeSpan();
        }

        public void AddEndTime(DateTime time)
        {
            var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
            if (dun != null && dun.StartTime < time)
            {
                dun.EndTime = time;
                SetCombatTimeSpan();
            }
        }

        private void SetCombatTimeSpan()
        {
            foreach (var time in DungeonRunTimes.Where(x => x.EndTime != null).ToList())
            {
                TotalRunTime += time.TimeSpan;
                DungeonRunTimes.Remove(time);
            }
        }
        
    }
}