using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels;

public class ThanksViewModel : INotifyPropertyChanged
{
    private ThanksTranslation _translation;

    public ThanksViewModel()
    {
        Translation = new ThanksTranslation();
    }

    public ThanksTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
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