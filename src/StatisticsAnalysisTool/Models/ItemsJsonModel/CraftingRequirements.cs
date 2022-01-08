using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Annotations;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftingRequirements
{
    [JsonPropertyName("@silver")]
    public string Silver { get; set; }

    [JsonPropertyName("@time")]
    public string Time { get; set; }

    [JsonPropertyName("@craftingfocus")]
    public string CraftingFocus { get; set; }

    [JsonPropertyName("craftresource")]
    public object CraftResource { get; set; }

    [JsonPropertyName("@swaptransaction")]
    public string SwapTransaction { get; set; }

    [JsonPropertyName("playerfactionstanding")]
    public PlayerFactionStanding PlayerFactionStanding { get; set; }

    [JsonPropertyName("currency")]
    public Currency Currency { get; set; }

    [JsonPropertyName("@amountcrafted")]
    public string AmountCrafted { get; set; }

    [JsonPropertyName("@forcesinglecraft")]
    public string ForceSingleCraft { get; set; }
}