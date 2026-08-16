using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatEvent
{
    private readonly Dictionary<long, CombatEventParticipant> _participants = new();
    private readonly Dictionary<Guid, int> _knownMobIndexes = [];
    private readonly List<CombatMobCacheEntry> _knownMobs = [];

    public Guid CombatEventId { get; init; } = Guid.NewGuid();
    public string ClusterKey { get; init; }
    public string ClusterName { get; init; }
    public DashboardContentType ContentType { get; init; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public DateTime? EndTime { get; private set; }
    public DateTime LastEventTime { get; private set; } = DateTime.UtcNow;
    public bool IsActive { get; private set; } = true;
    public bool IsImplicit { get; private set; }
    public HashSet<long> PlayerObjectIds { get; } = [];
    public HashSet<long> MobObjectIds { get; } = [];
    public HashSet<Guid> MobInstanceIds { get; } = [];
    public List<CombatEventContribution> Contributions { get; } = [];
    public IReadOnlyList<CombatMobCacheEntry> KnownMobs => _knownMobs;
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

    public void AddContribution(CombatEventValueType valueType, long sourceObjectId, long targetObjectId, Guid? targetMobInstanceId, long value, int causingSpellIndex, CombatEventParticipant participant)
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
            TargetMobInstanceId = targetMobInstanceId,
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

        if (!_knownMobIndexes.TryGetValue(mob.MobInstanceId, out var knownMobIndex))
        {
            MobInstanceIds.Add(mob.MobInstanceId);
            _knownMobIndexes.Add(mob.MobInstanceId, _knownMobs.Count);
            _knownMobs.Add(mob.Clone());
            return;
        }

        if (_knownMobs[knownMobIndex].IsProvisional && !mob.IsProvisional)
        {
            _knownMobs[knownMobIndex] = mob.Clone();
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

    internal CombatEvent Clone()
    {
        var combatEvent = new CombatEvent
        {
            CombatEventId = CombatEventId,
            ClusterKey = ClusterKey,
            ClusterName = ClusterName,
            ContentType = ContentType,
            StartTime = StartTime,
            EndTime = EndTime,
            LastEventTime = LastEventTime,
            IsActive = IsActive,
            IsImplicit = IsImplicit,
            Damage = Damage,
            Heal = Heal,
            TakenDamage = TakenDamage
        };

        foreach (var playerObjectId in PlayerObjectIds)
        {
            combatEvent.PlayerObjectIds.Add(playerObjectId);
        }

        foreach (var mobObjectId in MobObjectIds)
        {
            combatEvent.MobObjectIds.Add(mobObjectId);
        }

        foreach (var mobInstanceId in MobInstanceIds)
        {
            combatEvent.MobInstanceIds.Add(mobInstanceId);
        }

        foreach (var knownMob in _knownMobs)
        {
            combatEvent.AddMob(knownMob);
        }

        foreach (var contribution in Contributions)
        {
            combatEvent.Contributions.Add(new CombatEventContribution
            {
                CombatEventId = contribution.CombatEventId,
                Timestamp = contribution.Timestamp,
                ValueType = contribution.ValueType,
                SourceObjectId = contribution.SourceObjectId,
                TargetObjectId = contribution.TargetObjectId,
                Value = contribution.Value,
                TargetMobInstanceId = contribution.TargetMobInstanceId,
                CausingSpellIndex = contribution.CausingSpellIndex
            });
        }

        foreach (var participant in _participants.Values)
        {
            var participantClone = new CombatEventParticipant
            {
                ObjectId = participant.ObjectId,
                Name = participant.Name,
                IsPlayer = participant.IsPlayer,
                IsMob = participant.IsMob
            };

            participantClone.AddValue(CombatEventValueType.Damage, participant.Damage);
            participantClone.AddValue(CombatEventValueType.Heal, participant.Heal);
            participantClone.AddValue(CombatEventValueType.TakenDamage, participant.TakenDamage);
            combatEvent._participants.Add(participantClone.ObjectId, participantClone);
        }

        return combatEvent;
    }
}
