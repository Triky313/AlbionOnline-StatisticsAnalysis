namespace StatisticsAnalysisTool.Common
{
    public static class MobController
    {
        public static bool IsMob(string uniqueName)
        {
            return !string.IsNullOrEmpty(uniqueName) && uniqueName.Contains("@MOB");
        }
    }
}