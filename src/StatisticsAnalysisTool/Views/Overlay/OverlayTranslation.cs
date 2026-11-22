using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Views.Overlay;

/// <summary>
/// Localized texts for the in-game overlays and their settings (keys in localization.json).
/// Used from XAML via {x:Static overlay:OverlayTranslation.*}.
/// </summary>
public static class OverlayTranslation
{
    // Settings card
    public static string SettingsTitle => LocalizationController.Translation("OVERLAY_SETTINGS_TITLE");
    public static string SessionEnable => LocalizationController.Translation("OVERLAY_SESSION_ENABLE");
    public static string SessionEnableDescription => LocalizationController.Translation("OVERLAY_SESSION_ENABLE_DESCRIPTION");
    public static string DamageMeterEnable => LocalizationController.Translation("OVERLAY_DAMAGE_METER_ENABLE");
    public static string DamageMeterEnableDescription => LocalizationController.Translation("OVERLAY_DAMAGE_METER_ENABLE_DESCRIPTION");
    public static string GatheringEnableDescription => LocalizationController.Translation("OVERLAY_GATHERING_ENABLE_DESCRIPTION");
    public static string AutoHide => LocalizationController.Translation("OVERLAY_AUTO_HIDE");
    public static string AutoHideDescription => LocalizationController.Translation("OVERLAY_AUTO_HIDE_DESCRIPTION");
    public static string ClickThrough => LocalizationController.Translation("OVERLAY_CLICK_THROUGH");
    public static string ClickThroughDescription => LocalizationController.Translation("OVERLAY_CLICK_THROUGH_DESCRIPTION");
    public static string BackgroundOpacity => LocalizationController.Translation("OVERLAY_BACKGROUND_OPACITY");
    public static string Size => LocalizationController.Translation("OVERLAY_SIZE");
    public static string ToggleTooltip => LocalizationController.Translation("OVERLAY_TOGGLE_TOOLTIP");

    // Overlays
    public static string DragHint => LocalizationController.Translation("OVERLAY_DRAG_HINT");
    public static string SessionTitle => LocalizationController.Translation("OVERLAY_SESSION_TITLE");
    public static string SessionTime => LocalizationController.Translation("SESSION_TIME");
    public static string PerHour => LocalizationController.Translation("OVERLAY_PER_HOUR");
    public static string Total => LocalizationController.Translation("OVERLAY_TOTAL");
    public static string ResetSessionHint => LocalizationController.Translation("OVERLAY_RESET_SESSION_HINT");
    public static string ResetSessionConfirm => LocalizationController.Translation("OVERLAY_RESET_SESSION_CONFIRM");
    public static string NoGainsYet => LocalizationController.Translation("OVERLAY_NO_GAINS_YET");
    public static string LootTooltip => LocalizationController.Translation("OVERLAY_LOOT_TOOLTIP");
    public static string GroupDpsTitle => LocalizationController.Translation("OVERLAY_GROUP_DPS_TITLE");
    public static string YourDps => LocalizationController.Translation("OVERLAY_YOUR_DPS");
    public static string NoCombatYet => LocalizationController.Translation("OVERLAY_NO_COMBAT_YET");
    public static string GatheringTitle => LocalizationController.Translation("OVERLAY_GATHERING_TITLE");
    public static string TotalSilver => LocalizationController.Translation("OVERLAY_TOTAL_SILVER");
    public static string GatheringTrackingOff => LocalizationController.Translation("OVERLAY_GATHERING_TRACKING_OFF");

    // Existing keys reused
    public static string Fame => LocalizationController.Translation("FAME");
    public static string Silver => LocalizationController.Translation("SILVER");
    public static string Loot => LocalizationController.Translation("LOOT");
    public static string ReSpec => LocalizationController.Translation("RESPEC");
    public static string Might => LocalizationController.Translation("MIGHT");
    public static string Favor => LocalizationController.Translation("FAVOR");
    public static string FactionPoints => LocalizationController.Translation("FACTION_POINTS");
    public static string Gathering => LocalizationController.Translation("GATHERING");
    public static string Resources => LocalizationController.Translation("RESOURCES");
    public static string Wood => LocalizationController.Translation("WOOD");
    public static string Ore => LocalizationController.Translation("ORE");
    public static string Fiber => LocalizationController.Translation("FIBER");
    public static string Hide => LocalizationController.Translation("HIDE");
    public static string Rock => LocalizationController.Translation("ROCK");
    public static string Fishing => LocalizationController.Translation("FISHING");
}
