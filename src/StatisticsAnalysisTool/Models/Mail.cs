using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class Mail : IComparable<Mail>
{
    public Mail(long tick, Guid? guid, long mailId, string clusterIndex, string mailTypeText, MailContent mailContent)
    {
        Tick = tick;
        Timestamp = new DateTime(tick);
        Guid = guid ?? default;
        MailId = mailId;
        ClusterIndex = clusterIndex;
        MailTypeText = mailTypeText;
        MailContent = mailContent;
    }

    public long Tick { get; init; }
    [JsonIgnore]
    public DateTime Timestamp { get; init; }
    public Guid Guid { get; init; }
    public long MailId { get; init; }
    public string ClusterIndex { get; init; }
    [JsonIgnore]
    public Location Location => Locations.GetLocationByIndex(ClusterIndex);
    [JsonIgnore]
    public string LocationName => Locations.GetName(Location);
    public string MailTypeText { get; set; }
    [JsonIgnore]
    public MailType MailType => MailController.ConvertToMailType(MailTypeText);
    public MailContent MailContent { get; set; }
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
                _ => LanguageController.Translation("MAIL")
            };
        }
    }

    #region Translations
    [JsonIgnore]
    public string TranslationSilver => LanguageController.Translation("SILVER");
    [JsonIgnore]
    public string TranslationCostPerItem => LanguageController.Translation("COST_PER_ITEM");
    [JsonIgnore]
    public string TranslationTotalCost => LanguageController.Translation("TOTAL_COST");

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
}