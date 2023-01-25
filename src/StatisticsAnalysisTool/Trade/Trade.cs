using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Trade;

public abstract class Trade : INotifyPropertyChanged
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public string ClusterIndex { get; init; }
    public string UniqueClusterName => WorldData.GetUniqueNameOrDefault(ClusterIndex);
    public DateTime Timestamp => new(Ticks);

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}