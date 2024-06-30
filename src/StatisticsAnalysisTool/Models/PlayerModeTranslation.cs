using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models;

public class PlayerModeTranslation
{
    public static string Load => LocalizationController.Translation("LOAD");
    public static string Id => LocalizationController.Translation("ID");
    public static string Name => LocalizationController.Translation("NAME");
    public static string AverageItemPower => LocalizationController.Translation("AVERAGE_ITEM_POWER");
    public static string GuildName => LocalizationController.Translation("GUILD_NAME");
    public static string GuildId => LocalizationController.Translation("GUILD_ID");
    public static string AllianceName => LocalizationController.Translation("ALLIANCE_NAME");
    public static string AllianceId => LocalizationController.Translation("ALLIANCE_ID");
    public static string DeathFame => LocalizationController.Translation("DEATH_FAME");
    public static string KillFame => LocalizationController.Translation("KILL_FAME");
    public static string FameRatio => LocalizationController.Translation("FAME_RATIO");
    public static string TotalKills => LocalizationController.Translation("TOTAL_KILLS");
    public static string GvgKills => LocalizationController.Translation("GVG_KILLS");
    public static string GvgWon => LocalizationController.Translation("GVG_WON");
    public static string CrystalLeague => LocalizationController.Translation("CRYSTAL_LEAGUE");
    public static string Total => LocalizationController.Translation("TOTAL");
    public static string Royal => LocalizationController.Translation("ROYAL");
    public static string Outlands => LocalizationController.Translation("OUTLANDS");
    public static string Hellgate => LocalizationController.Translation("HELLGATE");
    public static string Crafting => LocalizationController.Translation("CRAFTING");
    public static string Gathering => LocalizationController.Translation("GATHERING");
    public static string GatheringFiber => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("FIBER")})";
    public static string GatheringHide => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("HIDE")})";
    public static string GatheringOre => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("ORE")})";
    public static string GatheringRock => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("ROCK")})";
    public static string GatheringWood => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("WOOD")})";
    public static string GatheringAll => $"{LocalizationController.Translation("GATHERING")} ({LocalizationController.Translation("ALL")})";
    public static string All => LocalizationController.Translation("ALL");
    public static string Fiber => LocalizationController.Translation("FIBER");
    public static string Hide => LocalizationController.Translation("HIDE");
    public static string Ore => LocalizationController.Translation("ORE");
    public static string Rock => LocalizationController.Translation("ROCK");
    public static string Wood => LocalizationController.Translation("WOOD");
    public static string MainHand => LocalizationController.Translation("MAINHAND");
    public static string OffHand => LocalizationController.Translation("OFFHAND");
    public static string Head => LocalizationController.Translation("HEAD");
    public static string Armor => LocalizationController.Translation("ARMOR");
    public static string Shoes => LocalizationController.Translation("SHOES");
    public static string Bag => LocalizationController.Translation("BAG");
    public static string Cape => LocalizationController.Translation("CAPE");
    public static string Mount => LocalizationController.Translation("MOUNT");
    public static string Potion => LocalizationController.Translation("POTION");
    public static string Food => LocalizationController.Translation("FOOD");
    public static string GeneralInformation => LocalizationController.Translation("GENERAL_INFORMATION");
    public static string Pvp => LocalizationController.Translation("PVP");
    public static string Pve => LocalizationController.Translation("PVE");
    public static string SearchedPlayer => LocalizationController.Translation("SEARCHED_PLAYER");
    public static string LocalPlayer => LocalizationController.Translation("LOCAL_PLAYER");
}