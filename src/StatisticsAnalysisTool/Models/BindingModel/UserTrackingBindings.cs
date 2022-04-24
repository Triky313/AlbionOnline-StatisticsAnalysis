using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class UserTrackingBindings : INotifyPropertyChanged
{
    private string _username;
    private string _guildName;
    private string _allianceName;
    private string _currentMapName;
    private Visibility _usernameInformationVisibility;
    private Visibility _guildInformationVisibility;
    private Visibility _allianceInformationVisibility;
    private Visibility _currentMapInformationVisibility;
    private string _islandName;
    private Visibility _islandNameVisibility;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            UsernameInformationVisibility = !string.IsNullOrEmpty(_username) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string GuildName
    {
        get => _guildName;
        set
        {
            _guildName = value;
            GuildInformationVisibility = !string.IsNullOrEmpty(_guildName) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string AllianceName
    {
        get => _allianceName;
        set
        {
            _allianceName = value;
            AllianceInformationVisibility = !string.IsNullOrEmpty(_allianceName) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string CurrentMapName
    {
        get => _currentMapName;
        set
        {
            _currentMapName = value;
            CurrentMapInformationVisibility = !string.IsNullOrEmpty(_currentMapName) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public string IslandName
    {
        get => _islandName;
        set
        {
            _islandName = value;
            IslandNameVisibility = !string.IsNullOrEmpty(_islandName) ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }
    
    public Visibility UsernameInformationVisibility
    {
        get => _usernameInformationVisibility;
        set
        {
            _usernameInformationVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility GuildInformationVisibility
    {
        get => _guildInformationVisibility;
        set
        {
            _guildInformationVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility AllianceInformationVisibility
    {
        get => _allianceInformationVisibility;
        set
        {
            _allianceInformationVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility CurrentMapInformationVisibility
    {
        get => _currentMapInformationVisibility;
        set
        {
            _currentMapInformationVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility IslandNameVisibility
    {
        get => _islandNameVisibility;
        set
        {
            _islandNameVisibility = value;
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