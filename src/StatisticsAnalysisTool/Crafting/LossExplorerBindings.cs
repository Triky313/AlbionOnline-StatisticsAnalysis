using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerBindings : BaseViewModel, IDisposable
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(10);
    private const string CraftingShopCategory = "crafting";
    private const string ArtefactShopCategory = "artefacts";
    private static readonly HashSet<string> CraftingResourceSubCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "resources",
        "refinedresources",
        "cityresources",
        "fish",
        "alchemy"
    };
    private readonly LossExplorerService _service = new();
    private readonly SemaphoreSlim _loadSemaphore = new(1, 1);
    private CancellationTokenSource _monitorCancellationTokenSource;
    private Task _monitorTask;
    private bool _isDisposed;
    private List<LossExplorerItemRow> _allEquipmentItems = [];
    private List<LossExplorerItemRow> _allInventoryItems = [];
    private List<LossExplorerItemRow> _allCraftingItems = [];
    private LossExplorerCache _cache = new();
    private string _cacheFilePath = string.Empty;
    private string _searchText = string.Empty;
    private CategoryDropdownItem _selectedItemShopCategory;
    private ItemTier _selectedItemTier = ItemTier.Unknown;
    private ItemLevel _selectedItemLevel = ItemLevel.Unknown;
    private ItemQuality _selectedItemQuality = ItemQuality.Unknown;
    private bool _isBusy;
    private string _statusText = string.Empty;

    public LossExplorerBindings()
    {
        ImageController.ItemImageStored += OnItemImageStored;
        LoadFilterOptions();
        StatusText = TranslationLoadHint;
    }

    public ObservableRangeCollection<LossExplorerItemRow> EquipmentItems { get; } = [];

    public ObservableRangeCollection<LossExplorerItemRow> InventoryItems { get; } = [];

    public ObservableRangeCollection<LossExplorerItemRow> CraftingItems { get; } = [];

    public ObservableCollection<CategoryDropdownItem> ItemShopCategories { get; } = [];

    public IReadOnlyDictionary<ItemTier, string> ItemTiers { get; private set; }

    public IReadOnlyDictionary<ItemLevel, string> ItemLevels { get; private set; }

    public IReadOnlyDictionary<ItemQuality, string> ItemQualities { get; private set; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value ?? string.Empty;
            RefreshFilters();
            OnPropertyChanged();
        }
    }

    public CategoryDropdownItem SelectedItemShopCategory
    {
        get => _selectedItemShopCategory;
        set
        {
            _selectedItemShopCategory = value;
            RefreshFilters();
            OnPropertyChanged();
        }
    }

    public ItemTier SelectedItemTier
    {
        get => _selectedItemTier;
        set
        {
            _selectedItemTier = value;
            RefreshFilters();
            OnPropertyChanged();
        }
    }

    public ItemLevel SelectedItemLevel
    {
        get => _selectedItemLevel;
        set
        {
            _selectedItemLevel = value;
            RefreshFilters();
            OnPropertyChanged();
        }
    }

    public ItemQuality SelectedItemQuality
    {
        get => _selectedItemQuality;
        set
        {
            _selectedItemQuality = value;
            RefreshFilters();
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public long StoredEventCount => _cache?.DailyEventCounts?.Sum(x => x.EventCount) ?? 0;

    public string StoredEventCountText => string.Format(CultureInfo.CurrentCulture, TranslationStoredEvents, StoredEventCount);

    public bool HasEquipmentItems => EquipmentItems.Count > 0;

    public bool HasInventoryItems => InventoryItems.Count > 0;

    public bool HasCraftingItems => CraftingItems.Count > 0;

    public static string TranslationLossExplorer => LocalizationController.Translation("LOSS_EXPLORER");

    public static string TranslationEquipment => LocalizationController.Translation("EQUIPMENT");

    public static string TranslationInventory => LocalizationController.Translation("INVENTORY");

    public static string TranslationCrafting => LocalizationController.Translation("CRAFTING");

    public static string TranslationQuality => LocalizationController.Translation("QUALITY");

    public static string TranslationNoData => LocalizationController.Translation("NO_DATA");

    private static string TranslationLoadingEvents => LocalizationController.Translation("LOSS_EXPLORER_LOADING_EVENTS");

    private static string TranslationLoadingPrices => LocalizationController.Translation("LOSS_EXPLORER_LOADING_PRICES");

    private static string TranslationReady => LocalizationController.Translation("LOSS_EXPLORER_READY");

    private static string TranslationStoredEvents => LocalizationController.Translation("LOSS_EXPLORER_STORED_EVENTS");

    private static string TranslationLoadHint => LocalizationController.Translation("LOSS_EXPLORER_LOAD_HINT");

    private static string TranslationLoadError => LocalizationController.Translation("LOSS_EXPLORER_LOAD_ERROR");

    private static string TranslationServerUnavailable => LocalizationController.Translation("LOSS_EXPLORER_SERVER_UNAVAILABLE");

    public async Task LoadCachedDataAsync()
    {
        if (!SettingsController.CurrentSettings.LossExplorer)
        {
            return;
        }

        await _loadSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            await LoadCacheForActiveServerAsync().ConfigureAwait(false);
        }
        finally
        {
            _loadSemaphore.Release();
        }

        StartMonitoring();
    }

    public Task LoadAsync()
    {
        return LoadInternalAsync(CancellationToken.None, true);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        ImageController.ItemImageStored -= OnItemImageStored;
        _monitorCancellationTokenSource?.Cancel();
    }

    private void OnItemImageStored(object sender, ItemImageStoredEventArgs eventArgs)
    {
        if (_isDisposed)
        {
            return;
        }

        _ = RunOnUiThreadAsync(() =>
        {
            if (_isDisposed)
            {
                return;
            }

            foreach (var item in _allEquipmentItems
                         .Concat(_allInventoryItems)
                         .Concat(_allCraftingItems)
                         .Distinct()
                         .Where(x => x.QualityLevel == eventArgs.QualityLevel
                                     && string.Equals(x.ItemUniqueName, eventArgs.ItemUniqueName, StringComparison.Ordinal)))
            {
                item.RefreshIcon();
            }
        });
    }

    private async Task LoadInternalAsync(CancellationToken cancellationToken, bool showProgress)
    {
        if (!SettingsController.CurrentSettings.LossExplorer)
        {
            return;
        }

        await _loadSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!AppDataPaths.IsUserDataAvailable)
            {
                if (showProgress)
                {
                    SetStatusText(TranslationServerUnavailable);
                }

                return;
            }

            var cacheFilePath = AppDataPaths.UserDataFile(LossExplorerService.CacheFileName);
            if (!string.Equals(_cacheFilePath, cacheFilePath, StringComparison.OrdinalIgnoreCase))
            {
                await LoadCacheForActiveServerAsync().ConfigureAwait(false);
            }

            if (_cache.LastSuccessfulSyncUtc >= DateTime.UtcNow.Subtract(RefreshInterval)
                && _cache.Items.All(x => x.HasPrice))
            {
                if (showProgress)
                {
                    SetReadyStatus();
                }

                return;
            }

            if (showProgress)
            {
                SetIsBusy(true);
                SetStatusText(string.Format(CultureInfo.CurrentCulture, TranslationLoadingEvents, 0));
            }

            var requestedCachePath = _cacheFilePath;
            var serverLocation = AppDataPaths.ActiveUserDataServerLocation;

            var updatedCache = await _service.UpdateAsync(
                _cache,
                requestedCachePath,
                serverLocation,
                pageCount =>
                {
                    if (showProgress)
                    {
                        SetStatusText(string.Format(CultureInfo.CurrentCulture, TranslationLoadingEvents, pageCount));
                    }
                },
                (batch, total) =>
                {
                    if (showProgress)
                    {
                        SetStatusText(string.Format(CultureInfo.CurrentCulture, TranslationLoadingPrices, batch, total));
                    }
                },
                cancellationToken).ConfigureAwait(false);

            if (string.Equals(requestedCachePath, AppDataPaths.UserDataFile(LossExplorerService.CacheFileName), StringComparison.OrdinalIgnoreCase))
            {
                _cache = updatedCache;
                await RunOnUiThreadAsync(() =>
                {
                    ApplyCacheToUi(updatedCache);
                    StatusText = CreateReadyStatusText();
                }).ConfigureAwait(false);

                if (!showProgress)
                {
                    Log.Information(
                        "Loss Explorer background refresh completed. StoredEvents={StoredEvents}, LastSuccessfulSyncUtc={LastSuccessfulSyncUtc}",
                        StoredEventCount,
                        updatedCache.LastSuccessfulSyncUtc);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Loss Explorer data could not be loaded");
            if (showProgress)
            {
                SetStatusText(TranslationLoadError);
            }
        }
        finally
        {
            if (showProgress)
            {
                SetIsBusy(false);
            }

            _loadSemaphore.Release();
        }
    }

    private void StartMonitoring()
    {
        if (_isDisposed || _monitorTask != null || !SettingsController.CurrentSettings.LossExplorer)
        {
            return;
        }

        _monitorCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _monitorCancellationTokenSource.Token;
        _monitorTask = Task.Run(() => MonitorAsync(cancellationToken), cancellationToken);
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("Loss Explorer background monitoring started. RefreshInterval={RefreshInterval}", RefreshInterval);
            await LoadInternalAsync(cancellationToken, false).ConfigureAwait(false);
            using var timer = new PeriodicTimer(RefreshInterval);
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await LoadInternalAsync(cancellationToken, false).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Loss Explorer background monitoring stopped unexpectedly");
        }
    }

    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedItemShopCategory = ItemShopCategories.FirstOrDefault();
        SelectedItemTier = ItemTier.Unknown;
        SelectedItemLevel = ItemLevel.Unknown;
        SelectedItemQuality = ItemQuality.Unknown;
    }

    public void RefreshLocalization()
    {
        var selectedCategoryId = SelectedItemShopCategory?.Id;
        LoadFilterOptions();
        SelectedItemShopCategory = ItemShopCategories.FirstOrDefault(x => string.Equals(x.Id, selectedCategoryId, StringComparison.OrdinalIgnoreCase))
                                   ?? ItemShopCategories.FirstOrDefault();
        ApplyCacheToUi(_cache);
        SetStatusText(_cache?.Items?.Count > 0
            ? CreateReadyStatusText()
            : TranslationLoadHint);
    }

    private async Task LoadCacheForActiveServerAsync()
    {
        if (!AppDataPaths.IsUserDataAvailable)
        {
            _cache = new LossExplorerCache();
            _cacheFilePath = string.Empty;
            await RunOnUiThreadAsync(() =>
            {
                ApplyCacheToUi(_cache);
                StatusText = TranslationServerUnavailable;
            }).ConfigureAwait(false);
            return;
        }

        var cacheFilePath = AppDataPaths.UserDataFile(LossExplorerService.CacheFileName);
        _cache = await _service.LoadCacheAsync(cacheFilePath).ConfigureAwait(false);
        _cacheFilePath = cacheFilePath;
        await RunOnUiThreadAsync(() =>
        {
            ApplyCacheToUi(_cache);
            StatusText = _cache.Items.Count > 0 ? CreateReadyStatusText() : TranslationLoadHint;
        }).ConfigureAwait(false);
    }

    private void ApplyCacheToUi(LossExplorerCache cache)
    {
        var equipmentItems = new List<LossExplorerItemRow>();
        var inventoryItems = new List<LossExplorerItemRow>();
        var craftingItems = new List<LossExplorerItemRow>();

        foreach (var cachedItem in cache?.Items ?? [])
        {
            if (cachedItem.EquipmentQuantity > 0)
            {
                equipmentItems.Add(new LossExplorerItemRow(cachedItem, cachedItem.EquipmentQuantity));
            }

            if (cachedItem.InventoryQuantity <= 0)
            {
                continue;
            }

            var inventoryItem = new LossExplorerItemRow(cachedItem, cachedItem.InventoryQuantity);
            inventoryItems.Add(inventoryItem);
            if (IsCraftingResourceOrArtefact(inventoryItem))
            {
                craftingItems.Add(new LossExplorerItemRow(cachedItem, cachedItem.InventoryQuantity));
            }
        }

        _allEquipmentItems = SortRows(equipmentItems);
        _allInventoryItems = SortRows(inventoryItems);
        _allCraftingItems = SortRows(craftingItems);
        OnPropertyChanged(nameof(StoredEventCount));
        OnPropertyChanged(nameof(StoredEventCountText));
        RefreshFilters();
    }

    private static List<LossExplorerItemRow> SortRows(IEnumerable<LossExplorerItemRow> rows)
    {
        return rows
            .OrderByDescending(x => x.TotalValue)
            .ThenBy(x => x.ItemName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private void RefreshFilters()
    {
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            _ = Application.Current.Dispatcher.BeginInvoke(RefreshFilters);
            return;
        }

        EquipmentItems.ReplaceRange(SortRows(_allEquipmentItems.Where(MatchesFilters)));
        InventoryItems.ReplaceRange(SortRows(_allInventoryItems.Where(MatchesFilters)));
        CraftingItems.ReplaceRange(SortRows(_allCraftingItems.Where(MatchesFilters)));
        OnPropertyChanged(nameof(HasEquipmentItems));
        OnPropertyChanged(nameof(HasInventoryItems));
        OnPropertyChanged(nameof(HasCraftingItems));
    }

    private bool MatchesFilters(LossExplorerItemRow row)
    {
        if (!string.IsNullOrWhiteSpace(SearchText)
            && !row.ItemName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            && !row.ItemUniqueName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SelectedItemShopCategory?.Id)
            && !string.Equals(row.ShopCategory, SelectedItemShopCategory.Id, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (SelectedItemTier != ItemTier.Unknown && row.Tier != (int) SelectedItemTier)
        {
            return false;
        }

        if (SelectedItemLevel != ItemLevel.Unknown && row.EnchantmentLevel != (int) SelectedItemLevel)
        {
            return false;
        }

        return SelectedItemQuality == ItemQuality.Unknown
               || row.QualityLevel == FrequentlyValues.ItemQualities[SelectedItemQuality];
    }

    private void LoadFilterOptions()
    {
        ItemShopCategories.Clear();
        ItemShopCategories.Add(new CategoryDropdownItem
        {
            Id = string.Empty,
            Value = string.Empty,
            DisplayName = string.Empty
        });

        foreach (var category in ItemController.GetRootCategories()
                     .OrderBy(x => x.Value, StringComparer.Ordinal)
                     .Select(x => new CategoryDropdownItem
                     {
                         Id = x.Id ?? string.Empty,
                         Value = x.Value ?? string.Empty,
                         DisplayName = LocalizationController.Translation("@MARKETPLACEGUI_ROLLOUT_SHOPCATEGORY_" + (x.Id ?? string.Empty).ToUpperInvariant())
                     }))
        {
            ItemShopCategories.Add(category);
        }

        ItemTiers = new Dictionary<ItemTier, string>(FrequentlyValues.ItemTiers);
        ItemLevels = new Dictionary<ItemLevel, string>(FrequentlyValues.ItemLevels)
        {
            [ItemLevel.Level0] = ".0",
            [ItemLevel.Level1] = ".1",
            [ItemLevel.Level2] = ".2",
            [ItemLevel.Level3] = ".3",
            [ItemLevel.Level4] = ".4"
        };
        ItemQualities = new Dictionary<ItemQuality, string>
        {
            [ItemQuality.Unknown] = string.Empty,
            [ItemQuality.Normal] = LocalizationController.Translation("NORMAL"),
            [ItemQuality.Good] = LocalizationController.Translation("GOOD"),
            [ItemQuality.Outstanding] = LocalizationController.Translation("OUTSTANDING"),
            [ItemQuality.Excellent] = LocalizationController.Translation("EXCELLENT"),
            [ItemQuality.Masterpiece] = LocalizationController.Translation("MASTERPIECE")
        };

        OnPropertyChanged(nameof(ItemTiers));
        OnPropertyChanged(nameof(ItemLevels));
        OnPropertyChanged(nameof(ItemQualities));
    }

    private static bool IsCraftingResourceOrArtefact(LossExplorerItemRow row)
    {
        if (string.Equals(row.ShopCategory, ArtefactShopCategory, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(row.ShopCategory, CraftingShopCategory, StringComparison.OrdinalIgnoreCase)
               && CraftingResourceSubCategories.Contains(row.ShopSubCategory1);
    }

    private void SetReadyStatus()
    {
        SetStatusText(CreateReadyStatusText());
    }

    private string CreateReadyStatusText()
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            TranslationReady,
            _cache?.ObservedDays?.Count ?? 0,
            (_cache?.LastSuccessfulSyncUtc > DateTime.MinValue
                ? _cache.LastSuccessfulSyncUtc
                : _cache?.CreatedUtc ?? DateTime.MinValue).ToLocalTime());
    }

    private void SetIsBusy(bool isBusy)
    {
        RunOnUiThread(() => IsBusy = isBusy);
    }

    private void SetStatusText(string statusText)
    {
        RunOnUiThread(() => StatusText = statusText);
    }

    private void RunOnUiThread(Action action)
    {
        _ = RunOnUiThreadAsync(action);
    }
}