using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class PlayerGameObject : GameObject
{
    private ConcurrentDictionary<DashboardContentType, DamageMeterPlayerStats> _damageMeterContentStats = new();

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
    public int LastContributionWeaponItemIndex { get; set; }
    public List<UsedSpell> Spells { get; set; } = new();
    public long Overhealed { get; set; }
    public double Dps => Utilities.GetValuePerSecondToDouble(Damage, CombatStart, GetCombatTime(DateTime.UtcNow), 9999);
    public double Hps => Utilities.GetValuePerSecondToDouble(Heal, CombatStart, GetCombatTime(DateTime.UtcNow), 9999);

    public DamageMeterPlayerStats GetOrCreateDamageMeterContentStats(DashboardContentType contentType)
    {
        return _damageMeterContentStats.GetOrAdd(contentType, _ => new DamageMeterPlayerStats());
    }

    public PlayerGameObject CreateDamageMeterContentView(DashboardContentType contentType)
    {
        if (!_damageMeterContentStats.TryGetValue(contentType, out var stats))
        {
            return null;
        }

        var player = new PlayerGameObject(ObjectId)
        {
            UserGuid = UserGuid,
            InteractGuid = InteractGuid,
            Name = Name,
            Guild = Guild,
            Alliance = Alliance,
            IsInParty = IsInParty,
            ItemPower = ItemPower,
            CharacterEquipment = CharacterEquipment,
            LastContributionWeaponItemIndex = LastContributionWeaponItemIndex,
            ObjectType = ObjectType,
            ObjectSubType = ObjectSubType
        };

        stats.ApplyTo(player);
        return player;
    }

    public void CopyDamageMeterContentStatsFrom(PlayerGameObject source)
    {
        if (source == null)
        {
            return;
        }

        _damageMeterContentStats = source._damageMeterContentStats;
    }

    public void EndDamageMeterContentCombatIntervals(DateTime endTime)
    {
        foreach (var stats in _damageMeterContentStats.Values)
        {
            stats.EndCombatInterval(endTime);
        }
    }

    public void ResetDamageMeterContentStats()
    {
        _damageMeterContentStats.Clear();
    }

    public override string ToString()
    {
        return $"{ObjectType}[ObjectId: {ObjectId}, Name: '{Name}']";
    }

    #region Combat

    public void StartCombatInterval(DateTime startTime)
    {
        lock (CombatTimes)
        {
            SetCombatTimeSpan();
            if (CombatTimes.Any(x => x.EndTime == null))
            {
                return;
            }

            CombatTimes.Add(new ActionInterval(startTime));
        }
    }

    public void EndCombatInterval(DateTime endTime)
    {
        lock (CombatTimes)
        {
            var combatTime = CombatTimes.FirstOrDefault(x => x.EndTime == null);
            if (combatTime == null)
            {
                return;
            }

            combatTime.EndTime = endTime;
            SetCombatTimeSpan();
        }
    }

    public TimeSpan GetCombatTime(DateTime currentTime)
    {
        lock (CombatTimes)
        {
            return CombatTimes.Aggregate(
                CombatTime,
                (total, interval) => total + interval.GetDuration(currentTime));
        }
    }

    public void ResetCombatTimes()
    {
        lock (CombatTimes)
        {
            CombatTimes.Clear();
            CombatTime = TimeSpan.Zero;
        }
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