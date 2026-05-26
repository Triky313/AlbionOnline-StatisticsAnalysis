using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Updater;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public class FooterViewModel : BaseViewModel
{
    public FooterViewModel()
    {
        AutoUpdateController.UpdateAvailabilityChanged += OnUpdateAvailabilityChanged;
    }

    public static string DonateUrl => Settings.Default.DonateUrl;
    public static string PatreonUrl => Settings.Default.PatreonUrl;
    public static string DiscordUrl => Settings.Default.DiscordUrl;
    public static string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;

    public string Version
    {
        get
        {
            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            return $"v{attr?.InformationalVersion}";
        }
    }

    public bool IsUpdateAvailable
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
        }
    } = AutoUpdateController.IsUpdateAvailable;

    public async Task OpenUpdateWindowAsync()
    {
        if (!IsUpdateAvailable)
        {
            return;
        }

        await AutoUpdateController.ShowAvailableUpdateWindowAsync();
        IsUpdateAvailable = AutoUpdateController.IsUpdateAvailable;
    }

    private void OnUpdateAvailabilityChanged(bool isUpdateAvailable)
    {
        IsUpdateAvailable = isUpdateAvailable;
    }
}