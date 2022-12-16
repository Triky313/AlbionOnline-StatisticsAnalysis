﻿using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class VaultBindings : INotifyPropertyChanged
{
    private List<ContainerItem> _vaultContainerContent;
    private List<Vault> _vaults;
    private Vault _vaultSelected;
    private List<VaultContainer> _vaultContainer;
    private VaultContainer _vaultContainerSelected;
    private Visibility _lastUpdateVisibility = Visibility.Hidden;
    private DateTime _lastUpdate;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private ListCollectionView _vaultCollectionView;
    private List<VaultSearchItem> _vaultSearchList = new();
    private string _searchText;

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

    public List<Vault> Vaults
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
            VaultContainer = _vaultSelected?.VaultContainer?.FindAll(x => x?.LastUpdate.Ticks > 0).OrderBy(y => y.Name).ToList();
            OnPropertyChanged();
        }
    }

    public List<VaultContainer> VaultContainer
    {
        get => _vaultContainer;
        set
        {
            _vaultContainer = value;
            OnPropertyChanged();
        }
    }

    public VaultContainer VaultContainerSelected
    {
        get => _vaultContainerSelected;
        set
        {
            _vaultContainerSelected = value;
            VaultContainerContent = _vaultContainer?.FirstOrDefault(x => x.Guid == _vaultContainerSelected.Guid)?.Items ?? new List<ContainerItem>();
            LastUpdate = _vaultContainerSelected?.LastUpdate ?? new DateTime(0);
            LastUpdateVisibility = _vaultContainerSelected?.LastUpdate.Ticks <= 1 ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public List<ContainerItem> VaultContainerContent
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}