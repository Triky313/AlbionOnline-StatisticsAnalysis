using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class GatherFame
{
    [JsonPropertyName("@mintier")]
    public string Mintier { get; set; }

    [JsonPropertyName("@value")]
    public string Value { get; set; }
    [JsonPropertyName("validitem")]
    public List<ValidItem> ValidItem { get; set; }
}