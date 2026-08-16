using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterFragment : BaseViewModel
{
    private Visibility _playersContainerVisibility = Visibility.Collapsed;
    private ObservableCollection<DamageMeterFragment> _players = [];

    public Guid MobInstanceId { get; init; }
    public long MobObjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string ClusterName { get; init; } = string.Empty;
    public BitmapImage AvatarSource { get; init; }

    public long Damage { get; init; }
    public string DamageShortString => Damage.ToShortNumberString();
    public double DamageInPercent { get; init; }
    public double DamagePercentage { get; init; }
    public DateTime FirstAttackTime { get; init; }
    public DateTime FirstAttackTimeLocal => FirstAttackTime.ToLocalTime();
    public TimeSpan CombatTime { get; init; }
    public string CombatTimeString => $"{(int) CombatTime.TotalHours:00}:{CombatTime.Minutes:00}:{CombatTime.Seconds:00}";
    public double Dps { get; init; }
    public string DpsShortString => Dps.ToShortNumberString();
    public short MobTier { get; init; }
    public string MobTierString => MobTier > 0 ? $"T{MobTier}" : "T?";
    public string MobType { get; init; } = string.Empty;
    public string MobRank { get; init; } = string.Empty;
    public string ContentTypeName { get; init; } = string.Empty;
    public string MapName { get; init; } = string.Empty;
    public Tier MapTier { get; init; } = Tier.Unknown;
    public string MapTierString => MapTier == Tier.Unknown ? "T?" : MapTier.ToString();


    public ObservableCollection<DamageMeterFragment> Players
    {
        get => _players;
        init => _players = value;
    }

    public Visibility PlayersContainerVisibility
    {
        get => _playersContainerVisibility;
        set
        {
            _playersContainerVisibility = value;
            OnPropertyChanged();
        }
    }

    public int PlayerCount => Players.Count;

    private void TogglePlayers(object value)
    {
        PlayersContainerVisibility = PlayersContainerVisibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public static string TranslationFirstAttackTime => LocalizationController.Translation("FIRST_ATTACK_TIME");
    public static string TranslationCombatTime => LocalizationController.Translation("COMBAT_TIME");
    public static string TranslationPlayers => LocalizationController.Translation("PLAYERS");
    public static string TranslationLocation => LocalizationController.Translation("LOCATION");

    private ICommand _showPlayers;
    public ICommand ShowPlayers => _showPlayers ??= new CommandHandler(TogglePlayers, true);
}