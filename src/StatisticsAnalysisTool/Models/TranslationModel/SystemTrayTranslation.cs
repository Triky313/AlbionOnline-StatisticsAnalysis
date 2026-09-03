using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class SystemTrayTranslation
{
    public static string OpenApplication => LocalizationController.Translation("OPEN_APPLICATION");
    public static string ActivateTracking => LocalizationController.Translation("ACTIVATE_TRACKING");
    public static string DeactivateTracking => LocalizationController.Translation("DEACTIVATE_TRACKING");
    public static string ExitApplication => LocalizationController.Translation("EXIT_APPLICATION");
    public static string ApplicationContinuesToRunInSystemTray => LocalizationController.Translation("APPLICATION_CONTINUES_TO_RUN_IN_SYSTEM_TRAY");
}