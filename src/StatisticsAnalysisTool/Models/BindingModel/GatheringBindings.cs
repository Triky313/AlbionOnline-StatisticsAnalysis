using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : INotifyPropertyChanged
{
    public GatheringBindings()
    {
    }

    #region Bindings

    //public ListCollectionView NotificationsCollectionView
    //{
    //    get => _notificationsCollectionView;
    //    set
    //    {
    //        _notificationsCollectionView = value;
    //        OnPropertyChanged();
    //    }
    //}

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}