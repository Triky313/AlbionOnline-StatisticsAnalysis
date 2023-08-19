using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class UserTrackingBindings : BaseViewModel
{
    private string _username;
    private string _guildName;
    private string _allianceName;
    private Visibility _usernameInformationVisibility;
    private Visibility _guildInformationVisibility;
    private Visibility _allianceInformationVisibility;
    private CurrentMapInfoBinding _currentMapInfoBinding = new();

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

    public CurrentMapInfoBinding CurrentMapInfoBinding
    {
        get => _currentMapInfoBinding;
        set
        {
            _currentMapInfoBinding = value;
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
}