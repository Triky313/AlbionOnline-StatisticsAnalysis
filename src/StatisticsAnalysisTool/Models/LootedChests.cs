using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Models;

public class LootedChests : INotifyPropertyChanged
{
    private int _openWorldCommonWeek;

    public int OpenWorldCommonWeek
    {
        get => _openWorldCommonWeek;
        set
        {
            _openWorldCommonWeek = value;
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