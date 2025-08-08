using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Weapon : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    //[JsonPropertyName("@mesh")]
    //public string Mesh { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@maxqualitylevel")]
    public string MaxQualityLevel { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@physicalspelldamagebonus")]
    public string PhysicalSpellDamageBonus { get; set; }

    [JsonPropertyName("@magicspelldamagebonus")]
    public string MagicSpellDamageBonus { get; set; }

    [JsonPropertyName("@slottype")]
    public string SlotType { get; set; }
    [JsonIgnore]
    public SlotType SlotTypeEnum => ItemController.GetSlotType(SlotType);

    [JsonPropertyName("@shopcategory")]
    public override string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public override string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@attacktype")]
    public string AttackType { get; set; }

    [JsonPropertyName("@attackdamage")]
    public string AttackDamage { get; set; }

    [JsonPropertyName("@attackspeed")]
    public string AttackSpeed { get; set; }

    [JsonPropertyName("@attackrange")]
    public string AttackRange { get; set; }

    [JsonPropertyName("@twohanded")]
    public bool TwoHanded { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonPropertyName("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

    [JsonPropertyName("@durability")]
    public double Durability { get; set; }

    [JsonPropertyName("@durabilityloss_attack")]
    public string DurabilityLossAttack { get; set; }

    [JsonPropertyName("@durabilityloss_spelluse")]
    public string DurabilityLossSpellUse { get; set; }

    [JsonPropertyName("@durabilityloss_receivedattack")]
    public string DurabilityLossReceivedAttack { get; set; }

    [JsonPropertyName("@durabilityloss_receivedspell")]
    public string DurabilityLossReceivedSpell { get; set; }

    //[JsonPropertyName("@mainhandanimationtype")]
    //public string Mainhandanimationtype { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public bool UnlockedToCraft { get; set; }

    [JsonPropertyName("@unlockedtoequip")]
    public bool UnlockedToEquip { get; set; }

    [JsonPropertyName("@itempower")]
    public string ItemPower { get; set; }

    [JsonPropertyName("@hitpointsregenerationbonus")]
    public string HitPointsRegenerationBonus { get; set; }

    [JsonPropertyName("@focusfireprotectionpenetration")]
    public string FocusFireProtectionPenetration { get; set; }
    
    //[JsonPropertyName("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonPropertyName("@healmodifier")]
    public string HealModifier { get; set; }

    [JsonPropertyName("@canbeovercharged")]
    public bool CanBeOvercharged { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public bool ShowInMarketPlace { get; set; }

    [JsonPropertyName("canharvest")]
    public CanHarvest CanHarvest { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    //public AudioInfo AudioInfo { get; set; }
    //public SocketPreset SocketPreset { get; set; }

    [JsonPropertyName("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonIgnore] 
    public CraftingJournalType CraftingJournalType => CraftingController.GetCraftingJournalType(UniqueName, CraftingCategory);

    [JsonPropertyName("@physicalattackdamagebonus")]
    public string PhysicAttackDamageBonus { get; set; }

    [JsonPropertyName("@magicattackdamagebonus")]
    public string MagicAttackDamageBonus { get; set; }

    [JsonPropertyName("@descriptionlocatag")]
    public string DescriptionLocaTag { get; set; }

    [JsonPropertyName("craftingspelllist")]
    public CraftingSpellList CraftingSpellList { get; set; }

    [JsonPropertyName("enchantments")]
    public Enchantments Enchantments { get; set; }

    [JsonPropertyName("@shopsubcategory2")]
    public override string ShopSubCategory2 { get; set; }

    [JsonPropertyName("@shopsubcategory3")]
    public override string ShopSubCategory3 { get; set; }
}