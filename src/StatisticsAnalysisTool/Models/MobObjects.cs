using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class MobObjects
{
    [JsonPropertyName("Mob")]
    public List<MobJsonObject> Mob { get; set; }
}