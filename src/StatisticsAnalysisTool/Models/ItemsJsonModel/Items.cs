using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Items
{
    [JsonPropertyName("shopcategories")]
    public ShopCategories ShopCategories { get; set; }
    [JsonPropertyName("hideoutitem")]
    public HideoutItem HideoutItem { get; set; }
    [JsonPropertyName("farmableitem")]
    public List<FarmableItem> FarmableItem { get; set; }
    [JsonPropertyName("simpleitem")]
    public List<SimpleItem> SimpleItem { get; set; }
    [JsonPropertyName("consumableitem")]
    public List<ConsumableItem> ConsumableItem { get; set; }
    [JsonPropertyName("consumablefrominventoryitem")]
    public List<ConsumableFromInventoryItem> ConsumableFromInventoryItem { get; set; }
    [JsonPropertyName("equipmentitem")]
    public List<EquipmentItem> EquipmentItem { get; set; }
    [JsonPropertyName("weapon")]
    public List<Weapon> Weapon { get; set; }
    [JsonPropertyName("mount")]
    public List<Mount> Mount { get; set; }
    [JsonPropertyName("furnitureitem")]
    public List<FurnitureItem> FurnitureItem { get; set; }
    [JsonPropertyName("journalitem")]
    public List<JournalItem> JournalItem { get; set; }
    [JsonPropertyName("labourercontract")]
    public List<LabourerContract> LabourerContract { get; set; }
    [JsonPropertyName("mountskin")]
    public List<MountSkin> MountSkin { get; set; }
    [JsonPropertyName("crystalleagueitem")]
    public List<CrystalLeagueItem> CrystalLeagueItem { get; set; }
}