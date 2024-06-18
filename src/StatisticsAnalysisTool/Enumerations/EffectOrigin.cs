namespace StatisticsAnalysisTool.Enumerations;

public enum EffectOrigin : byte
{
    MeleeAttack,
    RangedAttack,
    SpellDirect,
    SpellArea,
    SpellPassive,
    OverTimeEffect,
    Reflected,
    SpellCost,
    ServerAuthority,
    Unknown
}