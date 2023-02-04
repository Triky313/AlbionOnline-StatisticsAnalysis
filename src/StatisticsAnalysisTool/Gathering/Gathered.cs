using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Gathering;

public class Gathered : INotifyPropertyChanged
{
    private int _gainedStandardAmount;
    private int _gainedBonusAmount;
    private int _gainedPremiumBonusAmount;
    private int _gainedFame;
    private bool _isClosed;

    public long Timestamp { get; init; }
    public long ObjectId { get; init; }
    public long UserObjectId { get; init; }
    public string UniqueName { get; init; }
    public Item Item => ItemController.GetItemByUniqueName(UniqueName);

    public int GainedStandardAmount
    {
        get => _gainedStandardAmount;
        set
        {
            _gainedStandardAmount = value;
            OnPropertyChanged();
        }
    }

    public int GainedBonusAmount
    {
        get => _gainedBonusAmount;
        set
        {
            _gainedBonusAmount = value;
            OnPropertyChanged();
        }
    }

    public int GainedPremiumBonusAmount
    {
        get => _gainedPremiumBonusAmount;
        set
        {
            _gainedPremiumBonusAmount = value;
            OnPropertyChanged();
        }
    }

    public int GainedFame
    {
        get => _gainedFame;
        set
        {
            _gainedFame = value;
            OnPropertyChanged();
        }
    }

    public string ClusterIndex { get; init; }
    public string ClusterUniqueName => WorldData.GetUniqueNameOrDefault(ClusterIndex);
    public bool IsClosed
    {
        get => _isClosed;
        set
        {
            _isClosed = value;
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