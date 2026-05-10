using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.ViewModels;

public class FooterViewModel : BaseViewModel
{
    private bool _isUpdateAvailable = AutoUpdateController.IsUpdateAvailable;

    public FooterViewModel()
    {
        AutoUpdateController.UpdateAvailabilityChanged += OnUpdateAvailabilityChanged;
    }

    public static string DonateUrl => Settings.Default.DonateUrl;
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
        get => _isUpdateAvailable;
        private set
        {
            if (_isUpdateAvailable == value)
            {
                return;
            }

            _isUpdateAvailable = value;
            OnPropertyChanged();
        }
    }

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