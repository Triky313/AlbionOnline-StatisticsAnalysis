using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.ViewModels
{
    public class FooterViewModel : INotifyPropertyChanged
    {
        public static string DonateUrl => Settings.Default.DonateUrl;
        public static string DiscordUrl => Settings.Default.DiscordUrl;
        public static string GitHubRepoUrl => Settings.Default.GitHubRepoUrl;

        public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}