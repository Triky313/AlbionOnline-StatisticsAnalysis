using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemDetailsMainTabBindings : BaseViewModel
{
    private readonly ItemDetailsViewModel _itemDetailsViewModel;
    private List<QualityStruct> _qualities = new();
    private QualityStruct _qualitiesSelection;
    private ObservableCollection<ItemPricesObject> _itemPrices = new();

    public ItemDetailsMainTabBindings(ItemDetailsViewModel itemDetailsViewModel)
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
            if (_qualitiesSelection.Quality == value.Quality)
            {
                return;
            }

            _qualitiesSelection = value;
            OnPropertyChanged();
            _itemDetailsViewModel.ApplySelectedQualityFilter();
        }
    }

    public ObservableCollection<ItemPricesObject> ItemPrices
    {
        get => _itemPrices;
        set
        {
            _itemPrices = value;
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