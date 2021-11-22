using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class PlayerGameObject : GameObject
    {
        public PlayerGameObject(long objectId)
        {
            ObjectId = objectId;
        }

        public Guid UserGuid { get; set; }
        public Guid? InteractGuid { get; set; }
        public string Name { get; set; } = "Unknown";
        public CharacterEquipment CharacterEquipment { get; set; } = null;
        public DateTime? CombatStart { get; set; }
        public List<TimeCollectObject> CombatTimes { get; } = new ();
        public TimeSpan CombatTime { get; set; } = new (1);
        public long Damage { get; set; }
        public long Heal { get; set; }
        public double Dps => Utilities.GetValuePerSecondToDouble(Damage, CombatStart, CombatTime, 9999);
        public double Hps => Utilities.GetValuePerSecondToDouble(Heal, CombatStart, CombatTime, 9999);

        public override string ToString()
        {
            return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
        }

        public void AddCombatTime(TimeCollectObject timeCollectObject)
        {
            CombatTimes.Add(timeCollectObject);
            SetCombatTimeSpan();
        }

        public int CompareTo(object obj)
        {
            if (obj is not long dmg)
            {
                return -1;
            }

            if (Damage > dmg) return 1;

            if (Damage == dmg) return 0;

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
                CombatTime += combatTime.TimeSpan;
                CombatTimes.Remove(combatTime);
            }
        }
    }
}