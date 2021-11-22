using System.Collections.Generic;
using System.Linq;
using StatisticsAnalysisTool.Models;
// ReSharper disable All

namespace StatisticsAnalysisTool.Common
{
    public static class CategoryController
    {
        public readonly static List<CategoryObject> Categories = new()
        {
            new ("bag", Category.Bag, ParentCategory.Accessories),
            new ("cape", Category.Cape, ParentCategory.Accessories),

            new ("cloth_armor", Category.ClothArmor, ParentCategory.Armor),
            new ("cloth_helmet", Category.ClothHelmet, ParentCategory.Armor),
            new ("cloth_shoes", Category.ClothShoes, ParentCategory.Armor),
            new ("leather_armor", Category.LeatherArmor, ParentCategory.Armor),
            new ("leather_helmet", Category.LeatherHelmet, ParentCategory.Armor),
            new ("leather_shoes", Category.LeatherShoes, ParentCategory.Armor),
            new ("plate_armor", Category.PlateArmor, ParentCategory.Armor),
            new ("plate_helmet", Category.PlateHelmet, ParentCategory.Armor),
            new ("plate_shoes", Category.PlateShoes, ParentCategory.Armor),
            new ("unique_armor", Category.UniqueArmor, ParentCategory.Armor),
            new ("unique_helmet", Category.UniqueHelmet, ParentCategory.Armor),
            new ("unique_shoes", Category.UniqueShoes, ParentCategory.Armor),

            new ("armor_artefact", Category.ArmorArtefact, ParentCategory.Artifact),
            new ("magic_artefact", Category.MagicArtefact, ParentCategory.Artifact),
            new ("melee_artefact", Category.MeleeArtefact, ParentCategory.Artifact),
            new ("offhand_artefact", Category.OffhandArtefact, ParentCategory.Artifact),
            new ("ranged_artefact", Category.RangedArtefact, ParentCategory.Artifact),

            new ("beastheart", Category.BeastHeart, ParentCategory.CityResources),
            new ("mountainheart", Category.MountainHeart, ParentCategory.CityResources),
            new ("rockheart", Category.RockHeart, ParentCategory.CityResources),
            new ("treeheart", Category.TreeHeart, ParentCategory.CityResources),
            new ("vineheart", Category.VineHeart, ParentCategory.CityResources),

            new ("cooked", Category.Cooked, ParentCategory.Consumable),
            new ("fish", Category.Fish, ParentCategory.Consumable),
            new ("fishingbait", Category.FishingBait, ParentCategory.Consumable),
            new ("maps", Category.Maps, ParentCategory.Consumable),
            new ("Other", Category.Other, ParentCategory.Consumable),
            new ("potion", Category.Potion, ParentCategory.Consumable),
            new ("skillbook", Category.SkillBook, ParentCategory.Consumable),
            new ("vanity", Category.Vanity, ParentCategory.Consumable),

            new ("animals", Category.Animals, ParentCategory.Farmable),
            new ("seed", Category.Seed, ParentCategory.Farmable),

            new ("banner", Category.Banner, ParentCategory.Furniture),
            new ("bed", Category.Bed, ParentCategory.Furniture),
            new ("chest", Category.Chest, ParentCategory.Furniture),
            new ("decoration_furniture", Category.DecorationFurniture, ParentCategory.Furniture),
            new ("flag", Category.Flag, ParentCategory.Furniture),
            new ("heretic_furniture", Category.HereticFurniture, ParentCategory.Furniture),
            new ("keeper_furniture", Category.KeeperFurniture, ParentCategory.Furniture),
            new ("morgana_furniture", Category.MorganaFurniture, ParentCategory.Furniture),
            new ("table", Category.Table, ParentCategory.Furniture),
            new ("repairkit", Category.RepairKit, ParentCategory.Furniture),
            new ("unique", Category.Unique, ParentCategory.Furniture),

            new ("fibergatherer_armor", Category.FibergathererArmor, ParentCategory.GatheringGear),
            new ("fibergatherer_helmet", Category.FibergathererHelmet, ParentCategory.GatheringGear),
            new ("fibergatherer_shoes", Category.FibergathererShoes, ParentCategory.GatheringGear),
            new ("fibergatherer_backpack", Category.FibergathererBackpack, ParentCategory.GatheringGear),

            new ("fishgatherer_armor", Category.FishgathererArmor, ParentCategory.GatheringGear),
            new ("fishgatherer_helmet", Category.FishgathererHelmet, ParentCategory.GatheringGear),
            new ("fishgatherer_shoes", Category.FishgathererShoes, ParentCategory.GatheringGear),
            new ("fishgatherer_backpack", Category.FishgathererBackpack, ParentCategory.GatheringGear),

            new ("hidegatherer_armor", Category.HidegathererArmor, ParentCategory.GatheringGear),
            new ("hidegatherer_helmet", Category.HidegathererHelmet, ParentCategory.GatheringGear),
            new ("hidegatherer_shoes", Category.HidegathererShoes, ParentCategory.GatheringGear),
            new ("hidegatherer_backpack", Category.HidegathererBackpack, ParentCategory.GatheringGear),

            new ("oregatherer_armor", Category.OregathererArmor, ParentCategory.GatheringGear),
            new ("oregatherer_helmet", Category.OregathererHelmet, ParentCategory.GatheringGear),
            new ("oregatherer_shoes", Category.OregathererShoes, ParentCategory.GatheringGear),
            new ("oregatherer_backpack", Category.OregathererBackpack, ParentCategory.GatheringGear),

            new ("rockgatherer_armor", Category.RockgathererArmor, ParentCategory.GatheringGear),
            new ("rockgatherer_helmet", Category.RockgathererHelmet, ParentCategory.GatheringGear),
            new ("rockgatherer_shoes", Category.RockgathererShoes, ParentCategory.GatheringGear),
            new ("rockgatherer_backpack", Category.RockgathererBackpack, ParentCategory.GatheringGear),

            new ("woodgatherer_armor", Category.WoodgathererArmor, ParentCategory.GatheringGear),
            new ("woodgatherer_helmet", Category.WoodgathererHelmet, ParentCategory.GatheringGear),
            new ("woodgatherer_shoes", Category.WoodgathererShoes, ParentCategory.GatheringGear),
            new ("woodgatherer_backpack", Category.WoodgathererBackpack, ParentCategory.GatheringGear),

            new ("bridgewatch", Category.Bridgewatch, ParentCategory.LuxuryGoods),
            new ("caerleon", Category.Caerleon, ParentCategory.LuxuryGoods),
            new ("fortsterling", Category.FortSterling, ParentCategory.LuxuryGoods),
            new ("lymhurst", Category.Lymhurst, ParentCategory.LuxuryGoods),
            new ("martlock", Category.Martlock, ParentCategory.LuxuryGoods),
            new ("thetford", Category.Thetford, ParentCategory.LuxuryGoods),

            new ("arcanestaff", Category.ArcaneStaff, ParentCategory.Magic),
            new ("cursestaff", Category.CurseStaff, ParentCategory.Magic),
            new ("firestaff", Category.FireStaff, ParentCategory.Magic),
            new ("froststaff", Category.FrostStaff, ParentCategory.Magic),
            new ("holystaff", Category.HolyStaff, ParentCategory.Magic),
            new ("naturestaff", Category.NatureStaff, ParentCategory.Magic),

            new ("essence", Category.Essence, ParentCategory.Materials),
            new ("other", Category.OtherMaterials, ParentCategory.Materials),
            new ("relic", Category.Relic, ParentCategory.Materials),
            new ("rune", Category.Rune, ParentCategory.Materials),
            new ("soul", Category.Soul, ParentCategory.Materials),

            new ("axe", Category.Axe, ParentCategory.Melee),
            new ("dagger", Category.Dagger, ParentCategory.Melee),
            new ("hammer", Category.Hammer, ParentCategory.Melee),
            new ("mace", Category.Mace, ParentCategory.Melee),
            new ("quarterstaff", Category.QuarterStaff, ParentCategory.Melee),
            new ("spear", Category.Spear, ParentCategory.Melee),
            new ("sword", Category.Sword, ParentCategory.Melee),

            new ("armoredhorse", Category.ArmoredHorse, ParentCategory.Mount),
            new ("ox", Category.Ox, ParentCategory.Mount),
            new ("rare_mount", Category.RareMount, ParentCategory.Mount),
            new ("ridinghorse", Category.RidingHorse, ParentCategory.Mount),

            new ("book", Category.Book, ParentCategory.OffHand),
            new ("horn", Category.Horn, ParentCategory.OffHand),
            new ("orb", Category.Orb, ParentCategory.OffHand),
            new ("shield", Category.Shield, ParentCategory.OffHand),
            new ("torch", Category.Torch, ParentCategory.OffHand),
            new ("totem", Category.Totem, ParentCategory.OffHand),

            new ("trash", Category.Trash, ParentCategory.Other),
            new ("farming", Category.Farming, ParentCategory.Product),
            new ("journal", Category.Journal, ParentCategory.Product),

            new ("bow", Category.Bow, ParentCategory.Ranged),
            new ("crossbow", Category.Crossbow, ParentCategory.Ranged),

            new ("cloth", Category.Cloth, ParentCategory.Resource),
            new ("fiber", Category.Fiber, ParentCategory.Resource),
            new ("hide", Category.Hide, ParentCategory.Resource),
            new ("leather", Category.Leather, ParentCategory.Resource),
            new ("metalbar", Category.Metalbar, ParentCategory.Resource),
            new ("ore", Category.Ore, ParentCategory.Resource),
            new ("wood", Category.Wood, ParentCategory.Resource),
            new ("planks", Category.Planks, ParentCategory.Resource),
            new ("rock", Category.Rock, ParentCategory.Resource),
            new ("stoneblock", Category.Stoneblock, ParentCategory.Resource),

            new ("arenasigils", Category.ArenaSigils, ParentCategory.Token),
            new ("event", Category.Event, ParentCategory.Token),
            new ("royalsigils", Category.RoyalSigils, ParentCategory.Token),

            new ("demolitionhammer", Category.DemolitionHammer, ParentCategory.Tool),
            new ("fishing", Category.Fishing, ParentCategory.Tool),
            new ("pickaxe", Category.Pickaxe, ParentCategory.Tool),
            new ("sickle", Category.Sickle, ParentCategory.Tool),
            new ("skinningknife", Category.SkinningKnife, ParentCategory.Tool),
            new ("stonehammer", Category.StoneHammer, ParentCategory.Tool),
            new ("woodaxe", Category.WoodAxe, ParentCategory.Tool),

            new ("fibertrophy", Category.FiberTrophy, ParentCategory.Trophies),
            new ("fishtrophy", Category.FishTrophy, ParentCategory.Trophies),
            new ("generaltrophy", Category.GeneralTrophy, ParentCategory.Trophies),
            new ("hidetrophy", Category.HideTrophy, ParentCategory.Trophies),
            new ("mercenarytrophy", Category.MercenaryTrophy, ParentCategory.Trophies),
            new ("oretrophy", Category.OreTrophy, ParentCategory.Trophies),
            new ("rocktrophy", Category.RockTrophy, ParentCategory.Trophies),
            new ("woodtrophy", Category.WoodTrophy, ParentCategory.Trophies)
        };

        public static readonly Dictionary<Category, string> CategoryNames = new()
        {
            {Category.Unknown, LanguageController.Translation("UNKNOWN")},

            #region Accessories

            {Category.Bag, LanguageController.Translation("BAG")},
            {Category.Cape, LanguageController.Translation("CAPE")},

            #endregion Accessories

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

            #endregion Armor

            #region Artifact

            {Category.ArmorArtefact, LanguageController.Translation("ARMOR_ARTEFACT")},
            {Category.MagicArtefact, LanguageController.Translation("MAGIC_ARTEFACT")},
            {Category.MeleeArtefact, LanguageController.Translation("MELEE_ARTEFACT")},
            {Category.OffhandArtefact, LanguageController.Translation("OFFHAND_ARTEFACT")},
            {Category.RangedArtefact, LanguageController.Translation("RANGED_ARTEFACT")},

            #endregion Artifact

            #region CityResources

            {Category.BeastHeart, LanguageController.Translation("BEASTHEART")},
            {Category.MountainHeart, LanguageController.Translation("MOUNTAINHEART")},
            {Category.RockHeart, LanguageController.Translation("ROCKHEART")},
            {Category.TreeHeart, LanguageController.Translation("TREEHEART")},
            {Category.VineHeart, LanguageController.Translation("VINEHEART")},

            #endregion CityResources

            #region Consumable

            {Category.Cooked, LanguageController.Translation("COOKED")},
            {Category.Fish, LanguageController.Translation("FISH")},
            {Category.FishingBait, LanguageController.Translation("FISHING_BAIT")},
            {Category.Maps, LanguageController.Translation("MAPS")},
            {Category.Other, LanguageController.Translation("OTHER")},
            {Category.Potion, LanguageController.Translation("POTION")},
            {Category.SkillBook, LanguageController.Translation("SKILL_BOOK")},
            {Category.Vanity, LanguageController.Translation("VANITY")},

            #endregion Consumable

            #region Farmable

            {Category.Animals, LanguageController.Translation("ANIMALS")},
            {Category.Seed, LanguageController.Translation("SEED")},

            #endregion Farmable

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

            #endregion Furniture

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

            #endregion GatheringGear

            #region LuxuryGoods

            {Category.Bridgewatch, LanguageController.Translation("BRIDGEWATCH")},
            {Category.Caerleon, LanguageController.Translation("CAERLEON")},
            {Category.FortSterling, LanguageController.Translation("FORT_STERLING")},
            {Category.Lymhurst, LanguageController.Translation("LYMHURST")},
            {Category.Martlock, LanguageController.Translation("MARTLOCK")},
            {Category.Thetford, LanguageController.Translation("THETFORD")},

            #endregion LuxuryGoods

            #region Magic

            {Category.ArcaneStaff, LanguageController.Translation("ARCANE_STAFF")},
            {Category.CurseStaff, LanguageController.Translation("CURSE_STAFF")},
            {Category.FireStaff, LanguageController.Translation("FIRE_STAFF")},
            {Category.FrostStaff, LanguageController.Translation("FROST_STAFF")},
            {Category.HolyStaff, LanguageController.Translation("HOLY_STAFF")},
            {Category.NatureStaff, LanguageController.Translation("NATURE_STAFF")},

            #endregion Magic

            #region Materials

            {Category.Essence, LanguageController.Translation("ESSENCE")},
            {Category.OtherMaterials, LanguageController.Translation("OTHER")},
            {Category.Relic, LanguageController.Translation("RELIC")},
            {Category.Rune, LanguageController.Translation("RUNE")},
            {Category.Soul, LanguageController.Translation("SOUL")},

            #endregion Materials

            #region Melee

            {Category.Axe, LanguageController.Translation("AXE")},
            {Category.Dagger, LanguageController.Translation("DAGGER")},
            {Category.Hammer, LanguageController.Translation("HAMMER")},
            {Category.Mace, LanguageController.Translation("MACE")},
            {Category.QuarterStaff, LanguageController.Translation("QUARTER_STAFF")},
            {Category.Spear, LanguageController.Translation("SPEAR")},
            {Category.Sword, LanguageController.Translation("SWORD")},

            #endregion Melee

            #region Mount

            {Category.ArmoredHorse, LanguageController.Translation("ARMORED_HORSE")},
            {Category.Ox, LanguageController.Translation("OX")},
            {Category.RareMount, LanguageController.Translation("RARE_MOUNT")},
            {Category.RidingHorse, LanguageController.Translation("RIDING_HORSE")},

            #endregion Mount

            #region Off-Hand

            {Category.Book, LanguageController.Translation("BOOK")},
            {Category.Horn, LanguageController.Translation("HORN")},
            {Category.Orb, LanguageController.Translation("ORB")},
            {Category.Shield, LanguageController.Translation("SHIELD")},
            {Category.Torch, LanguageController.Translation("TORCH")},
            {Category.Totem, LanguageController.Translation("TOTEM")},

            #endregion Off-Hand

            #region Other

            {Category.Trash, LanguageController.Translation("TRASH")},

            #endregion Other

            #region Product

            {Category.Farming, LanguageController.Translation("FARMING")},
            {Category.Journal, LanguageController.Translation("JOURNAL")},

            #endregion Product

            #region Ranged

            {Category.Bow, LanguageController.Translation("BOW")},
            {Category.Crossbow, LanguageController.Translation("CROSSBOW")},

            #endregion Ranged

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

            #endregion Resource

            #region Token

            {Category.ArenaSigils, LanguageController.Translation("ARENA_SIGILS")},
            {Category.Event, LanguageController.Translation("EVENT")},
            {Category.RoyalSigils, LanguageController.Translation("ROYAL_SIGILS")},

            #endregion Token

            #region Tool

            {Category.DemolitionHammer, LanguageController.Translation("DEMOLITION_HAMMER")},
            {Category.Fishing, LanguageController.Translation("FISHING")},
            {Category.Pickaxe, LanguageController.Translation("PICKAXE")},
            {Category.Sickle, LanguageController.Translation("SICKLE")},
            {Category.SkinningKnife, LanguageController.Translation("SKINNING_KNIFE")},
            {Category.StoneHammer, LanguageController.Translation("STONE_HAMMER")},
            {Category.WoodAxe, LanguageController.Translation("WOOD_AXE")},

            #endregion Tool

            #region Trophies

            {Category.FiberTrophy, LanguageController.Translation("FIBER_TROPHY")},
            {Category.FishTrophy, LanguageController.Translation("FISH_TROPHY")},
            {Category.GeneralTrophy, LanguageController.Translation("GENERAL_TROPHY")},
            {Category.HideTrophy, LanguageController.Translation("HIDE_TROPHY")},
            {Category.MercenaryTrophy, LanguageController.Translation("MERCENARY_TROPHY")},
            {Category.OreTrophy, LanguageController.Translation("ORE_TROPHY")},
            {Category.RockTrophy, LanguageController.Translation("ROCK_TROPHY")},
            {Category.WoodTrophy, LanguageController.Translation("WOOD_TROPHY")},

            #endregion Trophies
        };

        public static readonly Dictionary<ParentCategory, string> ParentCategoryNames = new()
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

        public static CategoryObject GetCategory(string categoryId)
        {
            return Categories.SingleOrDefault(x => x.CategoryId == categoryId);
        }

        public static string GetCategoryName(Category category)
        {
            return CategoryNames.TryGetValue(category, out var name) ? name : null;
        }

        public static string GetParentCategoryName(ParentCategory parentCategory)
        {
            return ParentCategoryNames.TryGetValue(parentCategory, out var name) ? name : null;
        }

        public static Dictionary<Category, string> GetCategoriesByParentCategory(ParentCategory parentCategory)
        {
            return Categories?.Where(x => x.ParentCategory == parentCategory).ToDictionary(x => x.Category, x => x.CategoryName);
        }
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
        WoodTrophy
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