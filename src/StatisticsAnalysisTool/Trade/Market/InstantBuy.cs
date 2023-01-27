using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade.Market;

public class InstantBuy : Trade, IComparable<InstantBuy>
{
    public int Amount { get; init; }
    public AuctionEntry AuctionEntry { get; init; }
    public double TaxRate { get; set; }

    [JsonIgnore]
    public Item Item => ItemController.GetItemByUniqueName(AuctionEntry.ItemTypeId);

    [JsonIgnore]
    public string TypeDescription => LanguageController.Translation("INSTANT_BUY");

    public int CompareTo(InstantBuy other)
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