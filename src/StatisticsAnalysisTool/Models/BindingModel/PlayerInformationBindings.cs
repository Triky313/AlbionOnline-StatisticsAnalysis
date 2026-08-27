using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class PlayerInformationBindings : BaseViewModel
{
    private const int MinimumSearchTextLength = 3;
    private static readonly TimeSpan SearchDelay = TimeSpan.FromMilliseconds(250);
    private PlayerModeInformationModel _playerModeInformation;
    private PlayerModeInformationModel _playerModeInformationLocal;
    private PlayerModeTranslation _playerModeTranslation;
    private PlayerInformationServerOption _selectedSearchServer;
    private CancellationTokenSource _activeSearchCancellationTokenSource;
    private bool _isSearchInProgress;
    private bool _isSearchResultOpen;
    private ObservableCollection<PlayerSearchStruct> _listBoxUserSearchItems = new();
    private Visibility _loadIconVisibility = Visibility.Collapsed;
    private Visibility _loadBarVisibility = Visibility.Collapsed;
    private int _selectedPlayerTabIndex;
    private int _searchSuggestionRequestVersion;
    private int _searchedPlayerRequestVersion;
    private string _localPlayerName = string.Empty;

    public PlayerInformationBindings()
    {
        PlayerModeTranslation = new PlayerModeTranslation();
        SearchServers =
        [
            new PlayerInformationServerOption(ServerLocation.America, LocalizationController.Translation("AMERICA_SERVER")),
            new PlayerInformationServerOption(ServerLocation.Asia, LocalizationController.Translation("ASIA_SERVER")),
            new PlayerInformationServerOption(ServerLocation.Europe, LocalizationController.Translation("EUROPE_SERVER"))
        ];
        SelectedSearchServer = SearchServers.First(x => x.ServerLocation == GetInitialSearchServerLocation());

        if (ServiceLocator.IsServiceInDictionary<AlbionServerDetectionService>())
        {
            ServiceLocator.Resolve<AlbionServerDetectionService>().ServerChanged += AlbionServerDetectionService_ServerChanged;
        }
    }

    private static ServerLocation GetInitialSearchServerLocation()
    {
        var currentServerLocation = GetCurrentServerLocation();
        if (IsKnownServerLocation(currentServerLocation))
        {
            return currentServerLocation;
        }

        var startupServerLocation = SettingsController.CurrentSettings.StartupUserDataServerLocation;
        return IsKnownServerLocation(startupServerLocation) ? startupServerLocation : ServerLocation.Europe;
    }

    private static ServerLocation GetCurrentServerLocation()
    {
        if (!ServiceLocator.IsServiceInDictionary<AlbionServerDetectionService>())
        {
            return ServerLocation.Unknown;
        }

        return ServiceLocator.Resolve<AlbionServerDetectionService>().CurrentServerLocation;
    }

    private static bool IsKnownServerLocation(ServerLocation serverLocation)
    {
        return serverLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe;
    }

    private static string GetServerName(ServerLocation serverLocation)
    {
        return serverLocation switch
        {
            ServerLocation.America => LocalizationController.Translation("AMERICA_SERVER"),
            ServerLocation.Asia => LocalizationController.Translation("ASIA_SERVER"),
            ServerLocation.Europe => LocalizationController.Translation("EUROPE_SERVER"),
            _ => string.Empty
        };
    }

    private static async Task<PlayerModeInformationModel> GetPlayerInformationAsync(
        SearchPlayerResponse searchPlayer,
        GameInfoSearchResponse gameInfoSearch,
        ServerLocation serverLocation,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchPlayer?.Id))
        {
            return null;
        }

        var gameInfoPlayers = await ApiController.GetGameInfoPlayersFromJsonAsync(
            searchPlayer.Id,
            serverLocation,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(gameInfoPlayers?.Id))
        {
            return null;
        }

        gameInfoPlayers.Avatar = string.IsNullOrWhiteSpace(gameInfoPlayers.Avatar)
            ? searchPlayer.Avatar
            : gameInfoPlayers.Avatar;
        gameInfoPlayers.AvatarRing = string.IsNullOrWhiteSpace(gameInfoPlayers.AvatarRing)
            ? searchPlayer.AvatarRing
            : gameInfoPlayers.AvatarRing;

        await AddMissingAvatarDataAsync(gameInfoPlayers, searchPlayer.Id, serverLocation, cancellationToken);

        return new PlayerModeInformationModel
        {
            Timestamp = DateTime.UtcNow,
            GameInfoSearch = gameInfoSearch,
            SearchPlayer = searchPlayer,
            GameInfoPlayers = gameInfoPlayers,
            ServerLocation = serverLocation,
            ServerName = GetServerName(serverLocation)
        };
    }

    private static async Task AddMissingAvatarDataAsync(
        GameInfoPlayersResponse gameInfoPlayers,
        string playerId,
        ServerLocation serverLocation,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(gameInfoPlayers.Avatar)
            && !string.IsNullOrWhiteSpace(gameInfoPlayers.AvatarRing))
        {
            return;
        }

        var deaths = await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(
            playerId,
            GameInfoPlayersType.Deaths,
            serverLocation,
            1,
            cancellationToken);
        var victim = deaths
            .Select(x => x.Victim)
            .FirstOrDefault(x => string.Equals(x?.Id, playerId, StringComparison.Ordinal));
        ApplyAvatarData(gameInfoPlayers, victim?.Avatar, victim?.AvatarRing);

        if (!string.IsNullOrWhiteSpace(gameInfoPlayers.Avatar)
            && !string.IsNullOrWhiteSpace(gameInfoPlayers.AvatarRing))
        {
            return;
        }

        var kills = await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(
            playerId,
            GameInfoPlayersType.Kills,
            serverLocation,
            1,
            cancellationToken);
        var killer = kills
            .Select(x => x.Killer)
            .FirstOrDefault(x => string.Equals(x?.Id, playerId, StringComparison.Ordinal));
        ApplyAvatarData(gameInfoPlayers, killer?.Avatar, killer?.AvatarRing);
    }

    private static void ApplyAvatarData(GameInfoPlayersResponse gameInfoPlayers, string avatar, string avatarRing)
    {
        if (string.IsNullOrWhiteSpace(gameInfoPlayers.Avatar))
        {
            gameInfoPlayers.Avatar = avatar;
        }

        if (string.IsNullOrWhiteSpace(gameInfoPlayers.AvatarRing))
        {
            gameInfoPlayers.AvatarRing = avatarRing;
        }
    }

    public async Task UpdateUsernameListBoxAsync(string searchText)
    {
        var requestVersion = ++_searchSuggestionRequestVersion;
        CancelActiveSearch();
        ListBoxUserSearchItems.Clear();
        IsSearchResultOpen = false;

        var normalizedSearchText = searchText?.Trim() ?? string.Empty;
        if (normalizedSearchText.Length < MinimumSearchTextLength || SelectedSearchServer == null)
        {
            return;
        }

        var selectedSearchServer = SelectedSearchServer;
        await Task.Delay(SearchDelay);
        if (requestVersion != _searchSuggestionRequestVersion
            || !ReferenceEquals(SelectedSearchServer, selectedSearchServer)
            || !IsSearchedPlayerTabSelected)
        {
            return;
        }

        var cancellationTokenSource = BeginSearch();
        try
        {
            var users = await ApiController.GetGameInfoSearchFromJsonAsync(
                normalizedSearchText,
                selectedSearchServer.ServerLocation,
                cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested
                || requestVersion != _searchSuggestionRequestVersion
                || !ReferenceEquals(SelectedSearchServer, selectedSearchServer))
            {
                return;
            }

            foreach (var user in users?.SearchPlayer ?? [])
            {
                ListBoxUserSearchItems.Add(new PlayerSearchStruct
                {
                    Name = user.Name,
                    Value = user
                });
            }

            IsSearchResultOpen = ListBoxUserSearchItems.Count > 0;
        }
        finally
        {
            CompleteSearch(cancellationTokenSource);
        }
    }

    public async Task LoadPlayerDataAsync(SearchPlayerResponse searchPlayer)
    {
        if (searchPlayer == null || SelectedSearchServer == null)
        {
            return;
        }

        CancelSearch();
        IsSearchResultOpen = false;
        SelectedPlayerTabIndex = 0;
        var requestVersion = ++_searchedPlayerRequestVersion;
        var selectedSearchServer = SelectedSearchServer;
        var cancellationTokenSource = BeginSearch();
        LoadBarVisibility = Visibility.Visible;
        try
        {
            var playerInformation = await GetPlayerInformationAsync(
                searchPlayer,
                null,
                selectedSearchServer.ServerLocation,
                cancellationTokenSource.Token);
            if (requestVersion == _searchedPlayerRequestVersion
                && !cancellationTokenSource.IsCancellationRequested
                && ReferenceEquals(SelectedSearchServer, selectedSearchServer))
            {
                PlayerModeInformation = playerInformation;
            }
        }
        finally
        {
            if (requestVersion == _searchedPlayerRequestVersion)
            {
                LoadBarVisibility = Visibility.Collapsed;
            }

            CompleteSearch(cancellationTokenSource);
        }
    }

    public async Task LoadLocalPlayerDataAsync(string playerName)
    {
        _localPlayerName = playerName?.Trim() ?? string.Empty;
        var serverLocation = GetCurrentServerLocation();
        if (string.IsNullOrWhiteSpace(_localPlayerName) || !IsKnownServerLocation(serverLocation))
        {
            PlayerModeInformationLocal = null;
            return;
        }

        var requestedPlayerName = _localPlayerName;
        if (PlayerModeInformationLocal?.SearchPlayer?.Name == requestedPlayerName
            && PlayerModeInformationLocal.ServerLocation == serverLocation)
        {
            return;
        }

        var gameInfoSearch = await ApiController.GetGameInfoSearchFromJsonAsync(requestedPlayerName, serverLocation);
        var searchPlayer = LocalUserData.GetWebApiUserId(gameInfoSearch, requestedPlayerName);
        var playerInformation = await GetPlayerInformationAsync(searchPlayer, gameInfoSearch, serverLocation);
        if (GetCurrentServerLocation() != serverLocation
            || !string.Equals(_localPlayerName, requestedPlayerName, StringComparison.Ordinal))
        {
            return;
        }

        PlayerModeInformationLocal = playerInformation;
    }

    private async void AlbionServerDetectionService_ServerChanged(object sender, AlbionServerChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_localPlayerName))
        {
            return;
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            await LoadLocalPlayerDataAsync(_localPlayerName);
            return;
        }

        await dispatcher.InvokeAsync(() => LoadLocalPlayerDataAsync(_localPlayerName)).Task.Unwrap();
    }

    public void CancelSearch()
    {
        _searchSuggestionRequestVersion++;
        _searchedPlayerRequestVersion++;
        CancelActiveSearch();
        IsSearchResultOpen = false;
        LoadBarVisibility = Visibility.Collapsed;
    }

    private CancellationTokenSource BeginSearch()
    {
        CancelActiveSearch();
        var cancellationTokenSource = new CancellationTokenSource();
        _activeSearchCancellationTokenSource = cancellationTokenSource;
        IsSearchInProgress = true;
        LoadIconVisibility = Visibility.Visible;
        return cancellationTokenSource;
    }

    private void CompleteSearch(CancellationTokenSource cancellationTokenSource)
    {
        if (ReferenceEquals(_activeSearchCancellationTokenSource, cancellationTokenSource))
        {
            _activeSearchCancellationTokenSource = null;
            IsSearchInProgress = false;
            LoadIconVisibility = Visibility.Collapsed;
        }

        cancellationTokenSource.Dispose();
    }

    private void CancelActiveSearch()
    {
        var cancellationTokenSource = _activeSearchCancellationTokenSource;
        _activeSearchCancellationTokenSource = null;
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }

        IsSearchInProgress = false;
        LoadIconVisibility = Visibility.Collapsed;
    }

    public struct PlayerSearchStruct
    {
        public string Name { get; set; }
        public SearchPlayerResponse Value { get; set; }
    }

    public bool IsSearchResultOpen
    {
        get => _isSearchResultOpen;
        set
        {
            _isSearchResultOpen = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PlayerInformationServerOption> SearchServers { get; }

    public PlayerInformationServerOption SelectedSearchServer
    {
        get => _selectedSearchServer;
        set
        {
            if (_selectedSearchServer == value)
            {
                return;
            }

            _selectedSearchServer = value;
            CancelSearch();
            ListBoxUserSearchItems.Clear();
            PlayerModeInformation = null;
            OnPropertyChanged();
        }
    }

    public int SelectedPlayerTabIndex
    {
        get => _selectedPlayerTabIndex;
        set
        {
            if (_selectedPlayerTabIndex == value)
            {
                return;
            }

            _selectedPlayerTabIndex = value;
            if (IsLocalPlayerTabSelected)
            {
                CancelSearch();
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchedPlayerTabSelected));
            OnPropertyChanged(nameof(IsLocalPlayerTabSelected));
            OnPropertyChanged(nameof(IsSearchInputEnabled));
        }
    }

    public bool IsSearchedPlayerTabSelected => SelectedPlayerTabIndex == 0;

    public bool IsLocalPlayerTabSelected => SelectedPlayerTabIndex == 1;

    public bool IsSearchInputEnabled => IsSearchedPlayerTabSelected && !IsSearchInProgress;

    public bool IsSearchInProgress
    {
        get => _isSearchInProgress;
        private set
        {
            if (_isSearchInProgress == value)
            {
                return;
            }

            _isSearchInProgress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsSearchInputEnabled));
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

    public Visibility LoadBarVisibility
    {
        get => _loadBarVisibility;
        set
        {
            _loadBarVisibility = value;
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
}
