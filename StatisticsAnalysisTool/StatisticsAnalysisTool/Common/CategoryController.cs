using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common
{
    public static class CategoryController
    {
        public static readonly Dictionary<Category, string> Categories = new Dictionary<Category, string>
        {
            #region Accessories

            {Category.Bag, "bag"},
            {Category.Cape, "cape"},

            #endregion

            #region Armor

            {Category.ClothArmor, "cloth_armor"},
            {Category.ClothHelmet, "cloth_helmet"},
            {Category.ClothShoes, "cloth_shoes"},
            {Category.LeatherArmor, "leather_armor"},
            {Category.LeatherHelmet, "leather_helmet"},
            {Category.LeatherShoes, "leather_shoes"},
            {Category.PlateArmor, "plate_armor"},
            {Category.PlateHelmet, "plate_helmet"},
            {Category.PlateShoes, "plate_shoes"},
            {Category.UniqueArmor, "unique_armor"},
            {Category.UniqueHelmet, "unique_helmet"},
            {Category.UniqueShoes, "unique_shoes"},

            #endregion

            #region Artifact

            {Category.ArmorArtefact, "armor_artefact"},
            {Category.MagicArtefact, "magic_artefact"},
            {Category.MeleeArtefact, "melee_artefact"},
            {Category.OffhandArtefact, "offhand_artefact"},
            {Category.RangedArtefact, "ranged_artefact"},

            #endregion

            #region CityResources

            {Category.BeastHeart, "beastheart"},
            {Category.MountainHeart, "mountainheart"},
            {Category.RockHeart, "rockheart"},
            {Category.TreeHeart, "treeheart"},
            {Category.VineHeart, "vineheart"},

            #endregion

            #region Consumable

            {Category.Cooked, "cooked"},
            {Category.Fish, "fish"},
            {Category.FishingBait, "fishingbait"},
            {Category.Maps, "maps"},
            {Category.Other, "Other"},
            {Category.SkillBook, "skillbook"},
            {Category.Vanity, "vanity"},

            #endregion

            #region Farmable

            {Category.Animals, "animals"},
            {Category.Seed, "seed"},

            #endregion

            #region Furniture

            {Category.Banner, "banner"},
            {Category.Bed, "bed"},
            {Category.Chest, "chest"},
            {Category.DecorationFurniture, "decoration_furniture"},
            {Category.Flag, "flag"},
            {Category.HereticFurniture, "heretic_furniture"},
            {Category.KeeperFurniture, "keeper_furniture"},
            {Category.MorganaFurniture, "morgana_furniture"},
            {Category.Table, "table"},
            {Category.RepairKit, "repairkit"},
            {Category.Unique, "unique"},

            #endregion

            #region GatheringGear

            {Category.FibergathererArmor, "fibergatherer_armor"},
            {Category.FibergathererHelmet, "fibergatherer_helmet"},
            {Category.FibergathererShoes, "fibergatherer_shoes"},
            {Category.FibergathererBackpack, "fibergatherer_backpack"},

            {Category.FishgathererArmor, "fishgatherer_armor"},
            {Category.FishgathererHelmet, "fishgatherer_helmet"},
            {Category.FishgathererShoes, "fishgatherer_shoes"},
            {Category.FishgathererBackpack, "fishgatherer_backpack"},

            {Category.HidegathererArmor, "hidegatherer_armor"},
            {Category.HidegathererHelmet, "hidegatherer_helmet"},
            {Category.HidegathererShoes, "hidegatherer_shoes"},
            {Category.HidegathererBackpack, "hidegatherer_backpack"},

            {Category.OregathererArmor, "oregatherer_armor"},
            {Category.OregathererHelmet, "oregatherer_helmet"},
            {Category.OregathererShoes, "oregatherer_shoes"},
            {Category.OregathererBackpack, "oregatherer_backpack"},

            {Category.RockgathererArmor, "rockgatherer_armor"},
            {Category.RockgathererHelmet, "rockgatherer_helmet"},
            {Category.RockgathererShoes, "rockgatherer_shoes"},
            {Category.RockgathererBackpack, "rockgatherer_backpack"},

            {Category.WoodgathererArmor, "woodgatherer_armor"},
            {Category.WoodgathererHelmet, "woodgatherer_helmet"},
            {Category.WoodgathererShoes, "woodgatherer_shoes"},
            {Category.WoodgathererBackpack, "woodgatherer_backpack"},

            #endregion

            #region LuxuryGoods

            {Category.Bridgewatch, "bridgewatch"},
            {Category.Caerleon, "caerleon"},
            {Category.FortSterling, "fortsterling"},
            {Category.Lymhurst, "lymhurst"},
            {Category.Martlock, "martlock"},
            {Category.Thetford, "thetford"},

            #endregion

            #region Magic

            {Category.ArcaneStaff, "arcanestaff"},
            {Category.CurseStaff, "cursestaff"},
            {Category.FireStaff, "firestaff"},
            {Category.FrostStaff, "froststaff"},
            {Category.HolyStaff, "holystaff"},
            {Category.NatureStaff, "naturestaff"},

            #endregion

            #region Materials

            {Category.Essence, "essence"},
            {Category.OtherMaterials, "other"},
            {Category.Relic, "relic"},
            {Category.Rune, "rune"},
            {Category.Soul, "soul"},

            #endregion

            #region Melee

            {Category.Axe, "axe"},
            {Category.Dagger, "dagger"},
            {Category.Hammer, "hammer"},
            {Category.Mace, "mace"},
            {Category.QuarterStaff, "quarterstaff"},
            {Category.Spear, "spear"},
            {Category.Sword, "sword"},

            #endregion

            #region Mount

            {Category.ArmoredHorse, "armoredhorse"},
            {Category.Ox, "ox"},
            {Category.RareMount, "rare_mount"},
            {Category.RidingHorse, "ridinghorse"},
            
            #endregion

            #region Off-Hand

            {Category.Book, "book"},
            {Category.Horn, "horn"},
            {Category.Orb, "orb"},
            {Category.Shield, "shield"},
            {Category.Torch, "torch"},
            {Category.Totem, "totem"},

            #endregion

            #region Other

            {Category.Trash, "trash"},

            #endregion

            #region Product

            {Category.Farming, "farming"},
            {Category.Journal, "journal"},

            #endregion

            #region Ranged

            {Category.Bow, "bow"},
            {Category.Crossbow, "crossbow"},

            #endregion

            #region Resource

            {Category.Cloth, "cloth"},
            {Category.Fiber, "fiber"},
            {Category.Hide, "hide"},
            {Category.Leather, "leather"},
            {Category.Metalbar, "metalbar"},
            {Category.Ore, "ore"},
            {Category.Planks, "planks"},
            {Category.Rock, "rock"},

            #endregion

            #region Token

            {Category.ArenaSigils, "arenasigils"},
            {Category.Event, "event"},
            {Category.RoyalSigils, "royalsigils"},
            
            #endregion

            #region Tool

            {Category.DemolitionHammer, "demolitionhammer"},
            {Category.Fishing, "fishing"},
            {Category.Pickaxe, "pickaxe"},
            {Category.Sickle, "sickle"},
            {Category.SkinningKnife, "skinningknife"},
            {Category.StoneHammer, "stonehammer"},
            {Category.WoodAxe, "woodaxe"},

            #endregion

            #region Trophies

            {Category.FiberTrophy, "fibertrophy"},
            {Category.FishTrophy, "fishtrophy"},
            {Category.GeneralTrophy, "generaltrophy"},
            {Category.HideTrophy, "hidetrophy"},
            {Category.MercenaryTrophy, "mercenarytrophy"},
            {Category.OreTrophy, "oretrophy"},
            {Category.RockTrophy, "rocktrophy"},
            {Category.WoodTrophy, "woodtrophy"},

            #endregion
        };
        
        public static readonly Dictionary<ParentCategory, Category> CategoryAssignment = new Dictionary<ParentCategory, Category>
        {
            #region Accessories

            {ParentCategory.Accessories, Category.Bag},
            {ParentCategory.Accessories, Category.Cape},

            #endregion
            
            #region Armor

            {ParentCategory.Armor, Category.ClothArmor},
            {ParentCategory.Armor, Category.ClothHelmet},
            {ParentCategory.Armor, Category.ClothShoes},
            {ParentCategory.Armor, Category.LeatherArmor},
            {ParentCategory.Armor, Category.LeatherHelmet},
            {ParentCategory.Armor, Category.LeatherShoes},
            {ParentCategory.Armor, Category.PlateArmor},
            {ParentCategory.Armor, Category.PlateHelmet},
            {ParentCategory.Armor, Category.PlateShoes},
            {ParentCategory.Armor, Category.UniqueArmor},
            {ParentCategory.Armor, Category.UniqueHelmet},
            {ParentCategory.Armor, Category.UniqueShoes},

            #endregion

            #region Artifact

            {ParentCategory.Artifact, Category.ArmorArtefact},
            {ParentCategory.Artifact, Category.MagicArtefact},
            {ParentCategory.Artifact, Category.MeleeArtefact},
            {ParentCategory.Artifact, Category.OffhandArtefact},
            {ParentCategory.Artifact, Category.RangedArtefact},

            #endregion

            #region CityResources

            {ParentCategory.CityResources, Category.BeastHeart},
            {ParentCategory.CityResources, Category.MountainHeart},
            {ParentCategory.CityResources, Category.RockHeart},
            {ParentCategory.CityResources, Category.TreeHeart},
            {ParentCategory.CityResources, Category.VineHeart},

            #endregion

            #region Consumable

            {ParentCategory.Consumable, Category.Cooked},
            {ParentCategory.Consumable, Category.Fish},
            {ParentCategory.Consumable, Category.FishingBait},
            {ParentCategory.Consumable, Category.Maps},
            {ParentCategory.Consumable, Category.Other},
            {ParentCategory.Consumable, Category.SkillBook},
            {ParentCategory.Consumable, Category.Vanity},

            #endregion
            
            #region Farmable

            {ParentCategory.Farmable, Category.Animals},
            {ParentCategory.Farmable, Category.Seed},

            #endregion

            #region Furniture

            {ParentCategory.Furniture, Category.Banner},
            {ParentCategory.Furniture, Category.Bed},
            {ParentCategory.Furniture, Category.Chest},
            {ParentCategory.Furniture, Category.DecorationFurniture},
            {ParentCategory.Furniture, Category.Flag},
            {ParentCategory.Furniture, Category.HereticFurniture},
            {ParentCategory.Furniture, Category.KeeperFurniture},
            {ParentCategory.Furniture, Category.MorganaFurniture},
            {ParentCategory.Furniture, Category.Table},
            {ParentCategory.Furniture, Category.RepairKit},
            {ParentCategory.Furniture, Category.Unique},

            #endregion

            #region GatheringGear

            {ParentCategory.GatheringGear, Category.FibergathererArmor},
            {ParentCategory.GatheringGear, Category.FibergathererHelmet},
            {ParentCategory.GatheringGear, Category.FibergathererShoes},
            {ParentCategory.GatheringGear, Category.FibergathererBackpack},

            {ParentCategory.GatheringGear, Category.FishgathererArmor},
            {ParentCategory.GatheringGear, Category.FishgathererHelmet},
            {ParentCategory.GatheringGear, Category.FishgathererShoes},
            {ParentCategory.GatheringGear, Category.FishgathererBackpack},

            {ParentCategory.GatheringGear, Category.HidegathererArmor},
            {ParentCategory.GatheringGear, Category.HidegathererHelmet},
            {ParentCategory.GatheringGear, Category.HidegathererShoes},
            {ParentCategory.GatheringGear, Category.HidegathererBackpack},

            {ParentCategory.GatheringGear, Category.OregathererArmor},
            {ParentCategory.GatheringGear, Category.OregathererHelmet},
            {ParentCategory.GatheringGear, Category.OregathererShoes},
            {ParentCategory.GatheringGear, Category.OregathererBackpack},

            {ParentCategory.GatheringGear, Category.RockgathererArmor},
            {ParentCategory.GatheringGear, Category.RockgathererHelmet},
            {ParentCategory.GatheringGear, Category.RockgathererShoes},
            {ParentCategory.GatheringGear, Category.RockgathererBackpack},

            {ParentCategory.GatheringGear, Category.WoodgathererArmor},
            {ParentCategory.GatheringGear, Category.WoodgathererHelmet},
            {ParentCategory.GatheringGear, Category.WoodgathererShoes},
            {ParentCategory.GatheringGear, Category.WoodgathererBackpack},

            #endregion

            #region LuxuryGoods

            {ParentCategory.LuxuryGoods, Category.Bridgewatch},
            {ParentCategory.LuxuryGoods, Category.Caerleon},
            {ParentCategory.LuxuryGoods, Category.FortSterling},
            {ParentCategory.LuxuryGoods, Category.Lymhurst},
            {ParentCategory.LuxuryGoods, Category.Martlock},
            {ParentCategory.LuxuryGoods, Category.Thetford},

            #endregion

            #region Magic

            {ParentCategory.Magic, Category.ArcaneStaff},
            {ParentCategory.Magic, Category.CurseStaff},
            {ParentCategory.Magic, Category.FireStaff},
            {ParentCategory.Magic, Category.FrostStaff},
            {ParentCategory.Magic, Category.HolyStaff},
            {ParentCategory.Magic, Category.NatureStaff},

            #endregion

            #region Materials

            {ParentCategory.Materials, Category.Essence},
            {ParentCategory.Materials, Category.OtherMaterials},
            {ParentCategory.Materials, Category.Relic},
            {ParentCategory.Materials, Category.Rune},
            {ParentCategory.Materials, Category.Soul},

            #endregion

            #region Melee

            {ParentCategory.Melee, Category.Axe},
            {ParentCategory.Melee, Category.Dagger},
            {ParentCategory.Melee, Category.Hammer},
            {ParentCategory.Melee, Category.Mace},
            {ParentCategory.Melee, Category.QuarterStaff},
            {ParentCategory.Melee, Category.Spear},
            {ParentCategory.Melee, Category.Sword},
            
            #endregion

            #region Mount

            {ParentCategory.Mount, Category.ArmoredHorse},
            {ParentCategory.Mount, Category.Ox},
            {ParentCategory.Mount, Category.RareMount},
            {ParentCategory.Mount, Category.RidingHorse},
            
            #endregion

            #region Off-Hand

            {ParentCategory.OffHand, Category.Book},
            {ParentCategory.OffHand, Category.Horn},
            {ParentCategory.OffHand, Category.Orb},
            {ParentCategory.OffHand, Category.Shield},
            {ParentCategory.OffHand, Category.Torch},
            {ParentCategory.OffHand, Category.Totem},
            
            #endregion

            #region Other

            {ParentCategory.Other, Category.Trash},

            #endregion

            #region Product

            {ParentCategory.Product, Category.Farming},
            {ParentCategory.Other, Category.Journal},

            #endregion

            #region Ranged

            {ParentCategory.Ranged, Category.Bow},
            {ParentCategory.Ranged, Category.Crossbow},

            #endregion

            #region Resource

            {ParentCategory.Resource, Category.Cloth},
            {ParentCategory.Resource, Category.Hide},
            {ParentCategory.Resource, Category.Leather},
            {ParentCategory.Resource, Category.Metalbar},
            {ParentCategory.Resource, Category.Ore},
            {ParentCategory.Resource, Category.Planks},
            {ParentCategory.Resource, Category.Rock},
            
            #endregion

            #region Token

            {ParentCategory.Token, Category.ArenaSigils},
            {ParentCategory.Token, Category.Event},
            {ParentCategory.Token, Category.RoyalSigils},
            
            #endregion

            #region Tool

            {ParentCategory.Tool, Category.DemolitionHammer},
            {ParentCategory.Tool, Category.Fishing},
            {ParentCategory.Tool, Category.Pickaxe},
            {ParentCategory.Tool, Category.Sickle},
            {ParentCategory.Tool, Category.SkinningKnife},
            {ParentCategory.Tool, Category.StoneHammer},
            {ParentCategory.Tool, Category.WoodAxe},
            
            #endregion

            #region Trophies

            {ParentCategory.Trophies, Category.FiberTrophy},
            {ParentCategory.Trophies, Category.FishTrophy},
            {ParentCategory.Trophies, Category.GeneralTrophy},
            {ParentCategory.Trophies, Category.HideTrophy},
            {ParentCategory.Trophies, Category.MercenaryTrophy},
            {ParentCategory.Trophies, Category.OreTrophy},
            {ParentCategory.Trophies, Category.RockTrophy},
            {ParentCategory.Trophies, Category.WoodTrophy},

            #endregion
        };

        public static readonly Dictionary<Category, string> CategoryNames = new Dictionary<Category, string>
        {
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
            {Category.Hide, LanguageController.Translation("HIDE")},
            {Category.Leather, LanguageController.Translation("LEATHER")},
            {Category.Metalbar, LanguageController.Translation("METALBAR")},
            {Category.Ore, LanguageController.Translation("ORE")},
            {Category.Planks, LanguageController.Translation("PLANKS")},
            {Category.Rock, LanguageController.Translation("ROCK")},
            
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

        //public static string GetName(Location location) => Names.TryGetValue(location, out var name) ? name : null;

        //public static string GetParameterName(Location location) => ParameterNames.TryGetValue(location, out var name) ? name : null;

        //public static Location GetName(string location) => ParameterNames.FirstOrDefault(x => x.Value == location).Key;
    }

    public enum Category
    {
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
        Rock,
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