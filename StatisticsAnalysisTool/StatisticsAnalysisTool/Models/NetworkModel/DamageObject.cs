using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DamageObject : IComparable
    {
        public DateTime StartTime { get; set; }
        public Guid CauserGuid { get; }
        public string CauserName { get; set; }
        public int MainHandItemIndex { get; set; }
        public long Damage { get; set; }
        public double Dps { get; set; }

        public DamageObject(DateTime startTime, Guid causerGuid, string causerName, int mainHandItemIndex, long damage, double dps)
        {
            StartTime = startTime;
            CauserGuid = causerGuid;
            CauserName = causerName;
            MainHandItemIndex = mainHandItemIndex;
            Damage = damage;
            Dps = dps;
        }

        public override string ToString()
        {
            return $"{StartTime} | {CauserGuid} | {CauserName} | {MainHandItemIndex} | {Damage} | {Dps}";
        }

        public int CompareTo(object obj)
        {
            if (!(obj is long))
            {
                throw new ArgumentException();
            }

            var dmg = (long)obj;
            if (Damage > dmg)
            {
                return 1;
            }

            if (Damage == dmg)
            {
                return 0;
            }

            return -1;
        }
    }
}