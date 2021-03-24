using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class PlayerGameObject : GameObject
    {
        public Guid UserGuid { get; set; }
        public string Name { get; set; } = "Unknown";
        public CharacterEquipment CharacterEquipment { get; set; } = null;
        public DateTime? CombatStart { get; set; }
        public List<CombatTime> CombatTimes { get; } = new List<CombatTime>();
        public TimeSpan CombatTime { get; set; } = new TimeSpan(1);
        public long Damage { get; set; }
        public double Dps => Utilities.GetValuePerSecondToDouble(Damage, CombatStart, CombatTime, 9999);

        public PlayerGameObject(long objectId)
        {
            ObjectId = objectId;
        }

        public override string ToString()
        {
            return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
        }

        public void AddCombatTime(CombatTime combatTime)
        {
            CombatTimes.Add(combatTime);
            SetCombatTimeSpan();
        }

        public int CompareTo(object obj)
        {
            if (!(obj is long))
            {
                return -1;
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

        public void ResetCombatTimes()
        {
            CombatTimes.Clear();
            CombatTime = new TimeSpan();
        }

        private void SetCombatTimeSpan()
        {
            foreach (var combatTime in CombatTimes.Where(x => x.EndTime != null).ToList())
            {
                CombatTime += combatTime.CombatTimeSpan;
                CombatTimes.Remove(combatTime);
            }
        }
    }
}