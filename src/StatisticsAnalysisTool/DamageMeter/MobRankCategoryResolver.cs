using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.DamageMeter;

internal static class MobRankCategoryResolver
{
    public static MobRankCategory Resolve(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return MobRankCategory.Normal;
        }

        var normalizedUniqueName = uniqueName.ToUpperInvariant();
        if (normalizedUniqueName.Contains("_MINIBOSS") || normalizedUniqueName.Contains("ELITE"))
        {
            return MobRankCategory.Elite;
        }

        return normalizedUniqueName.Contains("_BOSS") ? MobRankCategory.Boss : MobRankCategory.Normal;
    }

    public static string GetDisplayName(MobRankCategory category)
    {
        return category switch
        {
            MobRankCategory.Elite => LocalizationController.Translation("ELITE"),
            MobRankCategory.Boss => LocalizationController.Translation("BOSS"),
            _ => LocalizationController.Translation("NORMAL")
        };
    }
}