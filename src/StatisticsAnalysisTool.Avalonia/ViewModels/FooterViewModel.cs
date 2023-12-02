using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class FooterViewModel : ViewModelBase
{
    //public static string DonateUrl => AppSettings.DonateUrl;
    //public static string DiscordUrl => AppSettings.DiscordUrl;
    //public static string GitHubRepoUrl => AppSettings.GitHubRepoUrl;

    [RelayCommand]
    public void OpenGitHubRepoCommand()
    {
        var ps = new ProcessStartInfo(AppSettings.GitHubRepoUrl)
        {
            UseShellExecute = true,
            Verb = "open"
        };
        Process.Start(ps);
    }

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _version = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
}