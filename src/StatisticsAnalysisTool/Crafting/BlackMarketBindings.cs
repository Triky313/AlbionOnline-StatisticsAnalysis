using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Crafting;

public sealed class BlackMarketBindings : BaseViewModel
{
    private const string HistoryFileName = "BlackMarketHistory.json";
    private const int HistoryRetentionDays = 365;
    private const int SevenDaysTimeRange = 1;
    private const int FourWeeksTimeRange = 2;
    private const int SevenDaysWindowDays = 7;
    private const int FourWeeksWindowDays = 28;
    private readonly object _historyLock = new();
    private readonly Dictionary<(string ItemUniqueName, int QualityLevel), BlackMarketHistoryEntry> _history = new();
    private readonly Dictionary<(string ItemUniqueName, int QualityLevel), BlackMarketItemRow> _rowsByKey = new();
    private readonly ConcurrentDictionary<int, BlackMarketAverageStatsRequestContext> _requestContexts = new();
    private string _searchText = string.Empty;
    private CategoryDropdownItem _selectedItemShopCategory;
    private CategoryDropdownItem _selectedItemShopSubCategory1;
    private CategoryDropdownItem _selectedItemShopSubCategory2;
    private CategoryDropdownItem _selectedItemShopSubCategory3;
    private ItemTier _selectedItemTier = ItemTier.Unknown;
    private ItemLevel _selectedItemLevel = ItemLevel.Unknown;
    private int _selectedQualityLevel = -1;
    private int _selectedChartDays = 30;
    private BlackMarketItemRow _selectedItem;
    private double _selectedItemAverageItemsPerDay;
    private DateTime _chartFrameFromDate = DateTime.MinValue;
    private int _chartFrameDays;
    private string[] _chartLabels = [];

    public BlackMarketBindings()
    {
        LoadCategoriesToDropdown();
        ItemTiers = FrequentlyValues.ItemTiers;
        ItemLevels = FrequentlyValues.ItemLevels;
        ItemQualities = CreateQualityFilters();
        ItemsView = CollectionViewSource.GetDefaultView(Items);
        ItemsView.Filter = IsItemVisible;
        BuildRows();
        ResetChart();
        _ = LoadAsync();
    }

    public ObservableCollection<BlackMarketItemRow> Items { get; } = [];

    public ICollectionView ItemsView { get; }

    public ObservableCollection<CategoryDropdownItem> ItemShopCategories { get; } = [];

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories1 { get; private set; } = [];

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories2 { get; private set; } = [];

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories3 { get; private set; } = [];

    public Dictionary<ItemTier, string> ItemTiers { get; }

    public Dictionary<ItemLevel, string> ItemLevels { get; }

    public Dictionary<int, string> ItemQualities { get; }

    public Dictionary<int, string> ChartDayFilters { get; } = CreateChartDayFilters();

    public ObservableCollection<ISeries> ChartSeries
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = [];

    public Axis[] ChartXAxes
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = [];

    public Axis[] ChartYAxes
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = [];

    public GridLength GridSplitterPosition
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = new(1, GridUnitType.Star);

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value ?? string.Empty;
            RefreshItemsView();
            OnPropertyChanged();
        }
    }

    public CategoryDropdownItem SelectedItemShopCategory
    {
        get => _selectedItemShopCategory;
        set
        {
            _selectedItemShopCategory = value;
            ItemSubCategories1 = ToCategoryDropdownItems(ItemController.GetSubCategories1(value?.Id));
            SelectedItemShopSubCategory1 = null;
            RefreshItemsView();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ItemSubCategories1));
        }
    }

    public CategoryDropdownItem SelectedItemShopSubCategory1
    {
        get => _selectedItemShopSubCategory1;
        set
        {
            _selectedItemShopSubCategory1 = value;
            ItemSubCategories2 = ToCategoryDropdownItems(ItemController.GetSubCategories2(SelectedItemShopCategory?.Id, value?.Id));
            SelectedItemShopSubCategory2 = null;
            RefreshItemsView();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ItemSubCategories2));
        }
    }

    public CategoryDropdownItem SelectedItemShopSubCategory2
    {
        get => _selectedItemShopSubCategory2;
        set
        {
            _selectedItemShopSubCategory2 = value;
            ItemSubCategories3 = ToCategoryDropdownItems(ItemController.GetSubCategories3(SelectedItemShopCategory?.Id, SelectedItemShopSubCategory1?.Id, value?.Id));
            SelectedItemShopSubCategory3 = null;
            RefreshItemsView();
            OnPropertyChanged();
            OnPropertyChanged(nameof(ItemSubCategories3));
        }
    }

    public CategoryDropdownItem SelectedItemShopSubCategory3
    {
        get => _selectedItemShopSubCategory3;
        set
        {
            _selectedItemShopSubCategory3 = value;
            RefreshItemsView();
            OnPropertyChanged();
        }
    }

    public ItemTier SelectedItemTier
    {
        get => _selectedItemTier;
        set
        {
            _selectedItemTier = value;
            RefreshItemsView();
            OnPropertyChanged();
        }
    }

    public ItemLevel SelectedItemLevel
    {
        get => _selectedItemLevel;
        set
        {
            _selectedItemLevel = value;
            RefreshItemsView();
            OnPropertyChanged();
        }
    }

    public int SelectedQualityLevel
    {
        get => _selectedQualityLevel;
        set
        {
            _selectedQualityLevel = value;
            RefreshItemsView();
            OnPropertyChanged();
        }
    }

    public int SelectedChartDays
    {
        get => _selectedChartDays;
        set
        {
            var nextValue = ChartDayFilters.ContainsKey(value) ? value : 30;
            if (_selectedChartDays == nextValue)
            {
                return;
            }

            _selectedChartDays = nextValue;
            RefreshSelectedItemDetails();
            OnPropertyChanged();
        }
    }

    public BlackMarketItemRow SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value)
            {
                return;
            }

            _selectedItem = value;
            RefreshSelectedItemDetails();
            OnPropertyChanged();
        }
    }

    public string SelectedItemTitle
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = string.Empty;

    public double SelectedItemAverageItemsPerDay
    {
        get => _selectedItemAverageItemsPerDay;
        private set
        {
            _selectedItemAverageItemsPerDay = value;
            OnPropertyChanged();
        }
    }

    public string MarketStatusText
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }
    = string.Empty;

    public static string TranslationAverageBoughtPrice => "Average bought price";
    public static string TranslationAverageItemsPerDay => "Average items per day";
    public static string TranslationBlackMarketLocationHint => "Use the Black Market in Caerleon to capture these values.";
    public static string TranslationBoughtLast30Days => "Bought last 30 days";
    public static string TranslationBoughtLast365Days => "Bought last 365 days";
    public static string TranslationChartRange => "Chart range";
    public static string TranslationCurrentBuyPrice => "Current buy price";
    public static string TranslationQuality => LocalizationController.Translation("QUALITY");

    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedItemShopCategory = null;
        SelectedItemShopSubCategory1 = null;
        SelectedItemShopSubCategory2 = null;
        SelectedItemShopSubCategory3 = null;
        SelectedItemTier = ItemTier.Unknown;
        SelectedItemLevel = ItemLevel.Unknown;
        SelectedQualityLevel = -1;
    }

    public void CacheAverageStatsRequest(BlackMarketAverageStatsRequestContext context)
    {
        if (context?.RequestId <= 0 || string.IsNullOrWhiteSpace(context.ItemUniqueName))
        {
            return;
        }

        if (GetGameHistoryWindowDays(context.TimeRange) <= 0)
        {
            return;
        }

        _requestContexts[context.RequestId] = context;
        UpdateMarketStatus();
    }

    public void RecordCurrentBuyOrders(IEnumerable<AuctionEntry> auctionOrders, MarketLocation marketLocation)
    {
        if (marketLocation != MarketLocation.BlackMarket || auctionOrders == null)
        {
            return;
        }

        var changedEntries = new List<BlackMarketHistoryEntry>();
        var buyPrices = auctionOrders
            .Where(x => !string.IsNullOrWhiteSpace(x?.ItemTypeId) && x.QualityLevel > 0)
            .GroupBy(x => (x.ItemTypeId, x.QualityLevel))
            .Select(x => new
            {
                Key = x.Key,
                BuyPrice = x.Max(order => (ulong) FixPoint.FromInternalValue(order.UnitPriceSilver).IntegerValue)
            })
            .Where(x => x.BuyPrice > 0)
            .ToList();

        lock (_historyLock)
        {
            foreach (var buyPrice in buyPrices)
            {
                var key = buyPrice.Key;
                if (!_history.TryGetValue(key, out var entry))
                {
                    entry = CreateHistoryEntry(key.ItemTypeId, ItemController.GetItemByUniqueName(key.ItemTypeId)?.Index ?? 0, key.QualityLevel);
                    _history[key] = entry;
                }

                entry.CurrentBuyPrice = buyPrice.BuyPrice;
                entry.CurrentBuyPriceDateUtc = DateTime.UtcNow;
                entry.LastUpdatedUtc = DateTime.UtcNow;
                changedEntries.Add(entry);
            }
        }

        if (changedEntries.Count <= 0)
        {
            return;
        }

        _ = Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var entry in changedEntries)
            {
                RefreshRow(entry);
            }

            RefreshSelectedItemDetails();
        });
    }

    public async Task RecordAverageStatsResponseAsync(int requestId, IReadOnlyList<BlackMarketHistoryPoint> points)
    {
        if (!_requestContexts.TryRemove(requestId, out var context))
        {
            return;
        }

        if (context.MarketLocation != MarketLocation.BlackMarket)
        {
            return;
        }

        var windowDays = GetGameHistoryWindowDays(context.TimeRange);
        if (windowDays <= 0)
        {
            return;
        }

        BlackMarketHistoryEntry entry;
        lock (_historyLock)
        {
            var key = (context.ItemUniqueName, context.QualityLevel);
            if (!_history.TryGetValue(key, out entry))
            {
                entry = CreateHistoryEntry(context.ItemUniqueName, context.ItemIndex, context.QualityLevel);
                _history[key] = entry;
            }

            ReplaceGameHistoryWindow(entry, points ?? [], windowDays);
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            RefreshRow(entry);
            RefreshSelectedItemDetails();
        });
    }

    public async Task LoadAsync()
    {
        var entries = await FileController.LoadAsync<List<BlackMarketHistoryEntry>>(AppDataPaths.UserDataFile(HistoryFileName)).ConfigureAwait(true);
        lock (_historyLock)
        {
            _history.Clear();
            foreach (var entry in entries ?? [])
            {
                if (string.IsNullOrWhiteSpace(entry.ItemUniqueName))
                {
                    continue;
                }

                TrimHistory(entry);
                NormalizeHistoryPrices(entry);
                _history[(entry.ItemUniqueName, entry.QualityLevel)] = entry;
            }
        }

        await Application.Current.Dispatcher.InvokeAsync(RefreshRowsFromHistory);
    }

    public async Task SaveInFileAsync()
    {
        List<BlackMarketHistoryEntry> entries;
        lock (_historyLock)
        {
            entries = _history.Values
                .Where(x => IsBlackMarketSellableItem(ItemController.GetItemByUniqueName(x.ItemUniqueName)))
                .Select(CloneAndTrim)
                .OrderBy(x => x.ItemUniqueName, StringComparer.Ordinal)
                .ThenBy(x => x.QualityLevel)
                .ToList();
        }

        await FileController.SaveAsync(entries, AppDataPaths.UserDataFile(HistoryFileName)).ConfigureAwait(false);
    }

    private void BuildRows()
    {
        Items.Clear();
        _rowsByKey.Clear();

        foreach (var item in ItemController.Items.Where(IsBlackMarketSellableItem).OrderBy(x => x.LocalizedName))
        {
            for (var qualityLevel = 1; qualityLevel <= 5; qualityLevel++)
            {
                var key = (item.UniqueName, qualityLevel);
                var row = new BlackMarketItemRow(item, qualityLevel, GetHistoryEntry(key));
                Items.Add(row);
                _rowsByKey[key] = row;
            }
        }

        ItemCounterText = $"{Items.Count:N0}";
    }

    private static bool IsBlackMarketSellableItem(Item item)
    {
        return item?.FullItemInformation switch
        {
            Weapon weapon => weapon.SlotTypeEnum == SlotType.MainHand && weapon.CanHarvest == null,
            TransformationWeapon transformationWeapon => transformationWeapon.SlotTypeEnum == SlotType.MainHand,
            EquipmentItem equipmentItem => equipmentItem.SlotTypeEnum is SlotType.OffHand or SlotType.Armor or SlotType.Head or SlotType.Shoes or SlotType.Cape or SlotType.Bag,
            _ => false
        };
    }

    public string ItemCounterText
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    private void LoadCategoriesToDropdown()
    {
        var categories = ItemController.GetRootCategories()
            .OrderBy(x => x.Value, StringComparer.Ordinal)
            .Select(x => new CategoryDropdownItem
            {
                Id = x.Id,
                Value = x.Value,
                DisplayName = LocalizationController.Translation("@MARKETPLACEGUI_ROLLOUT_SHOPCATEGORY_" + x.Id.ToUpperInvariant())
            });

        ItemShopCategories.Clear();
        foreach (var item in categories)
        {
            ItemShopCategories.Add(item);
        }
    }

    private bool IsItemVisible(object value)
    {
        if (value is not BlackMarketItemRow row)
        {
            return false;
        }

        var item = row.Item;
        if (item == null)
        {
            return false;
        }

        var search = SearchText.Trim();
        var nameMatch = string.IsNullOrWhiteSpace(search)
                        || (item.LocalizedNameAndEnglish?.IndexOf(search, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                        || (item.UniqueName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        var categoryMatch = SelectedItemShopCategory == null || string.IsNullOrWhiteSpace(SelectedItemShopCategory.Id) || item.FullItemInformation?.ShopCategory == SelectedItemShopCategory.Id;
        var subCategory1Match = SelectedItemShopSubCategory1 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory1.Id) || item.FullItemInformation?.ShopSubCategory1 == SelectedItemShopSubCategory1.Id;
        var subCategory2Match = SelectedItemShopSubCategory2 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory2.Id) || item.FullItemInformation?.ShopSubCategory2 == SelectedItemShopSubCategory2.Id;
        var subCategory3Match = SelectedItemShopSubCategory3 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory3.Id) || item.FullItemInformation?.ShopSubCategory3 == SelectedItemShopSubCategory3.Id;
        var tierMatch = SelectedItemTier == ItemTier.Unknown || (ItemTier) item.Tier == SelectedItemTier;
        var levelMatch = SelectedItemLevel == ItemLevel.Unknown || (ItemLevel) item.Level == SelectedItemLevel;
        var qualityMatch = SelectedQualityLevel <= 0 || row.QualityLevel == SelectedQualityLevel;

        return nameMatch && categoryMatch && subCategory1Match && subCategory2Match && subCategory3Match && tierMatch && levelMatch && qualityMatch;
    }

    private void RefreshItemsView()
    {
        ItemsView?.Refresh();
        ItemCounterText = $"{((ListCollectionView) ItemsView)?.Count ?? 0:N0}/{Items.Count:N0}";
    }

    private void RefreshSelectedItemDetails()
    {
        UpdateMarketStatus();

        if (SelectedItem == null)
        {
            SelectedItemTitle = string.Empty;
            SelectedItemAverageItemsPerDay = 0;
            ResetChart();
            return;
        }

        SelectedItemTitle = $"{SelectedItem.Item?.LocalizedName} | {SelectedItem.TierLevelString} | {SelectedItem.QualityName}";
        RefreshChart(SelectedItem);
    }

    private void RefreshChart(BlackMarketItemRow row)
    {
        var entry = GetHistoryEntry((row.ItemUniqueName, row.QualityLevel));
        var chartDays = Math.Min(HistoryRetentionDays, Math.Max(1, SelectedChartDays));
        var fromDate = DateTime.UtcNow.Date.AddDays(-(chartDays - 1));
        EnsureChartFrame(chartDays, fromDate);

        var itemCounts = new int[chartDays];
        var priceTotals = new decimal[chartDays];
        var priceWeights = new int[chartDays];
        var totalItemCount = 0;

        foreach (var point in entry?.Points ?? [])
        {
            var index = (int) (point.Date.Date - fromDate).TotalDays;
            if (index < 0 || index >= chartDays)
            {
                continue;
            }

            var itemCount = Math.Max(0, point.ItemCount);
            itemCounts[index] += itemCount;
            totalItemCount += itemCount;

            if (point.AveragePrice > 0)
            {
                var weight = Math.Max(1, itemCount);
                priceTotals[index] += (decimal) point.AveragePrice * weight;
                priceWeights[index] += weight;
            }
        }

        var pricePoints = new List<ObservablePoint>(chartDays);
        for (var index = 0; index < chartDays; index++)
        {
            if (priceWeights[index] <= 0)
            {
                continue;
            }

            pricePoints.Add(new ObservablePoint(index, (double) (priceTotals[index] / priceWeights[index])));
        }

        SelectedItemAverageItemsPerDay = chartDays > 0 ? (double) totalItemCount / chartDays : 0;

        ChartSeries =
        [
            new ColumnSeries<int>
            {
                Name = "Items",
                Values = itemCounts,
                Fill = new SolidColorPaint(new SKColor(149, 35, 63, 160)),
                Stroke = null,
                ScalesYAt = 0
            },
            new LineSeries<ObservablePoint>
            {
                Name = "Silver",
                Values = pricePoints,
                Fill = null,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(new SKColor(208, 208, 208, 255), 2),
                GeometryStroke = new SolidColorPaint(new SKColor(208, 208, 208, 255), 2),
                GeometryFill = new SolidColorPaint(new SKColor(120, 120, 120, 255)),
                ScalesYAt = 1
            }
        ];

        EnsureChartYAxes();
    }

    private void EnsureChartFrame(int chartDays, DateTime fromDate)
    {
        if (_chartFrameDays == chartDays && _chartFrameFromDate == fromDate)
        {
            return;
        }

        _chartFrameDays = chartDays;
        _chartFrameFromDate = fromDate;
        _chartLabels = Enumerable.Range(0, chartDays)
            .Select(offset => fromDate.AddDays(offset).ToString("dd.MM", CultureInfo.CurrentCulture))
            .ToArray();

        ChartXAxes =
        [
            new Axis
            {
                Labels = _chartLabels,
                LabelsRotation = 45,
                MinStep = 1,
                ForceStepToMin = true,
                Labeler = value => GetChartLabel(value)
            }
        ];
    }

    private void EnsureChartYAxes()
    {
        if (ChartYAxes.Length == 2 && string.Equals(ChartYAxes[0].Name, "Items", StringComparison.Ordinal))
        {
            return;
        }

        ChartYAxes =
        [
            new Axis
            {
                Name = "Items",
                MinLimit = 0
            },
            new Axis
            {
                Name = "Silver",
                Position = LiveChartsCore.Measure.AxisPosition.End,
                MinLimit = 0,
                Labeler = value => value.ToString("N0", CultureInfo.CurrentCulture)
            }
        ];
    }

    private string GetChartLabel(double value)
    {
        var index = (int) Math.Round(value);
        if (index < 0 || index >= _chartLabels.Length)
        {
            return string.Empty;
        }

        var step = Math.Max(1, _chartLabels.Length / 12);
        return index % step == 0 || index == _chartLabels.Length - 1 ? _chartLabels[index] : string.Empty;
    }

    private void ResetChart()
    {
        _chartFrameDays = 0;
        _chartFrameFromDate = DateTime.MinValue;
        _chartLabels = [];
        ChartSeries = [];
        ChartXAxes = [new Axis()];
        ChartYAxes =
        [
            new Axis
            {
                MinLimit = 0
            }
        ];
    }

    private void RefreshRowsFromHistory()
    {
        foreach (var entry in _history.Values)
        {
            RefreshRow(entry);
        }

        RefreshSelectedItemDetails();
    }

    private void RefreshRow(BlackMarketHistoryEntry entry)
    {
        var key = (entry.ItemUniqueName, entry.QualityLevel);
        if (_rowsByKey.TryGetValue(key, out var row))
        {
            row.Refresh(entry);
        }
    }

    private BlackMarketHistoryEntry GetHistoryEntry((string ItemUniqueName, int QualityLevel) key)
    {
        lock (_historyLock)
        {
            return _history.TryGetValue(key, out var entry) ? entry : null;
        }
    }

    private static BlackMarketHistoryEntry CreateHistoryEntry(string itemUniqueName, int itemIndex, int qualityLevel)
    {
        return new BlackMarketHistoryEntry
        {
            ItemUniqueName = itemUniqueName,
            ItemIndex = itemIndex,
            Tier = ItemController.GetItemTier(itemUniqueName),
            EnchantmentLevel = ItemController.GetItemLevel(itemUniqueName),
            QualityLevel = qualityLevel
        };
    }

    private static void ReplaceGameHistoryWindow(BlackMarketHistoryEntry entry, IReadOnlyList<BlackMarketHistoryPoint> points, int windowDays)
    {
        var now = DateTime.UtcNow;
        var fromDate = now.Date.AddDays(-(windowDays - 1));
        var referencePrice = GetReferencePrice(entry);
        var normalizedPoints = points
            .Where(x => x.Date.Date >= fromDate)
            .GroupBy(x => x.Date.Date)
            .Select(x => CreateDailyHistoryPoint(x.Key, x, referencePrice, now))
            .ToList();

        entry.Points = entry.Points
            .Where(x => x.Date.Date < fromDate)
            .Concat(normalizedPoints)
            .OrderBy(x => x.Date)
            .ToList();

        entry.LastUpdatedUtc = now;
        TrimHistory(entry);
        NormalizeHistoryPrices(entry);
    }

    private static int GetGameHistoryWindowDays(int timeRange)
    {
        return timeRange switch
        {
            SevenDaysTimeRange => SevenDaysWindowDays,
            FourWeeksTimeRange => FourWeeksWindowDays,
            _ => 0
        };
    }

    private static void NormalizeHistoryPrices(BlackMarketHistoryEntry entry)
    {
        var referencePrice = GetReferencePrice(entry);
        foreach (var point in entry.Points)
        {
            point.AveragePrice = NormalizeAveragePrice(point.ItemCount, point.AveragePrice, referencePrice);
        }
    }

    private static BlackMarketHistoryPoint CreateDailyHistoryPoint(
        DateTime date,
        IEnumerable<BlackMarketHistoryPoint> points,
        long referencePrice,
        DateTime lastUpdatedUtc)
    {
        var normalizedPoints = points
            .Select(x => new
            {
                ItemCount = Math.Max(0, x.ItemCount),
                AveragePrice = NormalizeAveragePrice(x.ItemCount, x.AveragePrice, referencePrice)
            })
            .ToList();
        var itemCount = normalizedPoints.Sum(x => x.ItemCount);
        var averagePrice = CalculateWeightedAveragePrice(normalizedPoints.Select(x => (x.ItemCount, x.AveragePrice)));

        return new BlackMarketHistoryPoint
        {
            Date = date.Date,
            ItemCount = itemCount,
            AveragePrice = averagePrice,
            LastUpdatedUtc = lastUpdatedUtc
        };
    }

    private static long CalculateWeightedAveragePrice(IEnumerable<(int ItemCount, long AveragePrice)> points)
    {
        decimal totalPrice = 0;
        var totalItems = 0;

        foreach (var point in points)
        {
            if (point.ItemCount <= 0 || point.AveragePrice <= 0)
            {
                continue;
            }

            totalItems += point.ItemCount;
            totalPrice += (decimal) point.AveragePrice * point.ItemCount;
        }

        return totalItems > 0 ? (long) Math.Round(totalPrice / totalItems) : 0;
    }

    private static long NormalizeAveragePrice(int itemCount, long averagePrice, long referencePrice)
    {
        var price = Math.Max(0, averagePrice);
        if (itemCount <= 1 || price <= 0)
        {
            return price;
        }

        if (referencePrice <= 0)
        {
            return price;
        }

        var dividedPrice = price / itemCount;
        var multipliedPrice = price * itemCount;
        if (dividedPrice <= 0 || multipliedPrice <= 0)
        {
            return price;
        }

        var priceValue = (double) price;
        var dividedPriceValue = (double) dividedPrice;
        var multipliedPriceValue = (double) multipliedPrice;
        var referencePriceValue = (double) referencePrice;
        var isLikelyDividedPrice = priceValue < referencePriceValue * 0.4
                                   && multipliedPriceValue >= referencePriceValue * 0.5
                                   && multipliedPriceValue <= referencePriceValue * 3;
        if (isLikelyDividedPrice)
        {
            return multipliedPrice;
        }

        var totalPriceThreshold = referencePriceValue * Math.Max(6, itemCount * 0.75);
        var isLikelyTotalPrice = itemCount >= 4
                                 && priceValue > totalPriceThreshold
                                 && dividedPriceValue >= referencePriceValue * 0.35
                                 && dividedPriceValue <= referencePriceValue * 3;

        return isLikelyTotalPrice ? dividedPrice : price;
    }

    private static long GetReferencePrice(BlackMarketHistoryEntry entry)
    {
        var item = ItemController.GetItemByUniqueName(entry.ItemUniqueName);
        if (item?.EstimatedMarketValues != null && item.EstimatedMarketValues.Count > 0)
        {
            try
            {
                var estimatedPrice = Math.Max(0, item.AverageEstMarketValue);
                if (estimatedPrice > 0)
                {
                    return estimatedPrice;
                }
            }
            catch
            {
            }
        }

        if (entry.CurrentBuyPrice is > 0 and <= long.MaxValue)
        {
            return (long) entry.CurrentBuyPrice;
        }

        var historyReferencePrice = GetHistoryReferencePrice(entry);
        if (historyReferencePrice > 0)
        {
            return historyReferencePrice;
        }

        return 0;
    }

    private static long GetHistoryReferencePrice(BlackMarketHistoryEntry entry)
    {
        var prices = entry.Points
            .Where(x => x.AveragePrice > 0)
            .Select(x => x.AveragePrice)
            .Where(x => x > 0)
            .OrderBy(x => x)
            .ToList();

        if (prices.Count <= 0)
        {
            return 0;
        }

        return prices[prices.Count / 2];
    }

    private static BlackMarketHistoryEntry CloneAndTrim(BlackMarketHistoryEntry source)
    {
        var clone = new BlackMarketHistoryEntry
        {
            ItemUniqueName = source.ItemUniqueName,
            ItemIndex = source.ItemIndex,
            Tier = source.Tier,
            EnchantmentLevel = source.EnchantmentLevel,
            QualityLevel = source.QualityLevel,
            CurrentBuyPrice = source.CurrentBuyPrice,
            CurrentBuyPriceDateUtc = source.CurrentBuyPriceDateUtc,
            LastUpdatedUtc = source.LastUpdatedUtc,
            Points = source.Points
                .Select(x => new BlackMarketHistoryPoint
                {
                    Date = x.Date,
                    ItemCount = x.ItemCount,
                    AveragePrice = x.AveragePrice,
                    LastUpdatedUtc = x.LastUpdatedUtc
                })
                .ToList()
        };

        TrimHistory(clone);
        return clone;
    }

    private static void TrimHistory(BlackMarketHistoryEntry entry)
    {
        var minDate = DateTime.UtcNow.Date.AddDays(-(HistoryRetentionDays - 1));
        entry.Points = entry.Points
            .Where(x => x.Date.Date >= minDate)
            .GroupBy(x => x.Date.Date)
            .Select(x => x.OrderByDescending(p => p.LastUpdatedUtc).First())
            .OrderBy(x => x.Date)
            .ToList();
    }

    private void UpdateMarketStatus()
    {
        var location = ClusterController.CurrentCluster.SourceClusterIndex?.GetMarketLocationByLocationNameOrId()
                       ?? ClusterController.CurrentCluster.Index?.GetMarketLocationByLocationNameOrId()
                       ?? MarketLocation.Unknown;

        MarketStatusText = location == MarketLocation.BlackMarket
            ? "Black Market in Caerleon"
            : TranslationBlackMarketLocationHint;
    }

    private static Dictionary<int, string> CreateQualityFilters()
    {
        return new Dictionary<int, string>
        {
            { -1, string.Empty },
            { 1, LocalizationController.Translation("NORMAL") },
            { 2, LocalizationController.Translation("GOOD") },
            { 3, LocalizationController.Translation("OUTSTANDING") },
            { 4, LocalizationController.Translation("EXCELLENT") },
            { 5, LocalizationController.Translation("MASTERPIECE") }
        };
    }

    private static Dictionary<int, string> CreateChartDayFilters()
    {
        return new Dictionary<int, string>
        {
            { 30, "30 Tage" },
            { 90, "90 Tage" },
            { 180, "180 Tage" },
            { 365, "365 Tage" }
        };
    }

    private static ObservableCollection<CategoryDropdownItem> ToCategoryDropdownItems(IEnumerable<(string Id, string Value)> source)
    {
        if (source == null)
        {
            return [];
        }

        return new ObservableCollection<CategoryDropdownItem>(
            source.Select(x =>
            {
                var id = x.Id ?? string.Empty;
                var translationKey = string.IsNullOrWhiteSpace(id) ? "UNKNOWN" : id.ToUpperInvariant();
                return new CategoryDropdownItem
                {
                    Id = id,
                    Value = x.Value ?? string.Empty,
                    DisplayName = LocalizationController.Translation("@MARKETPLACEGUI_ROLLOUT_SHOPSUBCATEGORY_" + translationKey) ?? translationKey
                };
            })
        );
    }
}
