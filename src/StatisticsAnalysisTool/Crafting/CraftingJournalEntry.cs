using StatisticsAnalysisTool.ViewModels;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingJournalEntry : BaseViewModel
{
    [JsonIgnore]
    public Action ValuesChanged { get; set; }
    public string EmptyJournalUniqueName { get; set; }
    public string FullJournalUniqueName { get; set; }
    public string DisplayName { get; set; }
    public decimal FamePerRun { get; set; }
    public decimal MaxFamePerJournal { get; set; }
    public double UnitWeight { get; set; }

    [JsonIgnore]
    public BitmapImage Icon { get; set; }

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