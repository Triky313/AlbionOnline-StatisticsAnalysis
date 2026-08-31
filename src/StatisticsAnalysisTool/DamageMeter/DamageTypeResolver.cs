using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class DamageTypeResolver
{
    public static DamageType Resolve(EffectType effectType, int spellIndex, bool isMobTarget)
    {
        var spell = SpellData.GetSpellByIndex(spellIndex);
        var ignoresArmor = isMobTarget
            ? spell.IgnoresArmorAgainstMobs
            : spell.IgnoresArmorAgainstPlayers;
        if (ignoresArmor == true)
        {
            return DamageType.True;
        }

        return MapEffectType(effectType);
    }

    public static DamageType ResolveFromSpell(int spellIndex)
    {
        if (spellIndex == 0)
        {
            return DamageType.Physical;
        }

        var spell = SpellData.GetSpellByIndex(spellIndex);
        var playerDamageType = ResolveFromSpell(spell, false);
        var mobDamageType = ResolveFromSpell(spell, true);
        return playerDamageType == mobDamageType ? playerDamageType : DamageType.Unknown;
    }

    private static DamageType ResolveFromSpell(GameFileDataSpell spell, bool isMobTarget)
    {
        var ignoresArmor = isMobTarget
            ? spell.IgnoresArmorAgainstMobs
            : spell.IgnoresArmorAgainstPlayers;
        if (ignoresArmor == true)
        {
            return DamageType.True;
        }

        var effectType = isMobTarget
            ? spell.DamageEffectTypeAgainstMobs
            : spell.DamageEffectTypeAgainstPlayers;
        return effectType.HasValue ? MapEffectType(effectType.Value) : DamageType.Unknown;
    }

    private static DamageType MapEffectType(EffectType effectType)
    {
        return effectType switch
        {
            EffectType.Physical => DamageType.Physical,
            EffectType.Magic => DamageType.Magic,
            _ => DamageType.Unknown
        };
    }
}
