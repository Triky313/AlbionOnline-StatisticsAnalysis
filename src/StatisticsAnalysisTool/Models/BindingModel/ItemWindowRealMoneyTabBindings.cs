using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowRealMoneyTabBindings : INotifyPropertyChanged
{
    private readonly ItemWindowViewModel _itemWindowViewModel;
    private List<QualityStruct> _qualities = new();
    private QualityStruct _qualitiesSelection;
    private ObservableCollection<ItemPricesObject> _prices = new();

    public ItemWindowRealMoneyTabBindings(ItemWindowViewModel itemWindowViewModel)
    {
        _itemWindowViewModel = itemWindowViewModel;
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
            _itemWindowViewModel.UpdateMainTabItemPrices(null, null);
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}