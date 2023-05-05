using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class DialogWindowViewModel : INotifyPropertyChanged
{
    private string _title;
    private string _message;
    private DialogWindowTranslation _dialogWindowTranslation = new();
    private DialogType _type;
    private Visibility _yesNoVisibility = Visibility.Collapsed;
    private Visibility _errorTypeVisibility = Visibility.Collapsed;
    private string _okButtonText = "Ok";

    public DialogWindowViewModel(string title, string message, DialogType type)
    {
        Title = title;
        Message = message;
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
                    ErrorTypeVisibility = Visibility.Collapsed;
                    YesNoVisibility = Visibility.Visible;
                    break;
                case DialogType.Error:
                    YesNoVisibility = Visibility.Collapsed;
                    ErrorTypeVisibility = Visibility.Visible;
                    OkButtonText = "Ok";
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}