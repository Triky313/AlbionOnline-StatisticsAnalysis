using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FootStepVfxPreset
{
    [JsonProperty("@name")]
    public string Name { get; set; }
}