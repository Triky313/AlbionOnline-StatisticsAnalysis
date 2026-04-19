using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace StatisticsAnalysisTool.EventLogging;

public class LootingPlayer : BaseViewModel
{
    private string _playerName;
    private string _playerGuild;
    private string _playerAlliance;
    private ObservableCollection<LootedItem> _lootedItems = new();
    private Visibility _lootingPlayerVisibility = Visibility.Visible;

    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string PlayerGuild
    {
        get => _playerGuild;
        set
        {
            _playerGuild = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string PlayerAlliance
    {
        get => _playerAlliance;
        set
        {
            _playerAlliance = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string DisplayName
        => BuildDisplayName();

    public ObservableCollection<LootedItem> LootedItems
    {
        get => _lootedItems;
        set
        {
            _lootedItems = value;
            OnPropertyChanged();
        }
    }

    public Visibility LootingPlayerVisibility
    {
        get => _lootingPlayerVisibility;
        set
        {
            _lootingPlayerVisibility = value;
            OnPropertyChanged();
        }
    }

    private string BuildDisplayName()
    {
        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            return string.Empty;
        }

        List<string> affiliations = [];

        if (!string.IsNullOrWhiteSpace(PlayerGuild))
        {
            affiliations.Add(PlayerGuild);
        }

        if (!string.IsNullOrWhiteSpace(PlayerAlliance))
        {
            affiliations.Add(PlayerAlliance);
        }

        return affiliations.Count > 0 ? $"{PlayerName} ({string.Join(", ", affiliations)})" : PlayerName;
    }
}