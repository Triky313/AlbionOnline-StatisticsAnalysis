using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Net;

namespace StatisticsAnalysisTool.Models
{
    public class ItemInformation
    {
        [JsonProperty(PropertyName = "itemType")]
        public string ItemType { get; set; }
        [JsonProperty(PropertyName = "uniqueName")]
        public string UniqueName { get; set; }
        [JsonProperty(PropertyName = "uiSprite")]
        public string UiSprite { get; set; }
        [JsonProperty(PropertyName = "uiSpriteOverlay1")]
        public object UiSpriteOverlay1 { get; set; }
        [JsonProperty(PropertyName = "uiSpriteOverlay2")]
        public object UiSpriteOverlay2 { get; set; }
        [JsonProperty(PropertyName = "uiSpriteOverlay3")]
        public object UiSpriteOverlay3 { get; set; }
        [JsonProperty(PropertyName = "uiAtlas")]
        public object UiAtlas { get; set; }
        [JsonProperty(PropertyName = "showinmarketplace")]
        public bool ShowInMarketplace { get; set; }
        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }
        [JsonProperty(PropertyName = "tier")]
        public int Tier { get; set; }
        [JsonProperty(PropertyName = "enchantmentLevel")]
        public int EnchantmentLevel { get; set; }
        [JsonProperty(PropertyName = "categoryId")]
        public string CategoryId { get; set; }
        [JsonProperty(PropertyName = "categoryName")]
        public string CategoryName { get; set; }
        [JsonProperty(PropertyName = "revision")]
        public int Revision { get; set; }
        [JsonProperty(PropertyName = "enchantments")]
        public object Enchantments { get; set; }
        //[JsonProperty(PropertyName = "activeSlots")]
        //public ActiveSlots ActiveSlots { get; set; }
        //[JsonProperty(PropertyName = "passiveSlots")]
        //public PassiveSlots PassiveSlots { get; set; }
        [JsonProperty(PropertyName = "localizedNames")]
        public LocalizedNames LocalizedNames { get; set; }
        [JsonProperty(PropertyName = "localizedDescriptions")]
        public LocalizedDescriptions LocalizedDescriptions { get; set; }
        [JsonProperty(PropertyName = "slotType")]
        public object SlotType { get; set; }
        [JsonProperty("physicalAttackDamageBonus")]
        public double PhysicalAttackDamageBonus { get; set; }
        [JsonProperty("skinCount")]
        public object SkinCount { get; set; }
        [JsonProperty("physicalArmor")]
        public int PhysicalArmor { get; set; }
        [JsonProperty("magicResistance")]
        public int MagicResistance { get; set; }
        [JsonProperty("magicAttackDamageBonus")]
        public object MagicAttackDamageBonus { get; set; }
        [JsonProperty("itemPowerProgressionType")]
        public string ItemPowerProgressionType { get; set; }
        [JsonProperty("craftingRequirements")]
        public CraftingRequirements CraftingRequirements { get; set; }
        [JsonProperty(PropertyName = "unlockedToEquip")]
        public bool? UnlockedToEquip { get; set; }
        [JsonProperty(PropertyName = "mountHitPointsRegeneration")]
        public int? MountHitPointsRegeneration { get; set; }
        [JsonProperty(PropertyName = "prefabScaling")]
        public double? PrefabScaling { get; set; }
        [JsonProperty(PropertyName = "abilityPower")]
        public int? AbilityPower { get; set; }


        [JsonProperty(PropertyName = "attackDamage")]
        public int? AttackDamage { get; set; }
        [JsonProperty(PropertyName = "attackSpeed")]
        public double? AttackSpeed { get; set; }
        [JsonProperty(PropertyName = "attackRange")]
        public double? AttackRange { get; set; }
        [JsonProperty(PropertyName = "weight")]
        public double? Weight { get; set; }
        [JsonProperty(PropertyName = "activeSpellSlots")]
        public int? ActiveSpellSlots { get; set; }        
        [JsonProperty(PropertyName = "passiveSpellSlots")]
        public int? PassiveSpellSlots { get; set; }
        [JsonProperty(PropertyName = "durability")]
        public int? Durability { get; set; }
        [JsonProperty(PropertyName = "durabilityLossAttack")]
        public int? DurabilityLossAttack { get; set; }
        [JsonProperty(PropertyName = "durabilityLossSpelluse")]
        public int? DurabilityLossSpellUse { get; set; }
        [JsonProperty(PropertyName = "durabilityLossReceivedattack")]
        public int? DurabilityLossReceivedAttack { get; set; }
        [JsonProperty(PropertyName = "durabilityLossReceivedspell")]
        public int? DurabilityLossReceivedSpell { get; set; }
        [JsonProperty(PropertyName = "hitpointsMax")]
        public int? HitPointsMax { get; set; }
        [JsonProperty(PropertyName = "itemPower")]
        public int? ItemPower { get; set; }
        [JsonProperty(PropertyName = "dismountTime")]
        public int? DismountTime { get; set; }
        [JsonProperty(PropertyName = "mountHitPointsMax")]
        public int? MountHitPointsMax { get; set; }
        [JsonProperty(PropertyName = "prefabName")]
        public string PrefabName { get; set; }
        [JsonProperty(PropertyName = "dismountedBuff")]
        public int? DismountedBuff { get; set; }
        [JsonProperty(PropertyName = "spriteName")]
        public string SpriteName { get; set; }
        [JsonProperty(PropertyName = "stackable")]
        public bool Stackable { get; set; }
        [JsonProperty(PropertyName = "equipable")]
        public bool Equipable { get; set; }

        [JsonProperty(PropertyName = "lastUpdate")]
        public DateTime LastUpdate { get; set; }

        public string LastFullItemInformationUpdate => Common.Formatting.CurrentDateTimeFormat(LastUpdate) ?? string.Empty;

        public CategoryObject CategoryObject => CategoryController.GetCategory(CategoryId);
        public HttpStatusCode HttpStatus { get; set; }
    }

    public class CraftResourceList
    {
        [JsonProperty(PropertyName = "uniqueName")]
        public string UniqueName { get; set; }
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }
    }

    public class CraftingRequirements
    {
        [JsonProperty(PropertyName = "time")]
        public double Time { get; set; }
        [JsonProperty(PropertyName = "silver")]
        public int Silver { get; set; }
        [JsonProperty(PropertyName = "craftingFocus")]
        public int CraftingFocus { get; set; }
        [JsonProperty(PropertyName = "craftResourceList")]
        public List<CraftResourceList> CraftResourceList { get; set; }
    }

    public class Enchantment
    {
        [JsonProperty(PropertyName = "enchantmentLevel")]
        public int EnchantmentLevel { get; set; }
        [JsonProperty(PropertyName = "itemPower")]
        public int ItemPower { get; set; }
        [JsonProperty(PropertyName = "durability")]
        public int Durability { get; set; }
        [JsonProperty(PropertyName = "craftingRequirements")]
        public CraftingRequirements CraftingRequirements { get; set; }
    }

    public class Enchantments
    {
        [JsonProperty(PropertyName = "enchantmentLevel")]
        public List<Enchantment> EnchantmentsList { get; set; }
    }

    public class LocalizedNames
    {
        [JsonProperty(PropertyName = "EN-US")]
        public string EnUs { get; set; }
        [JsonProperty(PropertyName = "DE-DE")]
        public string DeDe { get; set; }
        [JsonProperty(PropertyName = "KO-KR")]
        public string KoKr { get; set; }
        [JsonProperty(PropertyName = "RU-RU")]
        public string RuRu { get; set; }
        [JsonProperty(PropertyName = "PL-PL")]
        public string PlPl { get; set; }
        [JsonProperty(PropertyName = "PT-BR")]
        public string PtBr { get; set; }
        [JsonProperty(PropertyName = "FR-FR")]
        public string FrFr { get; set; }
        [JsonProperty(PropertyName = "ES-ES")]
        public string EsEs { get; set; }
        [JsonProperty(PropertyName = "ZH-CN")]
        public string ZhCn { get; set; }
    }

    public class LocalizedDescriptions
    {
        [JsonProperty(PropertyName = "EN-US")]
        public string EnUs { get; set; }
        [JsonProperty(PropertyName = "DE-DE")]
        public string DeDe { get; set; }
        [JsonProperty(PropertyName = "KO-KR")]
        public string KoKr { get; set; }
        [JsonProperty(PropertyName = "RU-RUS")]
        public string RuRu { get; set; }
        [JsonProperty(PropertyName = "PL-PL")]
        public string PlPl { get; set; }
        [JsonProperty(PropertyName = "PT-BR")]
        public string PtBr { get; set; }
        [JsonProperty(PropertyName = "FR-FR")]
        public string FrFr { get; set; }
        [JsonProperty(PropertyName = "ES-ES")]
        public string EsEs { get; set; }
        [JsonProperty(PropertyName = "ZH-CN")]
        public string ZhCn { get; set; }
    }

    public class BuffOverTime
    {
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "time")]
        public double Time { get; set; }
        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }
        [JsonProperty(PropertyName = "effectAreaRadius")]
        public object EffectAreaRadius { get; set; }
    }

    public class DirectAttributeChange
    {
        [JsonProperty(PropertyName = "target")]
        public string Target { get; set; }
        [JsonProperty(PropertyName = "attribute")]
        public string Attribute { get; set; }
        [JsonProperty(PropertyName = "change")]
        public double Change { get; set; }
        [JsonProperty(PropertyName = "effectType")]
        public object EffectType { get; set; }
        [JsonProperty(PropertyName = "effectAreaRadius")]
        public object EffectAreaRadius { get; set; }
    }

    public class Channeling
    {
        [JsonProperty(PropertyName = "channelingAnimation")]
        public string ChannelingAnimation { get; set; }
        [JsonProperty(PropertyName = "initialEffectInterval")]
        public double InitialEffectInterval { get; set; }
        [JsonProperty(PropertyName = "effectCount")]
        public int EffectCount { get; set; }
        [JsonProperty(PropertyName = "effectInterval")]
        public double EffectInterval { get; set; }
        [JsonProperty(PropertyName = "energyUsage")]
        public double EnergyUsage { get; set; }
        [JsonProperty(PropertyName = "disruptionFactor")]
        public double DisruptionFactor { get; set; }
        [JsonProperty(PropertyName = "directAttributeChange")]
        public DirectAttributeChange DirectAttributeChange { get; set; }
        [JsonProperty(PropertyName = "buffOverTime")]
        public object BuffOverTime { get; set; }
        [JsonProperty(PropertyName = "damageShield")]
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
    //    [JsonProperty(PropertyName = "1")]
    //    public List<One> One { get; set; }
    //}
    
    public class Buff
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; }
    }

    //public class PassiveSlots
    //{
    //    public List<Two> Two { get; set; }
    //}
}