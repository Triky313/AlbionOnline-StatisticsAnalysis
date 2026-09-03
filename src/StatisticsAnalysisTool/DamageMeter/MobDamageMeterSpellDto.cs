using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterSpellDto
{
    public int SpellIndex { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ItemUniqueName { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ItemIndex { get; set; }
    public long DamageHealValue { get; set; }
    public int Ticks { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string UniqueName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Target { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Category { get; set; }
}