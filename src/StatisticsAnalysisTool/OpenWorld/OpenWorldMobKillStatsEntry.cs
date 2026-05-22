using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldMobKillStatsEntry : BaseViewModel
{
    private int _kills;
    private double _killsPerHour;

    public string MobUniqueName { get; init; } = string.Empty;
    public string MobName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string AvatarPath => string.IsNullOrWhiteSpace(Avatar)
        ? string.Empty
        : $"pack://application:,,,/Assets/MobAvatars/{Avatar}";

    public int Kills
    {
        get => _kills;
        set
        {
            _kills = value;
            OnPropertyChanged();
        }
    }

    public double KillsPerHour
    {
        get => _killsPerHour;
        set
        {
            _killsPerHour = value;
            OnPropertyChanged();
        }
    }
}