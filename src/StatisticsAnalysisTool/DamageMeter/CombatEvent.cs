using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatEvent
{
    private readonly Dictionary<long, CombatEventParticipant> _participants = new();

    public Guid CombatEventId { get; init; } = Guid.NewGuid();
    public string ClusterKey { get; init; }
    public string ClusterName { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EndTime { get; private set; }
    public DateTime LastEventTime { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;
    public bool IsImplicit { get; private set; }
    public HashSet<long> PlayerObjectIds { get; } = [];
    public HashSet<long> MobObjectIds { get; } = [];
    public List<CombatMobCacheEntry> KnownMobs { get; } = [];
    public List<CombatEventContribution> Contributions { get; } = [];
    public IReadOnlyCollection<CombatEventParticipant> Participants => _participants.Values;
    public long Damage { get; private set; }
    public long Heal { get; private set; }
    public long TakenDamage { get; private set; }

    public void MarkImplicit()
    {
        IsImplicit = true;
    }

    public void MarkExplicit()
    {
        IsImplicit = false;
    }

    public void AddContribution(CombatEventValueType valueType, long sourceObjectId, long targetObjectId, long value, int causingSpellIndex, CombatEventParticipant participant)
    {
        LastEventTime = DateTime.UtcNow;

        switch (valueType)
        {
            case CombatEventValueType.Damage:
                Damage += value;
                break;
            case CombatEventValueType.Heal:
                Heal += value;
                break;
            case CombatEventValueType.TakenDamage:
                TakenDamage += value;
                break;
        }

        if (participant != null)
        {
            if (!_participants.TryGetValue(participant.ObjectId, out var existingParticipant))
            {
                existingParticipant = participant;
                _participants.Add(participant.ObjectId, existingParticipant);
            }

            existingParticipant.AddValue(valueType, value);
        }

        Contributions.Add(new CombatEventContribution
        {
            CombatEventId = CombatEventId,
            Timestamp = LastEventTime,
            ValueType = valueType,
            SourceObjectId = sourceObjectId,
            TargetObjectId = targetObjectId,
            Value = value,
            CausingSpellIndex = causingSpellIndex
        });
    }

    public void AddPlayerObjectId(long objectId)
    {
        PlayerObjectIds.Add(objectId);
    }

    public void AddMob(CombatMobCacheEntry mob)
    {
        if (mob == null)
        {
            return;
        }

        MobObjectIds.Add(mob.MobObjectId);

        if (!KnownMobs.Exists(x => x.MobObjectId == mob.MobObjectId && x.ClusterKey == mob.ClusterKey))
        {
            KnownMobs.Add(mob);
        }
    }

    public void End(DateTime endTime)
    {
        if (!IsActive)
        {
            return;
        }

        EndTime = endTime;
        LastEventTime = endTime;
        IsActive = false;
    }
}