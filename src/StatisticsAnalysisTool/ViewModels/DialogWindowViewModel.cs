using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.TranslationModel;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class DialogWindowViewModel : BaseViewModel
{
    private string _title;
    private string _message;
    private DialogWindowTranslation _dialogWindowTranslation = new();
    private DialogType _type;
    private Visibility _yesNoVisibility = Visibility.Collapsed;
    private Visibility _infoTypeVisibility = Visibility.Collapsed;
    private Visibility _errorTypeVisibility = Visibility.Collapsed;
    private string _okButtonText = "Okay";
    private string _url;
    private string _urlText;


    public DialogWindowViewModel(string title, string message, DialogType type, string url = null, string urlText = null)
    {
        Title = title;
        Message = message;
        Url = url;
        UrlText = string.IsNullOrWhiteSpace(urlText) ? url : urlText;
        Type = type;
    }

    public bool Canceled { get; set; }

    #region Binding

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

    public DialogType Type
    {
        get => _type;
        set
        {
            _type = value;
            switch (_type)
            {
                case DialogType.YesNo:
                    InfoTypeVisibility = Visibility.Collapsed;
                    ErrorTypeVisibility = Visibility.Collapsed;
                    YesNoVisibility = Visibility.Visible;
                    break;
                case DialogType.Ok:
                    YesNoVisibility = Visibility.Collapsed;
                    InfoTypeVisibility = Visibility.Visible;
                    ErrorTypeVisibility = Visibility.Collapsed;
                    OkButtonText = Translation.Okay;
                    break;
                case DialogType.Error:
                    YesNoVisibility = Visibility.Collapsed;
                    InfoTypeVisibility = Visibility.Collapsed;
                    ErrorTypeVisibility = Visibility.Visible;
                    OkButtonText = Translation.Okay;
                    break;
            }
            OnPropertyChanged();
        }
    }

    public Visibility YesNoVisibility
    {
        get => _yesNoVisibility;
        set
        {
            _yesNoVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility InfoTypeVisibility
    {
        get => _infoTypeVisibility;
        set
        {
            _infoTypeVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ErrorTypeVisibility
    {
        get => _errorTypeVisibility;
        set
        {
            _errorTypeVisibility = value;
            OnPropertyChanged();
        }
    }

    public string OkButtonText
    {
        get => _okButtonText;
        set
        {
            _okButtonText = value;
            OnPropertyChanged();
        }
    }

    public string Url
    {
        get => _url;
        set
        {
            _url = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UrlVisibility));
        }
    }

    public string UrlText
    {
        get => _urlText;
        set
        {
            _urlText = value;
            OnPropertyChanged();
        }
    }

    public Visibility UrlVisibility => string.IsNullOrWhiteSpace(Url) ? Visibility.Collapsed : Visibility.Visible;

    public DialogWindowTranslation Translation
    {
        get => _dialogWindowTranslation;
        set
        {
            _dialogWindowTranslation = value;
            OnPropertyChanged();
        }
    }

    #endregion
}
