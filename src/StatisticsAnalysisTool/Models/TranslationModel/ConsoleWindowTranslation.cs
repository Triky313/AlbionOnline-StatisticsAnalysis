using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class ConsoleWindowTranslation
{
    public static string Title => $"{LanguageController.Translation("DEBUG_CONSOLE")}";
    public static string Start => $"{LanguageController.Translation("START")}";
    public static string Stop => $"{LanguageController.Translation("STOP")}";
    public static string Reset => $"{LanguageController.Translation("RESET")}";
}