using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models
{
    public class ItemCategoryTree
    {
        [JsonPropertyName("head")] public Head Head { get; set; }

        [JsonPropertyName("mainhand")] public Mainhand Mainhand { get; set; }

        [JsonPropertyName("potion")] public Potion Potion { get; set; }

        [JsonPropertyName("armor")] public Armor Armor { get; set; }

        [JsonPropertyName("bag")] public Bag Bag { get; set; }

        [JsonPropertyName("mount")] public Mount Mount { get; set; }

        [JsonPropertyName("shoes")] public Shoes Shoes { get; set; }

        [JsonPropertyName("cape")] public Cape Cape { get; set; }

        [JsonPropertyName("food")] public Food Food { get; set; }

        [JsonPropertyName("offhand")] public Offhand Offhand { get; set; }
    }

    public class Head
    {
        [JsonPropertyName("fibergatherer_helmet")] public string FibergathererHelmet { get; set; }

        [JsonPropertyName("woodgatherer_helmet")] public string WoodgathererHelmet { get; set; }

        [JsonPropertyName("leather_helmet")] public string LeatherHelmet { get; set; }

        [JsonPropertyName("plate_helmet")] public string PlateHelmet { get; set; }

        [JsonPropertyName("hidegatherer_helmet")] public string HidegathererHelmet { get; set; }

        [JsonPropertyName("rockgatherer_helmet")] public string RockgathererHelmet { get; set; }

        [JsonPropertyName("cloth_helmet")] public string ClothHelmet { get; set; }

        [JsonPropertyName("oregatherer_helmet")] public string OregathererHelmet { get; set; }

        [JsonPropertyName("fishgatherer_helmet")] public string FishgathererHelmet { get; set; }
    }

    public class Mainhand
    {
        [JsonPropertyName("firestaff")] public string Firestaff { get; set; }

        [JsonPropertyName("dagger")] public string Dagger { get; set; }

        [JsonPropertyName("hammer")] public string Hammer { get; set; }

        [JsonPropertyName("crossbow")] public string Crossbow { get; set; }

        [JsonPropertyName("mace")] public string Mace { get; set; }

        [JsonPropertyName("cursestaff")] public string Cursestaff { get; set; }

        [JsonPropertyName("bow")] public string Bow { get; set; }

        [JsonPropertyName("arcanestaff")] public string Arcanestaff { get; set; }

        [JsonPropertyName("axe")] public string Axe { get; set; }

        [JsonPropertyName("sword")] public string Sword { get; set; }

        [JsonPropertyName("spear")] public string Spear { get; set; }

        [JsonPropertyName("froststaff")] public string Froststaff { get; set; }

        [JsonPropertyName("naturestaff")] public string Naturestaff { get; set; }

        [JsonPropertyName("quarterstaff")] public string Quarterstaff { get; set; }

        [JsonPropertyName("holystaff")] public string Holystaff { get; set; }
    }

    public class Potion
    {
        [JsonPropertyName("potion")] public string PotionValue { get; set; }

        [JsonPropertyName("fishingbait")] public string Fishingbait { get; set; }

        [JsonPropertyName("vanity")] public string Vanity { get; set; }
    }

    public class Armor
    {
        [JsonPropertyName("rockgatherer_armor")] public string RockgathererArmor { get; set; }

        [JsonPropertyName("oregatherer_armor")] public string OregathererArmor { get; set; }

        [JsonPropertyName("leather_armor")] public string LeatherArmor { get; set; }

        [JsonPropertyName("fibergatherer_armor")] public string FibergathererArmor { get; set; }

        [JsonPropertyName("hidegatherer_armor")] public string HidegathererArmor { get; set; }

        [JsonPropertyName("plate_armor")] public string PlateArmor { get; set; }

        [JsonPropertyName("cloth_armor")] public string ClothArmor { get; set; }

        [JsonPropertyName("fishgatherer_armor")] public string FishgathererArmor { get; set; }

        [JsonPropertyName("woodgatherer_armor")] public string WoodgathererArmor { get; set; }
    }

    public class Bag
    {
        [JsonPropertyName("bag")] public string BagValue { get; set; }
    }

    public class Mount
    {
        [JsonPropertyName("rare_mount")] public string RareMount { get; set; }

        [JsonPropertyName("ridinghorse")] public string Ridinghorse { get; set; }

        [JsonPropertyName("armoredhorse")] public string Armoredhorse { get; set; }

        [JsonPropertyName("ox")] public string Ox { get; set; }
    }

    public class Shoes
    {
        [JsonPropertyName("unique_shoes")] public string UniqueShoes { get; set; }

        [JsonPropertyName("fibergatherer_shoes")] public string FibergathererShoes { get; set; }

        [JsonPropertyName("rockgatherer_shoes")] public string RockgathererShoes { get; set; }

        [JsonPropertyName("hidegatherer_shoes")] public string HidegathererShoes { get; set; }

        [JsonPropertyName("plate_shoes")] public string PlateShoes { get; set; }

        [JsonPropertyName("cloth_shoes")] public string ClothShoes { get; set; }

        [JsonPropertyName("fishgatherer_shoes")] public string FishgathererShoes { get; set; }

        [JsonPropertyName("woodgatherer_shoes")] public string WoodgathererShoes { get; set; }

        [JsonPropertyName("leather_shoes")] public string LeatherShoes { get; set; }

        [JsonPropertyName("oregatherer_shoes")] public string OregathererShoes { get; set; }
    }

    public class Cape
    {
        [JsonPropertyName("rockgatherer_backpack")]
        public string RockgathererBackpack { get; set; }

        [JsonPropertyName("oregatherer_backpack")] public string OregathererBackpack { get; set; }

        [JsonPropertyName("fishgatherer_backpack")]
        public string FishgathererBackpack { get; set; }

        [JsonPropertyName("woodgatherer_backpack")]
        public string WoodgathererBackpack { get; set; }

        [JsonPropertyName("hidegatherer_backpack")]
        public string HidegathererBackpack { get; set; }

        [JsonPropertyName("cape")] public string CapeValue { get; set; }

        [JsonPropertyName("fibergatherer_backpack")]
        public string FibergathererBackpack { get; set; }
    }

    public class Food
    {
        [JsonPropertyName("fish")] public string Fish { get; set; }

        [JsonPropertyName("cooked")] public string Cooked { get; set; }

        [JsonPropertyName("vanity")] public string Vanity { get; set; }
    }

    public class Offhand
    {
        [JsonPropertyName("shield")] public string Shield { get; set; }

        [JsonPropertyName("horn")] public string Horn { get; set; }

        [JsonPropertyName("book")] public string Book { get; set; }

        [JsonPropertyName("totem")] public string Totem { get; set; }

        [JsonPropertyName("torch")] public string Torch { get; set; }

        [JsonPropertyName("orb")] public string Orb { get; set; }
    }
}