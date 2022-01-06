using System.Collections.Generic;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class CraftingRequirements
{
    [JsonProperty("@silver")]
    public string Silver { get; set; }

    [JsonProperty("@time")]
    public string Time { get; set; }

    [JsonProperty("@craftingfocus")]
    public string CraftingFocus { get; set; }
    [JsonProperty("craftresource")]
    public List<CraftResource> CraftResource { get; set; }

    [JsonProperty("@swaptransaction")]
    public string SwapTransaction { get; set; }
    [JsonProperty("playerfactionstanding")]
    public PlayerFactionStanding PlayerFactionStanding { get; set; }
    [JsonProperty("currency")]
    public Currency Currency { get; set; }

    [JsonProperty("@amountcrafted")]
    public string AmountCrafted { get; set; }

    [JsonProperty("@forcesinglecraft")]
    public string ForceSingleCraft { get; set; }
}