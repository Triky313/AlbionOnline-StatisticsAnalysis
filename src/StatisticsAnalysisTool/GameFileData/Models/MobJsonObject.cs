using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class MobJsonObject
{
    [JsonPropertyName("@uniquename")]
    public string UniqueName { get; set; }
    [JsonPropertyName("@tier")]
    public short Tier { get; set; }
    [JsonPropertyName("@npchostility")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NpcHostility { get; set; }
    [JsonPropertyName("@abilitypower")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AbilityPower { get; set; }
    [JsonPropertyName("@fame")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Fame { get; set; }
    [JsonPropertyName("@roamingradius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RoamingRadius { get; set; }
    [JsonPropertyName("@roamingidletimemin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RoamingIdleTimeMin { get; set; }
    [JsonPropertyName("@roamingidletimemax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RoamingIdleTimeMax { get; set; }
    [JsonPropertyName("@aggroradius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AggroRadius { get; set; }
    [JsonPropertyName("@pursuitradius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PursuitRadius { get; set; }
    [JsonPropertyName("@damageaggrofactor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? DamageAggroFactor { get; set; }
    [JsonPropertyName("@healingaggrofactor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HealingAggroFactor { get; set; }
    [JsonPropertyName("@shieldaggrofactor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShieldAggroFactor { get; set; }
    [JsonPropertyName("@alertradius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AlertRadius { get; set; }
    [JsonPropertyName("@hitpointsmax")]
    public double HitPointsMax { get; set; }
    [JsonPropertyName("@attackcollisionradius")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AttackCollisionRadius { get; set; }
    [JsonPropertyName("@attacktype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AttackType { get; set; }
    [JsonPropertyName("@attackrange")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AttackRange { get; set; }
    [JsonPropertyName("@attackdamage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AttackDamage { get; set; }
    [JsonPropertyName("@hitpointsregeneration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? HitPointsRegeneration { get; set; }
    [JsonPropertyName("@energymax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? EnergyMax { get; set; }
    [JsonPropertyName("@energyregeneration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? EnergyRegeneration { get; set; }
    [JsonPropertyName("@movespeed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MoveSpeed { get; set; }
    [JsonPropertyName("@attackmovespeed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AttackMoveSpeed { get; set; }
    [JsonPropertyName("@meleeattackdamagetime")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MeleeAttackDamageTime { get; set; }
    [JsonPropertyName("@attackspeed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AttackSpeed { get; set; }
    [JsonPropertyName("@physicalarmor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? PhysicalArmor { get; set; }
    [JsonPropertyName("@magicresistance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? MagicResistance { get; set; }
    [JsonPropertyName("@crowdcontrolresistance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? CrowdControlResistance { get; set; }
    [JsonPropertyName("@respawntimesecondsmin")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RespawnTimeSecondsMin { get; set; }
    [JsonPropertyName("@respawntimesecondsmax")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? RespawnTimeSecondsMax { get; set; }
    [JsonPropertyName("@namelocatag")]
    public string NameLocatag { get; set; } = string.Empty;
    [JsonPropertyName("@avatar")]
    public string Avatar { get; set; } = string.Empty;
    [JsonPropertyName("@faction")]
    public string Faction { get; set; } = string.Empty;
    [JsonPropertyName("@maxcharges")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxCharges { get; set; }
    [JsonPropertyName("@timeperchargeseconds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TimePerChargeSeconds { get; set; }
    [JsonPropertyName("@dangerstate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string DangerState { get; set; }
    [JsonPropertyName("@aggrodelayafterspawn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? AggroDelayAfterSpawn { get; set; }
    [JsonPropertyName("@category")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Category { get; set; }
    [JsonPropertyName("@chargesperchargeup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ChargesPerChargeUp { get; set; }
    [JsonPropertyName("@energyrewardspell")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string EnergyRewardSpell { get; set; }
    [JsonPropertyName("@energyreward")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EnergyReward { get; set; }
    [JsonPropertyName("@mobvalue")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MobValue { get; set; }
    [JsonPropertyName("@ignoredifficultybonus")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string IgnoreDifficultyBonus { get; set; }
    [JsonPropertyName("@chargeupchance")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ChargeUpChance { get; set; }
}