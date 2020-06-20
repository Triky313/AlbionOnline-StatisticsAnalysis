using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class ItemCategoryTree
    {

        [JsonProperty("head")]
        public Head Head { get; set; }

        [JsonProperty("mainhand")]
        public Mainhand Mainhand { get; set; }

        [JsonProperty("potion")]
        public Potion Potion { get; set; }

        [JsonProperty("armor")]
        public Armor Armor { get; set; }

        [JsonProperty("bag")]
        public Bag Bag { get; set; }

        [JsonProperty("mount")]
        public Mount Mount { get; set; }

        [JsonProperty("shoes")]
        public Shoes Shoes { get; set; }

        [JsonProperty("cape")]
        public Cape Cape { get; set; }

        [JsonProperty("food")]
        public Food Food { get; set; }

        [JsonProperty("offhand")]
        public Offhand Offhand { get; set; }
    }

    public class Head
    {
        [JsonProperty("fibergatherer_helmet")]
        public string FibergathererHelmet { get; set; }

        [JsonProperty("woodgatherer_helmet")]
        public string WoodgathererHelmet { get; set; }

        [JsonProperty("leather_helmet")]
        public string LeatherHelmet { get; set; }

        [JsonProperty("plate_helmet")]
        public string PlateHelmet { get; set; }

        [JsonProperty("hidegatherer_helmet")]
        public string HidegathererHelmet { get; set; }

        [JsonProperty("rockgatherer_helmet")]
        public string RockgathererHelmet { get; set; }

        [JsonProperty("cloth_helmet")]
        public string ClothHelmet { get; set; }

        [JsonProperty("oregatherer_helmet")]
        public string OregathererHelmet { get; set; }

        [JsonProperty("fishgatherer_helmet")]
        public string FishgathererHelmet { get; set; }
    }

    public class Mainhand
    {

        [JsonProperty("firestaff")]
        public string Firestaff { get; set; }

        [JsonProperty("dagger")]
        public string Dagger { get; set; }

        [JsonProperty("hammer")]
        public string Hammer { get; set; }

        [JsonProperty("crossbow")]
        public string Crossbow { get; set; }

        [JsonProperty("mace")]
        public string Mace { get; set; }

        [JsonProperty("cursestaff")]
        public string Cursestaff { get; set; }

        [JsonProperty("bow")]
        public string Bow { get; set; }

        [JsonProperty("arcanestaff")]
        public string Arcanestaff { get; set; }

        [JsonProperty("axe")]
        public string Axe { get; set; }

        [JsonProperty("sword")]
        public string Sword { get; set; }

        [JsonProperty("spear")]
        public string Spear { get; set; }

        [JsonProperty("froststaff")]
        public string Froststaff { get; set; }

        [JsonProperty("naturestaff")]
        public string Naturestaff { get; set; }

        [JsonProperty("quarterstaff")]
        public string Quarterstaff { get; set; }

        [JsonProperty("holystaff")]
        public string Holystaff { get; set; }
    }

    public class Potion
    {

        [JsonProperty("potion")]
        public string PotionValue { get; set; }

        [JsonProperty("fishingbait")]
        public string Fishingbait { get; set; }

        [JsonProperty("vanity")]
        public string Vanity { get; set; }
    }

    public class Armor
    {

        [JsonProperty("rockgatherer_armor")]
        public string RockgathererArmor { get; set; }

        [JsonProperty("oregatherer_armor")]
        public string OregathererArmor { get; set; }

        [JsonProperty("leather_armor")]
        public string LeatherArmor { get; set; }

        [JsonProperty("fibergatherer_armor")]
        public string FibergathererArmor { get; set; }

        [JsonProperty("hidegatherer_armor")]
        public string HidegathererArmor { get; set; }

        [JsonProperty("plate_armor")]
        public string PlateArmor { get; set; }

        [JsonProperty("cloth_armor")]
        public string ClothArmor { get; set; }

        [JsonProperty("fishgatherer_armor")]
        public string FishgathererArmor { get; set; }

        [JsonProperty("woodgatherer_armor")]
        public string WoodgathererArmor { get; set; }
    }

    public class Bag
    {

        [JsonProperty("bag")]
        public string BagValue { get; set; }
    }

    public class Mount
    {

        [JsonProperty("rare_mount")]
        public string RareMount { get; set; }

        [JsonProperty("ridinghorse")]
        public string Ridinghorse { get; set; }

        [JsonProperty("armoredhorse")]
        public string Armoredhorse { get; set; }

        [JsonProperty("ox")]
        public string Ox { get; set; }
    }

    public class Shoes
    {

        [JsonProperty("unique_shoes")]
        public string UniqueShoes { get; set; }

        [JsonProperty("fibergatherer_shoes")]
        public string FibergathererShoes { get; set; }

        [JsonProperty("rockgatherer_shoes")]
        public string RockgathererShoes { get; set; }

        [JsonProperty("hidegatherer_shoes")]
        public string HidegathererShoes { get; set; }

        [JsonProperty("plate_shoes")]
        public string PlateShoes { get; set; }

        [JsonProperty("cloth_shoes")]
        public string ClothShoes { get; set; }

        [JsonProperty("fishgatherer_shoes")]
        public string FishgathererShoes { get; set; }

        [JsonProperty("woodgatherer_shoes")]
        public string WoodgathererShoes { get; set; }

        [JsonProperty("leather_shoes")]
        public string LeatherShoes { get; set; }

        [JsonProperty("oregatherer_shoes")]
        public string OregathererShoes { get; set; }
    }

    public class Cape
    {

        [JsonProperty("rockgatherer_backpack")]
        public string RockgathererBackpack { get; set; }

        [JsonProperty("oregatherer_backpack")]
        public string OregathererBackpack { get; set; }

        [JsonProperty("fishgatherer_backpack")]
        public string FishgathererBackpack { get; set; }

        [JsonProperty("woodgatherer_backpack")]
        public string WoodgathererBackpack { get; set; }

        [JsonProperty("hidegatherer_backpack")]
        public string HidegathererBackpack { get; set; }

        [JsonProperty("cape")]
        public string CapeValue { get; set; }

        [JsonProperty("fibergatherer_backpack")]
        public string FibergathererBackpack { get; set; }
    }

    public class Food
    {

        [JsonProperty("fish")]
        public string Fish { get; set; }

        [JsonProperty("cooked")]
        public string Cooked { get; set; }

        [JsonProperty("vanity")]
        public string Vanity { get; set; }
    }

    public class Offhand
    {

        [JsonProperty("shield")]
        public string Shield { get; set; }

        [JsonProperty("horn")]
        public string Horn { get; set; }

        [JsonProperty("book")]
        public string Book { get; set; }

        [JsonProperty("totem")]
        public string Totem { get; set; }

        [JsonProperty("torch")]
        public string Torch { get; set; }

        [JsonProperty("orb")]
        public string Orb { get; set; }
    }
}
