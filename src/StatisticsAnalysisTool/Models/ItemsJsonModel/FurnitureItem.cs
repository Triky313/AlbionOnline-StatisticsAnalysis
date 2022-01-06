using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class FurnitureItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@durability")]
    public string Durability { get; set; }

    [JsonProperty("@durabilitylossperdayfactor")]
    public string Durabilitylossperdayfactor { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@placeableindoors")]
    public string PlaceableIndoors { get; set; }

    [JsonProperty("@placeableoutdoors")]
    public string PlaceableOutdoors { get; set; }

    [JsonProperty("@placeableindungeons")]
    public string PlaceableInDungeons { get; set; }

    [JsonProperty("@placeableinexpeditions")]
    public string PlaceableInExpeditions { get; set; }

    [JsonProperty("@accessrightspreset")]
    public string AccessRightsPreset { get; set; }

    //[JsonProperty("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonProperty("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }
    [JsonProperty("craftingrequirements")]
    public object CraftingRequirements { get; set; }
    [JsonProperty("repairkit")]
    public RepairKit RepairKit { get; set; }

    [JsonProperty("@customizewithguildlogo")]
    public string CustomizeWithGuildLogo { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    [JsonProperty("@tile")]
    public string Tile { get; set; }

    [JsonProperty("@itemvalue")]
    public string ItemValue { get; set; }
    public Container container { get; set; }

    [JsonProperty("@showinmarketplace")]
    public string ShowInMarketPlace { get; set; }

    [JsonProperty("@residencyslots")]
    public string ResidencySlots { get; set; }

    [JsonProperty("@labourerfurnituretype")]
    public string LabourerFurnitureType { get; set; }

    [JsonProperty("@labourersaffected")]
    public string LabourersAffected { get; set; }

    [JsonProperty("@labourerhappiness")]
    public string LabourerHappiness { get; set; }

    [JsonProperty("@labourersperfurnitureitem")]
    public string LabourersPerFurnitureItem { get; set; }

    [JsonProperty("@placeableonlyonislands")]
    public string PlaceableOnlyOnIslands { get; set; }

    [JsonProperty("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }
}