using System.Collections.Generic;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common.Converters;

namespace StatisticsAnalysisTool.Models.ItemsJsonModel;

public class Mount : ItemJsonObject
{
    [JsonPropertyName("@uniquename")]
    public override string UniqueName { get; set; }

    [JsonPropertyName("@mountcategory")]
    public string Mountcategory { get; set; }

    [JsonPropertyName("@maxqualitylevel")]
    public string Maxqualitylevel { get; set; }

    [JsonPropertyName("@itempower")]
    public string Itempower { get; set; }

    [JsonPropertyName("@abilitypower")]
    public string AbilityPower { get; set; }

    [JsonPropertyName("@slottype")]
    public string SlotType { get; set; }

    [JsonPropertyName("@shopcategory")]
    public string ShopCategory { get; set; }

    [JsonPropertyName("@shopsubcategory1")]
    public string ShopSubCategory1 { get; set; }

    [JsonPropertyName("@mountedbuff")]
    public string MountedBuff { get; set; }

    [JsonPropertyName("@halfmountedbuff")]
    public string HalfMountedBuff { get; set; }

    [JsonPropertyName("@tier")]
    public string Tier { get; set; }

    [JsonPropertyName("@weight")]
    public string Weight { get; set; }

    [JsonPropertyName("@activespellslots")]
    public string ActiveSpellSlots { get; set; }

    [JsonPropertyName("@passivespellslots")]
    public string PassiveSpellSlots { get; set; }

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

    [JsonPropertyName("@durabilityloss_receivedattack_mounted")]
    public string DurabilityLossReceivedAttackMounted { get; set; }

    [JsonPropertyName("@durabilityloss_receivedspell_mounted")]
    public string DurabilityLossReceivedSpellMounted { get; set; }

    [JsonPropertyName("@durabilityloss_mounting")]
    public string DurabilityLossMounting { get; set; }

    [JsonPropertyName("@unlockedtocraft")]
    public string UnlockedToCraft { get; set; }

    [JsonPropertyName("@unlockedtoequip")]
    public string UnlockedToEquip { get; set; }

    [JsonPropertyName("@mounttime")]
    public string MountTime { get; set; }

    [JsonPropertyName("@dismounttime")]
    public string DismountTime { get; set; }

    [JsonPropertyName("@mounthitpointsmax")]
    public string MountHitPointsMax { get; set; }

    [JsonPropertyName("@mounthitpointsregeneration")]
    public string MountHitPointsRegeneration { get; set; }

    //[JsonPropertyName("@prefabname")]
    //public string PrefabName { get; set; }

    //[JsonPropertyName("@prefabscaling")]
    //public string Prefabscaling { get; set; }

    //[JsonPropertyName("@despawneffect")]
    //public string DespawnEffect { get; set; }

    //[JsonPropertyName("@despawneffectscaling")]
    //public string DespawnEffectScaling { get; set; }

    [JsonPropertyName("@remountdistance")]
    public string RemountDistance { get; set; }

    [JsonPropertyName("@halfmountrange")]
    public string HalfMountRange { get; set; }

    [JsonPropertyName("@forceddismountcooldown")]
    public string ForcedDismountCooldown { get; set; }

    [JsonPropertyName("@forceddismountspellcooldown")]
    public string ForcedDismountSpellCooldown { get; set; }

    [JsonPropertyName("@fulldismountcooldown")]
    public string FullDismountCooldown { get; set; }

    [JsonPropertyName("@remounttime")]
    public string RemountTime { get; set; }

    //[JsonPropertyName("@uicraftsoundstart")]
    //public string Uicraftsoundstart { get; set; }

    //[JsonPropertyName("@uicraftsoundfinish")]
    //public string Uicraftsoundfinish { get; set; }

    [JsonPropertyName("@dismountbuff")]
    public string DismountBuff { get; set; }

    [JsonPropertyName("@forceddismountbuff")]
    public string ForcedDismountBuff { get; set; }

    [JsonPropertyName("@hostiledismountbuff")]
    public string Hostiledismountbuff { get; set; }

    [JsonPropertyName("@showinmarketplace")]
    public bool ShowInMarketPlace { get; set; }

    [JsonPropertyName("@hidefromplayer")]
    public string HideFromPlayer { get; set; }

    [JsonConverter(typeof(CraftingRequirementsToCraftingRequirementsList))]
    [JsonPropertyName("craftingrequirements")]
    public List<CraftingRequirements> CraftingRequirements { get; set; }

    [JsonPropertyName("craftingspelllist")]
    public CraftingSpellList CraftingSpellList { get; set; }
    //public SocketPreset SocketPreset { get; set; }
    //public AudioInfo AudioInfo { get; set; }
    //public AssetVfxPreset AssetVfxPreset { get; set; }
    //public FootStepVfxPreset FootStepVfxPreset { get; set; }

    [JsonPropertyName("@uisprite")]
    public string UiSprite { get; set; }

    [JsonPropertyName("@nametagoffset")]
    public string NameTagOffset { get; set; }

    [JsonPropertyName("@canuseingvg")]
    public string CanUseinGvg { get; set; }
    [JsonPropertyName("mountspelllist")]
    public MountSpellList MountSpellList { get; set; }

    [JsonPropertyName("@enchantmentlevel")]
    public string EnchantmentLevel { get; set; }

    //[JsonPropertyName("@vfxAddonKeyword")]
    //public string VfxAddonKeyword { get; set; }

    [JsonPropertyName("@canuseinfactionwarfare")]
    public string CanUseInFactionWarfare { get; set; }

    [JsonPropertyName("@itemvalue")]
    public string ItemValue { get; set; }
}