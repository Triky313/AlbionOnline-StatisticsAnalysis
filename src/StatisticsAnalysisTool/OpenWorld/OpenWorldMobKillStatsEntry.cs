using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldMobKillStatsEntry : BaseViewModel
{
    private int _kills;
    private double _killsPerHour;

    public string MobUniqueName { get; init; } = string.Empty;
    public string MobName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Faction { get; init; } = string.Empty;
    public string FactionDisplay
    {
        get
        {
#if DEBUG
            if (string.IsNullOrWhiteSpace(MobUniqueName))
            {
                return Faction;
            }

            return string.IsNullOrWhiteSpace(Faction) ? MobUniqueName : $"{Faction} | {MobUniqueName}";
#else
            return Faction;
#endif
        }
    }
    public long LastKillTimestampUtc { get; init; }
    public string LastKillDate => new DateTime(LastKillTimestampUtc, DateTimeKind.Utc).CurrentDateTimeFormat();
    public string LastKillDateSeparator => string.IsNullOrWhiteSpace(FactionDisplay) ? string.Empty : " | ";
    public BitmapImage AvatarSource => MobAvatarImageProvider.GetAvatarSource(Avatar);

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