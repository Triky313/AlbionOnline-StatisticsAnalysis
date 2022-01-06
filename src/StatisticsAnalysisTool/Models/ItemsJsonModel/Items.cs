using System.Collections.Generic;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Items
{
    [JsonProperty("shopcategories")]
    public ShopCategories ShopCategories { get; set; }
    [JsonProperty("hideoutitem")]
    public HideoutItem HideoutItem { get; set; }
    [JsonProperty("farmableitem")]
    public List<FarmableItem> FarmableItem { get; set; }
    [JsonProperty("simpleitem")]
    public List<SimpleItem> SimpleItem { get; set; }
    [JsonProperty("consumableitem")]
    public List<ConsumableItem> ConsumableItem { get; set; }
    [JsonProperty("consumablefrominventoryitem")]
    public List<ConsumableFromInventoryItem> ConsumableFromInventoryItem { get; set; }
    [JsonProperty("equipmentitem")]
    public List<EquipmentItem> EquipmentItem { get; set; }
    [JsonProperty("weapon")]
    public List<Weapon> Weapon { get; set; }
    [JsonProperty("mount")]
    public List<Mount> Mount { get; set; }
    [JsonProperty("furnitureitem")]
    public List<FurnitureItem> FurnitureItem { get; set; }
    [JsonProperty("journalitem")]
    public List<JournalItem> JournalItem { get; set; }
    [JsonProperty("labourercontract")]
    public List<LabourerContract> LabourerContract { get; set; }
    [JsonProperty("mountskin")]
    public List<MountSkin> MountSkin { get; set; }
    [JsonProperty("crystalleagueitem")]
    public List<CrystalLeagueItem> CrystalLeagueItem { get; set; }
}