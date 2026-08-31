namespace StatisticsAnalysisTool.GameFileData.Models;

using System.Collections.Generic;
using StatisticsAnalysisTool.Enumerations;

public class GameFileDataSpell
{
    public int Index { get; init; }
    public string UniqueName { get; init; }
    public string Target { get; init; }
    public string Category { get; init; }
    public string NameLocatag { get; init; }
    public string DescriptionLocatag { get; init; }
    public string SpellKind { get; init; }
    public string UiSprite { get; init; }
    public string UiType { get; init; }
    public string EnergyUsage { get; init; }
    public string CastingTime { get; init; }
    public string RecastDelay { get; init; }
    public string CastRange { get; init; }
    public string ChannelingTime { get; init; }
    public string StatBlockLocatag { get; init; }
    public IReadOnlyList<string> DescriptionValues { get; init; } = [];
    public bool? IgnoresArmorAgainstPlayers { get; init; }
    public bool? IgnoresArmorAgainstMobs { get; init; }
    public EffectType? DamageEffectTypeAgainstPlayers { get; init; }
    public EffectType? DamageEffectTypeAgainstMobs { get; init; }
    public bool HasIcon => !string.IsNullOrWhiteSpace(UiSprite);
}