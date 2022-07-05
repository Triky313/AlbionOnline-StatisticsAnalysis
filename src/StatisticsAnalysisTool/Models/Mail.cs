using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models;

public class Mail : IComparable<Mail>, INotifyPropertyChanged
{
    private bool? _isSelectedForDeletion = false;
    
    public long Tick { get; set; }
    [JsonIgnore]
    public DateTime Timestamp => new (Tick);
    public Guid Guid { get; set; }
    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    [JsonIgnore]
    public Location Location => Locations.GetLocationByIndex(ClusterIndex);
    [JsonIgnore]
    public string LocationName
    {
        get
        {
            if (Location == Location.Unknown && ClusterIndex.Contains("HIDEOUT"))
            {
                return $"{ClusterIndex.Split("_")[1]} ({LanguageController.Translation("HIDEOUT")})";
            }
            
            if (Location == Location.BlackMarket)
            {
                return "Black Market";
            }

            return WorldData.GetUniqueNameOrDefault((int)Location) ?? LanguageController.Translation("UNKNOWN");
        }
    }
    public string MailTypeText { get; set; }
    [JsonIgnore]
    public MailType MailType => MailController.ConvertToMailType(MailTypeText);
    public MailContent MailContent { get; set; }
    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(MailContent.UniqueItemName);
    [JsonIgnore]
    public string MailTypeDescription
    {
        get
        {
            return MailType switch
            {
                MailType.MarketplaceBuyOrderFinished => LanguageController.Translation("BOUGHT"),
                MailType.MarketplaceSellOrderFinished => LanguageController.Translation("SOLD"),
                MailType.MarketplaceSellOrderExpired => LanguageController.Translation("SELL_EXPIRED"),
                MailType.MarketplaceBuyOrderExpired => LanguageController.Translation("BUY_EXPIRED"),
                _ => LanguageController.Translation("MAIL")
            };
        }
    }
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

    #region Translations

    [JsonIgnore]
    public static string TranslationSilver => LanguageController.Translation("SILVER");
    [JsonIgnore]
    public static string TranslationCostPerItem => LanguageController.Translation("COST_PER_ITEM");
    [JsonIgnore]
    public static string TranslationTotalCost => LanguageController.Translation("TOTAL_COST");
    [JsonIgnore]
    public static string TranslationTotalRevenue => LanguageController.Translation("TOTAL_REVENUE");
    [JsonIgnore]
    public static string TranslationTax => LanguageController.Translation("TAX");
    [JsonIgnore] 
    public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
    [JsonIgnore] 
    public static string TranslationFrom => LanguageController.Translation("FROM");

    #endregion

    public int CompareTo(Mail other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var tickComparison = Tick.CompareTo(other.Tick);
        if (tickComparison != 0) return tickComparison;
        var guidComparison = Guid.CompareTo(other.Guid);
        if (guidComparison != 0) return guidComparison;
        return MailId.CompareTo(other.MailId);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}