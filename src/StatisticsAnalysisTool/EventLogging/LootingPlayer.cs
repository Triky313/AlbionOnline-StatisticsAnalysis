using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.EventLogging;

public class LootingPlayer : BaseViewModel
{
    private string _playerName;
    private string _playerGuild;
    private string _playerAlliance;
    private ObservableCollection<LootedItem> _lootedItems = new();

    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
        }
    }

    public string PlayerGuild
    {
        get => _playerGuild;
        set
        {
            _playerGuild = value;
            OnPropertyChanged();
        }
    }

    public string PlayerAlliance
    {
        get => _playerAlliance;
        set
        {
            _playerAlliance = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LootedItem> LootedItems
    {
        get => _lootedItems;
        set
        {
            _lootedItems = value;
            OnPropertyChanged();
        }
    }
}