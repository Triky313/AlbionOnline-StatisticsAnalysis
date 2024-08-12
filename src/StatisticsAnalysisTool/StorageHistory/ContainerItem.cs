using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.StorageHistory;

public class ContainerItem : BaseViewModel
{
    private int _itemIndex;
    private int _quantity;

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
    public Item Item => ItemController.GetItemByIndex(ItemIndex);
}