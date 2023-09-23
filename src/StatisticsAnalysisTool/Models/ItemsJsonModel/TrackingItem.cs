using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class TrackingItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@findtrackspell")]
    public string FindTrackSpell { get; set; }

    [JsonPropertyName("@trackingtimereduction")]
    public string TrackingTimeReduction { get; set; }

    [JsonPropertyName("@trackingfamebonus")]
    public string TrackingFameBonus { get; set; }

    [JsonPropertyName("@durability")]
    public string Durability { get; set; }

    [JsonPropertyName("@itempower")]
    public string ItemPower { get; set; }

    [JsonPropertyName("@durabilityloss_inspecttrack")]
    public string DurabilityLossInspectTrack { get; set; }

    [JsonPropertyName("@durabilityloss_findtrack")]
    public string DurabilityLossFindTrack { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonIgnore]
    public CraftingJournalType CraftingJournalType => CraftingController.GetCraftingJournalType(UniqueName, CraftingCategory);

    [JsonPropertyName("@maxstacksize")]
    public string MaxStackSize { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@shopcategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string UiCraftSoundStart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string UiCraftSoundFinish { get; set; }

    [JsonPropertyName("@slottype")]
    public string SlotType { get; set; }

    [JsonPropertyName("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

    [JsonPropertyName("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }
}