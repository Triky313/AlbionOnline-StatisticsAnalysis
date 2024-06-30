using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class ConsoleWindowTranslation
{
    public static string Title => $"{LocalizationController.Translation("DEBUG_CONSOLE")}";
    public static string Start => $"{LocalizationController.Translation("START")}";
    public static string Stop => $"{LocalizationController.Translation("STOP")}";
    public static string Reset => $"{LocalizationController.Translation("RESET")}";
}