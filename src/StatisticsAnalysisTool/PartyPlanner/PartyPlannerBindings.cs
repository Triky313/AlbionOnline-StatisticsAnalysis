using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace StatisticsAnalysisTool.PartyPlanner;

public class PartyPlannerBindings : INotifyPropertyChanged
{
    private ObservableCollection<PartyBuilderPlayer> _party = new();
    private ListCollectionView _partyCollectionView;

    public PartyPlannerBindings()
    {
        PartyCollectionView = CollectionViewSource.GetDefaultView(Party) as ListCollectionView;

        if (PartyCollectionView != null)
        {
            PartyCollectionView.IsLiveSorting = true;
            PartyCollectionView.IsLiveFiltering = true;
            PartyCollectionView.CustomSort = new PartyPlannerPlayerComparer();
            PartyCollectionView.Refresh();
        }
    }

    public ListCollectionView PartyCollectionView
    {
        get => _partyCollectionView;
        set
        {
            _partyCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PartyBuilderPlayer> Party
    {
        get => _party;
        set
        {
            _party = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}