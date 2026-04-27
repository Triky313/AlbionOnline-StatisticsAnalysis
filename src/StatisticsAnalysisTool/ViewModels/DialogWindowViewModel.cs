using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.TranslationModel;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class DialogWindowViewModel : BaseViewModel
{
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

    public DialogType Type
    {
        get;
        set
        {
            field = value;
            switch (field)
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
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility InfoTypeVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility ErrorTypeVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public string OkButtonText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = "Okay";

    public string Url
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UrlVisibility));
        }
    }

    public string UrlText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility UrlVisibility => string.IsNullOrWhiteSpace(Url) ? Visibility.Collapsed : Visibility.Visible;

    public DialogWindowTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    #endregion
}