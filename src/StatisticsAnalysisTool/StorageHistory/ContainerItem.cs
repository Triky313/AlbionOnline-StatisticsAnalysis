using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.StorageHistory;

public class ContainerItem : BaseViewModel
{
    private int _itemIndex;
    private int _quantity;
    private Visibility _averagePricesDisplayedOnItemVisibility = Visibility.Collapsed;

    public int ItemIndex
    {
        get => _itemIndex;
        set
        {
            _itemIndex = value;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public Visibility AveragePricesDisplayedOnItemVisibility
    {
        get => _averagePricesDisplayedOnItemVisibility;
        set
        {
            _averagePricesDisplayedOnItemVisibility = value;
            OnPropertyChanged();
        }
    }

    public Item Item => ItemController.GetItemByIndex(ItemIndex);

    public double TotalAvgEstMarketValue => Quantity * Item?.AverageEstMarketValue ?? 0;
    public double TotalWeight => Quantity * ItemController.GetWeight(Item?.FullItemInformation);
}