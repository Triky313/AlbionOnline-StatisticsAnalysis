using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.ViewModels;

public class LanguageSelectionWindowViewModel : BaseViewModel
{
    private string _title;
    private string _message;
    private string _errorMessage;
    private ObservableCollection<FileInformation> _languages = new();
    private FileInformation _selectedFileInformation;
    private bool _isConfirmButtonEnabled = true;

    public LanguageSelectionWindowViewModel()
    {
        Title = "Select a language";
        Message = "Please select a language";
        Languages = new ObservableCollection<FileInformation>(LocalizationController.GetLanguageInformation());

        if (Languages?.Count <= 0)
        {
            ErrorMessage = "No language file found!";
            IsConfirmButtonEnabled = false;
            return;
        }

        SelectedFileInformation = Languages?.FirstOrDefault();
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

    public ObservableCollection<FileInformation> Languages
    {
        get => _languages;
        set
        {
            _languages = value;
            OnPropertyChanged();
        }
    }

    public FileInformation SelectedFileInformation
    {
        get => _selectedFileInformation;
        set
        {
            _selectedFileInformation = value;
            OnPropertyChanged();
        }
    }
}