using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.Models.TranslationModel;

public class DialogWindowTranslation
{
    public string Okay => LocalizationController.Translation("OKAY");
    public string Yes => LocalizationController.Translation("YES");
    public string No => LocalizationController.Translation("NO");
}
