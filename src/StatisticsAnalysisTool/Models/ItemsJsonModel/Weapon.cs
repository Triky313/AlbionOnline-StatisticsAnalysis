using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class Weapon
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@mesh")]
    public string Mesh { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@maxqualitylevel")]
    public string Maxqualitylevel { get; set; }

    [JsonProperty("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonProperty("@slottype")]
    public string SlotType { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonProperty("@attacktype")]
    public string AttackType { get; set; }

    [JsonProperty("@attackdamage")]
    public string AttackDamage { get; set; }

    [JsonProperty("@attackspeed")]
    public string AttackSpeed { get; set; }

    [JsonProperty("@attackrange")]
    public string AttackRange { get; set; }

    [JsonProperty("@twohanded")]
    public string TwoHanded { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonProperty("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

    [JsonProperty("@durability")]
    public string Durability { get; set; }

    [JsonProperty("@durabilityloss_attack")]
    public string DurabilityLossAttack { get; set; }

    [JsonProperty("@durabilityloss_spelluse")]
    public string DurabilityLossSpellUse { get; set; }

    [JsonProperty("@durabilityloss_receivedattack")]
    public string DurabilityLossReceivedAttack { get; set; }

    [JsonProperty("@durabilityloss_receivedspell")]
    public string DurabilityLossReceivedSpell { get; set; }

    //[JsonProperty("@mainhandanimationtype")]
    //public string Mainhandanimationtype { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@unlockedtoequip")]
    public string UnlockedToEquip { get; set; }

    [JsonProperty("@itempower")]
    public string ItemPower { get; set; }

    [JsonProperty("@unequipincombat")]
    public string UnequipInCombat { get; set; }

    //[JsonProperty("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonProperty("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonProperty("@canbeovercharged")]
    public string CanBeOvercharged { get; set; }
    [JsonProperty("canharvest")]
    public CanHarvest CanHarvest { get; set; }
    public CraftingRequirements CraftingRequirements { get; set; }
    //public AudioInfo AudioInfo { get; set; }
    //public SocketPreset SocketPreset { get; set; }

    [JsonProperty("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonProperty("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }
    [JsonProperty("craftingspelllist")]
    public CraftingSpellList CraftingSpellList { get; set; }
}