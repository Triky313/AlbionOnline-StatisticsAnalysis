using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using System.Text.Json.Serialization;
using System.Windows.Input;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Trade.Market;

public class InstantSell : Trade, IComparable<InstantSell>
{
    public int Amount { get; init; }
    public AuctionEntry AuctionEntry { get; init; }

    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(AuctionEntry.ItemTypeId);

    public int CompareTo(InstantSell other)
    {
        throw new NotImplementedException();
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