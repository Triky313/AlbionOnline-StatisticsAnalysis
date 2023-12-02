using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class FooterViewModel : ViewModelBase
{
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

    [RelayCommand]
    public void OpenDiscordUrlCommand()
    {
        var ps = new ProcessStartInfo(AppSettings.DiscordUrl)
        {
            UseShellExecute = true,
            Verb = "open"
        };
        Process.Start(ps);
    }

    [RelayCommand]
    public void OpenDonateUrlCommand()
    {
        var ps = new ProcessStartInfo(AppSettings.DonateUrl)
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