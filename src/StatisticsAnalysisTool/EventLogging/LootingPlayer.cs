using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.EventLogging;

public class LootingPlayer : BaseViewModel
{
    private string _playerName;
    private string _playerGuild;
    private string _playerAlliance;
    private ObservableCollection<LootedItem> _lootedItems = new();
    private readonly object _lootedItemsSyncRoot = new();
    private Visibility _lootingPlayerVisibility = Visibility.Visible;

    public LootingPlayer()
    {
        SubscribeLootedItems(_lootedItems);
    }

    public string PlayerName
    {
        get => _playerName;
        set
        {
            _playerName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string PlayerGuild
    {
        get => _playerGuild;
        set
        {
            _playerGuild = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string PlayerAlliance
    {
        get => _playerAlliance;
        set
        {
            _playerAlliance = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public string DisplayName
        => BuildDisplayName();

    public ObservableCollection<LootedItem> LootedItems
    {
        get => _lootedItems;
        set
        {
            UnsubscribeLootedItems(_lootedItems);
            _lootedItems = value ?? [];
            SubscribeLootedItems(_lootedItems);
            OnPropertyChanged();
            NotifyEstimatedMarketValueChanged();
        }
    }

    public Visibility LootingPlayerVisibility
    {
        get => _lootingPlayerVisibility;
        set
        {
            _lootingPlayerVisibility = value;
            OnPropertyChanged();
        }
    }

    public long TotalEstimatedMarketValue
        => GetLootedItemsSnapshot()
            .Select(item => item.Quantity * (item.Item?.AverageEstMarketValue ?? 0))
            .Where(estimatedMarketValue => estimatedMarketValue > 0)
            .Sum();

    public string TotalEstimatedMarketValueShortString
        => TotalEstimatedMarketValue.ToShortNumberString();

    public Visibility TotalEstimatedMarketValueVisibility
        => TotalEstimatedMarketValue > 0 ? Visibility.Visible : Visibility.Collapsed;

    private string BuildDisplayName()
    {
        if (string.IsNullOrWhiteSpace(PlayerName))
        {
            return string.Empty;
        }

        List<string> affiliations = [];

        if (!string.IsNullOrWhiteSpace(PlayerGuild))
        {
            affiliations.Add(PlayerGuild);
        }

        if (!string.IsNullOrWhiteSpace(PlayerAlliance))
        {
            affiliations.Add(PlayerAlliance);
        }

        return affiliations.Count > 0 ? $"{PlayerName} ({string.Join(", ", affiliations)})" : PlayerName;
    }

    public int LootedItemCount
    {
        get
        {
            lock (_lootedItemsSyncRoot)
            {
                return _lootedItems.Count;
            }
        }
    }

    public List<LootedItem> GetLootedItemsSnapshot()
    {
        lock (_lootedItemsSyncRoot)
        {
            return _lootedItems.ToList();
        }
    }

    public void AddLootedItem(LootedItem lootedItem)
    {
        if (lootedItem is null)
        {
            return;
        }

        lock (_lootedItemsSyncRoot)
        {
            _lootedItems.Add(lootedItem);
        }
    }

    public void RemoveLootedItems(IEnumerable<LootedItem> lootedItems)
    {
        if (lootedItems is null)
        {
            return;
        }

        foreach (var lootedItem in lootedItems.ToList())
        {
            lock (_lootedItemsSyncRoot)
            {
                _lootedItems.Remove(lootedItem);
            }
        }
    }

    private void SubscribeLootedItems(ObservableCollection<LootedItem> lootedItems)
    {
        if (lootedItems is null)
        {
            return;
        }

        BindingOperations.EnableCollectionSynchronization(lootedItems, _lootedItemsSyncRoot);
        lootedItems.CollectionChanged += LootedItemsCollectionChanged;
        lock (_lootedItemsSyncRoot)
        {
            foreach (var lootedItem in lootedItems)
            {
                lootedItem.PropertyChanged += LootedItemPropertyChanged;
            }
        }
    }

    private void UnsubscribeLootedItems(ObservableCollection<LootedItem> lootedItems)
    {
        if (lootedItems is null)
        {
            return;
        }

        lootedItems.CollectionChanged -= LootedItemsCollectionChanged;
        BindingOperations.DisableCollectionSynchronization(lootedItems);
        lock (_lootedItemsSyncRoot)
        {
            foreach (var lootedItem in lootedItems)
            {
                lootedItem.PropertyChanged -= LootedItemPropertyChanged;
            }
        }
    }

    private void LootedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.OldItems is not null)
        {
            foreach (LootedItem oldItem in args.OldItems)
            {
                oldItem.PropertyChanged -= LootedItemPropertyChanged;
            }
        }

        if (args.NewItems is not null)
        {
            foreach (LootedItem newItem in args.NewItems)
            {
                newItem.PropertyChanged += LootedItemPropertyChanged;
            }
        }

        NotifyEstimatedMarketValueChanged();
    }

    private void LootedItemPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(LootedItem.ItemIndex) or nameof(LootedItem.Quantity))
        {
            NotifyEstimatedMarketValueChanged();
        }
    }

    private void NotifyEstimatedMarketValueChanged()
    {
        OnPropertyChanged(nameof(TotalEstimatedMarketValue));
        OnPropertyChanged(nameof(TotalEstimatedMarketValueShortString));
        OnPropertyChanged(nameof(TotalEstimatedMarketValueVisibility));
    }
}
