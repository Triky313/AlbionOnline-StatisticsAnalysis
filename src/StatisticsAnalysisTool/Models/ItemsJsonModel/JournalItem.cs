using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class JournalItem
{
    [JsonPropertyName("@salvageable")]
    public string Salvageable { get; set; }

    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@maxfame")]
    public string MaxFame { get; set; }

    [JsonPropertyName("@baselootamount")]
    public string BaseLootAmount { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@fasttravelfactor")]
    public string FastTravelFactor { get; set; }

    [JsonPropertyName("craftingrequirements")]
    public CraftingRequirements CraftingRequirements { get; set; }

    [JsonPropertyName("famefillingmissions")]
    public FameFillingMissions FameFillingMissions { get; set; }

    [JsonPropertyName("lootlist")]
    public LootList LootList { get; set; }
}