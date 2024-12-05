using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultBindings : BaseViewModel
{
    private ObservableCollection<ContainerItem> _vaultContainerContent;
    private ObservableCollection<Vault> _vaults;
    private Vault _vaultSelected;
    private ObservableCollection<VaultContainer> _vaultContainer;
    private VaultContainer _vaultContainerSelected;
    private Visibility _lastUpdateVisibility = Visibility.Hidden;
    private DateTime _lastUpdate;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private ListCollectionView _vaultCollectionView;
    private List<VaultSearchItem> _vaultSearchList = new();
    private string _searchText;
    private bool _isAveragePricesDisplayedOnItem;
    private double _totalContainerValue;
    private double _totalWeight;
    private double _totalVaultValue;

    public VaultBindings()
    {
        VaultSearchCollectionView = CollectionViewSource.GetDefaultView(VaultSearchList) as ListCollectionView;

        if (VaultSearchCollectionView != null)
        {
            VaultSearchCollectionView.IsLiveSorting = true;
            VaultSearchCollectionView.IsLiveFiltering = true;

            VaultSearchCollectionView?.Refresh();
        }
    }

    public ObservableCollection<Vault> Vaults
    {
        get => _vaults;
        set
        {
            _vaults = value;
            OnPropertyChanged();
        }
    }

    public Vault VaultSelected
    {
        get => _vaultSelected;
        set
        {
            _vaultSelected = value;
            VaultContainer = new ObservableCollection<VaultContainer>(
                _vaultSelected?.VaultContainer?.FindAll(x => x?.LastUpdate.Ticks > 0).OrderBy(y => y.Name).ToList() ??
                new List<VaultContainer>()
            );

            OnPropertyChanged();
        }
    }

    public ObservableCollection<VaultContainer> VaultContainer
    {
        get => _vaultContainer;
        set
        {
            var vaultContainerSelectedGuid = VaultContainerSelected?.Guid;
            _vaultContainer = value;
            if (VaultContainerSelected is not null)
            {
                VaultContainerSelected = VaultContainer.FirstOrDefault(x => x?.Guid == vaultContainerSelectedGuid);
            }
            OnPropertyChanged();
        }
    }

    public VaultContainer VaultContainerSelected
    {
        get => _vaultContainerSelected;
        set
        {
            _vaultContainerSelected = value;
            VaultContainerContent = new ObservableCollection<ContainerItem>(
                (_vaultContainer?.FirstOrDefault(x => x.Guid == _vaultContainerSelected?.Guid)?.Items ?? new ObservableCollection<ContainerItem>())
                .Select(containerItem =>
                {
                    containerItem.AveragePricesDisplayedOnItemVisibility =
                        IsAveragePricesDisplayedOnItem
                        && containerItem.Item?.EstimatedMarketValueStatus is PastTime.New or PastTime.AlmostNew or PastTime.LittleNew or PastTime.BitOld or PastTime.Old or PastTime.VeryOld or PastTime.VeryVeryOld
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    return containerItem;
                }));

            TotalContainerValue = VaultContainerContent.Sum(x => x.TotalAvgEstMarketValue);
            TotalVaultValue = VaultSelected?.VaultContainer?.SelectMany(x => x?.Items).Sum(x => x.TotalAvgEstMarketValue) ?? 0;
            TotalWeight = VaultContainerContent.Sum(x => x.TotalWeight);
            LastUpdate = _vaultContainerSelected?.LastUpdate ?? new DateTime(0);
            LastUpdateVisibility = _vaultContainerSelected?.LastUpdate.Ticks <= 1 ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ContainerItem> VaultContainerContent
    {
        get => _vaultContainerContent;
        set
        {
            _vaultContainerContent = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView VaultSearchCollectionView
    {
        get => _vaultCollectionView;
        set
        {
            _vaultCollectionView = value;
            OnPropertyChanged();
        }
    }

    public List<VaultSearchItem> VaultSearchList
    {
        get => _vaultSearchList;
        set
        {
            _vaultSearchList = value;
            OnPropertyChanged();
        }
    }

    public bool IsAveragePricesDisplayedOnItem
    {
        get => _isAveragePricesDisplayedOnItem;
        set
        {
            _isAveragePricesDisplayedOnItem = value;
            foreach (ContainerItem containerItem in VaultContainerContent ?? new ObservableCollection<ContainerItem>())
            {
                if (IsAveragePricesDisplayedOnItem
                    && containerItem.Item?.EstimatedMarketValueStatus is PastTime.New or PastTime.AlmostNew or PastTime.LittleNew or PastTime.BitOld or PastTime.Old or PastTime.VeryOld or PastTime.VeryVeryOld)
                {
                    containerItem.AveragePricesDisplayedOnItemVisibility = Visibility.Visible;
                }
                else
                {
                    containerItem.AveragePricesDisplayedOnItemVisibility = Visibility.Collapsed;
                }
            }
            OnPropertyChanged();
        }
    }

    public double TotalContainerValue
    {
        get => _totalContainerValue;
        set
        {
            _totalContainerValue = value;
            OnPropertyChanged();
        }
    }

    public double TotalVaultValue
    {
        get => _totalVaultValue;
        set
        {
            _totalVaultValue = value;
            OnPropertyChanged();
        }
    }

    public double TotalWeight
    {
        get => _totalWeight;
        set
        {
            _totalWeight = value;
            OnPropertyChanged();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;

            if (_searchText.Length >= 2)
            {
                VaultSearchCollectionView.Filter = Filter;
            }

            if (_searchText.Length <= 0)
            {
                VaultSearchCollectionView.Filter = null;
            }

            OnPropertyChanged();
        }
    }

    public Visibility LastUpdateVisibility
    {
        get => _lastUpdateVisibility;
        set
        {
            _lastUpdateVisibility = value;
            OnPropertyChanged();
        }
    }

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.StorageHistoryGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    #region Filter

    private bool Filter(object obj)
    {
        return obj is VaultSearchItem vaultSearchItem && ((vaultSearchItem.Location != null && vaultSearchItem.Location.ToLower().Contains(SearchText?.ToLower() ?? string.Empty))
                                                          || ($"T{vaultSearchItem.Item?.Tier}.{vaultSearchItem.Item?.Level}".ToLower().Contains(SearchText?.ToLower() ?? string.Empty))
                                                          || vaultSearchItem.MainLocation != null && vaultSearchItem.MainLocation.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)
                                                          || (vaultSearchItem.Item != null && vaultSearchItem.Item.LocalizedName.ToLower().Contains(SearchText?.ToLower() ?? string.Empty)));
    }

    #endregion
}