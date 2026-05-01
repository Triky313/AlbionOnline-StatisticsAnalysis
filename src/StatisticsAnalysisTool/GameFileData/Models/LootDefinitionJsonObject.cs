using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class LootDefinitionJsonObject
{
    [JsonPropertyName("Lootlist")]
    [JsonConverter(typeof(SingleOrArrayConverter<LootListJsonObject>))]
    public List<LootListJsonObject> Lootlist { get; set; } = [];
}
