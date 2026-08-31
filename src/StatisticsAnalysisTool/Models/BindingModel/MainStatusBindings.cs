using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class MainStatusBindings : BaseViewModel
{
    private bool _isInGame;
    private bool _isServerDetected;
    private bool _isCharacterDetected;
    private bool _isLocationDetected;
    private bool _isGameDataDetected;
    private ServerLocation _serverLocation = ServerLocation.Unknown;
    private string _characterName = string.Empty;
    private string _locationName = string.Empty;
    private string _serverStatusText;
    private string _characterStatusText;
    private string _locationStatusText;
    private string _gameDataStatusText;

    public MainStatusBindings()
    {
        RefreshLocalization();
    }

    public bool IsInGame
    {
        get => _isInGame;
        private set
        {
            _isInGame = value;
            OnPropertyChanged();
        }
    }

    public bool IsServerDetected
    {
        get => _isServerDetected;
        private set
        {
            _isServerDetected = value;
            OnPropertyChanged();
        }
    }

    public bool IsCharacterDetected
    {
        get => _isCharacterDetected;
        private set
        {
            _isCharacterDetected = value;
            OnPropertyChanged();
        }
    }

    public bool IsLocationDetected
    {
        get => _isLocationDetected;
        private set
        {
            _isLocationDetected = value;
            OnPropertyChanged();
        }
    }

    public bool IsGameDataDetected
    {
        get => _isGameDataDetected;
        private set
        {
            _isGameDataDetected = value;
            OnPropertyChanged();
        }
    }

    public string ServerStatusText
    {
        get => _serverStatusText;
        private set
        {
            _serverStatusText = value;
            OnPropertyChanged();
        }
    }

    public string CharacterStatusText
    {
        get => _characterStatusText;
        private set
        {
            _characterStatusText = value;
            OnPropertyChanged();
        }
    }

    public string LocationStatusText
    {
        get => _locationStatusText;
        private set
        {
            _locationStatusText = value;
            OnPropertyChanged();
        }
    }

    public string GameDataStatusText
    {
        get => _gameDataStatusText;
        private set
        {
            _gameDataStatusText = value;
            OnPropertyChanged();
        }
    }

    public void SetInGame(bool isInGame)
    {
        IsInGame = isInGame;
    }

    public void SetServerLocation(ServerLocation serverLocation)
    {
        _serverLocation = serverLocation;
        IsServerDetected = IsKnownServerLocation(serverLocation);
        ServerStatusText = IsServerDetected
            ? string.Format(LocalizationController.Translation("STATUS_SERVER_DETECTED_FORMAT"), GetServerStatusName(serverLocation))
            : LocalizationController.Translation("STATUS_SERVER_UNDETECTED");
    }

    public void SetCharacter(string characterName)
    {
        _characterName = characterName ?? string.Empty;
        IsCharacterDetected = !string.IsNullOrWhiteSpace(_characterName);
        CharacterStatusText = IsCharacterDetected
            ? string.Format(LocalizationController.Translation("STATUS_CHARACTER_DETECTED_FORMAT"), _characterName)
            : LocalizationController.Translation("STATUS_CHARACTER_UNDETECTED");
    }

    public void SetLocation(string locationName)
    {
        _locationName = locationName ?? string.Empty;
        IsLocationDetected = !string.IsNullOrWhiteSpace(_locationName);
        LocationStatusText = IsLocationDetected
            ? LocalizationController.Translation("STATUS_LOCATION_DETECTED")
            : LocalizationController.Translation("STATUS_LOCATION_UNDETECTED");
    }

    public void SetGameDataDetected(bool isGameDataDetected)
    {
        IsGameDataDetected = isGameDataDetected;
        GameDataStatusText = LocalizationController.Translation(isGameDataDetected
            ? "STATUS_GAME_DATA_DETECTED"
            : "STATUS_WAITING_FOR_GAME_DATA");
    }

    public void ResetGameSession()
    {
        SetInGame(false);
        SetCharacter(string.Empty);
        SetLocation(string.Empty);
        SetGameDataDetected(false);
    }

    public void RefreshLocalization()
    {
        SetServerLocation(_serverLocation);
        SetCharacter(_characterName);
        SetLocation(_locationName);
        SetGameDataDetected(_isGameDataDetected);
    }

    private static bool IsKnownServerLocation(ServerLocation serverLocation)
    {
        return serverLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe;
    }

    private static string GetServerStatusName(ServerLocation serverLocation)
    {
        return serverLocation switch
        {
            ServerLocation.America => LocalizationController.Translation("STATUS_SERVER_AMERICA"),
            ServerLocation.Asia => LocalizationController.Translation("STATUS_SERVER_ASIA"),
            ServerLocation.Europe => LocalizationController.Translation("STATUS_SERVER_EUROPE"),
            _ => string.Empty
        };
    }
}