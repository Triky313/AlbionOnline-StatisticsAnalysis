using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for MapHistoryControl.xaml
/// </summary>
public partial class MapHistoryControl
{
    private MainWindowViewModel _mainWindowViewModel;
    private ObservableCollection<ClusterInfo> _enteredCluster;

    public MapHistoryControl()
    {
        InitializeComponent();

        Loaded += MapHistoryControl_Loaded;
        Unloaded += MapHistoryControl_Unloaded;
        DataContextChanged += MapHistoryControl_DataContextChanged;
    }

    private async Task DeleteAllMapHistoryEntriesAsync()
    {
        var dialog = new DialogWindow(
            LocalizationController.Translation("DELETE_ALL_MAP_HISTORY_ENTRIES"),
            LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_ALL_MAP_HISTORY_ENTRIES"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is not true)
        {
            return;
        }

        var trackingController = ServiceLocator.Resolve<TrackingController>();

        if (trackingController == null)
        {
            return;
        }

        await trackingController.ClusterController.ClearMapHistoryAsync();
    }

    private void CopySelectedMapHistoryToClipboard()
    {
        var selectedEntries = _mainWindowViewModel?.EnteredCluster?
            .Where(x => x.IsSelectedInMapHistory)
            .OrderBy(x => x.Entered)
            .Select(x => x.MapHistoryClipboardText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (selectedEntries == null || selectedEntries.Count <= 0)
        {
            UpdateCopyButtonState();
            return;
        }

        Clipboard.SetText(string.Join(" -> ", selectedEntries));
    }

    private void MapHistoryControl_Loaded(object sender, RoutedEventArgs e)
    {
        AttachToViewModel(DataContext as MainWindowViewModel);
    }

    private void MapHistoryControl_Unloaded(object sender, RoutedEventArgs e)
    {
        DetachFromViewModel();
    }

    private void MapHistoryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (ReferenceEquals(e.OldValue, e.NewValue))
        {
            return;
        }

        DetachFromViewModel();
        AttachToViewModel(e.NewValue as MainWindowViewModel);
    }

    private void AttachToViewModel(MainWindowViewModel mainWindowViewModel)
    {
        if (ReferenceEquals(_mainWindowViewModel, mainWindowViewModel))
        {
            UpdateCopyButtonState();
            return;
        }

        if (mainWindowViewModel == null)
        {
            UpdateCopyButtonState();
            return;
        }

        _mainWindowViewModel = mainWindowViewModel;
        _mainWindowViewModel.PropertyChanged += MainWindowViewModel_PropertyChanged;
        AttachToCollection(_mainWindowViewModel.EnteredCluster);
        UpdateCopyButtonState();
    }

    private void DetachFromViewModel()
    {
        DetachFromCollection();

        if (_mainWindowViewModel != null)
        {
            _mainWindowViewModel.PropertyChanged -= MainWindowViewModel_PropertyChanged;
            _mainWindowViewModel = null;
        }

        UpdateCopyButtonState();
    }

    private void MainWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainWindowViewModel.EnteredCluster))
        {
            return;
        }

        AttachToCollection(_mainWindowViewModel?.EnteredCluster);
        UpdateCopyButtonState();
    }

    private void AttachToCollection(ObservableCollection<ClusterInfo> enteredCluster)
    {
        if (ReferenceEquals(_enteredCluster, enteredCluster))
        {
            return;
        }

        DetachFromCollection();
        _enteredCluster = enteredCluster;

        if (_enteredCluster == null)
        {
            return;
        }

        _enteredCluster.CollectionChanged += EnteredCluster_CollectionChanged;
        foreach (var clusterInfo in _enteredCluster)
        {
            clusterInfo.PropertyChanged += ClusterInfo_PropertyChanged;
        }
    }

    private void DetachFromCollection()
    {
        if (_enteredCluster == null)
        {
            return;
        }

        _enteredCluster.CollectionChanged -= EnteredCluster_CollectionChanged;
        foreach (var clusterInfo in _enteredCluster)
        {
            clusterInfo.PropertyChanged -= ClusterInfo_PropertyChanged;
        }

        _enteredCluster = null;
    }

    private void EnteredCluster_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems.OfType<ClusterInfo>())
            {
                item.PropertyChanged -= ClusterInfo_PropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems.OfType<ClusterInfo>())
            {
                item.PropertyChanged += ClusterInfo_PropertyChanged;
            }
        }

        UpdateCopyButtonState();
    }

    private void ClusterInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ClusterInfo.IsSelectedInMapHistory))
        {
            UpdateCopyButtonState();
        }
    }

    private void UpdateCopyButtonState()
    {
        BtnCopySelectedMapHistoryToClipboard.IsEnabled = _mainWindowViewModel?.EnteredCluster?.Any(x => x.IsSelectedInMapHistory) ?? false;
    }

    private void BtnCopySelectedMapHistoryToClipboard_Click(object sender, RoutedEventArgs e)
    {
        CopySelectedMapHistoryToClipboard();
    }

    private async void BtnDeleteAllMapHistoryEntries_Click(object sender, RoutedEventArgs e)
    {
        await DeleteAllMapHistoryEntriesAsync();
    }
}