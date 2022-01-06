using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class EquipmentItem
{
    [JsonProperty("@uniquename")]
    public string UniqueName { get; set; }

    [JsonProperty("@uisprite")]
    public string UiSprite { get; set; }

    [JsonProperty("@maxqualitylevel")]
    public string MaxQualityLevel { get; set; }

    [JsonProperty("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonProperty("@slottype")]
    public string Slottype { get; set; }

    [JsonProperty("@itempowerprogressiontype")]
    public string ItemPowerProgressionType { get; set; }

    [JsonProperty("@ShopCategory")]
    public string ShopCategory { get; set; }

    [JsonProperty("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    //[JsonProperty("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonProperty("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonProperty("@skincount")]
    public string SkinCount { get; set; }

    [JsonProperty("@tier")]
    public string Tier { get; set; }

    [JsonProperty("@weight")]
    public string Weight { get; set; }

    [JsonProperty("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonProperty("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

    [JsonProperty("@physicalarmor")]
    public string PhysicalArmor { get; set; }

    [JsonProperty("@magicresistance")]
    public string MagicResistance { get; set; }

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

    [JsonProperty("@offhandanimationtype")]
    public string OffHandAnimationType { get; set; }

    [JsonProperty("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonProperty("@unlockedtoequip")]
    public string UnlockedToEquip { get; set; }

    [JsonProperty("@hitpointsmax")]
    public string HitPointsMax { get; set; }

    [JsonProperty("@hitpointsregenerationbonus")]
    public string HitPointsRegenerationBonus { get; set; }

    [JsonProperty("@energymax")]
    public string EnergyMax { get; set; }

    [JsonProperty("@energyregenerationbonus")]
    public string EnergyRegenerationBonus { get; set; }

    [JsonProperty("@crowdcontrolresistance")]
    public string CrowdControlResistance { get; set; }

    [JsonProperty("@itempower")]
    public string ItemPower { get; set; }

    [JsonProperty("@physicalattackdamagebonus")]
    public string PhysicalAttackDamageBonus { get; set; }

    [JsonProperty("@magicattackdamagebonus")]
    public string MagicAttackDamageBonus { get; set; }

    [JsonProperty("@physicalspelldamagebonus")]
    public string PhysicalSpellDamageBonus { get; set; }

    [JsonProperty("@magicspelldamagebonus")]
    public string MagicSpellDamageBonus { get; set; }

    [JsonProperty("@healbonus")]
    public string HealBonus { get; set; }

    [JsonProperty("@bonusccdurationvsplayers")]
    public string BonusCcDurationVsPlayers { get; set; }

    [JsonProperty("@bonusccdurationvsmobs")]
    public string BonusCcDurationVsMobs { get; set; }

    [JsonProperty("@threatbonus")]
    public string ThreatBonus { get; set; }

    [JsonProperty("@magiccooldownreduction")]
    public string MagicCooldownReduction { get; set; }

    [JsonProperty("@bonusdefensevsplayers")]
    public string BonusDefenseVsPlayers { get; set; }

    [JsonProperty("@bonusdefensevsmobs")]
    public string BonusDefenseVsMobs { get; set; }

    [JsonProperty("@magiccasttimereduction")]
    public string MagicCastTimeReduction { get; set; }

    [JsonProperty("@attackspeedbonus")]
    public string AttackSpeedBonus { get; set; }

    [JsonProperty("@movespeedbonus")]
    public string MoveSpeedBonus { get; set; }

    [JsonProperty("@healmodifier")]
    public string HealModifier { get; set; }

    [JsonProperty("@canbeovercharged")]
    public string CanBeOvercharged { get; set; }

    [JsonProperty("@showinmarketplace")]
    public string ShowInMarketPlace { get; set; }

    [JsonProperty("@energycostreduction")]
    public string EnergyCostReduction { get; set; }

    [JsonProperty("@masterymodifier")]
    public string MasteryModifier { get; set; }
    [JsonProperty("craftingrequirements")]
    public object CraftingRequirements { get; set; }

    [JsonProperty("@craftingcategory")]
    public string CraftingCategory { get; set; }

    [JsonProperty("@combatspecachievement")]
    public string CombatSpecAchievement { get; set; }
    //public SocketPreset SocketPreset { get; set; }
    [JsonProperty("enchantments")]
    public Enchantments Enchantments { get; set; }

    [JsonProperty("@destinycraftfamefactor")]
    public string Destinycraftfamefactor { get; set; }
    //public AssetVfxPreset AssetVfxPreset { get; set; }
}