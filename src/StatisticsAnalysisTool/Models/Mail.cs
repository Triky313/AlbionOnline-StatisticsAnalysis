﻿using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Models;

public class Mail : IComparable<Mail>, INotifyPropertyChanged
{
    private bool? _isSelectedForDeletion = false;

    public long Tick { get; set; }
    [JsonIgnore]
    public DateTime Timestamp => new(Tick);
    public Guid Guid { get; set; }
    public long MailId { get; set; }
    public string ClusterIndex { get; set; }
    [JsonIgnore]
    public MarketLocation Location => Locations.GetMarketLocationByIndex(ClusterIndex);
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
    public static string TranslationSetupTax => LanguageController.Translation("SETUP_TAX");
    [JsonIgnore]
    public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
    [JsonIgnore]
    public static string TranslationFrom => LanguageController.Translation("FROM");
    [JsonIgnore]
    public static string TranslationTotalPriceWithDeductedTaxes => LanguageController.Translation("TOTAL_PRICE_WITH_DEDUCTED_TAXES");

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

    #region Commands

    public void OpenItemWindow(object value)
    {
        MainWindowViewModel.OpenItemWindow(Item);
    }

    private ICommand _openItemWindowCommand;

    public ICommand OpenItemWindowCommand => _openItemWindowCommand ??= new CommandHandler(OpenItemWindow, true);

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}