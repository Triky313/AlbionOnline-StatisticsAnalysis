using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class JournalItem
{
    [JsonProperty("@salvageable")]
    public string Salvageable { get; set; }

    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@maxfame")]
    public string MaxFame { get; set; }

    [JsonProperty("@baselootamount")]
    public string BaseLootAmount { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@fasttravelfactor")]
    public string FastTravelFactor { get; set; }

    [JsonProperty("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }

    [JsonProperty("famefillingmissions")]
    public FameFillingMissions FameFillingMissions { get; set; }

    [JsonProperty("lootlist")]
    public LootList LootList { get; set; }
}