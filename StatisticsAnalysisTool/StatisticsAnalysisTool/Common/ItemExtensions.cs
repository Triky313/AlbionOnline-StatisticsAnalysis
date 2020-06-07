using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Common
{
    public static class ItemExtensions
    {
        public static string UniqueNameWithoutAtSign(this Item i)
        {
            if (!i.UniqueName.Contains("@"))
            {
                return i.UniqueName;
            }

            var uniqueNamePart = i.UniqueName.Split('@');
            return uniqueNamePart[0];
        }
    }
}
