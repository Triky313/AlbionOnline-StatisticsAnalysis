using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemDetailsRealMoneyTabBindings : BaseViewModel
{
    private readonly ItemDetailsViewModel _itemDetailsViewModel;
    private List<QualityStruct> _qualities = new();
    private QualityStruct _qualitiesSelection;
    private ObservableCollection<ItemPricesObject> _prices = new();

    public ItemDetailsRealMoneyTabBindings(ItemDetailsViewModel itemDetailsViewModel)
    {
        _itemDetailsViewModel = itemDetailsViewModel;
    }

    #region Bindings

    public List<QualityStruct> Qualities
    {
        get => _qualities;
        set
        {
            _qualities = value;
            OnPropertyChanged();
        }
    }

    public QualityStruct QualitiesSelection
    {
        get => _qualitiesSelection;
        set
        {
            _qualitiesSelection = value;
            _itemDetailsViewModel.UpdateMainTabItemPrices();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ItemPricesObject> Prices
    {
        get => _prices;
        set
        {
            _prices = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public struct QualityStruct
    {
        public string Name { get; set; }
        public int Quality { get; set; }
    }
}