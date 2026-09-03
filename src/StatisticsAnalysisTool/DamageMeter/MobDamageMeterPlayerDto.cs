using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterPlayerDto
{
    public int PlayerId { get; set; }
    public TimeSpan CombatTime { get; set; }
    public long Damage { get; set; }
    public string CauserMainHandItemUniqueName { get; set; } = string.Empty;
    public List<MobDamageMeterSpellDto> Spells { get; set; } = [];

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid CauserGuid { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; }
}