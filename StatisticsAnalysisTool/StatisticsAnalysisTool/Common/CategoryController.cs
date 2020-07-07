using StatisticsAnalysisTool.Models;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common
{
    public static class CategoryController
    {
        public static List<CategoryObject> Categories = new List<CategoryObject>()
        {
            new CategoryObject("bag", Category.Bag, ParentCategory.Accessories),
            new CategoryObject("cape", Category.Cape, ParentCategory.Accessories),

            new CategoryObject("cloth_armor", Category.ClothArmor, ParentCategory.Armor),
            new CategoryObject("cloth_helmet", Category.ClothHelmet, ParentCategory.Armor),
            new CategoryObject("cloth_shoes", Category.ClothShoes, ParentCategory.Armor),
            new CategoryObject("leather_armor", Category.LeatherArmor, ParentCategory.Armor),
            new CategoryObject("leather_helmet", Category.LeatherHelmet, ParentCategory.Armor),
            new CategoryObject("leather_shoes", Category.LeatherShoes, ParentCategory.Armor),
            new CategoryObject("plate_armor", Category.PlateArmor, ParentCategory.Armor),
            new CategoryObject("plate_helmet", Category.PlateHelmet, ParentCategory.Armor),
            new CategoryObject("plate_shoes", Category.PlateShoes, ParentCategory.Armor),
            new CategoryObject("unique_armor", Category.UniqueArmor, ParentCategory.Armor),
            new CategoryObject("unique_helmet", Category.UniqueHelmet, ParentCategory.Armor),
            new CategoryObject("unique_shoes", Category.UniqueShoes, ParentCategory.Armor),

            new CategoryObject("armor_artefact", Category.ArmorArtefact, ParentCategory.Artifact),
            new CategoryObject("magic_artefact", Category.MagicArtefact, ParentCategory.Artifact),
            new CategoryObject("melee_artefact", Category.MeleeArtefact, ParentCategory.Artifact),
            new CategoryObject("offhand_artefact", Category.OffhandArtefact, ParentCategory.Artifact),
            new CategoryObject("ranged_artefact", Category.RangedArtefact, ParentCategory.Artifact),

            new CategoryObject("beastheart", Category.BeastHeart, ParentCategory.CityResources),
            new CategoryObject("mountainheart", Category.MountainHeart, ParentCategory.CityResources),
            new CategoryObject("rockheart", Category.RockHeart, ParentCategory.CityResources),
            new CategoryObject("treeheart", Category.TreeHeart, ParentCategory.CityResources),
            new CategoryObject("vineheart", Category.VineHeart, ParentCategory.CityResources),

            new CategoryObject("cooked", Category.Cooked, ParentCategory.Consumable),
            new CategoryObject("fish", Category.Fish, ParentCategory.Consumable),
            new CategoryObject("fishingbait", Category.FishingBait, ParentCategory.Consumable),
            new CategoryObject("maps", Category.Maps, ParentCategory.Consumable),
            new CategoryObject("Other", Category.Other, ParentCategory.Consumable),
            new CategoryObject("potion", Category.Potion, ParentCategory.Consumable),
            new CategoryObject("skillbook", Category.SkillBook, ParentCategory.Consumable),
            new CategoryObject("vanity", Category.Vanity, ParentCategory.Consumable),

            new CategoryObject("animals", Category.Animals, ParentCategory.Farmable),
            new CategoryObject("seed", Category.Seed, ParentCategory.Farmable),

            new CategoryObject("banner", Category.Banner, ParentCategory.Furniture),
            new CategoryObject("bed", Category.Bed, ParentCategory.Furniture),
            new CategoryObject("chest", Category.Chest, ParentCategory.Furniture),
            new CategoryObject("decoration_furniture", Category.DecorationFurniture, ParentCategory.Furniture),
            new CategoryObject("flag", Category.Flag, ParentCategory.Furniture),
            new CategoryObject("heretic_furniture", Category.HereticFurniture, ParentCategory.Furniture),
            new CategoryObject("keeper_furniture", Category.KeeperFurniture, ParentCategory.Furniture),
            new CategoryObject("morgana_furniture", Category.MorganaFurniture, ParentCategory.Furniture),
            new CategoryObject("table", Category.Table, ParentCategory.Furniture),
            new CategoryObject("repairkit", Category.RepairKit, ParentCategory.Furniture),
            new CategoryObject("unique", Category.Unique, ParentCategory.Furniture),

            new CategoryObject("fibergatherer_armor", Category.FibergathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("fibergatherer_helmet", Category.FibergathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("fibergatherer_shoes", Category.FibergathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("fibergatherer_backpack", Category.FibergathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("fishgatherer_armor", Category.FishgathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("fishgatherer_helmet", Category.FishgathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("fishgatherer_shoes", Category.FishgathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("fishgatherer_backpack", Category.FishgathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("hidegatherer_armor", Category.HidegathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("hidegatherer_helmet", Category.HidegathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("hidegatherer_shoes", Category.HidegathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("hidegatherer_backpack", Category.HidegathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("oregatherer_armor", Category.OregathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("oregatherer_helmet", Category.OregathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("oregatherer_shoes", Category.OregathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("oregatherer_backpack", Category.OregathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("rockgatherer_armor", Category.RockgathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("rockgatherer_helmet", Category.RockgathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("rockgatherer_shoes", Category.RockgathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("rockgatherer_backpack", Category.RockgathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("woodgatherer_armor", Category.WoodgathererArmor, ParentCategory.GatheringGear),
            new CategoryObject("woodgatherer_helmet", Category.WoodgathererHelmet, ParentCategory.GatheringGear),
            new CategoryObject("woodgatherer_shoes", Category.WoodgathererShoes, ParentCategory.GatheringGear),
            new CategoryObject("woodgatherer_backpack", Category.WoodgathererBackpack, ParentCategory.GatheringGear),

            new CategoryObject("bridgewatch", Category.Bridgewatch, ParentCategory.LuxuryGoods),
            new CategoryObject("caerleon", Category.Caerleon, ParentCategory.LuxuryGoods),
            new CategoryObject("fortsterling", Category.FortSterling, ParentCategory.LuxuryGoods),
            new CategoryObject("lymhurst", Category.Lymhurst, ParentCategory.LuxuryGoods),
            new CategoryObject("martlock", Category.Martlock, ParentCategory.LuxuryGoods),
            new CategoryObject("thetford", Category.Thetford, ParentCategory.LuxuryGoods),

            new CategoryObject("arcanestaff", Category.ArcaneStaff, ParentCategory.Magic),
            new CategoryObject("cursestaff", Category.CurseStaff, ParentCategory.Magic),
            new CategoryObject("firestaff", Category.FireStaff, ParentCategory.Magic),
            new CategoryObject("froststaff", Category.FrostStaff, ParentCategory.Magic),
            new CategoryObject("holystaff", Category.HolyStaff, ParentCategory.Magic),
            new CategoryObject("naturestaff", Category.NatureStaff, ParentCategory.Magic),

            new CategoryObject("essence", Category.Essence, ParentCategory.Materials),
            new CategoryObject("other", Category.OtherMaterials, ParentCategory.Materials),
            new CategoryObject("relic", Category.Relic, ParentCategory.Materials),
            new CategoryObject("rune", Category.Rune, ParentCategory.Materials),
            new CategoryObject("soul", Category.Soul, ParentCategory.Materials),

            new CategoryObject("axe", Category.Axe, ParentCategory.Melee),
            new CategoryObject("dagger", Category.Dagger, ParentCategory.Melee),
            new CategoryObject("hammer", Category.Hammer, ParentCategory.Melee),
            new CategoryObject("mace", Category.Mace, ParentCategory.Melee),
            new CategoryObject("quarterstaff", Category.QuarterStaff, ParentCategory.Melee),
            new CategoryObject("spear", Category.Spear, ParentCategory.Melee),
            new CategoryObject("sword", Category.Sword, ParentCategory.Melee),

            new CategoryObject("armoredhorse", Category.ArmoredHorse, ParentCategory.Mount),
            new CategoryObject("ox", Category.Ox, ParentCategory.Mount),
            new CategoryObject("rare_mount", Category.RareMount, ParentCategory.Mount),
            new CategoryObject("ridinghorse", Category.RidingHorse, ParentCategory.Mount),

            new CategoryObject("book", Category.Book, ParentCategory.OffHand),
            new CategoryObject("horn", Category.Horn, ParentCategory.OffHand),
            new CategoryObject("orb", Category.Orb, ParentCategory.OffHand),
            new CategoryObject("shield", Category.Shield, ParentCategory.OffHand),
            new CategoryObject("torch", Category.Torch, ParentCategory.OffHand),
            new CategoryObject("totem", Category.Totem, ParentCategory.OffHand),

            new CategoryObject("trash", Category.Trash, ParentCategory.Other),
            new CategoryObject("farming", Category.Farming, ParentCategory.Product),
            new CategoryObject("journal", Category.Journal, ParentCategory.Product),

            new CategoryObject("bow", Category.Bow, ParentCategory.Ranged),
            new CategoryObject("crossbow", Category.Crossbow, ParentCategory.Ranged),

            new CategoryObject("cloth", Category.Cloth, ParentCategory.Resource),
            new CategoryObject("fiber", Category.Fiber, ParentCategory.Resource),
            new CategoryObject("hide", Category.Hide, ParentCategory.Resource),
            new CategoryObject("leather", Category.Leather, ParentCategory.Resource),
            new CategoryObject("metalbar", Category.Metalbar, ParentCategory.Resource),
            new CategoryObject("ore", Category.Ore, ParentCategory.Resource),
            new CategoryObject("wood", Category.Wood, ParentCategory.Resource),
            new CategoryObject("planks", Category.Planks, ParentCategory.Resource),
            new CategoryObject("rock", Category.Rock, ParentCategory.Resource),
            new CategoryObject("stoneblock", Category.Stoneblock, ParentCategory.Resource),

            new CategoryObject("arenasigils", Category.ArenaSigils, ParentCategory.Token),
            new CategoryObject("event", Category.Event, ParentCategory.Token),
            new CategoryObject("royalsigils", Category.RoyalSigils, ParentCategory.Token),

            new CategoryObject("demolitionhammer", Category.DemolitionHammer, ParentCategory.Tool),
            new CategoryObject("fishing", Category.Fishing, ParentCategory.Tool),
            new CategoryObject("pickaxe", Category.Pickaxe, ParentCategory.Tool),
            new CategoryObject("sickle", Category.Sickle, ParentCategory.Tool),
            new CategoryObject("skinningknife", Category.SkinningKnife, ParentCategory.Tool),
            new CategoryObject("stonehammer", Category.StoneHammer, ParentCategory.Tool),
            new CategoryObject("woodaxe", Category.WoodAxe, ParentCategory.Tool),

            new CategoryObject("fibertrophy", Category.FiberTrophy, ParentCategory.Trophies),
            new CategoryObject("fishtrophy", Category.FishTrophy, ParentCategory.Trophies),
            new CategoryObject("generaltrophy", Category.GeneralTrophy, ParentCategory.Trophies),
            new CategoryObject("hidetrophy", Category.HideTrophy, ParentCategory.Trophies),
            new CategoryObject("mercenarytrophy", Category.MercenaryTrophy, ParentCategory.Trophies),
            new CategoryObject("oretrophy", Category.OreTrophy, ParentCategory.Trophies),
            new CategoryObject("rocktrophy", Category.RockTrophy, ParentCategory.Trophies),
            new CategoryObject("woodtrophy", Category.WoodTrophy, ParentCategory.Trophies),
        };

        public static readonly Dictionary<Category, string> CategoryNames = new Dictionary<Category, string>
        {
            {Category.Unknown, LanguageController.Translation("UNKNOWN")},

            #region Accessories

            {Category.Bag, LanguageController.Translation("BAG")},
            {Category.Cape, LanguageController.Translation("CAPE")},

            #endregion

            #region Armor

            {Category.ClothArmor, LanguageController.Translation("CLOTH_ARMOR")},
            {Category.ClothHelmet, LanguageController.Translation("CLOTH_HELMET")},
            {Category.ClothShoes, LanguageController.Translation("CLOTH_SHOES")},
            {Category.LeatherArmor, LanguageController.Translation("LEATHER_ARMOR")},
            {Category.LeatherHelmet, LanguageController.Translation("LEATHER_HELMET")},
            {Category.LeatherShoes, LanguageController.Translation("LEATHER_SHOES")},
            {Category.PlateArmor, LanguageController.Translation("PLATE_ARMOR")},
            {Category.PlateHelmet, LanguageController.Translation("PLATE_HELMET")},
            {Category.PlateShoes, LanguageController.Translation("PLATE_SHOES")},
            {Category.UniqueArmor, LanguageController.Translation("UNIQUE_ARMOR")},
            {Category.UniqueHelmet, LanguageController.Translation("UNIQUE_HELMET")},
            {Category.UniqueShoes, LanguageController.Translation("UNIQUE_SHOES")},

            #endregion

            #region Artifact

            {Category.ArmorArtefact, LanguageController.Translation("ARMOR_ARTEFACT")},
            {Category.MagicArtefact, LanguageController.Translation("MAGIC_ARTEFACT")},
            {Category.MeleeArtefact, LanguageController.Translation("MELEE_ARTEFACT")},
            {Category.OffhandArtefact, LanguageController.Translation("OFFHAND_ARTEFACT")},
            {Category.RangedArtefact, LanguageController.Translation("RANGED_ARTEFACT")},

            #endregion

            #region CityResources

            {Category.BeastHeart, LanguageController.Translation("BEASTHEART")},
            {Category.MountainHeart, LanguageController.Translation("MOUNTAINHEART")},
            {Category.RockHeart, LanguageController.Translation("ROCKHEART")},
            {Category.TreeHeart, LanguageController.Translation("TREEHEART")},
            {Category.VineHeart, LanguageController.Translation("VINEHEART")},

            #endregion

            #region Consumable

            {Category.Cooked, LanguageController.Translation("COOKED")},
            {Category.Fish, LanguageController.Translation("FISH")},
            {Category.FishingBait, LanguageController.Translation("FISHING_BAIT")},
            {Category.Maps, LanguageController.Translation("MAPS")},
            {Category.Other, LanguageController.Translation("OTHER")},
            {Category.Potion, LanguageController.Translation("POTION")},
            {Category.SkillBook, LanguageController.Translation("SKILL_BOOK")},
            {Category.Vanity, LanguageController.Translation("VANITY")},

            #endregion

            #region Farmable

            {Category.Animals, LanguageController.Translation("ANIMALS")},
            {Category.Seed, LanguageController.Translation("SEED")},

            #endregion

            #region Furniture

            {Category.Banner, LanguageController.Translation("BANNER")},
            {Category.Bed, LanguageController.Translation("BED")},
            {Category.Chest, LanguageController.Translation("CHEST")},
            {Category.DecorationFurniture, LanguageController.Translation("DECORATION_FURNITURE")},
            {Category.Flag, LanguageController.Translation("FLAG")},
            {Category.HereticFurniture, LanguageController.Translation("HERETIC_FURNITURE")},
            {Category.KeeperFurniture, LanguageController.Translation("KEEPER_FURNITURE")},
            {Category.MorganaFurniture, LanguageController.Translation("MORGANA_FURNITURE")},
            {Category.Table, LanguageController.Translation("TABLE")},
            {Category.RepairKit, LanguageController.Translation("REPAIR_KIT")},
            {Category.Unique, LanguageController.Translation("UNIQUE")},

            #endregion

            #region GatheringGear

            {Category.FibergathererArmor, LanguageController.Translation("FIBERGATHERER_ARMOR")},
            {Category.FibergathererHelmet, LanguageController.Translation("FIBERGATHERER_HELMET")},
            {Category.FibergathererShoes, LanguageController.Translation("FIBERGATHERER_SHOES")},
            {Category.FibergathererBackpack, LanguageController.Translation("FIBERGATHERER_BACKPACK")},

            {Category.FishgathererArmor, LanguageController.Translation("FISHGATHERER_ARMOR")},
            {Category.FishgathererHelmet, LanguageController.Translation("FISHGATHERER_HELMET")},
            {Category.FishgathererShoes, LanguageController.Translation("FISHGATHERER_SHOES")},
            {Category.FishgathererBackpack, LanguageController.Translation("FISHGATHERER_BACKPACK")},

            {Category.HidegathererArmor, LanguageController.Translation("HIDEGATHERER_ARMOR")},
            {Category.HidegathererHelmet, LanguageController.Translation("HIDEGATHERER_HELMET")},
            {Category.HidegathererShoes, LanguageController.Translation("HIDEGATHERER_SHOES")},
            {Category.HidegathererBackpack, LanguageController.Translation("HIDEGATHERERR_BACKPACK")},

            {Category.OregathererArmor, LanguageController.Translation("OREGATHERER_ARMOR")},
            {Category.OregathererHelmet, LanguageController.Translation("OREGATHERER_HELMET")},
            {Category.OregathererShoes, LanguageController.Translation("OREGATHERER_SHOES")},
            {Category.OregathererBackpack, LanguageController.Translation("OREGATHERER_BACKPACK")},

            {Category.RockgathererArmor, LanguageController.Translation("ROCKGATHERER_ARMOR")},
            {Category.RockgathererHelmet, LanguageController.Translation("ROCKGATHERER_HELMET")},
            {Category.RockgathererShoes, LanguageController.Translation("ROCKGATHERER_SHOES")},
            {Category.RockgathererBackpack, LanguageController.Translation("ROCKGATHERER_BACKPACK")},

            {Category.WoodgathererArmor, LanguageController.Translation("WOODGATHERER_ARMOR")},
            {Category.WoodgathererHelmet, LanguageController.Translation("WOODGATHERER_HELMET")},
            {Category.WoodgathererShoes, LanguageController.Translation("WOODGATHERER_SHOES")},
            {Category.WoodgathererBackpack, LanguageController.Translation("WOODGATHERER_BACKPACK")},

            #endregion

            #region LuxuryGoods

            {Category.Bridgewatch, LanguageController.Translation("BRIDGEWATCH")},
            {Category.Caerleon, LanguageController.Translation("CAERLEON")},
            {Category.FortSterling, LanguageController.Translation("FORT_STERLING")},
            {Category.Lymhurst, LanguageController.Translation("LYMHURST")},
            {Category.Martlock, LanguageController.Translation("MARTLOCK")},
            {Category.Thetford, LanguageController.Translation("THETFORD")},

            #endregion

            #region Magic

            {Category.ArcaneStaff, LanguageController.Translation("ARCANE_STAFF")},
            {Category.CurseStaff, LanguageController.Translation("CURSE_STAFF")},
            {Category.FireStaff, LanguageController.Translation("FIRE_STAFF")},
            {Category.FrostStaff, LanguageController.Translation("FROST_STAFF")},
            {Category.HolyStaff, LanguageController.Translation("HOLY_STAFF")},
            {Category.NatureStaff, LanguageController.Translation("NATURE_STAFF")},

            #endregion

            #region Materials

            {Category.Essence, LanguageController.Translation("ESSENCE")},
            {Category.OtherMaterials, LanguageController.Translation("OTHER")},
            {Category.Relic, LanguageController.Translation("RELIC")},
            {Category.Rune, LanguageController.Translation("RUNE")},
            {Category.Soul, LanguageController.Translation("SOUL")},

            #endregion

            #region Melee

            {Category.Axe, LanguageController.Translation("AXE")},
            {Category.Dagger, LanguageController.Translation("DAGGER")},
            {Category.Hammer, LanguageController.Translation("HAMMER")},
            {Category.Mace, LanguageController.Translation("MACE")},
            {Category.QuarterStaff, LanguageController.Translation("QUARTER_STAFF")},
            {Category.Spear, LanguageController.Translation("SPEAR")},
            {Category.Sword, LanguageController.Translation("SWORD")},

            #endregion

            #region Mount

            {Category.ArmoredHorse, LanguageController.Translation("ARMORED_HORSE")},
            {Category.Ox, LanguageController.Translation("OX")},
            {Category.RareMount, LanguageController.Translation("RARE_MOUNT")},
            {Category.RidingHorse, LanguageController.Translation("RIDING_HORSE")},

            #endregion

            #region Off-Hand

            {Category.Book, LanguageController.Translation("BOOK")},
            {Category.Horn, LanguageController.Translation("HORN")},
            {Category.Orb, LanguageController.Translation("ORB")},
            {Category.Shield, LanguageController.Translation("SHIELD")},
            {Category.Torch, LanguageController.Translation("TORCH")},
            {Category.Totem, LanguageController.Translation("TOTEM")},

            #endregion

            #region Other

            {Category.Trash, LanguageController.Translation("TRASH")},

            #endregion

            #region Product

            {Category.Farming, LanguageController.Translation("FARMING")},
            {Category.Journal, LanguageController.Translation("JOURNAL")},

            #endregion

            #region Ranged

            {Category.Bow, LanguageController.Translation("BOW")},
            {Category.Crossbow, LanguageController.Translation("CROSSBOW")},

            #endregion

            #region Resource

            {Category.Cloth, LanguageController.Translation("CLOTH")},
            {Category.Fiber, LanguageController.Translation("FIBER")},
            {Category.Hide, LanguageController.Translation("HIDE")},
            {Category.Leather, LanguageController.Translation("LEATHER")},
            {Category.Metalbar, LanguageController.Translation("METALBAR")},
            {Category.Ore, LanguageController.Translation("ORE")},
            {Category.Planks, LanguageController.Translation("PLANKS")},
            {Category.Wood, LanguageController.Translation("WOOD")},
            {Category.Rock, LanguageController.Translation("ROCK")},
            {Category.Stoneblock, LanguageController.Translation("STONEBLOCK")},

            #endregion

            #region Token

            {Category.ArenaSigils, LanguageController.Translation("ARENA_SIGILS")},
            {Category.Event, LanguageController.Translation("EVENT")},
            {Category.RoyalSigils, LanguageController.Translation("ROYAL_SIGILS")},

            #endregion

            #region Tool

            {Category.DemolitionHammer, LanguageController.Translation("DEMOLITION_HAMMER")},
            {Category.Fishing, LanguageController.Translation("FISHING")},
            {Category.Pickaxe, LanguageController.Translation("PICKAXE")},
            {Category.Sickle, LanguageController.Translation("SICKLE")},
            {Category.SkinningKnife, LanguageController.Translation("SKINNING_KNIFE")},
            {Category.StoneHammer, LanguageController.Translation("STONE_HAMMER")},
            {Category.WoodAxe, LanguageController.Translation("WOOD_AXE")},

            #endregion

            #region Trophies

            {Category.FiberTrophy, LanguageController.Translation("FIBER_TROPHY")},
            {Category.FishTrophy, LanguageController.Translation("FISH_TROPHY")},
            {Category.GeneralTrophy, LanguageController.Translation("GENERAL_TROPHY")},
            {Category.HideTrophy, LanguageController.Translation("HIDE_TROPHY")},
            {Category.MercenaryTrophy, LanguageController.Translation("MERCENARY_TROPHY")},
            {Category.OreTrophy, LanguageController.Translation("ORE_TROPHY")},
            {Category.RockTrophy, LanguageController.Translation("ROCK_TROPHY")},
            {Category.WoodTrophy, LanguageController.Translation("WOOD_TROPHY")},

            #endregion
        };

        public static readonly Dictionary<ParentCategory, string> ParentCategoryNames = new Dictionary<ParentCategory, string>
        {
            {ParentCategory.Unknown, string.Empty},
            {ParentCategory.Accessories, LanguageController.Translation("ACCESSORIES")},
            {ParentCategory.Armor, LanguageController.Translation("ARMOR")},
            {ParentCategory.Artifact, LanguageController.Translation("ARTEFACT")},
            {ParentCategory.CityResources, LanguageController.Translation("CITY_RESOURCES")},
            {ParentCategory.Consumable, LanguageController.Translation("CONSUMABLE")},
            {ParentCategory.Farmable, LanguageController.Translation("FARMABLE")},
            {ParentCategory.Furniture, LanguageController.Translation("FURNITURE")},
            {ParentCategory.GatheringGear, LanguageController.Translation("GATHERING_GEAR")},
            {ParentCategory.LuxuryGoods, LanguageController.Translation("LUXURY_GOODS")},
            {ParentCategory.Magic, LanguageController.Translation("MAGIC")},
            {ParentCategory.Materials, LanguageController.Translation("MATERIALS")},
            {ParentCategory.Melee, LanguageController.Translation("MELEE")},
            {ParentCategory.Mount, LanguageController.Translation("MOUNT")},
            {ParentCategory.OffHand, LanguageController.Translation("OFFHAND")},
            {ParentCategory.Other, LanguageController.Translation("OTHER")},
            {ParentCategory.Product, LanguageController.Translation("PRODUCT")},
            {ParentCategory.Ranged, LanguageController.Translation("RANGED")},
            {ParentCategory.Resource, LanguageController.Translation("RESOURCE")},
            {ParentCategory.Token, LanguageController.Translation("TOKEN")},
            {ParentCategory.Tool, LanguageController.Translation("TOOL")},
            {ParentCategory.Trophies, LanguageController.Translation("TROPHIES")}
        };

        public static CategoryObject GetCategory(string categoryId) => Categories.SingleOrDefault(x => x.CategoryId == categoryId);

        public static string GetCategoryName(Category category) => CategoryNames.TryGetValue(category, out var name) ? name : null;

        public static string GetParentCategoryName(ParentCategory parentCategory) => ParentCategoryNames.TryGetValue(parentCategory, out var name) ? name : null;

        public static Dictionary<Category, string> GetCategoriesByParentCategory(ParentCategory parentCategory) =>
            Categories?.Where(x => x.ParentCategory == parentCategory).ToDictionary(x => x.Category, x => x.CategoryName);
    }

    public enum Category
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
    }

    public enum ParentCategory
    {
        Unknown,
        Accessories,
        Armor,
        Artifact,
        CityResources,
        Consumable,
        Farmable,
        Furniture,
        GatheringGear,
        LuxuryGoods,
        Magic,
        Materials,
        Melee,
        Mount,
        OffHand,
        Other,
        Product,
        Ranged,
        Resource,
        Token,
        Tool,
        Trophies
    }
}