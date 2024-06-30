using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public static class CategoryController
{
    public static readonly List<CategoryObject> SubCategories = new()
    {
        new ("bag", ShopSubCategory.Bag, ShopCategory.Accessories),
        new ("cape", ShopSubCategory.Cape, ShopCategory.Accessories),

        new ("cloth_armor", ShopSubCategory.ClothArmor, ShopCategory.Armor),
        new ("cloth_helmet", ShopSubCategory.ClothHelmet, ShopCategory.Armor),
        new ("cloth_shoes", ShopSubCategory.ClothShoes, ShopCategory.Armor),
        new ("leather_armor", ShopSubCategory.LeatherArmor, ShopCategory.Armor),
        new ("leather_helmet", ShopSubCategory.LeatherHelmet, ShopCategory.Armor),
        new ("leather_shoes", ShopSubCategory.LeatherShoes, ShopCategory.Armor),
        new ("plate_armor", ShopSubCategory.PlateArmor, ShopCategory.Armor),
        new ("plate_helmet", ShopSubCategory.PlateHelmet, ShopCategory.Armor),
        new ("plate_shoes", ShopSubCategory.PlateShoes, ShopCategory.Armor),
        new ("unique_armor", ShopSubCategory.UniqueArmor, ShopCategory.Armor),
        new ("unique_helmet", ShopSubCategory.UniqueHelmet, ShopCategory.Armor),
        new ("unique_shoes", ShopSubCategory.UniqueShoes, ShopCategory.Armor),

        new ("armor_artefact", ShopSubCategory.ArmorArtefact, ShopCategory.Artifact),
        new ("magic_artefact", ShopSubCategory.MagicArtefact, ShopCategory.Artifact),
        new ("melee_artefact", ShopSubCategory.MeleeArtefact, ShopCategory.Artifact),
        new ("offhand_artefact", ShopSubCategory.OffhandArtefact, ShopCategory.Artifact),
        new ("ranged_artefact", ShopSubCategory.RangedArtefact, ShopCategory.Artifact),

        new ("beastheart", ShopSubCategory.BeastHeart, ShopCategory.CityResources),
        new ("mountainheart", ShopSubCategory.MountainHeart, ShopCategory.CityResources),
        new ("rockheart", ShopSubCategory.RockHeart, ShopCategory.CityResources),
        new ("treeheart", ShopSubCategory.TreeHeart, ShopCategory.CityResources),
        new ("vineheart", ShopSubCategory.VineHeart, ShopCategory.CityResources),

        new ("cooked", ShopSubCategory.Cooked, ShopCategory.Consumables),
        new ("fish", ShopSubCategory.Fish, ShopCategory.Consumables),
        new ("fishingbait", ShopSubCategory.FishingBait, ShopCategory.Consumables),
        new ("maps", ShopSubCategory.Maps, ShopCategory.Consumables),
        new ("Other", ShopSubCategory.Other, ShopCategory.Consumables),
        new ("potion", ShopSubCategory.Potion, ShopCategory.Consumables),
        new ("skillbook", ShopSubCategory.SkillBook, ShopCategory.Consumables),
        new ("vanity", ShopSubCategory.Vanity, ShopCategory.Consumables),

        new ("animals", ShopSubCategory.Animals, ShopCategory.Farmable),
        new ("seed", ShopSubCategory.Seed, ShopCategory.Farmable),

        new ("banner", ShopSubCategory.Banner, ShopCategory.Furniture),
        new ("bed", ShopSubCategory.Bed, ShopCategory.Furniture),
        new ("chest", ShopSubCategory.Chest, ShopCategory.Furniture),
        new ("decoration_furniture", ShopSubCategory.DecorationFurniture, ShopCategory.Furniture),
        new ("flag", ShopSubCategory.Flag, ShopCategory.Furniture),
        new ("heretic_furniture", ShopSubCategory.HereticFurniture, ShopCategory.Furniture),
        new ("keeper_furniture", ShopSubCategory.KeeperFurniture, ShopCategory.Furniture),
        new ("morgana_furniture", ShopSubCategory.MorganaFurniture, ShopCategory.Furniture),
        new ("table", ShopSubCategory.Table, ShopCategory.Furniture),
        new ("repairkit", ShopSubCategory.RepairKit, ShopCategory.Furniture),
        new ("unique", ShopSubCategory.Unique, ShopCategory.Furniture),

        new ("fibergatherer_armor", ShopSubCategory.FibergathererArmor, ShopCategory.GatheringGear),
        new ("fibergatherer_helmet", ShopSubCategory.FibergathererHelmet, ShopCategory.GatheringGear),
        new ("fibergatherer_shoes", ShopSubCategory.FibergathererShoes, ShopCategory.GatheringGear),
        new ("fibergatherer_backpack", ShopSubCategory.FibergathererBackpack, ShopCategory.GatheringGear),

        new ("fishgatherer_armor", ShopSubCategory.FishgathererArmor, ShopCategory.GatheringGear),
        new ("fishgatherer_helmet", ShopSubCategory.FishgathererHelmet, ShopCategory.GatheringGear),
        new ("fishgatherer_shoes", ShopSubCategory.FishgathererShoes, ShopCategory.GatheringGear),
        new ("fishgatherer_backpack", ShopSubCategory.FishgathererBackpack, ShopCategory.GatheringGear),

        new ("hidegatherer_armor", ShopSubCategory.HidegathererArmor, ShopCategory.GatheringGear),
        new ("hidegatherer_helmet", ShopSubCategory.HidegathererHelmet, ShopCategory.GatheringGear),
        new ("hidegatherer_shoes", ShopSubCategory.HidegathererShoes, ShopCategory.GatheringGear),
        new ("hidegatherer_backpack", ShopSubCategory.HidegathererBackpack, ShopCategory.GatheringGear),

        new ("oregatherer_armor", ShopSubCategory.OregathererArmor, ShopCategory.GatheringGear),
        new ("oregatherer_helmet", ShopSubCategory.OregathererHelmet, ShopCategory.GatheringGear),
        new ("oregatherer_shoes", ShopSubCategory.OregathererShoes, ShopCategory.GatheringGear),
        new ("oregatherer_backpack", ShopSubCategory.OregathererBackpack, ShopCategory.GatheringGear),

        new ("rockgatherer_armor", ShopSubCategory.RockgathererArmor, ShopCategory.GatheringGear),
        new ("rockgatherer_helmet", ShopSubCategory.RockgathererHelmet, ShopCategory.GatheringGear),
        new ("rockgatherer_shoes", ShopSubCategory.RockgathererShoes, ShopCategory.GatheringGear),
        new ("rockgatherer_backpack", ShopSubCategory.RockgathererBackpack, ShopCategory.GatheringGear),

        new ("woodgatherer_armor", ShopSubCategory.WoodgathererArmor, ShopCategory.GatheringGear),
        new ("woodgatherer_helmet", ShopSubCategory.WoodgathererHelmet, ShopCategory.GatheringGear),
        new ("woodgatherer_shoes", ShopSubCategory.WoodgathererShoes, ShopCategory.GatheringGear),
        new ("woodgatherer_backpack", ShopSubCategory.WoodgathererBackpack, ShopCategory.GatheringGear),

        new ("bridgewatch", ShopSubCategory.Bridgewatch, ShopCategory.LuxuryGoods),
        new ("caerleon", ShopSubCategory.Caerleon, ShopCategory.LuxuryGoods),
        new ("fortsterling", ShopSubCategory.FortSterling, ShopCategory.LuxuryGoods),
        new ("lymhurst", ShopSubCategory.Lymhurst, ShopCategory.LuxuryGoods),
        new ("martlock", ShopSubCategory.Martlock, ShopCategory.LuxuryGoods),
        new ("thetford", ShopSubCategory.Thetford, ShopCategory.LuxuryGoods),

        new ("arcanestaff", ShopSubCategory.ArcaneStaff, ShopCategory.Magic),
        new ("cursestaff", ShopSubCategory.CurseStaff, ShopCategory.Magic),
        new ("firestaff", ShopSubCategory.FireStaff, ShopCategory.Magic),
        new ("froststaff", ShopSubCategory.FrostStaff, ShopCategory.Magic),
        new ("holystaff", ShopSubCategory.HolyStaff, ShopCategory.Magic),
        new ("naturestaff", ShopSubCategory.NatureStaff, ShopCategory.Magic),
        new ("shapeshifterstaff", ShopSubCategory.ShapeShifterStaff, ShopCategory.Magic),

        new ("essence", ShopSubCategory.Essence, ShopCategory.Materials),
        new ("other", ShopSubCategory.OtherMaterials, ShopCategory.Materials),
        new ("relic", ShopSubCategory.Relic, ShopCategory.Materials),
        new ("rune", ShopSubCategory.Rune, ShopCategory.Materials),
        new ("soul", ShopSubCategory.Soul, ShopCategory.Materials),

        new ("axe", ShopSubCategory.Axe, ShopCategory.Melee),
        new ("dagger", ShopSubCategory.Dagger, ShopCategory.Melee),
        new ("hammer", ShopSubCategory.Hammer, ShopCategory.Melee),
        new ("mace", ShopSubCategory.Mace, ShopCategory.Melee),
        new ("quarterstaff", ShopSubCategory.QuarterStaff, ShopCategory.Melee),
        new ("spear", ShopSubCategory.Spear, ShopCategory.Melee),
        new ("sword", ShopSubCategory.Sword, ShopCategory.Melee),
        new ("knuckles", ShopSubCategory.Knuckles, ShopCategory.Melee),

        new ("armoredhorse", ShopSubCategory.ArmoredHorse, ShopCategory.Mounts),
        new ("ox", ShopSubCategory.Ox, ShopCategory.Mounts),
        new ("rare_mount", ShopSubCategory.RareMount, ShopCategory.Mounts),
        new ("ridinghorse", ShopSubCategory.RidingHorse, ShopCategory.Mounts),

        new ("book", ShopSubCategory.Book, ShopCategory.OffHand),
        new ("horn", ShopSubCategory.Horn, ShopCategory.OffHand),
        new ("orb", ShopSubCategory.Orb, ShopCategory.OffHand),
        new ("shield", ShopSubCategory.Shield, ShopCategory.OffHand),
        new ("torch", ShopSubCategory.Torch, ShopCategory.OffHand),
        new ("totem", ShopSubCategory.Totem, ShopCategory.OffHand),

        new ("trash", ShopSubCategory.Trash, ShopCategory.Other),
        new ("farming", ShopSubCategory.Farming, ShopCategory.Products),
        new ("journal", ShopSubCategory.Journal, ShopCategory.Products),

        new ("bow", ShopSubCategory.Bow, ShopCategory.Ranged),
        new ("crossbow", ShopSubCategory.Crossbow, ShopCategory.Ranged),

        new ("cloth", ShopSubCategory.Cloth, ShopCategory.Resources),
        new ("fiber", ShopSubCategory.Fiber, ShopCategory.Resources),
        new ("hide", ShopSubCategory.Hide, ShopCategory.Resources),
        new ("leather", ShopSubCategory.Leather, ShopCategory.Resources),
        new ("metalbar", ShopSubCategory.Metalbar, ShopCategory.Resources),
        new ("ore", ShopSubCategory.Ore, ShopCategory.Resources),
        new ("wood", ShopSubCategory.Wood, ShopCategory.Resources),
        new ("planks", ShopSubCategory.Planks, ShopCategory.Resources),
        new ("rock", ShopSubCategory.Rock, ShopCategory.Resources),
        new ("stoneblock", ShopSubCategory.Stoneblock, ShopCategory.Resources),

        new ("arenasigils", ShopSubCategory.ArenaSigils, ShopCategory.Token),
        new ("event", ShopSubCategory.Event, ShopCategory.Token),
        new ("royalsigils", ShopSubCategory.RoyalSigils, ShopCategory.Token),

        new ("demolitionhammer", ShopSubCategory.DemolitionHammer, ShopCategory.Tools),
        new ("fishing", ShopSubCategory.Fishing, ShopCategory.Tools),
        new ("pickaxe", ShopSubCategory.Pickaxe, ShopCategory.Tools),
        new ("sickle", ShopSubCategory.Sickle, ShopCategory.Tools),
        new ("skinningknife", ShopSubCategory.SkinningKnife, ShopCategory.Tools),
        new ("stonehammer", ShopSubCategory.StoneHammer, ShopCategory.Tools),
        new ("woodaxe", ShopSubCategory.WoodAxe, ShopCategory.Tools),
        new ("trackingtool", ShopSubCategory.TrackingTool, ShopCategory.Tools),

        new ("fibertrophy", ShopSubCategory.FiberTrophy, ShopCategory.Trophies),
        new ("fishtrophy", ShopSubCategory.FishTrophy, ShopCategory.Trophies),
        new ("generaltrophy", ShopSubCategory.GeneralTrophy, ShopCategory.Trophies),
        new ("hidetrophy", ShopSubCategory.HideTrophy, ShopCategory.Trophies),
        new ("mercenarytrophy", ShopSubCategory.MercenaryTrophy, ShopCategory.Trophies),
        new ("oretrophy", ShopSubCategory.OreTrophy, ShopCategory.Trophies),
        new ("rocktrophy", ShopSubCategory.RockTrophy, ShopCategory.Trophies),
        new ("woodtrophy", ShopSubCategory.WoodTrophy, ShopCategory.Trophies)
    };

    public static readonly Dictionary<ShopSubCategory, string> SubCategoryNames = new()
    {
        {ShopSubCategory.Unknown, LocalizationController.Translation("UNKNOWN")},

        #region Accessories

        {ShopSubCategory.Bag, LocalizationController.Translation("BAG")},
        {ShopSubCategory.Cape, LocalizationController.Translation("CAPE")},

        #endregion Accessories

        #region Armor

        {ShopSubCategory.ClothArmor, LocalizationController.Translation("CLOTH_ARMOR")},
        {ShopSubCategory.ClothHelmet, LocalizationController.Translation("CLOTH_HELMET")},
        {ShopSubCategory.ClothShoes, LocalizationController.Translation("CLOTH_SHOES")},
        {ShopSubCategory.LeatherArmor, LocalizationController.Translation("LEATHER_ARMOR")},
        {ShopSubCategory.LeatherHelmet, LocalizationController.Translation("LEATHER_HELMET")},
        {ShopSubCategory.LeatherShoes, LocalizationController.Translation("LEATHER_SHOES")},
        {ShopSubCategory.PlateArmor, LocalizationController.Translation("PLATE_ARMOR")},
        {ShopSubCategory.PlateHelmet, LocalizationController.Translation("PLATE_HELMET")},
        {ShopSubCategory.PlateShoes, LocalizationController.Translation("PLATE_SHOES")},
        {ShopSubCategory.UniqueArmor, LocalizationController.Translation("UNIQUE_ARMOR")},
        {ShopSubCategory.UniqueHelmet, LocalizationController.Translation("UNIQUE_HELMET")},
        {ShopSubCategory.UniqueShoes, LocalizationController.Translation("UNIQUE_SHOES")},

        #endregion Armor

        #region Artifact

        {ShopSubCategory.ArmorArtefact, LocalizationController.Translation("ARMOR_ARTEFACT")},
        {ShopSubCategory.MagicArtefact, LocalizationController.Translation("MAGIC_ARTEFACT")},
        {ShopSubCategory.MeleeArtefact, LocalizationController.Translation("MELEE_ARTEFACT")},
        {ShopSubCategory.OffhandArtefact, LocalizationController.Translation("OFFHAND_ARTEFACT")},
        {ShopSubCategory.RangedArtefact, LocalizationController.Translation("RANGED_ARTEFACT")},

        #endregion Artifact

        #region CityResources

        {ShopSubCategory.BeastHeart, LocalizationController.Translation("BEASTHEART")},
        {ShopSubCategory.MountainHeart, LocalizationController.Translation("MOUNTAINHEART")},
        {ShopSubCategory.RockHeart, LocalizationController.Translation("ROCKHEART")},
        {ShopSubCategory.TreeHeart, LocalizationController.Translation("TREEHEART")},
        {ShopSubCategory.VineHeart, LocalizationController.Translation("VINEHEART")},

        #endregion CityResources

        #region Consumable

        {ShopSubCategory.Cooked, LocalizationController.Translation("COOKED")},
        {ShopSubCategory.Fish, LocalizationController.Translation("FISH")},
        {ShopSubCategory.FishingBait, LocalizationController.Translation("FISHING_BAIT")},
        {ShopSubCategory.Maps, LocalizationController.Translation("MAPS")},
        {ShopSubCategory.Other, LocalizationController.Translation("OTHER")},
        {ShopSubCategory.Potion, LocalizationController.Translation("POTION")},
        {ShopSubCategory.SkillBook, LocalizationController.Translation("SKILL_BOOK")},
        {ShopSubCategory.Vanity, LocalizationController.Translation("VANITY")},

        #endregion Consumable

        #region Farmable

        {ShopSubCategory.Animals, LocalizationController.Translation("ANIMALS")},
        {ShopSubCategory.Seed, LocalizationController.Translation("SEED")},

        #endregion Farmable

        #region Furniture

        {ShopSubCategory.Banner, LocalizationController.Translation("BANNER")},
        {ShopSubCategory.Bed, LocalizationController.Translation("BED")},
        {ShopSubCategory.Chest, LocalizationController.Translation("CHEST")},
        {ShopSubCategory.DecorationFurniture, LocalizationController.Translation("DECORATION_FURNITURE")},
        {ShopSubCategory.Flag, LocalizationController.Translation("FLAG")},
        {ShopSubCategory.HereticFurniture, LocalizationController.Translation("HERETIC_FURNITURE")},
        {ShopSubCategory.KeeperFurniture, LocalizationController.Translation("KEEPER_FURNITURE")},
        {ShopSubCategory.MorganaFurniture, LocalizationController.Translation("MORGANA_FURNITURE")},
        {ShopSubCategory.Table, LocalizationController.Translation("TABLE")},
        {ShopSubCategory.RepairKit, LocalizationController.Translation("REPAIR_KIT")},
        {ShopSubCategory.Unique, LocalizationController.Translation("UNIQUE")},

        #endregion Furniture

        #region GatheringGear

        {ShopSubCategory.FibergathererArmor, LocalizationController.Translation("FIBERGATHERER_ARMOR")},
        {ShopSubCategory.FibergathererHelmet, LocalizationController.Translation("FIBERGATHERER_HELMET")},
        {ShopSubCategory.FibergathererShoes, LocalizationController.Translation("FIBERGATHERER_SHOES")},
        {ShopSubCategory.FibergathererBackpack, LocalizationController.Translation("FIBERGATHERER_BACKPACK")},
        {ShopSubCategory.FishgathererArmor, LocalizationController.Translation("FISHGATHERER_ARMOR")},
        {ShopSubCategory.FishgathererHelmet, LocalizationController.Translation("FISHGATHERER_HELMET")},
        {ShopSubCategory.FishgathererShoes, LocalizationController.Translation("FISHGATHERER_SHOES")},
        {ShopSubCategory.FishgathererBackpack, LocalizationController.Translation("FISHGATHERER_BACKPACK")},
        {ShopSubCategory.HidegathererArmor, LocalizationController.Translation("HIDEGATHERER_ARMOR")},
        {ShopSubCategory.HidegathererHelmet, LocalizationController.Translation("HIDEGATHERER_HELMET")},
        {ShopSubCategory.HidegathererShoes, LocalizationController.Translation("HIDEGATHERER_SHOES")},
        {ShopSubCategory.HidegathererBackpack, LocalizationController.Translation("HIDEGATHERERR_BACKPACK")},
        {ShopSubCategory.OregathererArmor, LocalizationController.Translation("OREGATHERER_ARMOR")},
        {ShopSubCategory.OregathererHelmet, LocalizationController.Translation("OREGATHERER_HELMET")},
        {ShopSubCategory.OregathererShoes, LocalizationController.Translation("OREGATHERER_SHOES")},
        {ShopSubCategory.OregathererBackpack, LocalizationController.Translation("OREGATHERER_BACKPACK")},
        {ShopSubCategory.RockgathererArmor, LocalizationController.Translation("ROCKGATHERER_ARMOR")},
        {ShopSubCategory.RockgathererHelmet, LocalizationController.Translation("ROCKGATHERER_HELMET")},
        {ShopSubCategory.RockgathererShoes, LocalizationController.Translation("ROCKGATHERER_SHOES")},
        {ShopSubCategory.RockgathererBackpack, LocalizationController.Translation("ROCKGATHERER_BACKPACK")},
        {ShopSubCategory.WoodgathererArmor, LocalizationController.Translation("WOODGATHERER_ARMOR")},
        {ShopSubCategory.WoodgathererHelmet, LocalizationController.Translation("WOODGATHERER_HELMET")},
        {ShopSubCategory.WoodgathererShoes, LocalizationController.Translation("WOODGATHERER_SHOES")},
        {ShopSubCategory.WoodgathererBackpack, LocalizationController.Translation("WOODGATHERER_BACKPACK")},

        #endregion GatheringGear

        #region LuxuryGoods

        {ShopSubCategory.Bridgewatch, LocalizationController.Translation("BRIDGEWATCH")},
        {ShopSubCategory.Caerleon, LocalizationController.Translation("CAERLEON")},
        {ShopSubCategory.FortSterling, LocalizationController.Translation("FORT_STERLING")},
        {ShopSubCategory.Lymhurst, LocalizationController.Translation("LYMHURST")},
        {ShopSubCategory.Martlock, LocalizationController.Translation("MARTLOCK")},
        {ShopSubCategory.Thetford, LocalizationController.Translation("THETFORD")},

        #endregion LuxuryGoods

        #region Magic

        {ShopSubCategory.ArcaneStaff, LocalizationController.Translation("ARCANE_STAFF")},
        {ShopSubCategory.CurseStaff, LocalizationController.Translation("CURSE_STAFF")},
        {ShopSubCategory.FireStaff, LocalizationController.Translation("FIRE_STAFF")},
        {ShopSubCategory.FrostStaff, LocalizationController.Translation("FROST_STAFF")},
        {ShopSubCategory.HolyStaff, LocalizationController.Translation("HOLY_STAFF")},
        {ShopSubCategory.NatureStaff, LocalizationController.Translation("NATURE_STAFF")},
        {ShopSubCategory.ShapeShifterStaff, LocalizationController.Translation("SHAPESHIFTER")},

        #endregion Magic

        #region Materials

        {ShopSubCategory.Essence, LocalizationController.Translation("ESSENCE")},
        {ShopSubCategory.OtherMaterials, LocalizationController.Translation("OTHER")},
        {ShopSubCategory.Relic, LocalizationController.Translation("RELIC")},
        {ShopSubCategory.Rune, LocalizationController.Translation("RUNE")},
        {ShopSubCategory.Soul, LocalizationController.Translation("SOUL")},

        #endregion Materials

        #region Melee

        {ShopSubCategory.Axe, LocalizationController.Translation("AXE")},
        {ShopSubCategory.Dagger, LocalizationController.Translation("DAGGER")},
        {ShopSubCategory.Hammer, LocalizationController.Translation("HAMMER")},
        {ShopSubCategory.Mace, LocalizationController.Translation("MACE")},
        {ShopSubCategory.QuarterStaff, LocalizationController.Translation("QUARTER_STAFF")},
        {ShopSubCategory.Spear, LocalizationController.Translation("SPEAR")},
        {ShopSubCategory.Sword, LocalizationController.Translation("SWORD")},
        {ShopSubCategory.Knuckles, LocalizationController.Translation("WAR_GLOVES") },

        #endregion Melee

        #region Mount

        {ShopSubCategory.ArmoredHorse, LocalizationController.Translation("ARMORED_HORSE")},
        {ShopSubCategory.Ox, LocalizationController.Translation("OX")},
        {ShopSubCategory.RareMount, LocalizationController.Translation("RARE_MOUNT")},
        {ShopSubCategory.RidingHorse, LocalizationController.Translation("RIDING_HORSE")},

        #endregion Mount

        #region Off-Hand

        {ShopSubCategory.Book, LocalizationController.Translation("BOOK")},
        {ShopSubCategory.Horn, LocalizationController.Translation("HORN")},
        {ShopSubCategory.Orb, LocalizationController.Translation("ORB")},
        {ShopSubCategory.Shield, LocalizationController.Translation("SHIELD")},
        {ShopSubCategory.Torch, LocalizationController.Translation("TORCH")},
        {ShopSubCategory.Totem, LocalizationController.Translation("TOTEM")},

        #endregion Off-Hand

        #region Other

        {ShopSubCategory.Trash, LocalizationController.Translation("TRASH")},

        #endregion Other

        #region Product

        {ShopSubCategory.Farming, LocalizationController.Translation("FARMING")},
        {ShopSubCategory.Journal, LocalizationController.Translation("JOURNAL")},

        #endregion Product

        #region Ranged

        {ShopSubCategory.Bow, LocalizationController.Translation("BOW")},
        {ShopSubCategory.Crossbow, LocalizationController.Translation("CROSSBOW")},

        #endregion Ranged

        #region Resource

        {ShopSubCategory.Cloth, LocalizationController.Translation("CLOTH")},
        {ShopSubCategory.Fiber, LocalizationController.Translation("FIBER")},
        {ShopSubCategory.Hide, LocalizationController.Translation("HIDE")},
        {ShopSubCategory.Leather, LocalizationController.Translation("LEATHER")},
        {ShopSubCategory.Metalbar, LocalizationController.Translation("METALBAR")},
        {ShopSubCategory.Ore, LocalizationController.Translation("ORE")},
        {ShopSubCategory.Planks, LocalizationController.Translation("PLANKS")},
        {ShopSubCategory.Wood, LocalizationController.Translation("WOOD")},
        {ShopSubCategory.Rock, LocalizationController.Translation("ROCK")},
        {ShopSubCategory.Stoneblock, LocalizationController.Translation("STONEBLOCK")},

        #endregion Resource

        #region Token

        {ShopSubCategory.ArenaSigils, LocalizationController.Translation("ARENA_SIGILS")},
        {ShopSubCategory.Event, LocalizationController.Translation("EVENT")},
        {ShopSubCategory.RoyalSigils, LocalizationController.Translation("ROYAL_SIGILS")},

        #endregion Token

        #region Tool

        {ShopSubCategory.DemolitionHammer, LocalizationController.Translation("DEMOLITION_HAMMER")},
        {ShopSubCategory.Fishing, LocalizationController.Translation("FISHING_ROD")},
        {ShopSubCategory.Pickaxe, LocalizationController.Translation("PICKAXE")},
        {ShopSubCategory.Sickle, LocalizationController.Translation("SICKLE")},
        {ShopSubCategory.SkinningKnife, LocalizationController.Translation("SKINNING_KNIFE")},
        {ShopSubCategory.StoneHammer, LocalizationController.Translation("STONE_HAMMER")},
        {ShopSubCategory.WoodAxe, LocalizationController.Translation("WOOD_AXE")},
        {ShopSubCategory.TrackingTool, LocalizationController.Translation("TRACKING_TOOL")},

        #endregion Tool

        #region Trophies

        {ShopSubCategory.FiberTrophy, LocalizationController.Translation("FIBER_TROPHY")},
        {ShopSubCategory.FishTrophy, LocalizationController.Translation("FISH_TROPHY")},
        {ShopSubCategory.GeneralTrophy, LocalizationController.Translation("GENERAL_TROPHY")},
        {ShopSubCategory.HideTrophy, LocalizationController.Translation("HIDE_TROPHY")},
        {ShopSubCategory.MercenaryTrophy, LocalizationController.Translation("MERCENARY_TROPHY")},
        {ShopSubCategory.OreTrophy, LocalizationController.Translation("ORE_TROPHY")},
        {ShopSubCategory.RockTrophy, LocalizationController.Translation("ROCK_TROPHY")},
        {ShopSubCategory.WoodTrophy, LocalizationController.Translation("WOOD_TROPHY")},

        #endregion Trophies
    };

    public static readonly Dictionary<ShopCategory, string> CategoryNames = new()
    {
        {ShopCategory.Unknown, string.Empty},
        {ShopCategory.Accessories, LocalizationController.Translation("ACCESSORIES")},
        {ShopCategory.Armor, LocalizationController.Translation("ARMOR")},
        {ShopCategory.Artifact, LocalizationController.Translation("ARTEFACT")},
        {ShopCategory.CityResources, LocalizationController.Translation("CITY_RESOURCES")},
        {ShopCategory.Consumables, LocalizationController.Translation("CONSUMABLE")},
        {ShopCategory.Farmable, LocalizationController.Translation("FARMABLE")},
        {ShopCategory.Furniture, LocalizationController.Translation("FURNITURE")},
        {ShopCategory.GatheringGear, LocalizationController.Translation("GATHERING_GEAR")},
        {ShopCategory.LuxuryGoods, LocalizationController.Translation("LUXURY_GOODS")},
        {ShopCategory.Magic, LocalizationController.Translation("MAGIC")},
        {ShopCategory.Materials, LocalizationController.Translation("MATERIALS")},
        {ShopCategory.Melee, LocalizationController.Translation("MELEE")},
        {ShopCategory.Mounts, LocalizationController.Translation("MOUNT")},
        {ShopCategory.OffHand, LocalizationController.Translation("OFFHAND")},
        {ShopCategory.Other, LocalizationController.Translation("OTHER")},
        {ShopCategory.Products, LocalizationController.Translation("PRODUCT")},
        {ShopCategory.Ranged, LocalizationController.Translation("RANGED")},
        {ShopCategory.Resources, LocalizationController.Translation("RESOURCE")},
        {ShopCategory.Token, LocalizationController.Translation("TOKEN")},
        {ShopCategory.Tools, LocalizationController.Translation("TOOLS")},
        {ShopCategory.Trophies, LocalizationController.Translation("TROPHIES")}
    };

    public static readonly Dictionary<ShopCategory, string> Categories = new()
    {
        {ShopCategory.Unknown, string.Empty},
        {ShopCategory.Accessories, "accessories" },
        {ShopCategory.Armor, "armor" },
        {ShopCategory.Artifact, "artefacts" },
        {ShopCategory.CityResources, "cityresources" },
        {ShopCategory.Consumables, "consumables" },
        {ShopCategory.Farmable, "farmables" },
        {ShopCategory.Furniture, "furniture" },
        {ShopCategory.GatheringGear, "gatherergear" },
        {ShopCategory.LuxuryGoods, "luxurygoods" },
        {ShopCategory.Magic, "magic" },
        {ShopCategory.Materials, "materials" },
        {ShopCategory.Melee, "melee" },
        {ShopCategory.Mounts, "mounts"},
        {ShopCategory.OffHand, "offhand"},
        {ShopCategory.Other, "other" },
        {ShopCategory.Products, "products" },
        {ShopCategory.Ranged, "ranged" },
        {ShopCategory.Resources, "resources" },
        {ShopCategory.Token, "token" },
        {ShopCategory.Tools, "tools" },
        {ShopCategory.Trophies, "trophies" },
        {ShopCategory.SkillBooks, "skillbooks" },
        {ShopCategory.Labourers, "labourers" }
    };

    public static ShopCategory ShopCategoryStringToCategory(string value)
    {
        return value.ToLower() switch
        {
            "melee" => ShopCategory.Melee,
            "magic" => ShopCategory.Magic,
            "ranged" => ShopCategory.Ranged,
            "offhand" => ShopCategory.OffHand,
            "armor" => ShopCategory.Armor,
            "accessories" => ShopCategory.Accessories,
            "mounts" => ShopCategory.Mounts,
            "gatherergear" => ShopCategory.GatheringGear,
            "tools" => ShopCategory.Tools,
            "consumables" => ShopCategory.Consumables,
            "skillbooks" => ShopCategory.SkillBooks,
            "resources" => ShopCategory.Resources,
            "cityresources" => ShopCategory.CityResources,
            "artefacts" => ShopCategory.Artifact,
            "materials" => ShopCategory.Materials,
            "token" => ShopCategory.Token,
            "farmables" => ShopCategory.Farmable,
            "products" => ShopCategory.Products,
            "luxurygoods" => ShopCategory.LuxuryGoods,
            "trophies" => ShopCategory.Trophies,
            "furniture" => ShopCategory.Furniture,
            "labourers" => ShopCategory.Labourers,
            "other" => ShopCategory.Other,
            _ => ShopCategory.Unknown
        };
    }

    public static ShopSubCategory ShopSubCategoryStringToShopSubCategory(string value)
    {
        return SubCategories?.FirstOrDefault(x => x.CategoryId == value)?.ShopSubCategory ?? ShopSubCategory.Unknown;
    }

    public static string ShopSubCategoryToShopSubCategoryString(ShopSubCategory shopSubCategory)
    {
        return SubCategories?.FirstOrDefault(x => x.ShopSubCategory == shopSubCategory)?.CategoryId ?? "unknown";
    }

    public static CategoryObject GetSubCategory(string categoryId)
    {
        return SubCategories.SingleOrDefault(x => x.CategoryId == categoryId);
    }

    public static string GetCategoryIdByShopCategory(ShopCategory shopCategory)
    {
        return Categories.TryGetValue(shopCategory, out string category) ? category : "unknown";
    }

    public static string GetSubCategoryName(ShopSubCategory shopSubCategory)
    {
        return SubCategoryNames.TryGetValue(shopSubCategory, out var name) ? name : null;
    }

    public static string GetCategoryName(ShopCategory shopCategory)
    {
        return CategoryNames.TryGetValue(shopCategory, out var name) ? name : null;
    }

    public static Dictionary<ShopSubCategory, string> GetSubCategoriesByCategory(ShopCategory shopCategory)
    {
        return SubCategories?.Where(x => x.ShopCategory == shopCategory).ToDictionary(x => x.ShopSubCategory, x => x.SubCategoryName);
    }
}

public enum ShopSubCategory
{
    Unknown,
    Bag,
    Cape,
    ClothArmor,
    ClothHelmet,
    ClothShoes,
    LeatherArmor,
    LeatherHelmet,
    LeatherShoes,
    PlateArmor,
    PlateHelmet,
    PlateShoes,
    UniqueArmor,
    UniqueHelmet,
    UniqueShoes,
    ArmorArtefact,
    MagicArtefact,
    MeleeArtefact,
    OffhandArtefact,
    RangedArtefact,
    BeastHeart,
    MountainHeart,
    RockHeart,
    TreeHeart,
    VineHeart,
    Cooked,
    Fish,
    FishingBait,
    Maps,
    Other,
    Potion,
    SkillBook,
    Vanity,
    Animals,
    Seed,
    Banner,
    Bed,
    Chest,
    DecorationFurniture,
    Flag,
    HereticFurniture,
    KeeperFurniture,
    MorganaFurniture,
    Table,
    RepairKit,
    Unique,
    FibergathererArmor,
    FibergathererHelmet,
    FibergathererShoes,
    FibergathererBackpack,
    FishgathererArmor,
    FishgathererHelmet,
    FishgathererShoes,
    FishgathererBackpack,
    HidegathererArmor,
    HidegathererHelmet,
    HidegathererShoes,
    HidegathererBackpack,
    OregathererArmor,
    OregathererHelmet,
    OregathererShoes,
    OregathererBackpack,
    RockgathererArmor,
    RockgathererHelmet,
    RockgathererShoes,
    RockgathererBackpack,
    WoodgathererArmor,
    WoodgathererHelmet,
    WoodgathererShoes,
    WoodgathererBackpack,
    Bridgewatch,
    Caerleon,
    FortSterling,
    Lymhurst,
    Martlock,
    Thetford,
    ArcaneStaff,
    CurseStaff,
    FireStaff,
    FrostStaff,
    HolyStaff,
    NatureStaff,
    Essence,
    OtherMaterials,
    Relic,
    Rune,
    Soul,
    Axe,
    Dagger,
    Hammer,
    Mace,
    QuarterStaff,
    Spear,
    Sword,
    Knuckles,
    ArmoredHorse,
    Ox,
    RareMount,
    RidingHorse,
    Book,
    Horn,
    Orb,
    Shield,
    Torch,
    Totem,
    Trash,
    Farming,
    Journal,
    Bow,
    Crossbow,
    Cloth,
    Fiber,
    Hide,
    Leather,
    Metalbar,
    Ore,
    Planks,
    Wood,
    Rock,
    Stoneblock,
    ArenaSigils,
    Event,
    RoyalSigils,
    DemolitionHammer,
    Fishing,
    Pickaxe,
    Sickle,
    SkinningKnife,
    StoneHammer,
    WoodAxe,
    FiberTrophy,
    FishTrophy,
    GeneralTrophy,
    HideTrophy,
    MercenaryTrophy,
    OreTrophy,
    RockTrophy,
    WoodTrophy,
    ShapeShifterStaff,
    TrackingTool
}

public enum ShopCategory
{
    Unknown,
    Melee = 10,
    Ranged = 11,
    Magic = 12,
    OffHand = 20,
    Armor = 30,
    Accessories = 40,
    Mounts = 50,
    GatheringGear = 60,
    Tools = 61,
    Consumables = 70,
    SkillBooks = 71,
    Resources = 72,
    CityResources = 73,
    Artifact = 74,
    Materials = 75,
    Token = 76,
    Farmable = 80,
    Products = 81,
    LuxuryGoods = 82,
    Trophies = 83,
    Furniture = 100,
    Labourers = 101,
    Other = 111,
}