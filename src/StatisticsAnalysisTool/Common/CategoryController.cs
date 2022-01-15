using System.Collections.Generic;
using System.Linq;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Common
{
    public static class CategoryController
    {
        public static readonly List<CategoryObject> SubCategories = new()
        {
            new ("bag", SubCategory.Bag, Category.Accessories),
            new ("cape", SubCategory.Cape, Category.Accessories),

            new ("cloth_armor", SubCategory.ClothArmor, Category.Armor),
            new ("cloth_helmet", SubCategory.ClothHelmet, Category.Armor),
            new ("cloth_shoes", SubCategory.ClothShoes, Category.Armor),
            new ("leather_armor", SubCategory.LeatherArmor, Category.Armor),
            new ("leather_helmet", SubCategory.LeatherHelmet, Category.Armor),
            new ("leather_shoes", SubCategory.LeatherShoes, Category.Armor),
            new ("plate_armor", SubCategory.PlateArmor, Category.Armor),
            new ("plate_helmet", SubCategory.PlateHelmet, Category.Armor),
            new ("plate_shoes", SubCategory.PlateShoes, Category.Armor),
            new ("unique_armor", SubCategory.UniqueArmor, Category.Armor),
            new ("unique_helmet", SubCategory.UniqueHelmet, Category.Armor),
            new ("unique_shoes", SubCategory.UniqueShoes, Category.Armor),

            new ("armor_artefact", SubCategory.ArmorArtefact, Category.Artifact),
            new ("magic_artefact", SubCategory.MagicArtefact, Category.Artifact),
            new ("melee_artefact", SubCategory.MeleeArtefact, Category.Artifact),
            new ("offhand_artefact", SubCategory.OffhandArtefact, Category.Artifact),
            new ("ranged_artefact", SubCategory.RangedArtefact, Category.Artifact),

            new ("beastheart", SubCategory.BeastHeart, Category.CityResources),
            new ("mountainheart", SubCategory.MountainHeart, Category.CityResources),
            new ("rockheart", SubCategory.RockHeart, Category.CityResources),
            new ("treeheart", SubCategory.TreeHeart, Category.CityResources),
            new ("vineheart", SubCategory.VineHeart, Category.CityResources),

            new ("cooked", SubCategory.Cooked, Category.Consumables),
            new ("fish", SubCategory.Fish, Category.Consumables),
            new ("fishingbait", SubCategory.FishingBait, Category.Consumables),
            new ("maps", SubCategory.Maps, Category.Consumables),
            new ("Other", SubCategory.Other, Category.Consumables),
            new ("potion", SubCategory.Potion, Category.Consumables),
            new ("skillbook", SubCategory.SkillBook, Category.Consumables),
            new ("vanity", SubCategory.Vanity, Category.Consumables),

            new ("animals", SubCategory.Animals, Category.Farmable),
            new ("seed", SubCategory.Seed, Category.Farmable),

            new ("banner", SubCategory.Banner, Category.Furniture),
            new ("bed", SubCategory.Bed, Category.Furniture),
            new ("chest", SubCategory.Chest, Category.Furniture),
            new ("decoration_furniture", SubCategory.DecorationFurniture, Category.Furniture),
            new ("flag", SubCategory.Flag, Category.Furniture),
            new ("heretic_furniture", SubCategory.HereticFurniture, Category.Furniture),
            new ("keeper_furniture", SubCategory.KeeperFurniture, Category.Furniture),
            new ("morgana_furniture", SubCategory.MorganaFurniture, Category.Furniture),
            new ("table", SubCategory.Table, Category.Furniture),
            new ("repairkit", SubCategory.RepairKit, Category.Furniture),
            new ("unique", SubCategory.Unique, Category.Furniture),

            new ("fibergatherer_armor", SubCategory.FibergathererArmor, Category.GatheringGear),
            new ("fibergatherer_helmet", SubCategory.FibergathererHelmet, Category.GatheringGear),
            new ("fibergatherer_shoes", SubCategory.FibergathererShoes, Category.GatheringGear),
            new ("fibergatherer_backpack", SubCategory.FibergathererBackpack, Category.GatheringGear),

            new ("fishgatherer_armor", SubCategory.FishgathererArmor, Category.GatheringGear),
            new ("fishgatherer_helmet", SubCategory.FishgathererHelmet, Category.GatheringGear),
            new ("fishgatherer_shoes", SubCategory.FishgathererShoes, Category.GatheringGear),
            new ("fishgatherer_backpack", SubCategory.FishgathererBackpack, Category.GatheringGear),

            new ("hidegatherer_armor", SubCategory.HidegathererArmor, Category.GatheringGear),
            new ("hidegatherer_helmet", SubCategory.HidegathererHelmet, Category.GatheringGear),
            new ("hidegatherer_shoes", SubCategory.HidegathererShoes, Category.GatheringGear),
            new ("hidegatherer_backpack", SubCategory.HidegathererBackpack, Category.GatheringGear),

            new ("oregatherer_armor", SubCategory.OregathererArmor, Category.GatheringGear),
            new ("oregatherer_helmet", SubCategory.OregathererHelmet, Category.GatheringGear),
            new ("oregatherer_shoes", SubCategory.OregathererShoes, Category.GatheringGear),
            new ("oregatherer_backpack", SubCategory.OregathererBackpack, Category.GatheringGear),

            new ("rockgatherer_armor", SubCategory.RockgathererArmor, Category.GatheringGear),
            new ("rockgatherer_helmet", SubCategory.RockgathererHelmet, Category.GatheringGear),
            new ("rockgatherer_shoes", SubCategory.RockgathererShoes, Category.GatheringGear),
            new ("rockgatherer_backpack", SubCategory.RockgathererBackpack, Category.GatheringGear),

            new ("woodgatherer_armor", SubCategory.WoodgathererArmor, Category.GatheringGear),
            new ("woodgatherer_helmet", SubCategory.WoodgathererHelmet, Category.GatheringGear),
            new ("woodgatherer_shoes", SubCategory.WoodgathererShoes, Category.GatheringGear),
            new ("woodgatherer_backpack", SubCategory.WoodgathererBackpack, Category.GatheringGear),

            new ("bridgewatch", SubCategory.Bridgewatch, Category.LuxuryGoods),
            new ("caerleon", SubCategory.Caerleon, Category.LuxuryGoods),
            new ("fortsterling", SubCategory.FortSterling, Category.LuxuryGoods),
            new ("lymhurst", SubCategory.Lymhurst, Category.LuxuryGoods),
            new ("martlock", SubCategory.Martlock, Category.LuxuryGoods),
            new ("thetford", SubCategory.Thetford, Category.LuxuryGoods),

            new ("arcanestaff", SubCategory.ArcaneStaff, Category.Magic),
            new ("cursestaff", SubCategory.CurseStaff, Category.Magic),
            new ("firestaff", SubCategory.FireStaff, Category.Magic),
            new ("froststaff", SubCategory.FrostStaff, Category.Magic),
            new ("holystaff", SubCategory.HolyStaff, Category.Magic),
            new ("naturestaff", SubCategory.NatureStaff, Category.Magic),

            new ("essence", SubCategory.Essence, Category.Materials),
            new ("other", SubCategory.OtherMaterials, Category.Materials),
            new ("relic", SubCategory.Relic, Category.Materials),
            new ("rune", SubCategory.Rune, Category.Materials),
            new ("soul", SubCategory.Soul, Category.Materials),

            new ("axe", SubCategory.Axe, Category.Melee),
            new ("dagger", SubCategory.Dagger, Category.Melee),
            new ("hammer", SubCategory.Hammer, Category.Melee),
            new ("mace", SubCategory.Mace, Category.Melee),
            new ("quarterstaff", SubCategory.QuarterStaff, Category.Melee),
            new ("spear", SubCategory.Spear, Category.Melee),
            new ("sword", SubCategory.Sword, Category.Melee),
            new ("knuckles", SubCategory.Knuckles, Category.Melee),

            new ("armoredhorse", SubCategory.ArmoredHorse, Category.Mounts),
            new ("ox", SubCategory.Ox, Category.Mounts),
            new ("rare_mount", SubCategory.RareMount, Category.Mounts),
            new ("ridinghorse", SubCategory.RidingHorse, Category.Mounts),

            new ("book", SubCategory.Book, Category.OffHand),
            new ("horn", SubCategory.Horn, Category.OffHand),
            new ("orb", SubCategory.Orb, Category.OffHand),
            new ("shield", SubCategory.Shield, Category.OffHand),
            new ("torch", SubCategory.Torch, Category.OffHand),
            new ("totem", SubCategory.Totem, Category.OffHand),

            new ("trash", SubCategory.Trash, Category.Other),
            new ("farming", SubCategory.Farming, Category.Products),
            new ("journal", SubCategory.Journal, Category.Products),

            new ("bow", SubCategory.Bow, Category.Ranged),
            new ("crossbow", SubCategory.Crossbow, Category.Ranged),

            new ("cloth", SubCategory.Cloth, Category.Resources),
            new ("fiber", SubCategory.Fiber, Category.Resources),
            new ("hide", SubCategory.Hide, Category.Resources),
            new ("leather", SubCategory.Leather, Category.Resources),
            new ("metalbar", SubCategory.Metalbar, Category.Resources),
            new ("ore", SubCategory.Ore, Category.Resources),
            new ("wood", SubCategory.Wood, Category.Resources),
            new ("planks", SubCategory.Planks, Category.Resources),
            new ("rock", SubCategory.Rock, Category.Resources),
            new ("stoneblock", SubCategory.Stoneblock, Category.Resources),

            new ("arenasigils", SubCategory.ArenaSigils, Category.Token),
            new ("event", SubCategory.Event, Category.Token),
            new ("royalsigils", SubCategory.RoyalSigils, Category.Token),

            new ("demolitionhammer", SubCategory.DemolitionHammer, Category.Tools),
            new ("fishing", SubCategory.Fishing, Category.Tools),
            new ("pickaxe", SubCategory.Pickaxe, Category.Tools),
            new ("sickle", SubCategory.Sickle, Category.Tools),
            new ("skinningknife", SubCategory.SkinningKnife, Category.Tools),
            new ("stonehammer", SubCategory.StoneHammer, Category.Tools),
            new ("woodaxe", SubCategory.WoodAxe, Category.Tools),

            new ("fibertrophy", SubCategory.FiberTrophy, Category.Trophies),
            new ("fishtrophy", SubCategory.FishTrophy, Category.Trophies),
            new ("generaltrophy", SubCategory.GeneralTrophy, Category.Trophies),
            new ("hidetrophy", SubCategory.HideTrophy, Category.Trophies),
            new ("mercenarytrophy", SubCategory.MercenaryTrophy, Category.Trophies),
            new ("oretrophy", SubCategory.OreTrophy, Category.Trophies),
            new ("rocktrophy", SubCategory.RockTrophy, Category.Trophies),
            new ("woodtrophy", SubCategory.WoodTrophy, Category.Trophies)
        };

        public static readonly Dictionary<SubCategory, string> SubCategoryNames = new()
        {
            {SubCategory.Unknown, LanguageController.Translation("UNKNOWN")},

            #region Accessories

            {SubCategory.Bag, LanguageController.Translation("BAG")},
            {SubCategory.Cape, LanguageController.Translation("CAPE")},

            #endregion Accessories

            #region Armor

            {SubCategory.ClothArmor, LanguageController.Translation("CLOTH_ARMOR")},
            {SubCategory.ClothHelmet, LanguageController.Translation("CLOTH_HELMET")},
            {SubCategory.ClothShoes, LanguageController.Translation("CLOTH_SHOES")},
            {SubCategory.LeatherArmor, LanguageController.Translation("LEATHER_ARMOR")},
            {SubCategory.LeatherHelmet, LanguageController.Translation("LEATHER_HELMET")},
            {SubCategory.LeatherShoes, LanguageController.Translation("LEATHER_SHOES")},
            {SubCategory.PlateArmor, LanguageController.Translation("PLATE_ARMOR")},
            {SubCategory.PlateHelmet, LanguageController.Translation("PLATE_HELMET")},
            {SubCategory.PlateShoes, LanguageController.Translation("PLATE_SHOES")},
            {SubCategory.UniqueArmor, LanguageController.Translation("UNIQUE_ARMOR")},
            {SubCategory.UniqueHelmet, LanguageController.Translation("UNIQUE_HELMET")},
            {SubCategory.UniqueShoes, LanguageController.Translation("UNIQUE_SHOES")},

            #endregion Armor

            #region Artifact

            {SubCategory.ArmorArtefact, LanguageController.Translation("ARMOR_ARTEFACT")},
            {SubCategory.MagicArtefact, LanguageController.Translation("MAGIC_ARTEFACT")},
            {SubCategory.MeleeArtefact, LanguageController.Translation("MELEE_ARTEFACT")},
            {SubCategory.OffhandArtefact, LanguageController.Translation("OFFHAND_ARTEFACT")},
            {SubCategory.RangedArtefact, LanguageController.Translation("RANGED_ARTEFACT")},

            #endregion Artifact

            #region CityResources

            {SubCategory.BeastHeart, LanguageController.Translation("BEASTHEART")},
            {SubCategory.MountainHeart, LanguageController.Translation("MOUNTAINHEART")},
            {SubCategory.RockHeart, LanguageController.Translation("ROCKHEART")},
            {SubCategory.TreeHeart, LanguageController.Translation("TREEHEART")},
            {SubCategory.VineHeart, LanguageController.Translation("VINEHEART")},

            #endregion CityResources

            #region Consumable

            {SubCategory.Cooked, LanguageController.Translation("COOKED")},
            {SubCategory.Fish, LanguageController.Translation("FISH")},
            {SubCategory.FishingBait, LanguageController.Translation("FISHING_BAIT")},
            {SubCategory.Maps, LanguageController.Translation("MAPS")},
            {SubCategory.Other, LanguageController.Translation("OTHER")},
            {SubCategory.Potion, LanguageController.Translation("POTION")},
            {SubCategory.SkillBook, LanguageController.Translation("SKILL_BOOK")},
            {SubCategory.Vanity, LanguageController.Translation("VANITY")},

            #endregion Consumable

            #region Farmable

            {SubCategory.Animals, LanguageController.Translation("ANIMALS")},
            {SubCategory.Seed, LanguageController.Translation("SEED")},

            #endregion Farmable

            #region Furniture

            {SubCategory.Banner, LanguageController.Translation("BANNER")},
            {SubCategory.Bed, LanguageController.Translation("BED")},
            {SubCategory.Chest, LanguageController.Translation("CHEST")},
            {SubCategory.DecorationFurniture, LanguageController.Translation("DECORATION_FURNITURE")},
            {SubCategory.Flag, LanguageController.Translation("FLAG")},
            {SubCategory.HereticFurniture, LanguageController.Translation("HERETIC_FURNITURE")},
            {SubCategory.KeeperFurniture, LanguageController.Translation("KEEPER_FURNITURE")},
            {SubCategory.MorganaFurniture, LanguageController.Translation("MORGANA_FURNITURE")},
            {SubCategory.Table, LanguageController.Translation("TABLE")},
            {SubCategory.RepairKit, LanguageController.Translation("REPAIR_KIT")},
            {SubCategory.Unique, LanguageController.Translation("UNIQUE")},

            #endregion Furniture

            #region GatheringGear

            {SubCategory.FibergathererArmor, LanguageController.Translation("FIBERGATHERER_ARMOR")},
            {SubCategory.FibergathererHelmet, LanguageController.Translation("FIBERGATHERER_HELMET")},
            {SubCategory.FibergathererShoes, LanguageController.Translation("FIBERGATHERER_SHOES")},
            {SubCategory.FibergathererBackpack, LanguageController.Translation("FIBERGATHERER_BACKPACK")},

            {SubCategory.FishgathererArmor, LanguageController.Translation("FISHGATHERER_ARMOR")},
            {SubCategory.FishgathererHelmet, LanguageController.Translation("FISHGATHERER_HELMET")},
            {SubCategory.FishgathererShoes, LanguageController.Translation("FISHGATHERER_SHOES")},
            {SubCategory.FishgathererBackpack, LanguageController.Translation("FISHGATHERER_BACKPACK")},

            {SubCategory.HidegathererArmor, LanguageController.Translation("HIDEGATHERER_ARMOR")},
            {SubCategory.HidegathererHelmet, LanguageController.Translation("HIDEGATHERER_HELMET")},
            {SubCategory.HidegathererShoes, LanguageController.Translation("HIDEGATHERER_SHOES")},
            {SubCategory.HidegathererBackpack, LanguageController.Translation("HIDEGATHERERR_BACKPACK")},

            {SubCategory.OregathererArmor, LanguageController.Translation("OREGATHERER_ARMOR")},
            {SubCategory.OregathererHelmet, LanguageController.Translation("OREGATHERER_HELMET")},
            {SubCategory.OregathererShoes, LanguageController.Translation("OREGATHERER_SHOES")},
            {SubCategory.OregathererBackpack, LanguageController.Translation("OREGATHERER_BACKPACK")},

            {SubCategory.RockgathererArmor, LanguageController.Translation("ROCKGATHERER_ARMOR")},
            {SubCategory.RockgathererHelmet, LanguageController.Translation("ROCKGATHERER_HELMET")},
            {SubCategory.RockgathererShoes, LanguageController.Translation("ROCKGATHERER_SHOES")},
            {SubCategory.RockgathererBackpack, LanguageController.Translation("ROCKGATHERER_BACKPACK")},

            {SubCategory.WoodgathererArmor, LanguageController.Translation("WOODGATHERER_ARMOR")},
            {SubCategory.WoodgathererHelmet, LanguageController.Translation("WOODGATHERER_HELMET")},
            {SubCategory.WoodgathererShoes, LanguageController.Translation("WOODGATHERER_SHOES")},
            {SubCategory.WoodgathererBackpack, LanguageController.Translation("WOODGATHERER_BACKPACK")},

            #endregion GatheringGear

            #region LuxuryGoods

            {SubCategory.Bridgewatch, LanguageController.Translation("BRIDGEWATCH")},
            {SubCategory.Caerleon, LanguageController.Translation("CAERLEON")},
            {SubCategory.FortSterling, LanguageController.Translation("FORT_STERLING")},
            {SubCategory.Lymhurst, LanguageController.Translation("LYMHURST")},
            {SubCategory.Martlock, LanguageController.Translation("MARTLOCK")},
            {SubCategory.Thetford, LanguageController.Translation("THETFORD")},

            #endregion LuxuryGoods

            #region Magic

            {SubCategory.ArcaneStaff, LanguageController.Translation("ARCANE_STAFF")},
            {SubCategory.CurseStaff, LanguageController.Translation("CURSE_STAFF")},
            {SubCategory.FireStaff, LanguageController.Translation("FIRE_STAFF")},
            {SubCategory.FrostStaff, LanguageController.Translation("FROST_STAFF")},
            {SubCategory.HolyStaff, LanguageController.Translation("HOLY_STAFF")},
            {SubCategory.NatureStaff, LanguageController.Translation("NATURE_STAFF")},

            #endregion Magic

            #region Materials

            {SubCategory.Essence, LanguageController.Translation("ESSENCE")},
            {SubCategory.OtherMaterials, LanguageController.Translation("OTHER")},
            {SubCategory.Relic, LanguageController.Translation("RELIC")},
            {SubCategory.Rune, LanguageController.Translation("RUNE")},
            {SubCategory.Soul, LanguageController.Translation("SOUL")},

            #endregion Materials

            #region Melee

            {SubCategory.Axe, LanguageController.Translation("AXE")},
            {SubCategory.Dagger, LanguageController.Translation("DAGGER")},
            {SubCategory.Hammer, LanguageController.Translation("HAMMER")},
            {SubCategory.Mace, LanguageController.Translation("MACE")},
            {SubCategory.QuarterStaff, LanguageController.Translation("QUARTER_STAFF")},
            {SubCategory.Spear, LanguageController.Translation("SPEAR")},
            {SubCategory.Sword, LanguageController.Translation("SWORD")},
            {SubCategory.Knuckles, LanguageController.Translation("WAR_GLOVES") },

            #endregion Melee

            #region Mount

            {SubCategory.ArmoredHorse, LanguageController.Translation("ARMORED_HORSE")},
            {SubCategory.Ox, LanguageController.Translation("OX")},
            {SubCategory.RareMount, LanguageController.Translation("RARE_MOUNT")},
            {SubCategory.RidingHorse, LanguageController.Translation("RIDING_HORSE")},

            #endregion Mount

            #region Off-Hand

            {SubCategory.Book, LanguageController.Translation("BOOK")},
            {SubCategory.Horn, LanguageController.Translation("HORN")},
            {SubCategory.Orb, LanguageController.Translation("ORB")},
            {SubCategory.Shield, LanguageController.Translation("SHIELD")},
            {SubCategory.Torch, LanguageController.Translation("TORCH")},
            {SubCategory.Totem, LanguageController.Translation("TOTEM")},

            #endregion Off-Hand

            #region Other

            {SubCategory.Trash, LanguageController.Translation("TRASH")},

            #endregion Other

            #region Product

            {SubCategory.Farming, LanguageController.Translation("FARMING")},
            {SubCategory.Journal, LanguageController.Translation("JOURNAL")},

            #endregion Product

            #region Ranged

            {SubCategory.Bow, LanguageController.Translation("BOW")},
            {SubCategory.Crossbow, LanguageController.Translation("CROSSBOW")},

            #endregion Ranged

            #region Resource

            {SubCategory.Cloth, LanguageController.Translation("CLOTH")},
            {SubCategory.Fiber, LanguageController.Translation("FIBER")},
            {SubCategory.Hide, LanguageController.Translation("HIDE")},
            {SubCategory.Leather, LanguageController.Translation("LEATHER")},
            {SubCategory.Metalbar, LanguageController.Translation("METALBAR")},
            {SubCategory.Ore, LanguageController.Translation("ORE")},
            {SubCategory.Planks, LanguageController.Translation("PLANKS")},
            {SubCategory.Wood, LanguageController.Translation("WOOD")},
            {SubCategory.Rock, LanguageController.Translation("ROCK")},
            {SubCategory.Stoneblock, LanguageController.Translation("STONEBLOCK")},

            #endregion Resource

            #region Token

            {SubCategory.ArenaSigils, LanguageController.Translation("ARENA_SIGILS")},
            {SubCategory.Event, LanguageController.Translation("EVENT")},
            {SubCategory.RoyalSigils, LanguageController.Translation("ROYAL_SIGILS")},

            #endregion Token

            #region Tool

            {SubCategory.DemolitionHammer, LanguageController.Translation("DEMOLITION_HAMMER")},
            {SubCategory.Fishing, LanguageController.Translation("FISHING")},
            {SubCategory.Pickaxe, LanguageController.Translation("PICKAXE")},
            {SubCategory.Sickle, LanguageController.Translation("SICKLE")},
            {SubCategory.SkinningKnife, LanguageController.Translation("SKINNING_KNIFE")},
            {SubCategory.StoneHammer, LanguageController.Translation("STONE_HAMMER")},
            {SubCategory.WoodAxe, LanguageController.Translation("WOOD_AXE")},

            #endregion Tool

            #region Trophies

            {SubCategory.FiberTrophy, LanguageController.Translation("FIBER_TROPHY")},
            {SubCategory.FishTrophy, LanguageController.Translation("FISH_TROPHY")},
            {SubCategory.GeneralTrophy, LanguageController.Translation("GENERAL_TROPHY")},
            {SubCategory.HideTrophy, LanguageController.Translation("HIDE_TROPHY")},
            {SubCategory.MercenaryTrophy, LanguageController.Translation("MERCENARY_TROPHY")},
            {SubCategory.OreTrophy, LanguageController.Translation("ORE_TROPHY")},
            {SubCategory.RockTrophy, LanguageController.Translation("ROCK_TROPHY")},
            {SubCategory.WoodTrophy, LanguageController.Translation("WOOD_TROPHY")},

            #endregion Trophies
        };

        public static readonly Dictionary<Category, string> CategoryNames = new()
        {
            {Category.Unknown, string.Empty},
            {Category.Accessories, LanguageController.Translation("ACCESSORIES")},
            {Category.Armor, LanguageController.Translation("ARMOR")},
            {Category.Artifact, LanguageController.Translation("ARTEFACT")},
            {Category.CityResources, LanguageController.Translation("CITY_RESOURCES")},
            {Category.Consumables, LanguageController.Translation("CONSUMABLE")},
            {Category.Farmable, LanguageController.Translation("FARMABLE")},
            {Category.Furniture, LanguageController.Translation("FURNITURE")},
            {Category.GatheringGear, LanguageController.Translation("GATHERING_GEAR")},
            {Category.LuxuryGoods, LanguageController.Translation("LUXURY_GOODS")},
            {Category.Magic, LanguageController.Translation("MAGIC")},
            {Category.Materials, LanguageController.Translation("MATERIALS")},
            {Category.Melee, LanguageController.Translation("MELEE")},
            {Category.Mounts, LanguageController.Translation("MOUNT")},
            {Category.OffHand, LanguageController.Translation("OFFHAND")},
            {Category.Other, LanguageController.Translation("OTHER")},
            {Category.Products, LanguageController.Translation("PRODUCT")},
            {Category.Ranged, LanguageController.Translation("RANGED")},
            {Category.Resources, LanguageController.Translation("RESOURCE")},
            {Category.Token, LanguageController.Translation("TOKEN")},
            {Category.Tools, LanguageController.Translation("TOOL")},
            {Category.Trophies, LanguageController.Translation("TROPHIES")}
        };

        public static Category ShopCategoryToCategory(string value)
        {
            return value.ToLower() switch
            {
                "melee" => Category.Melee,
                "magic" => Category.Magic,
                "ranged" => Category.Ranged,
                "offhand" => Category.OffHand,
                "armor" => Category.Armor,
                "accessories" => Category.Accessories,
                "mounts" => Category.Mounts,
                "gatherergear" => Category.GatheringGear,
                "tools" => Category.Tools,
                "consumables" => Category.Consumables,
                "skillbooks" => Category.SkillBooks,
                "resources" => Category.Resources,
                "cityresources" => Category.CityResources,
                "artefacts" => Category.Artifact,
                "materials" => Category.Materials,
                "token" => Category.Token,
                "farmables" => Category.Farmable,
                "products" => Category.Products,
                "luxurygoods" => Category.LuxuryGoods,
                "trophies" => Category.Trophies,
                "furniture" => Category.Furniture,
                "labourers" => Category.Labourers,
                "other" => Category.Other,
                _ => Category.Unknown
            };
        }

        public static CategoryObject GetSubCategory(string categoryId)
        {
            return SubCategories.SingleOrDefault(x => x.CategoryId == categoryId);
        }

        public static string GetSubCategoryName(SubCategory subCategory)
        {
            return SubCategoryNames.TryGetValue(subCategory, out var name) ? name : null;
        }

        public static string GetCategoryName(Category category)
        {
            return CategoryNames.TryGetValue(category, out var name) ? name : null;
        }

        public static Dictionary<SubCategory, string> GetSubCategoriesByCategory(Category category)
        {
            return SubCategories?.Where(x => x.Category == category).ToDictionary(x => x.SubCategory, x => x.SubCategoryName);
        }
    }

    public enum SubCategory
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

    public enum Category
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