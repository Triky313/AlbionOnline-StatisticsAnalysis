using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class DamageMeterWindowTranslation
{
    public string Title => $"{LanguageController.Translation("DAMAGE_METER")}";
}