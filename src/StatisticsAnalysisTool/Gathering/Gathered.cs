using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Models.ItemsJsonModel;

namespace StatisticsAnalysisTool.Gathering;

public class Gathered : INotifyPropertyChanged
{
    private int _gainedStandardAmount;
    private int _gainedBonusAmount;
    private int _gainedPremiumBonusAmount;
    private int _gainedFame;
    private bool _isClosed;
    private string _uniqueName;

    public long Timestamp { get; init; }
    public long ObjectId { get; init; }
    public long UserObjectId { get; init; }

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            Item = ItemController.GetItemByUniqueName(_uniqueName);
            if (Item.FullItemInformation is SimpleItem simpleItem && int.TryParse(simpleItem.FameValue, NumberStyles.Any, CultureInfo.CurrentCulture, out var gainedFame))
            {
                GainedFame = gainedFame;
            }
        }
    }

    public Item Item { get; set; }

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