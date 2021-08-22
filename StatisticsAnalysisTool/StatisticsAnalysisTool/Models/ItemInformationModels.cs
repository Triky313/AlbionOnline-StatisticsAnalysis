using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using Formatting = StatisticsAnalysisTool.Common.Formatting;

namespace StatisticsAnalysisTool.Models
{
    public class ItemInformation
    {
        [JsonPropertyName("itemType")]
        public string ItemType { get; set; }

        [JsonPropertyName("uniqueName")]
        public string UniqueName { get; set; }

        [JsonPropertyName("uiSprite")]
        public string UiSprite { get; set; }

        [JsonPropertyName("uiSpriteOverlay1")]
        public object UiSpriteOverlay1 { get; set; }

        [JsonPropertyName("uiSpriteOverlay2")]
        public object UiSpriteOverlay2 { get; set; }

        [JsonPropertyName("uiSpriteOverlay3")]
        public object UiSpriteOverlay3 { get; set; }

        [JsonPropertyName("uiAtlas")]
        public object UiAtlas { get; set; }

        [JsonPropertyName("showinmarketplace")]
        public bool ShowInMarketplace { get; set; }

        [JsonPropertyName("level")] public int Level { get; set; }

        [JsonPropertyName("tier")] public int Tier { get; set; }

        [JsonPropertyName("enchantmentLevel")]
        public int EnchantmentLevel { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        [JsonPropertyName("revision")]
        public int Revision { get; set; }

        [JsonPropertyName("enchantments")]
        public object Enchantments { get; set; }

        //[JsonPropertyName("activeSlots")]
        //public ActiveSlots ActiveSlots { get; set; }
        //[JsonPropertyName("passiveSlots")]
        //public PassiveSlots PassiveSlots { get; set; }
        [JsonPropertyName("localizedNames")]
        public LocalizedNames LocalizedNames { get; set; }

        [JsonPropertyName("localizedDescriptions")]
        public LocalizedDescriptions LocalizedDescriptions { get; set; }

        [JsonPropertyName("slotType")]
        public string SlotType { get; set; }

        [JsonPropertyName("physicalAttackDamageBonus")]
        public double PhysicalAttackDamageBonus { get; set; }

        [JsonPropertyName("skinCount")] public object SkinCount { get; set; }

        [JsonPropertyName("physicalArmor")] public int PhysicalArmor { get; set; }

        [JsonPropertyName("magicResistance")] public int MagicResistance { get; set; }

        [JsonPropertyName("magicAttackDamageBonus")]
        public object MagicAttackDamageBonus { get; set; }

        [JsonPropertyName("itemPowerProgressionType")]
        public string ItemPowerProgressionType { get; set; }

        [JsonPropertyName("craftingRequirements")] public CraftingRequirements CraftingRequirements { get; set; }

        [JsonPropertyName("unlockedToEquip")]
        public bool? UnlockedToEquip { get; set; }

        [JsonPropertyName("mountHitPointsRegeneration")]
        public int? MountHitPointsRegeneration { get; set; }

        [JsonPropertyName("prefabScaling")]
        public double? PrefabScaling { get; set; }

        [JsonPropertyName("abilityPower")]
        public int? AbilityPower { get; set; }

        [JsonPropertyName("attackDamage")]
        public int? AttackDamage { get; set; }

        [JsonPropertyName("attackSpeed")]
        public double? AttackSpeed { get; set; }

        [JsonPropertyName("attackRange")]
        public double? AttackRange { get; set; }

        [JsonPropertyName("weight")]
        public double? Weight { get; set; }

        [JsonPropertyName("activeSpellSlots")]
        public int? ActiveSpellSlots { get; set; }

        [JsonPropertyName("passiveSpellSlots")]
        public int? PassiveSpellSlots { get; set; }

        [JsonPropertyName("durability")]
        public int? Durability { get; set; }

        [JsonPropertyName("durabilityLossAttack")]
        public int? DurabilityLossAttack { get; set; }

        [JsonPropertyName("durabilityLossSpelluse")]
        public int? DurabilityLossSpellUse { get; set; }

        [JsonPropertyName("durabilityLossReceivedattack")]
        public int? DurabilityLossReceivedAttack { get; set; }

        [JsonPropertyName("durabilityLossReceivedspell")]
        public int? DurabilityLossReceivedSpell { get; set; }

        [JsonPropertyName("hitpointsMax")]
        public int? HitPointsMax { get; set; }

        [JsonPropertyName("itemPower")]
        public int? ItemPower { get; set; }

        [JsonPropertyName("dismountTime")]
        public int? DismountTime { get; set; }

        [JsonPropertyName("mountHitPointsMax")]
        public int? MountHitPointsMax { get; set; }

        [JsonPropertyName("prefabName")]
        public string PrefabName { get; set; }

        [JsonPropertyName("dismountedBuff")]
        public int? DismountedBuff { get; set; }

        [JsonPropertyName("spriteName")]
        public string SpriteName { get; set; }

        [JsonPropertyName("stackable")]
        public bool Stackable { get; set; }

        [JsonPropertyName("equipable")]
        public bool Equipable { get; set; }

        [JsonPropertyName("lastUpdate")]
        public DateTime LastUpdate { get; set; }

        [JsonIgnore] public string LastFullItemInformationUpdate => Formatting.CurrentDateTimeFormat(LastUpdate) ?? string.Empty;

        public CategoryObject CategoryObject => CategoryController.GetCategory(CategoryId);
        public HttpStatusCode HttpStatus { get; set; }
    }

    public class CraftResourceList
    {
        [JsonPropertyName("uniqueName")]
        public string UniqueName { get; set; }

        [JsonPropertyName("count")] public int Count { get; set; }
    }

    public class CraftingRequirements
    {
        [JsonPropertyName("time")] public double Time { get; set; }

        [JsonPropertyName("silver")]
        public int Silver { get; set; }

        [JsonPropertyName("craftingFocus")]
        public int CraftingFocus { get; set; }

        [JsonPropertyName("craftResourceList")]
        public List<CraftResourceList> CraftResourceList { get; set; }
    }

    public class Enchantment
    {
        [JsonPropertyName("enchantmentLevel")]
        public int EnchantmentLevel { get; set; }

        [JsonPropertyName("itemPower")]
        public int ItemPower { get; set; }

        [JsonPropertyName("durability")]
        public int Durability { get; set; }

        [JsonPropertyName("craftingRequirements")]
        public CraftingRequirements CraftingRequirements { get; set; }
    }

    public class Enchantments
    {
        [JsonPropertyName("enchantmentLevel")]
        public List<Enchantment> EnchantmentsList { get; set; }
    }

    public class LocalizedNames
    {
        [JsonPropertyName("EN-US")] public string EnUs { get; set; }

        [JsonPropertyName("DE-DE")] public string DeDe { get; set; }

        [JsonPropertyName("KO-KR")] public string KoKr { get; set; }

        [JsonPropertyName("RU-RU")] public string RuRu { get; set; }

        [JsonPropertyName("PL-PL")] public string PlPl { get; set; }

        [JsonPropertyName("PT-BR")] public string PtBr { get; set; }

        [JsonPropertyName("FR-FR")] public string FrFr { get; set; }

        [JsonPropertyName("ES-ES")] public string EsEs { get; set; }

        [JsonPropertyName("ZH-CN")] public string ZhCn { get; set; }
    }

    public class LocalizedDescriptions
    {
        [JsonPropertyName("EN-US")] public string EnUs { get; set; }

        [JsonPropertyName("DE-DE")] public string DeDe { get; set; }

        [JsonPropertyName("KO-KR")] public string KoKr { get; set; }

        [JsonPropertyName("RU-RUS")]
        public string RuRu { get; set; }

        [JsonPropertyName("PL-PL")] public string PlPl { get; set; }

        [JsonPropertyName("PT-BR")] public string PtBr { get; set; }

        [JsonPropertyName("FR-FR")] public string FrFr { get; set; }

        [JsonPropertyName("ES-ES")] public string EsEs { get; set; }

        [JsonPropertyName("ZH-CN")] public string ZhCn { get; set; }
    }

    public class BuffOverTime
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("time")] public double Time { get; set; }

        [JsonPropertyName("value")] public double Value { get; set; }

        [JsonPropertyName("effectAreaRadius")]
        public object EffectAreaRadius { get; set; }
    }

    public class DirectAttributeChange
    {
        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("attribute")]
        public string Attribute { get; set; }

        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("effectType")]
        public object EffectType { get; set; }

        [JsonPropertyName("effectAreaRadius")]
        public object EffectAreaRadius { get; set; }
    }

    public class Channeling
    {
        [JsonPropertyName("channelingAnimation")]
        public string ChannelingAnimation { get; set; }

        [JsonPropertyName("initialEffectInterval")]
        public double InitialEffectInterval { get; set; }

        [JsonPropertyName("effectCount")]
        public int EffectCount { get; set; }

        [JsonPropertyName("effectInterval")]
        public double EffectInterval { get; set; }

        [JsonPropertyName("energyUsage")]
        public double EnergyUsage { get; set; }

        [JsonPropertyName("disruptionFactor")]
        public double DisruptionFactor { get; set; }

        [JsonPropertyName("directAttributeChange")]
        public DirectAttributeChange DirectAttributeChange { get; set; }

        [JsonPropertyName("buffOverTime")]
        public object BuffOverTime { get; set; }

        [JsonPropertyName("damageShield")]
        public object DamageShield { get; set; }
    }

    //public class One
    //{
    //    public string spellType { get; set; }
    //    public string uniqueName { get; set; }
    //    public object uiAtlas { get; set; }
    //    public string uiSprite { get; set; }
    //    public int revision { get; set; }
    //    public LocalizedNames localizedNames { get; set; }
    //    public LocalizedDescriptions localizedDescriptions { get; set; }
    //    public string target { get; set; }
    //    public double castingTime { get; set; }
    //    public double? hitDelay { get; set; }
    //    public double standTime { get; set; }
    //    public double? disruptionFactor { get; set; }
    //    public double recastDelay { get; set; }
    //    public double energyUsage { get; set; }
    //    public double castRange { get; set; }
    //    public string category { get; set; }
    //    public string uiType { get; set; }
    //    public object maxCharges { get; set; }
    //    public List<BuffOverTime> buffOverTime { get; set; }
    //    public object directAttributeChange { get; set; }
    //    public object root { get; set; }
    //    public object stun { get; set; }
    //    public object attributeChangeOverTime { get; set; }
    //    public object silence { get; set; }
    //    public object damageShield { get; set; }
    //    public Channeling channeling { get; set; }
    //    public object knockback { get; set; }
    //    public object spellEffectArea { get; set; }
    //    public object invisibility { get; set; }
    //}

    //public class ActiveSlots
    //{
    //    [JsonPropertyName("1")]
    //    public List<One> One { get; set; }
    //}

    public class Buff
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("value")] public double Value { get; set; }
    }

    //public class PassiveSlots
    //{
    //    public List<Two> Two { get; set; }
    //}
}