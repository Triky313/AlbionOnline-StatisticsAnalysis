namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatEventParticipant
{
    public long ObjectId { get; init; }
    public string Name { get; set; }
    public bool IsPlayer { get; init; }
    public bool IsMob { get; init; }
    public long Damage { get; private set; }
    public long Heal { get; private set; }
    public long TakenDamage { get; private set; }

    public void AddValue(CombatEventValueType valueType, long value)
    {
        switch (valueType)
        {
            case CombatEventValueType.Damage:
                Damage += value;
                return;
            case CombatEventValueType.Heal:
                Heal += value;
                return;
            case CombatEventValueType.TakenDamage:
                TakenDamage += value;
                return;
        }
    }
}