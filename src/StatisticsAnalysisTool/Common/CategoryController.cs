using System.Collections.Generic;
using System.Linq;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Common
{
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
            {ShopSubCategory.Unknown, LanguageController.Translation("UNKNOWN")},

            #region Accessories

            {ShopSubCategory.Bag, LanguageController.Translation("BAG")},
            {ShopSubCategory.Cape, LanguageController.Translation("CAPE")},

            #endregion Accessories

            #region Armor

            {ShopSubCategory.ClothArmor, LanguageController.Translation("CLOTH_ARMOR")},
            {ShopSubCategory.ClothHelmet, LanguageController.Translation("CLOTH_HELMET")},
            {ShopSubCategory.ClothShoes, LanguageController.Translation("CLOTH_SHOES")},
            {ShopSubCategory.LeatherArmor, LanguageController.Translation("LEATHER_ARMOR")},
            {ShopSubCategory.LeatherHelmet, LanguageController.Translation("LEATHER_HELMET")},
            {ShopSubCategory.LeatherShoes, LanguageController.Translation("LEATHER_SHOES")},
            {ShopSubCategory.PlateArmor, LanguageController.Translation("PLATE_ARMOR")},
            {ShopSubCategory.PlateHelmet, LanguageController.Translation("PLATE_HELMET")},
            {ShopSubCategory.PlateShoes, LanguageController.Translation("PLATE_SHOES")},
            {ShopSubCategory.UniqueArmor, LanguageController.Translation("UNIQUE_ARMOR")},
            {ShopSubCategory.UniqueHelmet, LanguageController.Translation("UNIQUE_HELMET")},
            {ShopSubCategory.UniqueShoes, LanguageController.Translation("UNIQUE_SHOES")},

            #endregion Armor

            #region Artifact

            {ShopSubCategory.ArmorArtefact, LanguageController.Translation("ARMOR_ARTEFACT")},
            {ShopSubCategory.MagicArtefact, LanguageController.Translation("MAGIC_ARTEFACT")},
            {ShopSubCategory.MeleeArtefact, LanguageController.Translation("MELEE_ARTEFACT")},
            {ShopSubCategory.OffhandArtefact, LanguageController.Translation("OFFHAND_ARTEFACT")},
            {ShopSubCategory.RangedArtefact, LanguageController.Translation("RANGED_ARTEFACT")},

            #endregion Artifact

            #region CityResources

            {ShopSubCategory.BeastHeart, LanguageController.Translation("BEASTHEART")},
            {ShopSubCategory.MountainHeart, LanguageController.Translation("MOUNTAINHEART")},
            {ShopSubCategory.RockHeart, LanguageController.Translation("ROCKHEART")},
            {ShopSubCategory.TreeHeart, LanguageController.Translation("TREEHEART")},
            {ShopSubCategory.VineHeart, LanguageController.Translation("VINEHEART")},

            #endregion CityResources

            #region Consumable

            {ShopSubCategory.Cooked, LanguageController.Translation("COOKED")},
            {ShopSubCategory.Fish, LanguageController.Translation("FISH")},
            {ShopSubCategory.FishingBait, LanguageController.Translation("FISHING_BAIT")},
            {ShopSubCategory.Maps, LanguageController.Translation("MAPS")},
            {ShopSubCategory.Other, LanguageController.Translation("OTHER")},
            {ShopSubCategory.Potion, LanguageController.Translation("POTION")},
            {ShopSubCategory.SkillBook, LanguageController.Translation("SKILL_BOOK")},
            {ShopSubCategory.Vanity, LanguageController.Translation("VANITY")},

            #endregion Consumable

            #region Farmable

            {ShopSubCategory.Animals, LanguageController.Translation("ANIMALS")},
            {ShopSubCategory.Seed, LanguageController.Translation("SEED")},

            #endregion Farmable

            #region Furniture

            {ShopSubCategory.Banner, LanguageController.Translation("BANNER")},
            {ShopSubCategory.Bed, LanguageController.Translation("BED")},
            {ShopSubCategory.Chest, LanguageController.Translation("CHEST")},
            {ShopSubCategory.DecorationFurniture, LanguageController.Translation("DECORATION_FURNITURE")},
            {ShopSubCategory.Flag, LanguageController.Translation("FLAG")},
            {ShopSubCategory.HereticFurniture, LanguageController.Translation("HERETIC_FURNITURE")},
            {ShopSubCategory.KeeperFurniture, LanguageController.Translation("KEEPER_FURNITURE")},
            {ShopSubCategory.MorganaFurniture, LanguageController.Translation("MORGANA_FURNITURE")},
            {ShopSubCategory.Table, LanguageController.Translation("TABLE")},
            {ShopSubCategory.RepairKit, LanguageController.Translation("REPAIR_KIT")},
            {ShopSubCategory.Unique, LanguageController.Translation("UNIQUE")},

            #endregion Furniture

            #region GatheringGear

            {ShopSubCategory.FibergathererArmor, LanguageController.Translation("FIBERGATHERER_ARMOR")},
            {ShopSubCategory.FibergathererHelmet, LanguageController.Translation("FIBERGATHERER_HELMET")},
            {ShopSubCategory.FibergathererShoes, LanguageController.Translation("FIBERGATHERER_SHOES")},
            {ShopSubCategory.FibergathererBackpack, LanguageController.Translation("FIBERGATHERER_BACKPACK")},

            {ShopSubCategory.FishgathererArmor, LanguageController.Translation("FISHGATHERER_ARMOR")},
            {ShopSubCategory.FishgathererHelmet, LanguageController.Translation("FISHGATHERER_HELMET")},
            {ShopSubCategory.FishgathererShoes, LanguageController.Translation("FISHGATHERER_SHOES")},
            {ShopSubCategory.FishgathererBackpack, LanguageController.Translation("FISHGATHERER_BACKPACK")},

            {ShopSubCategory.HidegathererArmor, LanguageController.Translation("HIDEGATHERER_ARMOR")},
            {ShopSubCategory.HidegathererHelmet, LanguageController.Translation("HIDEGATHERER_HELMET")},
            {ShopSubCategory.HidegathererShoes, LanguageController.Translation("HIDEGATHERER_SHOES")},
            {ShopSubCategory.HidegathererBackpack, LanguageController.Translation("HIDEGATHERERR_BACKPACK")},

            {ShopSubCategory.OregathererArmor, LanguageController.Translation("OREGATHERER_ARMOR")},
            {ShopSubCategory.OregathererHelmet, LanguageController.Translation("OREGATHERER_HELMET")},
            {ShopSubCategory.OregathererShoes, LanguageController.Translation("OREGATHERER_SHOES")},
            {ShopSubCategory.OregathererBackpack, LanguageController.Translation("OREGATHERER_BACKPACK")},

            {ShopSubCategory.RockgathererArmor, LanguageController.Translation("ROCKGATHERER_ARMOR")},
            {ShopSubCategory.RockgathererHelmet, LanguageController.Translation("ROCKGATHERER_HELMET")},
            {ShopSubCategory.RockgathererShoes, LanguageController.Translation("ROCKGATHERER_SHOES")},
            {ShopSubCategory.RockgathererBackpack, LanguageController.Translation("ROCKGATHERER_BACKPACK")},

            {ShopSubCategory.WoodgathererArmor, LanguageController.Translation("WOODGATHERER_ARMOR")},
            {ShopSubCategory.WoodgathererHelmet, LanguageController.Translation("WOODGATHERER_HELMET")},
            {ShopSubCategory.WoodgathererShoes, LanguageController.Translation("WOODGATHERER_SHOES")},
            {ShopSubCategory.WoodgathererBackpack, LanguageController.Translation("WOODGATHERER_BACKPACK")},

            #endregion GatheringGear

            #region LuxuryGoods

            {ShopSubCategory.Bridgewatch, LanguageController.Translation("BRIDGEWATCH")},
            {ShopSubCategory.Caerleon, LanguageController.Translation("CAERLEON")},
            {ShopSubCategory.FortSterling, LanguageController.Translation("FORT_STERLING")},
            {ShopSubCategory.Lymhurst, LanguageController.Translation("LYMHURST")},
            {ShopSubCategory.Martlock, LanguageController.Translation("MARTLOCK")},
            {ShopSubCategory.Thetford, LanguageController.Translation("THETFORD")},

            #endregion LuxuryGoods

            #region Magic

            {ShopSubCategory.ArcaneStaff, LanguageController.Translation("ARCANE_STAFF")},
            {ShopSubCategory.CurseStaff, LanguageController.Translation("CURSE_STAFF")},
            {ShopSubCategory.FireStaff, LanguageController.Translation("FIRE_STAFF")},
            {ShopSubCategory.FrostStaff, LanguageController.Translation("FROST_STAFF")},
            {ShopSubCategory.HolyStaff, LanguageController.Translation("HOLY_STAFF")},
            {ShopSubCategory.NatureStaff, LanguageController.Translation("NATURE_STAFF")},

            #endregion Magic

            #region Materials

            {ShopSubCategory.Essence, LanguageController.Translation("ESSENCE")},
            {ShopSubCategory.OtherMaterials, LanguageController.Translation("OTHER")},
            {ShopSubCategory.Relic, LanguageController.Translation("RELIC")},
            {ShopSubCategory.Rune, LanguageController.Translation("RUNE")},
            {ShopSubCategory.Soul, LanguageController.Translation("SOUL")},

            #endregion Materials

            #region Melee

            {ShopSubCategory.Axe, LanguageController.Translation("AXE")},
            {ShopSubCategory.Dagger, LanguageController.Translation("DAGGER")},
            {ShopSubCategory.Hammer, LanguageController.Translation("HAMMER")},
            {ShopSubCategory.Mace, LanguageController.Translation("MACE")},
            {ShopSubCategory.QuarterStaff, LanguageController.Translation("QUARTER_STAFF")},
            {ShopSubCategory.Spear, LanguageController.Translation("SPEAR")},
            {ShopSubCategory.Sword, LanguageController.Translation("SWORD")},
            {ShopSubCategory.Knuckles, LanguageController.Translation("WAR_GLOVES") },

            #endregion Melee

            #region Mount

            {ShopSubCategory.ArmoredHorse, LanguageController.Translation("ARMORED_HORSE")},
            {ShopSubCategory.Ox, LanguageController.Translation("OX")},
            {ShopSubCategory.RareMount, LanguageController.Translation("RARE_MOUNT")},
            {ShopSubCategory.RidingHorse, LanguageController.Translation("RIDING_HORSE")},

            #endregion Mount

            #region Off-Hand

            {ShopSubCategory.Book, LanguageController.Translation("BOOK")},
            {ShopSubCategory.Horn, LanguageController.Translation("HORN")},
            {ShopSubCategory.Orb, LanguageController.Translation("ORB")},
            {ShopSubCategory.Shield, LanguageController.Translation("SHIELD")},
            {ShopSubCategory.Torch, LanguageController.Translation("TORCH")},
            {ShopSubCategory.Totem, LanguageController.Translation("TOTEM")},

            #endregion Off-Hand

            #region Other

            {ShopSubCategory.Trash, LanguageController.Translation("TRASH")},

            #endregion Other

            #region Product

            {ShopSubCategory.Farming, LanguageController.Translation("FARMING")},
            {ShopSubCategory.Journal, LanguageController.Translation("JOURNAL")},

            #endregion Product

            #region Ranged

            {ShopSubCategory.Bow, LanguageController.Translation("BOW")},
            {ShopSubCategory.Crossbow, LanguageController.Translation("CROSSBOW")},

            #endregion Ranged

            #region Resource

            {ShopSubCategory.Cloth, LanguageController.Translation("CLOTH")},
            {ShopSubCategory.Fiber, LanguageController.Translation("FIBER")},
            {ShopSubCategory.Hide, LanguageController.Translation("HIDE")},
            {ShopSubCategory.Leather, LanguageController.Translation("LEATHER")},
            {ShopSubCategory.Metalbar, LanguageController.Translation("METALBAR")},
            {ShopSubCategory.Ore, LanguageController.Translation("ORE")},
            {ShopSubCategory.Planks, LanguageController.Translation("PLANKS")},
            {ShopSubCategory.Wood, LanguageController.Translation("WOOD")},
            {ShopSubCategory.Rock, LanguageController.Translation("ROCK")},
            {ShopSubCategory.Stoneblock, LanguageController.Translation("STONEBLOCK")},

            #endregion Resource

            #region Token

            {ShopSubCategory.ArenaSigils, LanguageController.Translation("ARENA_SIGILS")},
            {ShopSubCategory.Event, LanguageController.Translation("EVENT")},
            {ShopSubCategory.RoyalSigils, LanguageController.Translation("ROYAL_SIGILS")},

            #endregion Token

            #region Tool

            {ShopSubCategory.DemolitionHammer, LanguageController.Translation("DEMOLITION_HAMMER")},
            {ShopSubCategory.Fishing, LanguageController.Translation("FISHING")},
            {ShopSubCategory.Pickaxe, LanguageController.Translation("PICKAXE")},
            {ShopSubCategory.Sickle, LanguageController.Translation("SICKLE")},
            {ShopSubCategory.SkinningKnife, LanguageController.Translation("SKINNING_KNIFE")},
            {ShopSubCategory.StoneHammer, LanguageController.Translation("STONE_HAMMER")},
            {ShopSubCategory.WoodAxe, LanguageController.Translation("WOOD_AXE")},

            #endregion Tool

            #region Trophies

            {ShopSubCategory.FiberTrophy, LanguageController.Translation("FIBER_TROPHY")},
            {ShopSubCategory.FishTrophy, LanguageController.Translation("FISH_TROPHY")},
            {ShopSubCategory.GeneralTrophy, LanguageController.Translation("GENERAL_TROPHY")},
            {ShopSubCategory.HideTrophy, LanguageController.Translation("HIDE_TROPHY")},
            {ShopSubCategory.MercenaryTrophy, LanguageController.Translation("MERCENARY_TROPHY")},
            {ShopSubCategory.OreTrophy, LanguageController.Translation("ORE_TROPHY")},
            {ShopSubCategory.RockTrophy, LanguageController.Translation("ROCK_TROPHY")},
            {ShopSubCategory.WoodTrophy, LanguageController.Translation("WOOD_TROPHY")},

            #endregion Trophies
        };

        public static readonly Dictionary<ShopCategory, string> CategoryNames = new()
        {
            {ShopCategory.Unknown, string.Empty},
            {ShopCategory.Accessories, LanguageController.Translation("ACCESSORIES")},
            {ShopCategory.Armor, LanguageController.Translation("ARMOR")},
            {ShopCategory.Artifact, LanguageController.Translation("ARTEFACT")},
            {ShopCategory.CityResources, LanguageController.Translation("CITY_RESOURCES")},
            {ShopCategory.Consumables, LanguageController.Translation("CONSUMABLE")},
            {ShopCategory.Farmable, LanguageController.Translation("FARMABLE")},
            {ShopCategory.Furniture, LanguageController.Translation("FURNITURE")},
            {ShopCategory.GatheringGear, LanguageController.Translation("GATHERING_GEAR")},
            {ShopCategory.LuxuryGoods, LanguageController.Translation("LUXURY_GOODS")},
            {ShopCategory.Magic, LanguageController.Translation("MAGIC")},
            {ShopCategory.Materials, LanguageController.Translation("MATERIALS")},
            {ShopCategory.Melee, LanguageController.Translation("MELEE")},
            {ShopCategory.Mounts, LanguageController.Translation("MOUNT")},
            {ShopCategory.OffHand, LanguageController.Translation("OFFHAND")},
            {ShopCategory.Other, LanguageController.Translation("OTHER")},
            {ShopCategory.Products, LanguageController.Translation("PRODUCT")},
            {ShopCategory.Ranged, LanguageController.Translation("RANGED")},
            {ShopCategory.Resources, LanguageController.Translation("RESOURCE")},
            {ShopCategory.Token, LanguageController.Translation("TOKEN")},
            {ShopCategory.Tools, LanguageController.Translation("TOOL")},
            {ShopCategory.Trophies, LanguageController.Translation("TROPHIES")}
        };

        public static readonly Dictionary<ShopCategory, string> Categories = new ()
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
            return Categories.ContainsKey(shopCategory) ? Categories[shopCategory] : "unknown";
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
        WoodTrophy
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
}