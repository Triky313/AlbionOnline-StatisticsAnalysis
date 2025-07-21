using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common.Converters;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class FurnitureItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@shopcategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@durability")]
    public double Durability { get; set; }

    [JsonPropertyName("@durabilitylossperdayfactor")]
    public string Durabilitylossperdayfactor { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@placeableindoors")]
    public string PlaceableIndoors { get; set; }

    [JsonPropertyName("@placeableoutdoors")]
    public string PlaceableOutdoors { get; set; }

    [JsonPropertyName("@placeableindungeons")]
    public string PlaceableInDungeons { get; set; }

    [JsonPropertyName("@placeableinexpeditions")]
    public string PlaceableInExpeditions { get; set; }

    [JsonPropertyName("@accessrightspreset")]
    public string AccessRightsPreset { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }
    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("repairkit")]
    public RepairKit RepairKit { get; set; }

    [JsonPropertyName("@customizewithguildlogo")]
    public string CustomizeWithGuildLogo { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonPropertyName("@tile")]
    public string Tile { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }
    public Container container { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public bool ShowInMarketPlace { get; set; }

    [JsonPropertyName("@residencyslots")]
    public string ResidencySlots { get; set; }

    [JsonPropertyName("@labourerfurnituretype")]
    public string LabourerFurnitureType { get; set; }

    [JsonPropertyName("@labourersaffected")]
    public string LabourersAffected { get; set; }

    [JsonPropertyName("@labourerhappiness")]
    public string LabourerHappiness { get; set; }

    [JsonPropertyName("@labourersperfurnitureitem")]
    public string LabourersPerFurnitureItem { get; set; }

    [JsonPropertyName("@placeableonlyonislands")]
    public string PlaceableOnlyOnIslands { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public string ShopSubCategory2 { get; set; }
}