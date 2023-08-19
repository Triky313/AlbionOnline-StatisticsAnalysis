using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.TranslationModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace StatisticsAnalysisTool.ViewModels;

public class ConsoleWindowViewModel : BaseViewModel
{
    private ConsoleWindowTranslation _translation;
    private ListCollectionView _consoleCollectionView;

    public ConsoleWindowViewModel()
    {
        Init();
    }

    private void Init()
    {
        Translation = new ConsoleWindowTranslation();

        ConsoleCollectionView = CollectionViewSource.GetDefaultView(ConsoleManager.Console) as ListCollectionView;
        if (ConsoleCollectionView != null)
        {
            ConsoleCollectionView.IsLiveSorting = true;
            ConsoleCollectionView.IsLiveFiltering = true;
        }
    }
        
    public ListCollectionView ConsoleCollectionView
    {
        get => _consoleCollectionView;
        set
        {
            _consoleCollectionView = value;
            OnPropertyChanged();
        }
    }
        
    public ConsoleWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }
}