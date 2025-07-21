using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class KillTrophyItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@expiretimedays")]
    public string ExpireTimeDays { get; set; }

    [JsonPropertyName("@shopcategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@durabilitylossperdayfactor")]
    public string DurabilityLossPerDayFactor { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@durability")]
    public string Durability { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@placeableindoors")]
    public string PlaceableInDoors { get; set; }

    [JsonPropertyName("@placeableoutdoors")]
    public string PlaceableOutDoors { get; set; }

    [JsonPropertyName("@placeableindungeons")]
    public string PlaceableInDungeons { get; set; }

    [JsonPropertyName("@placeableinexpeditions")]
    public string PlaceableInExpeditions { get; set; }

    [JsonPropertyName("@placeableonlyonislands")]
    public string PlaceableOnlyOnIslands { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public string ShowInMarketplace { get; set; }

    [JsonPropertyName("@hidefromplayeroncontext")]
    public string HideFromPlayerOnContext { get; set; }

    [JsonPropertyName("@salvageable")]
    public string Salvageable { get; set; }

    [JsonPropertyName("@customizewithguildlogo")]
    public string CustomizeWithGuildLogo { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public string ShopSubCategory2 { get; set; }
}