using Ookii.Dialogs.Wpf;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticAnalysisTool.Extractor.Enums;
using StatisticAnalysisTool.Extractor;

namespace StatisticsAnalysisTool.ViewModels;

public class GameDataPreparationWindowViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _message;
    private string _path;
    private bool _isConfirmButtonEnabled;
    private string _errorMessage;

    public GameDataPreparationWindowViewModel()
    {
        Title = LanguageController.Translation("SELECT_ALBION_ONLINE_MAIN_GAME_FOLDER");
        Message = LanguageController.Translation("PLEASE_SELECT_A_CORRECT_ALBION_ONLINE_MAIN_GAME_FOLDER");
    }

    public void OpenPathSelection()
    {
        ErrorMessage = string.Empty;

        var dialog = new VistaFolderBrowserDialog()
        {
            Description = LanguageController.Translation("SELECT_ALBION_ONLINE_MAIN_GAME_FOLDER"),
            RootFolder = Environment.SpecialFolder.Desktop,
            ShowNewFolderButton = false,
            UseDescriptionForTitle = true, 
            Multiselect = false
        };

        var result = dialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            if (Extractor.IsValidMainGameFolder(dialog.SelectedPath ?? string.Empty, ServerType.Live))
            {
                Path = dialog.SelectedPath;
                IsConfirmButtonEnabled = true;
            }
            else
            {
                ErrorMessage = LanguageController.Translation("PLEASE_SELECT_A_CORRECT_FOLDER");
                IsConfirmButtonEnabled = false;
            }
        }
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

    public string Path
    {
        get => _path;
        set
        {
            _path = value;
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

    public static string TranslationSelectAlbionOnlineMainGameFolder => LanguageController.Translation("ConfirmButtonText");
    public static string TranslationConfirm => LanguageController.Translation("CONFIRM");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}