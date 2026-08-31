using FontAwesome5;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Win32;
using Serilog;
using StatisticsAnalysisTool.Alert;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Crafting;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Party;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.StorageHistory;
using StatisticsAnalysisTool.Trade;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

// ReSharper disable UnusedMember.Global

namespace StatisticsAnalysisTool.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly ItemRefreshCooldownTracker _itemRefreshCooldownTracker = new();
    private readonly Dictionary<string, ItemDetailsViewModel> _itemDetailsCache = new(StringComparer.Ordinal);

    public AlertController AlertManager;

    public MainWindowViewModel()
    {
        UpgradeSettings();
        RegisterServerDetectionEvents();
        SetUiElements();
        InitDashboardChart();
        Translation = new MainWindowTranslation();
    }

    private void InitDashboardChart()
    {
        RefreshDashboardChartTranslations();
    }

    public void RefreshLocalization()
    {
        Translation = new MainWindowTranslation();
        UpdateServerStatus();
        RefreshDashboardChartTranslations();
        RefreshItemCategoryTranslations();
        DungeonBindings.RefreshLocalization();
        TradeMonitoringBindings.RefreshLocalization();
        DashboardBindings.Mobs.RefreshLocalization();
        CraftingBindings.RefreshLossExplorerLocalization();
        RefreshTrackingActivityText();
        MainStatusBindings.RefreshLocalization();
        DamageMeterBindings.RefreshContentFilters();
    }

    private void RefreshDashboardChartTranslations()
    {
        var selectedDashboardChartRange = SelectedDashboardChartRange;
        var selectedDashboardChartSeriesFilters = DashboardChartSeriesFilters.ToDictionary(x => x.ValueType, x => x.IsSelected);
        var settings = SettingsController.CurrentSettings;

        DashboardChartRanges = new ObservableCollection<DashboardChartRangeOption>(DashboardChartRangeOption.CreateDefault());
        var selectedBucketCount = selectedDashboardChartRange?.BucketCount ?? settings.SelectedDashboardChartRangeBucketCount;
        var selectedRangeUnit = selectedDashboardChartRange?.Unit ?? settings.SelectedDashboardChartRangeUnit;
        SelectedDashboardChartRange = DashboardChartRanges.FirstOrDefault(x => x.BucketCount == selectedBucketCount && x.Unit == selectedRangeUnit)
                                      ?? DashboardChartRanges.FirstOrDefault();

        DashboardChartSeriesFilters = new ObservableCollection<DashboardChartSeriesFilter>(CreateDashboardChartSeriesFilters(selectedDashboardChartSeriesFilters));
        RefreshDashboardMetadataFilters();
    }

    private static IEnumerable<DashboardChartSeriesFilter> CreateDashboardChartSeriesFilters(IReadOnlyDictionary<ValueType, bool> selectedFilters)
    {
        return
        [
            CreateDashboardChartSeriesFilter(ValueType.Fame, DashboardBindings.TranslationFame, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.Silver, DashboardBindings.TranslationSilver, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.ReSpec, DashboardBindings.TranslationReSpec, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.FactionStanding, DashboardBindings.TranslationFactionStanding, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.FactionPoints, DashboardBindings.TranslationFactionPoints, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.Might, DashboardBindings.TranslationMight, selectedFilters),
            CreateDashboardChartSeriesFilter(ValueType.Favor, DashboardBindings.TranslationFavor, selectedFilters)
        ];
    }

    private static DashboardChartSeriesFilter CreateDashboardChartSeriesFilter(ValueType valueType, string name, IReadOnlyDictionary<ValueType, bool> selectedFilters)
    {
        return new DashboardChartSeriesFilter()
        {
            ValueType = valueType,
            Name = name,
            Brush = DashboardChartSeriesFilter.GetBrush(valueType),
            IsSelected = !selectedFilters.TryGetValue(valueType, out var isSelected) || isSelected
        };
    }

    private static DashboardContentFilterOption CreateDashboardContentFilterOption(DashboardContentType contentType)
    {
        return new DashboardContentFilterOption(
            contentType,
            LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(contentType)));
    }

    private void RefreshDashboardMetadataFilters()
    {
        var selectedContentType = SelectedDashboardContentFilter is null
            ? SettingsController.CurrentSettings.SelectedDashboardContentType
            : SelectedDashboardContentFilter.ContentType;

        DashboardContentFilters = new ObservableCollection<DashboardContentFilterOption>
        {
            new(null, LocalizationController.Translation("ALL_CONTENT_TYPES"))
        };

        foreach (var contentType in DashboardContentTypeResolver.ContentTypes)
        {
            DashboardContentFilters.Add(CreateDashboardContentFilterOption(contentType));
        }
        SelectedDashboardContentFilter = DashboardContentFilters
            .FirstOrDefault(x => x.ContentType == selectedContentType)
            ?? DashboardContentFilters[0];

        if (DashboardSessionFilters.Count == 0)
        {
            DashboardSessionFilters =
            [
                new DashboardSessionFilterOption(null, LocalizationController.Translation("ALL_SESSIONS"))
            ];
            SelectedDashboardSessionFilter = DashboardSessionFilters[0];
        }
    }

    private void RefreshItemCategoryTranslations()
    {
        var selectedCategoryId = SelectedItemShopCategory?.Id;
        var selectedSubCategory1Id = SelectedItemShopSubCategory1?.Id;
        var selectedSubCategory2Id = SelectedItemShopSubCategory2?.Id;
        var selectedSubCategory3Id = SelectedItemShopSubCategory3?.Id;

        LoadCategoriesToDropdown();

        SelectedItemShopCategory = FindCategoryDropdownItem(ItemShopCategories, selectedCategoryId);
        SelectedItemShopSubCategory1 = FindCategoryDropdownItem(ItemSubCategories1, selectedSubCategory1Id);
        SelectedItemShopSubCategory2 = FindCategoryDropdownItem(ItemSubCategories2, selectedSubCategory2Id);
        SelectedItemShopSubCategory3 = FindCategoryDropdownItem(ItemSubCategories3, selectedSubCategory3Id);
    }

    private static CategoryDropdownItem FindCategoryDropdownItem(IEnumerable<CategoryDropdownItem> items, string id)
    {
        return string.IsNullOrWhiteSpace(id)
            ? null
            : items.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshTrackingActivityText()
    {
        TrackingActivityBindings.TrackingActiveText = TrackingActivityBindings.TrackingActivityType switch
        {
            TrackingIconType.Partially => MainWindowTranslation.TrackingIsPartiallyActive,
            TrackingIconType.On => MainWindowTranslation.TrackingIsActive,
            TrackingIconType.Off => MainWindowTranslation.TrackingIsNotActive,
            _ => TrackingActivityBindings.TrackingActiveText
        };
    }

    public void SetUiElements()
    {
        // Error bar
        ErrorBarVisibility = Visibility.Collapsed;

        // Unsupported OS
        UnsupportedOsVisibility = Environment.OSVersion.Version.Major < 10 ? Visibility.Visible : Visibility.Collapsed;

        // Item search
        LoadCategoriesToDropdown();

        ItemTiers = FrequentlyValues.ItemTiers;
        SelectedItemTier = ItemTier.Unknown;

        ItemLevels = FrequentlyValues.ItemLevels;
        SelectedItemLevel = ItemLevel.Unknown;

        // Tracking
        UserTrackingBindings.UsernameInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.GuildInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.AllianceInformationVisibility = Visibility.Hidden;
        UserTrackingBindings.CurrentMapInfoBinding.CurrentMapInformationVisibility = Visibility.Hidden;

        IsNavigationMenuOpen = SettingsController.CurrentSettings.IsNavigationMenuOpen;
        IsTrackingResetByMapChangeActive = SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive;

        // Dungeons
        DungeonBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.DungeonsGridSplitterPosition);

        // Mail Monitoring
        TradeMonitoringBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.MailMonitoringGridSplitterPosition);

        // Vault
        VaultBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.StorageHistoryGridSplitterPosition);

        // Damage Meter
        DamageMeterBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.DamageMeterGridSplitterPosition);

        // Gathering
        GatheringBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.GatheringGridSplitterPosition);

        // Party Builder
        PartyBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.PartyBuilderGridSplitterPosition);

        // Guild
        GuildBindings.GridSplitterPosition = new GridLength(SettingsController.CurrentSettings.GuildGridSplitterPosition);
    }

    #region Item list

    public void ItemFilterReset()
    {
        SearchText = string.Empty;
        SelectedItemShopCategory = null;
        SelectedItemShopSubCategory1 = null;
        SelectedItemShopSubCategory2 = null;
        SelectedItemShopSubCategory3 = null;
        SelectedItemLevel = ItemLevel.Unknown;
        SelectedItemTier = ItemTier.Unknown;
    }

    #endregion Item list

    #region Error bar

    public void SetErrorBar(Visibility visibility, string errorMessage, Exception exception = null)
    {
        ErrorBarText = errorMessage;
        ErrorBarException = exception;
        ErrorBarVisibility = visibility;
    }

    #endregion

    #region Inits

    private void InitAlerts()
    {
        if (AlertManager != null)
        {
            return;
        }

        SoundController.InitializeSoundFilesFromDirectory();
        AlertManager = new AlertController(ItemsView);
    }

    private static void UpgradeSettings()
    {
        if (!Settings.Default.UpgradeRequired)
        {
            return;
        }

        Settings.Default.Upgrade();
        Settings.Default.UpgradeRequired = false;
        Settings.Default.Save();
    }

    public async Task InitMainWindowDataAsync(
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
#if DEBUG
        DebugModeVisibility = Visibility.Visible;
#endif

        IsTaskProgressbarIndeterminate = true;
        IsTxtSearchEnabled = false;
        IsItemSearchCheckboxesEnabled = false;
        IsFilterResetEnabled = false;

        UpdateServerStatus();

        ItemsView = new ListCollectionView(ItemController.Items);
        InitAlerts();
        var userDataProgressEnd = progressStart + (progressEnd - progressStart) * 0.95;
        await LoadUserDataForActiveServerAsync(reportProgress, progressStart, userDataProgressEnd);
        reportProgress?.Invoke(userDataProgressEnd, LocalizationController.Translation("TRACKING"));
        LoggingBindings.Init();
        reportProgress?.Invoke(progressEnd, LocalizationController.Translation("TRACKING"));

        LoadIconVisibility = Visibility.Hidden;
        IsFilterResetEnabled = true;
        IsItemSearchCheckboxesEnabled = true;
        IsTxtSearchEnabled = true;
        IsTaskProgressbarIndeterminate = false;
        IsDataLoaded = true;

        CloseButtonActivationDelayAsync();
    }

    private async void CloseButtonActivationDelayAsync()
    {
        await Task.Delay(2000);
        IsCloseButtonActive = true;
    }

    #endregion

    #region Tool tasks

    public void SetToolTasksVisibility(Visibility value)
    {
        ToolTasksVisibility = value;
    }

    public void SwitchToolTasksState()
    {
        ToolTasksVisibility = ToolTasksVisibility switch
        {
            Visibility.Collapsed => Visibility.Visible,
            Visibility.Visible => Visibility.Collapsed,
            _ => ToolTasksVisibility
        };
    }

    #endregion

    #region Stats drop down

    public void SwitchStatsDropDownState()
    {
        StatsDropDownVisibility = StatsDropDownVisibility switch
        {
            Visibility.Collapsed => Visibility.Visible,
            Visibility.Visible => Visibility.Collapsed,
            _ => StatsDropDownVisibility
        };
    }

    #endregion

    #region Ui utility methods

    public void UpdateServerStatus()
    {
        var currentServerLocation = GetCurrentServerLocation();
        MainStatusBindings.SetServerLocation(currentServerLocation);
    }

    public async Task LoadUserDataForActiveServerAsync(
        Action<double, string> reportProgress = null,
        double progressStart = 0,
        double progressEnd = 100)
    {
        if (!AppDataPaths.IsUserDataAvailable)
        {
            Log.Debug("Skipped Albion user data load because no Albion server is active. Server={Server}, Directory={Directory}", AppDataPaths.ActiveUserDataServerLocation, AppDataPaths.UserDataDirectory);
            return;
        }

        Log.Information("Loading Albion user data. Server={Server}, Directory={Directory}", AppDataPaths.ActiveUserDataServerLocation, AppDataPaths.UserDataDirectory);

        const int totalTaskCount = 16;
        double GetProgress(int completedTaskCount) => progressStart + completedTaskCount / (double) totalTaskCount * (progressEnd - progressStart);

        ResetItemUserDataState();
        reportProgress?.Invoke(GetProgress(0), Settings.Default.FavoriteItemsFileName);
        await ItemController.SetFavoriteItemsFromLocalFileAsync();

        AlertManager.StopAllAlerts();
        reportProgress?.Invoke(GetProgress(1), Settings.Default.ActiveAlertsFileName);
        await AlertManager.LoadFromFileAsync();

        var trackingProgressStart = GetProgress(2);
        var trackingProgressEnd = GetProgress(15);
        if (ServiceLocator.IsServiceInDictionary<TrackingController>())
        {
            await ServiceLocator.Resolve<TrackingController>().LoadDataAsync(reportProgress, trackingProgressStart, trackingProgressEnd);
        }

        reportProgress?.Invoke(trackingProgressEnd, Settings.Default.EstimatedMarketValueFileName);
        await EstimatedMarketValueController.SetAllEstimatedMarketValuesToItemsAsync();
        ItemsView?.Refresh();
        LoggingBindings.RefreshLootComparatorSaves();
        Log.Information("Albion user data loaded. Server={Server}, Directory={Directory}", AppDataPaths.ActiveUserDataServerLocation, AppDataPaths.UserDataDirectory);
        reportProgress?.Invoke(progressEnd, Settings.Default.EstimatedMarketValueFileName);
    }

    private void ResetItemUserDataState()
    {
        foreach (var item in ItemController.Items ?? [])
        {
            item.IsFavorite = false;
            item.IsAlertActive = false;
            item.IsPriceAlertActive = false;
            item.IsAvailabilityAlertActive = false;
            item.IsBlackMarketBuyOrderAlertActive = false;
            item.IsAlertSoundEnabled = true;
            item.AlertModeMinSellPriceIsUndercutPrice = 0;
            item.PriceAlertMaximumPriceAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
            item.AvailabilityAlertMaximumPriceAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
            item.BlackMarketBuyOrderAlertThreshold = 0;
            item.BlackMarketAlertMaximumPriceAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
        }
    }

    private void RegisterServerDetectionEvents()
    {
        if (!ServiceLocator.IsServiceInDictionary<AlbionServerDetectionService>())
        {
            return;
        }

        ServiceLocator.Resolve<AlbionServerDetectionService>().ServerChanged += AlbionServerDetectionService_ServerChanged;
    }

    private void AlbionServerDetectionService_ServerChanged(object sender, AlbionServerChangedEventArgs e)
    {
        if (Application.Current?.Dispatcher?.CheckAccess() == true)
        {
            UpdateServerStatus();
            return;
        }

        _ = Application.Current?.Dispatcher?.BeginInvoke(UpdateServerStatus);
    }

    private static ServerLocation GetCurrentServerLocation()
    {
        if (!ServiceLocator.IsServiceInDictionary<AlbionServerDetectionService>())
        {
            return ServerLocation.Unknown;
        }

        return ServiceLocator.Resolve<AlbionServerDetectionService>().CurrentServerLocation;
    }

    public void ExportLootToFile()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"log-{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}utc",
            DefaultExt = ".csv",
            Filter = "CSV documents (.csv)|*.csv"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var trackingController = ServiceLocator.Resolve<TrackingController>();
                ExportFileWriter.WriteText(
                    dialog.FileName,
                    trackingController?.LootController?.GetLootLoggerObjectsAsCsv());
            }
            catch (Exception e)
            {
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    public void ExportLootToJsonFile()
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"log-{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}utc",
            DefaultExt = ".json",
            Filter = "JSON documents (.json)|*.json"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var trackingController = ServiceLocator.Resolve<TrackingController>();
                ExportFileWriter.WriteText(
                    dialog.FileName,
                    trackingController?.LootController?.GetLootLoggerObjectsAsJson());
            }
            catch (Exception e)
            {
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    #endregion

    #region Item View Filters

    private void LoadCategoriesToDropdown()
    {
        var categories = ItemController.GetRootCategories()
            .OrderBy(cat => cat.Value, StringComparer.Ordinal)
            .Select(cat => new CategoryDropdownItem
            {
                Id = cat.Id,
                Value = cat.Value,
                DisplayName = LocalizationController.Translation("@MARKETPLACEGUI_ROLLOUT_SHOPCATEGORY_" + cat.Id.ToUpperInvariant())
            });

        ItemShopCategories.Clear();
        foreach (var item in categories)
        {
            ItemShopCategories.Add(item);
        }

        SelectedItemShopCategory = null;
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
                var value = x.Value ?? string.Empty;
                var translationKey = string.IsNullOrWhiteSpace(id) ? "UNKNOWN" : id.ToUpperInvariant();
                var displayName = LocalizationController.Translation("@MARKETPLACEGUI_ROLLOUT_SHOPSUBCATEGORY_" + translationKey) ?? translationKey;

                return new CategoryDropdownItem
                {
                    Id = id,
                    Value = value,
                    DisplayName = displayName
                };
            })
        );
    }

    private void ItemsViewFilter()
    {
        if (ItemsView == null)
        {
            return;
        }

        string search = SearchText?.ToLower() ?? string.Empty;

        ItemsView.Filter = i =>
        {
            if (i is not Item item)
            {
                return false;
            }

            bool nameMatch = item.LocalizedNameAndEnglish?.ToLower().Contains(search) ?? false;

            bool catMatch = SelectedItemShopCategory == null || string.IsNullOrWhiteSpace(SelectedItemShopCategory.Id) || item.FullItemInformation?.ShopCategory == SelectedItemShopCategory.Id;
            bool sub1Match = SelectedItemShopSubCategory1 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory1.Id) || item.FullItemInformation?.ShopSubCategory1 == SelectedItemShopSubCategory1.Id;
            bool sub2Match = SelectedItemShopSubCategory2 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory2.Id) || item.FullItemInformation?.ShopSubCategory2 == SelectedItemShopSubCategory2.Id;
            bool sub3Match = SelectedItemShopSubCategory3 == null || string.IsNullOrWhiteSpace(SelectedItemShopSubCategory3.Id) || item.FullItemInformation?.ShopSubCategory3 == SelectedItemShopSubCategory3.Id;

            bool tierMatch = SelectedItemTier == ItemTier.Unknown || (ItemTier) item.Tier == SelectedItemTier;
            bool levelMatch = SelectedItemLevel == ItemLevel.Unknown || (ItemLevel) item.Level == SelectedItemLevel;

            var commonFiltersMatch = nameMatch && catMatch && sub1Match && sub2Match && sub3Match && tierMatch && levelMatch;
            var favoriteFilterMatch = !IsShowOnlyFavoritesActive || item.IsFavorite;
            var alertFilterMatch = !IsShowOnlyItemsWithAlertOnActive || item.IsAlertActive;

            return commonFiltersMatch && favoriteFilterMatch && alertFilterMatch;
        };

        SetItemCounterAsync();
    }

    private async void SetItemCounterAsync()
    {
        try
        {
            LocalImageCounter = await ImageController.LocalImagesCounterAsync();
            ItemCounterString = $"{((ListCollectionView) ItemsView)?.Count ?? 0}/{ItemController.Items?.Count ?? 0}";
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    #endregion

    #region Bindings

    public string SearchText
    {
        get;
        set
        {
            field = value;

            ItemsViewFilter();
            ItemsView?.Refresh();

            OnPropertyChanged();
        }
    }

    public ICollectionView ItemsView
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Item SelectedSearchItem
    {
        get;
        set
        {
            if (ReferenceEquals(field, value))
            {
                return;
            }

            var previousItemDetails = SelectedItemDetails;
            if (previousItemDetails != null)
            {
                previousItemDetails.Deactivate();

                if (!previousItemDetails.HasActiveRefreshCooldown)
                {
                    ReleaseItemDetails(previousItemDetails);
                }
            }

            field = value;
            SelectedItemDetails = field == null ? null : GetOrCreateItemDetails(field);
            OnPropertyChanged();
        }
    }

    public ItemDetailsViewModel SelectedItemDetails
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsItemDetailsSelected));
        }
    }

    public bool IsItemDetailsSelected => SelectedItemDetails != null;

    public async Task RefreshSelectedItemOnOpeningAsync(Item item)
    {
        if (item == null
            || SelectedItemDetails?.Item == null
            || !string.Equals(SelectedItemDetails.Item.UniqueName, item.UniqueName, StringComparison.Ordinal))
        {
            return;
        }

        await SelectedItemDetails.RefreshOnOpeningAsync();
    }

    private ItemDetailsViewModel GetOrCreateItemDetails(Item item)
    {
        if (!string.IsNullOrWhiteSpace(item?.UniqueName)
            && _itemDetailsCache.TryGetValue(item.UniqueName, out var cachedItemDetails))
        {
            cachedItemDetails.Activate();
            _ = cachedItemDetails.RefreshOnOpeningAsync();
            return cachedItemDetails;
        }

        var itemDetails = new ItemDetailsViewModel(item, _itemRefreshCooldownTracker, AlertManager);
        itemDetails.RefreshCooldownExpired += ItemDetails_RefreshCooldownExpired;
        itemDetails.Activate();

        if (!string.IsNullOrWhiteSpace(item?.UniqueName))
        {
            _itemDetailsCache[item.UniqueName] = itemDetails;
        }

        return itemDetails;
    }

    private void ItemDetails_RefreshCooldownExpired(object sender, EventArgs e)
    {
        if (sender is ItemDetailsViewModel itemDetails && !itemDetails.IsActive)
        {
            ReleaseItemDetails(itemDetails);
        }
    }

    private void ReleaseItemDetails(ItemDetailsViewModel itemDetails)
    {
        itemDetails.RefreshCooldownExpired -= ItemDetails_RefreshCooldownExpired;

        if (!string.IsNullOrWhiteSpace(itemDetails.Item?.UniqueName)
            && _itemDetailsCache.TryGetValue(itemDetails.Item.UniqueName, out var cachedItemDetails)
            && ReferenceEquals(cachedItemDetails, itemDetails))
        {
            _itemDetailsCache.Remove(itemDetails.Item.UniqueName);
        }

        itemDetails.Dispose();
    }

    public void DisposeItemDetails()
    {
        SelectedSearchItem = null;

        foreach (var itemDetails in _itemDetailsCache.Values.ToList())
        {
            ReleaseItemDetails(itemDetails);
        }

        _itemDetailsCache.Clear();
    }

    public Visibility IsDamageMeterPopupVisible
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Hidden;

    public string LoggingSearchText
    {
        get;
        set
        {
            field = value;
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            trackingController?.NotificationUiFilteringAsync(field);
            OnPropertyChanged();
        }
    }

    public bool IsTrackingPartyLootOnly
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingPartyLootOnly = field;
            OnPropertyChanged();
        }
    }

    public PartyBindings PartyBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public GuildBindings GuildBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public DamageMeterBindings DamageMeterBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public DungeonBindings DungeonBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public GatheringBindings GatheringBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();


    public CraftingBindings CraftingBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public UserTrackingBindings UserTrackingBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public PlayerInformationBindings PlayerInformationBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public double UsernameInfoWidth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double GuildInfoWidth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double AllianceInfoWidth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double CurrentMapInfoWidth
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsDataLoaded
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingResetByMapChangeActive
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsTrackingResetByMapChangeActive = field;
            OnPropertyChanged();
        }
    }

    public LoggingBindings LoggingBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public bool IsTrackingActive
    {
        get;
        set
        {
            field = value;
            var trackingController = ServiceLocator.IsServiceInDictionary<TrackingController>()
                ? ServiceLocator.Resolve<TrackingController>()
                : null;

            switch (field)
            {
                case true when trackingController is { ExistIndispensableInfos: false }:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsPartiallyActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.Partially;
                    break;
                case true when trackingController is { ExistIndispensableInfos: true }:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.On;
                    break;
                case false:
                    TrackingActivityBindings.TrackingActiveText = MainWindowTranslation.TrackingIsNotActive;
                    TrackingActivityBindings.TrackingActivityType = TrackingIconType.Off;
                    MainStatusBindings.ResetGameSession();
                    break;
            }

            OnPropertyChanged();
        }
    }

    public TrackingActivityBindings TrackingActivityBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public MainStatusBindings MainStatusBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public bool IsDamageMeterTrackingActive
    {
        get;
        set
        {
            field = value;

            DamageMeterBindings.DamageMeterActivationToggleIcon =
                field ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;

            var colorOn = new SolidColorBrush((Color) Application.Current.Resources["Color.Accent.Blue.2"]!);
            var colorOff = new SolidColorBrush((Color) Application.Current.Resources["Color.Text.1"]!);
            DamageMeterBindings.DamageMeterActivationToggleColor = field ? colorOn : colorOff;

            SettingsController.CurrentSettings.IsDamageMeterTrackingActive = field;
            OnPropertyChanged();
        }
    }

    public Visibility IsItemSearchPopupVisible
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Hidden;

    public bool IsShowOnlyFavoritesActive
    {
        get;
        set
        {
            field = value;

            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public bool IsShowOnlyItemsWithAlertOnActive
    {
        get;
        set
        {
            field = value;

            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PartyMemberCircle> PartyMemberCircles
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public int PartyMemberNumber
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CategoryDropdownItem> ItemShopCategories
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public CategoryDropdownItem SelectedItemShopCategory
    {
        get;
        set
        {
            field = value;

            var subCatsRaw = ItemController.GetSubCategories1(field?.Id);
            ItemSubCategories1 = ToCategoryDropdownItems(subCatsRaw);

            SelectedItemShopSubCategory1 = null;
            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories1
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public CategoryDropdownItem SelectedItemShopSubCategory1
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;

                if (field != null && SelectedItemShopCategory != null)
                {
                    var subCats2Raw = ItemController.GetSubCategories2(SelectedItemShopCategory.Id, field.Id);
                    ItemSubCategories2 = ToCategoryDropdownItems(subCats2Raw);
                }
                else
                {
                    ItemSubCategories2 = [];
                }

                SelectedItemShopSubCategory2 = null;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories2
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public CategoryDropdownItem SelectedItemShopSubCategory2
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;

                if (field != null && SelectedItemShopCategory != null && SelectedItemShopSubCategory1 != null)
                {
                    var subCats3Raw = ItemController.GetSubCategories3(SelectedItemShopCategory.Id,
                        SelectedItemShopSubCategory1.Id, field.Id);
                    ItemSubCategories3 = ToCategoryDropdownItems(subCats3Raw);
                }
                else
                {
                    ItemSubCategories3 = [];
                }

                SelectedItemShopSubCategory3 = null;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<CategoryDropdownItem> ItemSubCategories3
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public CategoryDropdownItem SelectedItemShopSubCategory3
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                ItemsViewFilter();
                ItemsView?.Refresh();
                OnPropertyChanged();
            }
        }
    }

    public Dictionary<ItemTier, string> ItemTiers
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ItemTier SelectedItemTier
    {
        get;
        set
        {
            field = value;
            ItemsViewFilter();
            ItemsView?.Refresh();
            OnPropertyChanged();
        }
    }

    public bool IsCloseButtonActive
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Dictionary<ItemLevel, string> ItemLevels
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ItemLevel SelectedItemLevel
    {
        get;
        set
        {
            field = value;
            ItemsView?.Refresh();
            SetItemCounterAsync();
            OnPropertyChanged();
        }
    }

    public int LocalImageCounter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemCounterString
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsTxtSearchEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsItemSearchCheckboxesEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsFilterResetEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility LoadIconVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public DashboardBindings DashboardBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public string LoadTranslation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string NumberOfValuesTranslation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility DebugModeVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public ObservableCollection<MainStatObject> FactionPointStats
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new() { new MainStatObject() { Value = 0, ValuePerHour = 0, CityFaction = CityFaction.Unknown } };

    public ObservableCollection<ISeries> SeriesDashboardHourValues
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public Axis[] XAxesDashboardHourValues
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardChartRangeOption> DashboardChartRanges
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public DashboardChartRangeOption SelectedDashboardChartRange
    {
        get;
        set
        {
            field = value;
            if (value != null)
            {
                SettingsController.CurrentSettings.SelectedDashboardChartRangeBucketCount = value.BucketCount;
                SettingsController.CurrentSettings.SelectedDashboardChartRangeUnit = value.Unit;
            }
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardChartSeriesFilter> DashboardChartSeriesFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public ObservableCollection<DashboardContentFilterOption> DashboardContentFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public DashboardContentFilterOption SelectedDashboardContentFilter
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.SelectedDashboardContentType = value?.ContentType;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DashboardSessionFilterOption> DashboardSessionFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public DashboardSessionFilterOption SelectedDashboardSessionFilter
    {
        get;
        set
        {
            field = value;
            if (DashboardSessionFilters.Count > 1 || SettingsController.CurrentSettings.SelectedDashboardSessionId is null)
            {
                SettingsController.CurrentSettings.SelectedDashboardSessionId = value?.SessionId;
            }
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMinimum
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double TaskProgressbarMaximum
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = 100;

    public double TaskProgressbarValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsTaskProgressbarIndeterminate
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility ToolTasksVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility StatsDropDownVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility UnsupportedOsVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility DashboardTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility ItemSearchTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility LoggingTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility GuildTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility DungeonsTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility DamageMeterTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility TradeMonitoringTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;


    public Visibility GatheringTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility CraftingTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility PartyTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility StorageHistoryTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility MapHistoryTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public Visibility PlayerInformationTabVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public bool IsNavigationMenuOpen
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            SettingsController.CurrentSettings.IsNavigationMenuOpen = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(NavigationMenuWidth));
        }
    } = true;

    public double NavigationMenuWidth
    {
        get
        {
            return IsNavigationMenuOpen ? 190 : 64;
        }
    }

    public double ToolTaskProgressBarValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ToolTaskCurrentTaskName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility ToolTaskFrontViewVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public TradeMonitoringBindings TradeMonitoringBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public VaultBindings VaultBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<ClusterInfo> EnteredCluster
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public string ErrorBarText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Exception ErrorBarException
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility ErrorBarVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public string WarningBarText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility WarningBarVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public string InformationBarText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility InformationBarVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public string UpdateTranslation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public MainWindowTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public static string ItemListJsonHyperlink => "https://raw.githubusercontent.com/ao-data/ao-bin-dumps/master/formatted/items.json";
    public static string ItemsJsonHyperlink => "https://raw.githubusercontent.com/broderickhyman/ao-bin-dumps/master/items.json";

    public static string ToolDirectory => AppDataPaths.InstallationDirectory;
    public static string Version => $"v{Assembly.GetExecutingAssembly().GetName().Version}";

    #endregion Bindings
}
