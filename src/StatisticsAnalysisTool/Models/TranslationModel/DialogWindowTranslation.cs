using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models.TranslationModel
{
    public class DialogWindowTranslation
    {
        public string Yes => LanguageController.Translation("YES");
        public string No => LanguageController.Translation("NO");
    }
}