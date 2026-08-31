using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.ViewModels;

public class LanguageSelectionWindowViewModel : BaseViewModel
{
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
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string Message
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ErrorMessage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsConfirmButtonEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public ObservableCollection<FileInformation> Languages
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public FileInformation SelectedFileInformation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}