using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Trade;

public abstract class Trade : INotifyPropertyChanged
{
    public long Id { get; init; }
    public long Ticks { get; init; }
    public string ClusterIndex { get; init; }
    public TradeType Type { get; init; }
    public string UniqueClusterName => WorldData.GetUniqueNameOrDefault(ClusterIndex);
    public DateTime Timestamp => new(Ticks);

    private bool? _isSelectedForDeletion = false;

    [JsonIgnore]
    public bool? IsSelectedForDeletion
    {
        get => _isSelectedForDeletion;
        set
        {
            _isSelectedForDeletion = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}