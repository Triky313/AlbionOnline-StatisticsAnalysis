using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingJournalEntry : BaseViewModel
{
    [JsonIgnore]
    public Action ValuesChanged { get; set; }
    public string EmptyJournalUniqueName { get; set; }
    public string FullJournalUniqueName { get; set; }

    [JsonIgnore]
    public string DisplayName => ItemController.GetItemByUniqueName(EmptyJournalUniqueName)?.LocalizedName ?? EmptyJournalUniqueName ?? string.Empty;

    public decimal FamePerRun { get; set; }
    public decimal MaxFamePerJournal { get; set; }
    public double UnitWeight { get; set; }

    [JsonIgnore]
    public BitmapImage Icon { get; set; }

    [JsonIgnore]
    public ObservableCollection<CraftingSellPriceOption> EmptyJournalPriceOptions
    {
        get;
    }
    = [];

    [JsonIgnore]
    public ObservableCollection<CraftingSellPriceOption> FullJournalPriceOptions
    {
        get;
    }
    = [];

    [JsonIgnore]
    public bool IsEmptyJournalPricePopupOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public bool IsFullJournalPricePopupOpen
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal EmptyJournalPrice
    {
        get;
        set
        {
            field = value;
            ValuesChanged?.Invoke();
            OnPropertyChanged();
        }
    }

    public decimal FullJournalPrice
    {
        get;
        set
        {
            field = value;
            ValuesChanged?.Invoke();
            OnPropertyChanged();
        }
    }

    public int RequiredEmptyJournals
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int ExpectedFullJournals
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal PartialJournalPercent
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalEmptyJournalCosts
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public decimal TotalFullJournalRevenue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}
