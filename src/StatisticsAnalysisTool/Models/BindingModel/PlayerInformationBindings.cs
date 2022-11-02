using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class PlayerInformationBindings : INotifyPropertyChanged
{
    private PlayerModeInformationModel _playerModeInformation;
    private PlayerModeInformationModel _playerModeInformationLocal;
    private PlayerModeTranslation _playerModeTranslation;
    private Visibility _listBoxUserSearchVisibility = Visibility.Collapsed;
    private ObservableCollection<PlayerSearchStruct> _listBoxUserSearchItems = new();
    private Visibility _loadIconVisibility = Visibility.Collapsed;

    public PlayerInformationBindings()
    {
        PlayerModeTranslation = new PlayerModeTranslation();
    }

    private async Task<PlayerModeInformationModel> GetPlayerInformationAsync(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return null;
        }

        var gameInfoSearch = await ApiController.GetGameInfoSearchFromJsonAsync(playerName);

        if (gameInfoSearch?.SearchPlayer?.FirstOrDefault()?.Id == null)
        {
            return null;
        }

        var searchPlayer = gameInfoSearch.SearchPlayer?.FirstOrDefault();
        var gameInfoPlayers = await ApiController.GetGameInfoPlayersFromJsonAsync(gameInfoSearch.SearchPlayer?.FirstOrDefault()?.Id);

        return new PlayerModeInformationModel
        {
            Timestamp = DateTime.UtcNow,
            GameInfoSearch = gameInfoSearch,
            SearchPlayer = searchPlayer,
            GameInfoPlayers = gameInfoPlayers
        };
    }

    public async Task UpdateUsernameListBoxAsync(string searchText)
    {
        LoadIconVisibility = Visibility.Visible;
        ListBoxUserSearchItems.Clear();

        if (searchText.Length < 3)
        {
            LoadIconVisibility = Visibility.Collapsed;
            return;
        }

        if (!string.IsNullOrEmpty(searchText))
        {
            var users = await ApiController.GetGameInfoSearchFromJsonAsync(searchText);

            foreach (var user in users.SearchPlayer)
            {
                ListBoxUserSearchItems.Add(new PlayerSearchStruct() { Name = user.Name, Value = user });
            }

            if (ListBoxUserSearchItems.Count > 0)
            {
                ListBoxUserSearchVisibility = Visibility.Visible;
            }
        }

        if (ListBoxUserSearchItems.Count <= 0)
        {
            ListBoxUserSearchVisibility = Visibility.Collapsed;
        }

        LoadIconVisibility = Visibility.Collapsed;
    }

    public async Task LoadPlayerDataAsync(string playerName)
    {
        ListBoxUserSearchVisibility = Visibility.Collapsed;
        PlayerModeInformation = await GetPlayerInformationAsync(playerName);
    }

    public async Task LoadLocalPlayerDataAsync(string playerName)
    {
        if (PlayerModeInformationLocal?.SearchPlayer?.Name == playerName)
        {
            return;
        }

        PlayerModeInformationLocal = await GetPlayerInformationAsync(playerName);
    }

    public struct PlayerSearchStruct
    {
        public string Name { get; set; }
        public SearchPlayerResponse Value { get; set; }
    }

    public Visibility ListBoxUserSearchVisibility
    {
        get => _listBoxUserSearchVisibility;
        set
        {
            _listBoxUserSearchVisibility = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PlayerSearchStruct> ListBoxUserSearchItems
    {
        get => _listBoxUserSearchItems;
        set
        {
            _listBoxUserSearchItems = value;
            OnPropertyChanged();
        }
    }

    public Visibility LoadIconVisibility
    {
        get => _loadIconVisibility;
        set
        {
            _loadIconVisibility = value;
            OnPropertyChanged();
        }
    }

    public PlayerModeTranslation PlayerModeTranslation
    {
        get => _playerModeTranslation;
        set
        {
            _playerModeTranslation = value;
            OnPropertyChanged();
        }
    }

    public PlayerModeInformationModel PlayerModeInformation
    {
        get => _playerModeInformation;
        set
        {
            _playerModeInformation = value;
            OnPropertyChanged();
        }
    }

    public PlayerModeInformationModel PlayerModeInformationLocal
    {
        get => _playerModeInformationLocal;
        set
        {
            _playerModeInformationLocal = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}