using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.ViewModels;

public class ServerLocationSelectionWindowViewModel : BaseViewModel
{
    private string _title;
    private string _message;
    private string _errorMessage;
    private ServerInfo _selectedServerLocation;
    private bool _isConfirmButtonEnabled = true;
    private ObservableCollection<ServerInfo> _serverLocations;

    public ServerLocationSelectionWindowViewModel()
    {
        Title = LanguageController.Translation("SELECT_SERVER_LOCATION");
        Message = LanguageController.Translation("PLEASE_SELECT_A_SERVER_LOCATION");

        ServerLocations = new ObservableCollection<ServerInfo>()
        {
            new ()
            {
                Name = LanguageController.Translation("AMERICA_SERVER"),
                ServerLocation = ServerLocation.America
            },
            new ()
            {
                Name = LanguageController.Translation("ASIA_SERVER"),
                ServerLocation = ServerLocation.Asia
            },
            new ()
            {
                Name = LanguageController.Translation("EUROPE_SERVER"),
                ServerLocation = ServerLocation.Europe
            }
        };

        SelectedServerLocation = ServerLocations.FirstOrDefault();
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            OnPropertyChanged();
        }
    }

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmButtonEnabled
    {
        get => _isConfirmButtonEnabled;
        set
        {
            _isConfirmButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ServerInfo> ServerLocations
    {
        get => _serverLocations;
        set
        {
            _serverLocations = value;
            OnPropertyChanged();
        }
    }

    public ServerInfo SelectedServerLocation
    {
        get => _selectedServerLocation;
        set
        {
            _selectedServerLocation = value;
            OnPropertyChanged();
        }
    }

    public struct ServerInfo
    {
        public string Name { get; set; }
        public ServerLocation ServerLocation { get; set; }
    }

    public static string TranslationConfirm => LanguageController.Translation("CONFIRM");
}