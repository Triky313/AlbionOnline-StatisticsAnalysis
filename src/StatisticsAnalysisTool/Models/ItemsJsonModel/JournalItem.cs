using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common.Converters;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class JournalItem : ItemJsonObject
{
    [JsonPropertyName("@salvageable")]
    public string Salvageable { get; set; }

    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

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

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("famefillingmissions")]
    public FameFillingMissions FameFillingMissions { get; set; }

    [JsonPropertyName("lootlist")]
    public LootList LootList { get; set; }
}