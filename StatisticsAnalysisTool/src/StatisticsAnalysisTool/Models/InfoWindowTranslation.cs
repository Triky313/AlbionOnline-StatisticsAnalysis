using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class InfoWindowTranslation
    {
        public string ShowNotAgainContent => LanguageController.Translation("SHOW_NOT_AGAIN");
        public string Title => LanguageController.Translation("NEW_FEATURE");
        public string FeatureDescription => LanguageController.Translation("FEATURE_DESCRIPTION");
    }
}