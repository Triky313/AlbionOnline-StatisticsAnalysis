using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.DamageMeter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class PlayerGameObject : GameObject
{
    public PlayerGameObject(long? objectId)
    {
        ObjectId ??= objectId;
        LastUpdate = DateTime.UtcNow.Ticks;
    }

    public long LastUpdate { get; private set; }

    public Guid UserGuid
    {
        get;
        init
        {
            field = value;
            LastUpdate = DateTime.UtcNow.Ticks;
        }
    }

    public Guid? InteractGuid
    {
        get;
        set
        {
            field = value;
            LastUpdate = DateTime.UtcNow.Ticks;
        }
    }

    public string Name { get; set; } = "Unknown";
    public string Guild { get; set; }
    public string Alliance { get; set; }
    public bool IsInParty { get; set; }
    public double ItemPower { get; set; }

    public CharacterEquipment CharacterEquipment
    {
        get;
        set
        {
            field = value;
            LastUpdate = DateTime.UtcNow.Ticks;
        }
    }

    public DateTime? CombatStart { get; set; }

    public List<ActionInterval> CombatTimes
    {
        get;
        set
        {
            field = value;
            LastUpdate = DateTime.UtcNow.Ticks;
        }
    } = [];

    public TimeSpan CombatTime { get; set; } = new(1);
    public long Damage { get; set; }
    public long Heal { get; set; }
    public long TakenDamage { get; set; }
    public List<UsedSpell> Spells { get; set; } = new();
    public long Overhealed { get; set; }
    public double Dps => Utilities.GetValuePerSecondToDouble(Damage, CombatStart, CombatTime, 9999);
    public double Hps => Utilities.GetValuePerSecondToDouble(Heal, CombatStart, CombatTime, 9999);

    public override string ToString()
    {
        return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
    }

    #region Combat

    public void AddCombatTime(ActionInterval actionInterval)
    {
        CombatTimes.Add(actionInterval);
        SetCombatTimeSpan();
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

    #endregion

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
}