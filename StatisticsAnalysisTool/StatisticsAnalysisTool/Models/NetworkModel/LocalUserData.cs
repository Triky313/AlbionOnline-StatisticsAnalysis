using System;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class LocalUserData
    {
        public long? UserObjectId { get; set; }
        public Guid? Guid { get; set; }
        public string Username { get; set; }
        public FixPoint? LearningPoints { get; set; }
        public double? Reputation { get; set; }
        public FixPoint? ReSpecPoints { get; set; }
        public FixPoint? Silver { get; set; }
        public FixPoint? Gold { get; set; }
        public string GuildName { get; set; }
        public string MainMapIndex { get; set; }
        public int? PlayTimeInSeconds { get; set; }
        public string AllianceName { get; set; }
        public long? CurrentDailyBonusPoints { get; set; }
    }
}