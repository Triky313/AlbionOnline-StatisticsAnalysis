using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.ViewModels
{
    public class FooterViewModel
    {
        public static string DonateUrl => Settings.Default.DonateUrl;
        public static string DiscordUrl => Settings.Default.DiscordUrl;
        public static string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;

    }
}