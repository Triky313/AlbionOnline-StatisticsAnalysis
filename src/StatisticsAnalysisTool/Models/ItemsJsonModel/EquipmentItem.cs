using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class EquipmentItem : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@maxqualitylevel")]
    public string MaxQualityLevel { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@slottype")]
    public string Slottype { get; set; }

    [JsonPropertyName("@itempowerprogressiontype")]
    public string ItemPowerProgressionType { get; set; }

    [JsonPropertyName("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonPropertyName("@skincount")]
    public string SkinCount { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonPropertyName("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

    [JsonPropertyName("@physicalarmor")]
    public string PhysicalArmor { get; set; }

    [JsonPropertyName("@magicresistance")]
    public string MagicResistance { get; set; }

    [JsonPropertyName("@durability")]
    public string Durability { get; set; }

    [JsonPropertyName("@durabilityloss_attack")]
    public string DurabilityLossAttack { get; set; }

    [JsonPropertyName("@durabilityloss_spelluse")]
    public string DurabilityLossSpellUse { get; set; }

    [JsonPropertyName("@durabilityloss_receivedattack")]
    public string DurabilityLossReceivedAttack { get; set; }

    [JsonPropertyName("@durabilityloss_receivedspell")]
    public string DurabilityLossReceivedSpell { get; set; }

    [JsonPropertyName("@offhandanimationtype")]
    public string OffHandAnimationType { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public bool UnlockedToCraft { get; set; }

    [JsonPropertyName("@unlockedtoequip")]
    public bool UnlockedToEquip { get; set; }

    [JsonPropertyName("@hitpointsmax")]
    public string HitPointsMax { get; set; }

    [JsonPropertyName("@hitpointsregenerationbonus")]
    public string HitPointsRegenerationBonus { get; set; }

    [JsonPropertyName("@energymax")]
    public string EnergyMax { get; set; }

    [JsonPropertyName("@energyregenerationbonus")]
    public string EnergyRegenerationBonus { get; set; }

    [JsonPropertyName("@crowdcontrolresistance")]
    public string CrowdControlResistance { get; set; }

    [JsonPropertyName("@itempower")]
    public string ItemPower { get; set; }

    [JsonPropertyName("@physicalattackdamagebonus")]
    public string PhysicalAttackDamageBonus { get; set; }

    [JsonPropertyName("@magicattackdamagebonus")]
    public string MagicAttackDamageBonus { get; set; }

    [JsonPropertyName("@physicalspelldamagebonus")]
    public string PhysicalSpellDamageBonus { get; set; }

    [JsonPropertyName("@magicspelldamagebonus")]
    public string MagicSpellDamageBonus { get; set; }

    [JsonPropertyName("@healbonus")]
    public string HealBonus { get; set; }

    [JsonPropertyName("@bonusccdurationvsplayers")]
    public string BonusCcDurationVsPlayers { get; set; }

    [JsonPropertyName("@bonusccdurationvsmobs")]
    public string BonusCcDurationVsMobs { get; set; }

    [JsonPropertyName("@threatbonus")]
    public string ThreatBonus { get; set; }

    [JsonPropertyName("@magiccooldownreduction")]
    public string MagicCooldownReduction { get; set; }

    [JsonPropertyName("@bonusdefensevsplayers")]
    public string BonusDefenseVsPlayers { get; set; }

    [JsonPropertyName("@bonusdefensevsmobs")]
    public string BonusDefenseVsMobs { get; set; }

    [JsonPropertyName("@magiccasttimereduction")]
    public string MagicCastTimeReduction { get; set; }

    [JsonPropertyName("@attackspeedbonus")]
    public string AttackSpeedBonus { get; set; }

    [JsonPropertyName("@movespeedbonus")]
    public string MoveSpeedBonus { get; set; }

    [JsonPropertyName("@healmodifier")]
    public string HealModifier { get; set; }

    [JsonPropertyName("@canbeovercharged")]
    public bool CanBeOvercharged { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public string ShowInMarketPlace { get; set; }

    [JsonPropertyName("@energycostreduction")]
    public string EnergyCostReduction { get; set; }

    [JsonPropertyName("@masterymodifier")]
    public string MasteryModifier { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonIgnore]
    public CraftingJournalType CraftingJournalType => CraftingController.GetCraftingJournalType(UniqueName, CraftingCategory);

    [JsonPropertyName("@combatspecachievement")]
    public string CombatSpecAchievement { get; set; }
    //public SocketPreset SocketPreset { get; set; }
    [JsonPropertyName("enchantments")]
    public Enchantments Enchantments { get; set; }

    [JsonPropertyName("@destinycraftfamefactor")]
    public string Destinycraftfamefactor { get; set; }
    //public AssetVfxPreset AssetVfxPreset { get; set; }
}