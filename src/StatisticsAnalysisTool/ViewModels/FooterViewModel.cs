using StatisticsAnalysisTool.Properties;
using System.Reflection;

namespace StatisticsAnalysisTool.ViewModels;

public class FooterViewModel : BaseViewModel
{
    public static string DonateUrl => Settings.Default.DonateUrl;
    public static string DiscordUrl => Settings.Default.DiscordUrl;
    public static string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;
    public static string Version
    {
        get
        {
            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            return $"v{attr?.InformationalVersion}";
        }
    }
}