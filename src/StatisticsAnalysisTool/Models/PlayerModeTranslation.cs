using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class PlayerModeTranslation
{
    public static string Load => LanguageController.Translation("LOAD");
    public static string Id => LanguageController.Translation("ID");
    public static string Name => LanguageController.Translation("NAME");
    public static string AverageItemPower => LanguageController.Translation("AVERAGE_ITEM_POWER");
    public static string GuildName => LanguageController.Translation("GUILD_NAME");
    public static string GuildId => LanguageController.Translation("GUILD_ID");
    public static string AllianceName => LanguageController.Translation("ALLIANCE_NAME");
    public static string AllianceId => LanguageController.Translation("ALLIANCE_ID");
    public static string DeathFame => LanguageController.Translation("DEATH_FAME");
    public static string KillFame => LanguageController.Translation("KILL_FAME");
    public static string FameRatio => LanguageController.Translation("FAME_RATIO");
    public static string TotalKills => LanguageController.Translation("TOTAL_KILLS");
    public static string GvgKills => LanguageController.Translation("GVG_KILLS");
    public static string GvgWon => LanguageController.Translation("GVG_WON");
    public static string CrystalLeague => LanguageController.Translation("CRYSTAL_LEAGUE");
    public static string Total => LanguageController.Translation("TOTAL");
    public static string Royal => LanguageController.Translation("ROYAL");
    public static string Outlands => LanguageController.Translation("OUTLANDS");
    public static string Hellgate => LanguageController.Translation("HELLGATE");
    public static string Crafting => LanguageController.Translation("CRAFTING");
    public static string Gathering => LanguageController.Translation("GATHERING");
    public static string GatheringFiber => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("FIBER")})";
    public static string GatheringHide => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("HIDE")})";
    public static string GatheringOre => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("ORE")})";
    public static string GatheringRock => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("ROCK")})";
    public static string GatheringWood => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("WOOD")})";
    public static string GatheringAll => $"{LanguageController.Translation("GATHERING")} ({LanguageController.Translation("ALL")})";
    public static string All => LanguageController.Translation("ALL");
    public static string Fiber => LanguageController.Translation("FIBER");
    public static string Hide => LanguageController.Translation("HIDE");
    public static string Ore => LanguageController.Translation("ORE");
    public static string Rock => LanguageController.Translation("ROCK");
    public static string Wood => LanguageController.Translation("WOOD");
    public static string MainHand => LanguageController.Translation("MAINHAND");
    public static string OffHand => LanguageController.Translation("OFFHAND");
    public static string Head => LanguageController.Translation("HEAD");
    public static string Armor => LanguageController.Translation("ARMOR");
    public static string Shoes => LanguageController.Translation("SHOES");
    public static string Bag => LanguageController.Translation("BAG");
    public static string Cape => LanguageController.Translation("CAPE");
    public static string Mount => LanguageController.Translation("MOUNT");
    public static string Potion => LanguageController.Translation("POTION");
    public static string Food => LanguageController.Translation("FOOD");
    public static string GeneralInformation => LanguageController.Translation("GENERAL_INFORMATION");
    public static string Pvp => LanguageController.Translation("PVP");
    public static string Pve => LanguageController.Translation("PVE");
    public static string SearchedPlayer => LanguageController.Translation("SEARCHED_PLAYER");
    public static string LocalPlayer => LanguageController.Translation("LOCAL_PLAYER");
}