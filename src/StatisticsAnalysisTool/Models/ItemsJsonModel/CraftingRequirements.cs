using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class CraftingRequirements
{
    [JsonPropertyName("@silver")]
    public string Silver { get; set; }

    [JsonPropertyName("@time")]
    public string Time { get; set; }

    [JsonPropertyName("@craftingfocus")]
    public string CraftingFocus { get; set; }

    [JsonPropertyName("craftresource")]
    public List<CraftResource> CraftResource { get; set; }

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

    [JsonPropertyName("@compensategold")]
    public string CompensateGold { get; set; }

    [JsonIgnore]
    public int TotalAmountResources
    {
        // TODO: Rework, needed?
        get
        {
            var result = 0;

            //switch (CraftResource)
            //{
            //    case List<CraftResource> craftResourceList:
            //        result = craftResourceList.ToList().Where(x => x != null && x.UniqueName != "QUESTITEM_TOKEN_AVALON" && !x.UniqueName.Contains("ARTEFACT_TOKEN_FAVOR")).Sum(x => x.Count);
            //        break;
            //    case CraftResource craftResource:
            //        result = craftResource.Count;
            //        break;
            //}

            return result;
        }
    }
}