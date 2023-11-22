using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class MobObjects
{
    [JsonPropertyName("Mob")]
    public List<MobJsonObject> Mob { get; set; }
}