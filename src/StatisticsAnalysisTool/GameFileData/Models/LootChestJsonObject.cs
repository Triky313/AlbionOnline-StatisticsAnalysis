using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootChestJsonObject
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; } = string.Empty;

    [JsonPropertyName("@avatar")]
    public string Avatar { get; set; } = string.Empty;

    [JsonPropertyName("@faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("@alertradius")]
    public double AlertRadius { get; set; }

    [JsonPropertyName("@spell")]
    public string Spell { get; set; } = string.Empty;

    [JsonPropertyName("@opensoundevent")]
    public string OpenSoundEvent { get; set; } = string.Empty;

    [JsonPropertyName("@spawnsoundevent")]
    public string SpawnSoundEvent { get; set; } = string.Empty;

    [JsonPropertyName("@namelocatag")]
    public string NameLocatag { get; set; } = string.Empty;

    [JsonPropertyName("@locklocatag")]
    public string LockLocatag { get; set; } = string.Empty;

    [JsonPropertyName("@dangerstate")]
    public string DangerState { get; set; } = string.Empty;

    [JsonPropertyName("@prefab")]
    public string Prefab { get; set; } = string.Empty;

    [JsonPropertyName("@chesttype")]
    public string ChestType { get; set; } = string.Empty;

    [JsonPropertyName("RareStates")]
    public LootChestRareStates RareStates { get; set; } = new();
}
