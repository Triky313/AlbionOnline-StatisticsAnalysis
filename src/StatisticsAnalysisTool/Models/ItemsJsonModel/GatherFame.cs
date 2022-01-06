using System.Collections.Generic;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class GatherFame
{
    [JsonProperty("@mintier")]
    public string Mintier { get; set; }

    [JsonProperty("@value")]
    public string Value { get; set; }
    [JsonProperty("validitem")]
    public List<ValidItem> ValidItem { get; set; }
}