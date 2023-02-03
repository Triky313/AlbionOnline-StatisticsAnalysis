using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : INotifyPropertyChanged
{
    private bool _isGatheringActive = true;
    private ObservableCollection<ResourcesContent> _currentResourcesContent = new();
    private List<ResourcesContent> _resourcesContent = new();
    private ObservableCollection<Gathered> _gatheredCollection = new ();
    private readonly ListCollectionView _gatheredCollectionView;
    private GridLength _gridSplitterPosition = GridLength.Auto;

    public GatheringBindings()
    {
        IsGatheringActive = SettingsController.CurrentSettings.IsGatheringActive;

        GatheredCollectionView = CollectionViewSource.GetDefaultView(GatheredCollection) as ListCollectionView;

        if (GatheredCollectionView != null)
        {
            GatheredCollectionView.IsLiveSorting = true;
            GatheredCollectionView.IsLiveFiltering = true;
            GatheredCollectionView.CustomSort = new GatheredComparer();

            GatheredCollectionView?.Refresh();
        }
    }

    #region Bindings

    public ListCollectionView GatheredCollectionView
    {
        get => _gatheredCollectionView;
        init
        {
            _gatheredCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Gathered> GatheredCollection
    {
        get => _gatheredCollection;
        set
        {
            _gatheredCollection = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.GatheringGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ResourcesContent> CurrentResourcesContent
    {
        get => _currentResourcesContent;
        set
        {
            _currentResourcesContent = value;
            OnPropertyChanged();
        }
    }

    public List<ResourcesContent> ResourcesContent
    {
        get => _resourcesContent;
        set
        {
            _resourcesContent = value;
            OnPropertyChanged();
        }
    }

    public bool IsGatheringActive
    {
        get => _isGatheringActive;
        set
        {
            _isGatheringActive = value;
            SettingsController.CurrentSettings.IsGatheringActive = _isGatheringActive;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationGatheringActive => LanguageController.Translation("GATHERING_ACTIVE");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}