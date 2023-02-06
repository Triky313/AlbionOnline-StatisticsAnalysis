using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class CraftingRequirements
{
    [JsonPropertyName("@silver")]
    public string Silver { get; set; } = null!;

    [JsonPropertyName("@time")]
    public string Time { get; set; } = null!;

    [JsonPropertyName("@craftingfocus")]
    public string CraftingFocus { get; set; } = null!;

    [JsonPropertyName("craftresource")]
    public List<CraftResource> CraftResource { get; set; } = null!;

    [JsonPropertyName("@swaptransaction")]
    public string SwapTransaction { get; set; } = null!;

    [JsonPropertyName("playerfactionstanding")]
    public PlayerFactionStanding? PlayerFactionStanding { get; set; } = null!;

    [JsonPropertyName("currency")]
    public Currency Currency { get; set; } = null!;

    [JsonPropertyName("@amountcrafted")]
    public string AmountCrafted { get; set; } = null!;

    [JsonPropertyName("@forcesinglecraft")]
    public string ForceSingleCraft { get; set; } = null!;

    [JsonPropertyName("@compensategold")]
    public string CompensateGold { get; set; } = null!;
}