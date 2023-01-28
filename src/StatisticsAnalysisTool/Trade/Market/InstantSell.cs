using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade.Market;

public class InstantSell : Trade, IComparable<InstantSell>
{
    public int Amount { get; init; }
    public AuctionEntry AuctionEntry { get; init; }
    public double TaxRate { get; set; }

    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(AuctionEntry.ItemTypeId);

    [JsonIgnore]
    public string TypeDescription => LanguageController.Translation("INSTANT_SELL");

    public int CompareTo(InstantSell other)
    {
        if (ReferenceEquals(this, other)) 
            return 0;
        if (ReferenceEquals(null, other)) 
            return 1;
        var tickComparison = Ticks.CompareTo(other.Ticks);
        if (tickComparison != 0) 
            return tickComparison;

        return Id.CompareTo(other.Id);
    }

    #region Commands

    public void OpenItemWindow(object value)
    {
        MainWindowViewModel.OpenItemWindow(Item);
    }

    private ICommand _openItemWindowCommand;

    public ICommand OpenItemWindowCommand => _openItemWindowCommand ??= new CommandHandler(OpenItemWindow, true);

    #endregion
}