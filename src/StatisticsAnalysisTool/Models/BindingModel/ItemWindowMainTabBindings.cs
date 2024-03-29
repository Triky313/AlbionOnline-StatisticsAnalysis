﻿using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowMainTabBindings : BaseViewModel
{
    private readonly ItemWindowViewModel _itemWindowViewModel;
    private List<QualityStruct> _qualities = new();
    private QualityStruct _qualitiesSelection;
    private ObservableCollection<ItemPricesObject> _itemPrices = new();

    public ItemWindowMainTabBindings(ItemWindowViewModel itemWindowViewModel)
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
            SettingsController.CurrentSettings.ItemWindowMainTabQualitySelection = _qualitiesSelection.Quality;
            _itemWindowViewModel.UpdateMainTabItemPrices(null, null);
            OnPropertyChanged();
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