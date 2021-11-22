using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DungeonObject
    {
        [JsonIgnore]
        public List<TimeCollectObject> DungeonRunTimes { get; } = new ();
        public int TotalRunTimeInSeconds { get; set; }
        public List<Guid> GuidList { get; set; } = new ();
        public DateTime EnterDungeonFirstTime { get; set; }
        public string MainMapIndex { get; set; }
        public List<DungeonEventObject> DungeonEventObjects { get; set; } = new ();
        public DungeonStatus Status { get; set; }
        public double Fame { get; set; }
        public double ReSpec { get; set; }
        public double Silver { get; set; }
        public double FactionCoins { get; set; }
        public double FactionFlags { get; set; }
        public string DiedName { get; set; }
        public string KilledBy { get; set; }
        public bool DiedInDungeon { get; set; }
        public Faction Faction { get; set; } = Faction.Unknown;
        public DungeonMode Mode { get; set; } = DungeonMode.Unknown;
        public CityFaction CityFaction { get; set; } = CityFaction.Unknown;
        public Tier Tier { get; set; } = Tier.Unknown;
        [JsonIgnore]
        public string DungeonHash => $"{EnterDungeonFirstTime.Ticks}{string.Join(",", GuidList)}";
        
        private double? _lastReSpecValue;

        public DungeonObject()
        {
        }

        public DungeonObject(string mainMapIndex, Guid guid, DungeonStatus status, Tier tier)
        {
            MainMapIndex = mainMapIndex;
            EnterDungeonFirstTime = DateTime.UtcNow;
            GuidList.Add(guid);
            Status = status;
            AddTimer(DateTime.UtcNow);
            Mode = (Mode == DungeonMode.Unknown) ? DungeonObjectData.GetDungeonMode(mainMapIndex) : Mode;
            Tier = tier;
        }

        public void Add(double value, ValueType type, CityFaction cityFaction = CityFaction.Unknown)
        {
            switch (type)
            {
                case ValueType.Fame:
                    Fame += value;
                    return;
                case ValueType.ReSpec:
                    var internalReSpecValue = Utilities.AddValue(value, _lastReSpecValue, out _lastReSpecValue);
                    if (internalReSpecValue <= 0)
                    {
                        return;
                    }

                    ReSpec += internalReSpecValue;
                    return;
                case ValueType.Silver:
                    Silver += value;
                    return;
                case ValueType.FactionFame:
                    FactionFlags += value;
                    return;
                case ValueType.FactionPoints:
                    FactionCoins += value;
                    if (cityFaction != CityFaction.Unknown)
                    {
                        CityFaction = cityFaction;
                    }
                    return;
            }
        }
        
        public void AddTimer(DateTime time)
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

            SetTotalRunTimeInSeconds();
        }

        public void EndTimer()
        {
            var dateTime = DateTime.UtcNow;

            var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
            if (dun != null && dun.StartTime < dateTime)
            {
                dun.EndTime = dateTime;
                SetTotalRunTimeInSeconds();
            }
        }

        private void SetTotalRunTimeInSeconds()
        {
            foreach (var time in DungeonRunTimes.Where(x => x.EndTime != null).ToList())
            {
                TotalRunTimeInSeconds += (int)time.TimeSpan.TotalSeconds;
                DungeonRunTimes.Remove(time);
            }
        }
    }
}