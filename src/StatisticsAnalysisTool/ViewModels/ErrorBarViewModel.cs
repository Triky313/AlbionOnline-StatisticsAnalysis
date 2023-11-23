using StatisticsAnalysisTool.Common;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.ViewModels;

public class ErrorBarViewModel : BaseViewModel
{
    private string _text;
    private Visibility _visibility;

    public ErrorBarViewModel()
    {
        Visibility = Visibility.Collapsed;
        CloseCommand = new CommandHandler(ExecuteCloseCommand, true);
    }

    public ICommand CloseCommand { get; }

    private void ExecuteCloseCommand(object parameter)
    {
        Text = string.Empty;
        Visibility = Visibility.Collapsed;
    }

    public void Set(Visibility visibility, string errorMessage)
    {
        Text = errorMessage;
        Visibility = visibility;
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }
}