using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Trade;

public abstract class Trade : INotifyPropertyChanged
{
    private bool? _isSelectedForDeletion = false;

    public long Id { get; init; }
    public long Ticks { get; init; }
    public string ClusterIndex { get; init; }
    public string UniqueClusterName => WorldData.GetUniqueNameOrDefault(ClusterIndex);
    public DateTime Timestamp => new(Ticks);

    [JsonIgnore]
    protected MarketLocation Location => Locations.GetMarketLocationByIndex(ClusterIndex);

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

    [JsonIgnore]
    public string LocationName
    {
        get
        {
            if (Location == MarketLocation.Unknown && ClusterIndex.Contains("HIDEOUT"))
            {
                return $"{ClusterIndex.Split("_")[1]} ({LanguageController.Translation("HIDEOUT")})";
            }

            if (Location == MarketLocation.BlackMarket)
            {
                return "Black Market";
            }

            return WorldData.GetUniqueNameOrDefault((int) Location) ?? LanguageController.Translation("UNKNOWN");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}