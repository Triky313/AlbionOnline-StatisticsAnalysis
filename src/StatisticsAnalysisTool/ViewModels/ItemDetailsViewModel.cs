using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Alert;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.Models.ItemDetailsModel;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.ViewModels;

public class ItemDetailsViewModel : BaseViewModel, IDisposable
{
    private readonly ItemRefreshCooldownTracker _itemRefreshCooldownTracker;
    private readonly DispatcherTimer _refreshCooldownTimer = new();
    private readonly AlertController _alertController;
    private bool _isRefreshInProgress;
    private bool _isInitialized;
    private bool _isRefreshCooldownActive;
    private int _historyRequestVersion;
    private volatile bool _isDisposed;

    public enum Error
    {
        NoPrices,
        NoItemInfo,
        GeneralError,
        ToManyRequests
    }

    public event EventHandler RefreshCooldownExpired;

    public ItemDetailsViewModel(Item item, ItemRefreshCooldownTracker itemRefreshCooldownTracker, AlertController alertController)
    {
        _itemRefreshCooldownTracker = itemRefreshCooldownTracker ?? throw new ArgumentNullException(nameof(itemRefreshCooldownTracker));
        _alertController = alertController ?? throw new ArgumentNullException(nameof(alertController));
        _alertController.AlertStateChanged += AlertController_AlertStateChanged;
        _refreshCooldownTimer.Tick += RefreshCooldownTimer_Tick;
        ErrorBarVisibility = Visibility.Hidden;

        Item = item;

        Translation = new ItemDetailsTranslation();
        _ = InitAsync(item);

        ItemListViewLanguage = XmlLanguage.GetLanguage(CultureInfo.DefaultThreadCurrentCulture?.IetfLanguageTag ?? string.Empty);
    }

    #region Inits

    private async Task InitAsync(Item item)
    {
        IsTaskProgressbarIndeterminate = true;
        Icon = null;
        TitleName = "-";
        ItemTierLevel = string.Empty;
        ItemTier = "-";
        ItemCategoryName = "-";
        ItemTypeName = "-";
        ItemUniqueName = "-";
        LastUpdatedText = $"{LocalizationController.Translation("LAST_UPDATE")}: -";

        Item = item;

        if (item == null)
        {
            SetErrorValues(Error.NoItemInfo);
            return;
        }

        InitBindings();
        InitMainTabLocationFiltering();
        InitQualityFiltering();
        InitExtraItemInformation();

        if (Application.Current.Dispatcher == null)
        {
            SetErrorValues(Error.GeneralError);
            return;
        }

        ChangeHeaderValues(item);

        await RefreshOnOpeningAsync();

        if (!_isDisposed)
        {
            _isInitialized = true;
        }
    }

    private void InitBindings()
    {
        MainTabBindings = new ItemDetailsMainTabBindings(this);
        QualityTabBindings = new ItemDetailsQualityTabBindings();
        HistoryBindings = new ItemDetailsHistoryBindings(this);
        RealMoneyTabBindings = new ItemDetailsRealMoneyTabBindings(this);
    }

    private void InitMainTabLocationFiltering()
    {
        var locationFilters = new List<MainTabLocationFilterObject>
        {
            new (MarketLocation.CaerleonMarket, Locations.GetParameterName(MarketLocation.CaerleonMarket), true),
            new (MarketLocation.ThetfordMarket, Locations.GetParameterName(MarketLocation.ThetfordMarket), true),
            new (MarketLocation.FortSterlingMarket, Locations.GetParameterName(MarketLocation.FortSterlingMarket), true),
            new (MarketLocation.LymhurstMarket, Locations.GetParameterName(MarketLocation.LymhurstMarket), true),
            new (MarketLocation.BridgewatchMarket, Locations.GetParameterName(MarketLocation.BridgewatchMarket), true),
            new (MarketLocation.MartlockMarket, Locations.GetParameterName(MarketLocation.MartlockMarket), true),
            new (MarketLocation.BrecilienMarket, Locations.GetParameterName(MarketLocation.BrecilienMarket), true),
            new (MarketLocation.BlackMarket, Locations.GetParameterName(MarketLocation.BlackMarket), true),
            new (MarketLocation.ForestCross, Locations.GetParameterName(MarketLocation.ForestCross), true),
            new (MarketLocation.SwampCross, Locations.GetParameterName(MarketLocation.SwampCross), true),
            new (MarketLocation.SteppeCross, Locations.GetParameterName(MarketLocation.SteppeCross), true),
            new (MarketLocation.HighlandCross, Locations.GetParameterName(MarketLocation.HighlandCross), true),
            new (MarketLocation.MountainCross, Locations.GetParameterName(MarketLocation.MountainCross), true),
            new (MarketLocation.SmugglersDen, Locations.GetParameterName(MarketLocation.SmugglersDen), true)
        };

        foreach (var itemDetailsLocationFilter in SettingsController.CurrentSettings.ItemDetailsLocationFilters)
        {
            var filter = locationFilters.FirstOrDefault(x => x.Location == itemDetailsLocationFilter?.Location);
            if (filter != null)
            {
                filter.IsChecked = itemDetailsLocationFilter.IsChecked;
            }
        }

        LocationFilters = new ObservableCollection<MainTabLocationFilterObject>(locationFilters.OrderBy(x => x.Name));

        AddLocationFiltersEvents();
    }

    private void AddLocationFiltersEvents()
    {
        foreach (var cityFilterObject in LocationFilters)
        {
            cityFilterObject.OnCheckedChanged += UpdateMainTabItemPrices;
            cityFilterObject.OnCheckedChanged += UpdateQualityTabItemPrices;
            cityFilterObject.OnCheckedChanged += UpdateHistoryChartPricesAsync;
        }
    }

    public void RemoveLocationFiltersEvents()
    {
        foreach (var cityFilterObject in LocationFilters)
        {
            cityFilterObject.OnCheckedChanged -= UpdateMainTabItemPrices;
            cityFilterObject.OnCheckedChanged -= UpdateQualityTabItemPrices;
            cityFilterObject.OnCheckedChanged -= UpdateHistoryChartPricesAsync;
        }
    }

    private void InitQualityFiltering()
    {
        var normalQuality = new ItemDetailsMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("NORMAL"), Quality = 1 };
        var goodQuality = new ItemDetailsMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("GOOD"), Quality = 2 };
        var outstandingQuality = new ItemDetailsMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("OUTSTANDING"), Quality = 3 };
        var excellentQuality = new ItemDetailsMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("EXCELLENT"), Quality = 4 };
        var masterpieceQuality = new ItemDetailsMainTabBindings.QualityStruct() { Name = LocalizationController.Translation("MASTERPIECE"), Quality = 5 };

        MainTabBindings.Qualities.Add(normalQuality);
        MainTabBindings.Qualities.Add(goodQuality);
        MainTabBindings.Qualities.Add(outstandingQuality);
        MainTabBindings.Qualities.Add(excellentQuality);
        MainTabBindings.Qualities.Add(masterpieceQuality);

        if (MainTabBindings.Qualities != null)
        {
            MainTabBindings.QualitiesSelection = MainTabBindings.Qualities.FirstOrDefault();
        }

    }

    private void InitExtraItemInformation()
    {
        switch (Item?.FullItemInformation)
        {
            case Weapon weapon:
                ExtraItemInformation.ShopCategory = weapon.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = weapon.ShopSubCategory1;
                ExtraItemInformation.CanBeOvercharged = weapon.CanBeOvercharged.SetYesOrNo();
                ExtraItemInformation.Durability = weapon.Durability;
                ExtraItemInformation.ShowInMarketPlace = weapon.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = weapon.Weight;
                break;
            case TransformationWeapon transformationWeapon:
                ExtraItemInformation.ShopCategory = transformationWeapon.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = transformationWeapon.ShopSubCategory1;
                ExtraItemInformation.Weight = transformationWeapon.Weight;
                break;
            case HideoutItem hideoutItem:
                ExtraItemInformation.ShopCategory = hideoutItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = hideoutItem.ShopSubCategory1;
                ExtraItemInformation.Weight = hideoutItem.Weight;
                break;
            case FarmableItem farmableItem:
                ExtraItemInformation.ShopCategory = farmableItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = farmableItem.ShopSubCategory1;
                ExtraItemInformation.ShowInMarketPlace = farmableItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = farmableItem.Weight;
                break;
            case SimpleItem simpleItem:
                ExtraItemInformation.ShopCategory = simpleItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = simpleItem.ShopSubCategory1;
                ExtraItemInformation.Weight = simpleItem.Weight;
                break;
            case ConsumableItem consumableItem:
                ExtraItemInformation.ShopCategory = consumableItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = consumableItem.ShopSubCategory1;
                ExtraItemInformation.Weight = consumableItem.Weight;
                break;
            case ConsumableFromInventoryItem consumableFromInventoryItem:
                ExtraItemInformation.ShopCategory = consumableFromInventoryItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = consumableFromInventoryItem.ShopSubCategory1;
                ExtraItemInformation.Weight = consumableFromInventoryItem.Weight;
                break;
            case EquipmentItem equipmentItem:
                ExtraItemInformation.ShopCategory = equipmentItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = equipmentItem.ShopSubCategory1;
                ExtraItemInformation.CanBeOvercharged = equipmentItem.CanBeOvercharged.SetYesOrNo();
                ExtraItemInformation.Durability = equipmentItem.Durability;
                ExtraItemInformation.ShowInMarketPlace = equipmentItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = equipmentItem.Weight;
                break;
            case Mount mount:
                ExtraItemInformation.ShopCategory = mount.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = mount.ShopSubCategory1;
                ExtraItemInformation.Durability = mount.Durability;
                ExtraItemInformation.ShowInMarketPlace = mount.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = mount.Weight;
                break;
            case FurnitureItem furnitureItem:
                ExtraItemInformation.ShopCategory = furnitureItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = furnitureItem.ShopSubCategory1;
                ExtraItemInformation.Durability = furnitureItem.Durability;
                ExtraItemInformation.ShowInMarketPlace = furnitureItem.ShowInMarketPlace.SetYesOrNo();
                ExtraItemInformation.Weight = furnitureItem.Weight;
                break;
            case JournalItem journalItem:
                ExtraItemInformation.ShopCategory = journalItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = journalItem.ShopSubCategory1;
                ExtraItemInformation.Weight = journalItem.Weight;
                break;
            case LabourerContract labourerContract:
                ExtraItemInformation.ShopCategory = labourerContract.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = labourerContract.ShopSubCategory1;
                ExtraItemInformation.Weight = labourerContract.Weight;
                break;
            case CrystalLeagueItem crystalLeagueItem:
                ExtraItemInformation.ShopCategory = crystalLeagueItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = crystalLeagueItem.ShopSubCategory1;
                ExtraItemInformation.Weight = crystalLeagueItem.Weight;
                break;
            case TrackingItem trackingItem:
                ExtraItemInformation.ShopCategory = trackingItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = trackingItem.ShopSubCategory1;
                ExtraItemInformation.Weight = trackingItem.Weight;
                break;
            case KillTrophyItem killTrophyItem:
                ExtraItemInformation.ShopCategory = killTrophyItem.ShopCategory;
                ExtraItemInformation.ShopSubCategory1 = killTrophyItem.ShopSubCategory1;
                ExtraItemInformation.Weight = killTrophyItem.Weight;
                break;
        }
    }

    #endregion

    #region Saving

    public void SaveSettings()
    {
        SettingsController.CurrentSettings.ItemDetailsLocationFilters = LocationFilters?.Select(x => new MainTabLocationFilterSettingsObject()
        {
            IsChecked = x.IsChecked ?? false,
            Location = x.Location
        }).ToList();
    }

    #endregion

    #region Ui

    private void ChangeHeaderValues(Item item)
    {
        var localizedName = ItemController.LocalizedName(item.LocalizedNames, null, item.UniqueName);
        var isTierKnown = item.Tier is >= 1 and <= 8;
        var isEnchantmentKnown = item.Level is >= 0 and <= 4;

        Icon = item.Icon;
        TitleName = localizedName;
        ItemTierLevel = isTierKnown && isEnchantmentKnown ? $"T{item.Tier}.{item.Level}" : string.Empty;
        ItemTier = isTierKnown ? $"T{item.Tier}" : "-";
        ItemCategoryName = GetShopSubCategoryDisplayName(item.FullItemInformation?.ShopSubCategory1);
        ItemTypeName = GetItemTypeDisplayName(item.FullItemInformation?.ItemType ?? ItemType.Unknown);
        ItemUniqueName = string.IsNullOrWhiteSpace(item.UniqueName) ? "-" : item.UniqueName;
    }

    private static string GetShopSubCategoryDisplayName(string shopSubCategory)
    {
        if (string.IsNullOrWhiteSpace(shopSubCategory))
        {
            return "-";
        }

        var translationKey = "@MARKETPLACEGUI_ROLLOUT_SHOPSUBCATEGORY_" + shopSubCategory.ToUpperInvariant();
        var localizedName = LocalizationController.GameTranslation(translationKey);

        return string.Equals(localizedName, translationKey, StringComparison.OrdinalIgnoreCase)
            ? FormatIdentifier(shopSubCategory)
            : localizedName;
    }

    private static string GetItemTypeDisplayName(ItemType itemType)
    {
        if (itemType == ItemType.Unknown)
        {
            return "-";
        }

        var translationKey = itemType.ToString().ToUpperInvariant();
        var localizedName = LocalizationController.Translation(translationKey);

        return string.Equals(localizedName, translationKey, StringComparison.OrdinalIgnoreCase)
            ? FormatIdentifier(itemType.ToString())
            : localizedName;
    }

    private static string FormatIdentifier(string value)
    {
        var words = string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString()));

        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words.Replace('_', ' ').ToLowerInvariant());
    }

    private static SolidColorPaint CreateChartTextPaint()
    {
        var textBrush = Application.Current?.TryFindResource("SolidColorBrush.Text.1") as SolidColorBrush;
        var color = textBrush?.Color ?? Colors.White;

        return new SolidColorPaint(new SKColor(color.R, color.G, color.B, color.A));
    }

    #endregion

    #region Monitoring

    public void TogglePriceAlert()
    {
        var result = _alertController.SetPriceAlert(
            Item,
            !IsPriceAlertActive,
            PriceAlertThreshold,
            PriceAlertMaximumPriceAgeMinutes,
            IsAlertSoundEnabled);

        SetMonitoringActivationResult(result);
    }

    public void ToggleAvailabilityAlert()
    {
        var result = _alertController.SetAvailabilityAlert(
            Item,
            !IsAvailabilityAlertActive,
            AvailabilityAlertMaximumPriceAgeMinutes,
            IsAlertSoundEnabled);

        SetMonitoringActivationResult(result);
    }

    public void ToggleBlackMarketBuyOrderAlert()
    {
        var result = _alertController.SetBlackMarketBuyOrderAlert(
            Item,
            !IsBlackMarketBuyOrderAlertActive,
            BlackMarketBuyOrderAlertThreshold,
            BlackMarketAlertMaximumPriceAgeMinutes,
            IsAlertSoundEnabled);

        SetMonitoringActivationResult(result);
    }

    private void AlertController_AlertStateChanged(object sender, AlertStateChangedEventArgs e)
    {
        if (!string.Equals(Item?.UniqueName, e.ItemUniqueName, StringComparison.Ordinal))
        {
            return;
        }

        OnPropertyChanged(nameof(PriceAlertThreshold));
        OnPropertyChanged(nameof(PriceAlertThresholdText));
        OnPropertyChanged(nameof(PriceAlertMaximumPriceAgeMinutes));
        OnPropertyChanged(nameof(PriceAlertMaximumPriceAgeMinutesText));
        OnPropertyChanged(nameof(AvailabilityAlertMaximumPriceAgeMinutes));
        OnPropertyChanged(nameof(AvailabilityAlertMaximumPriceAgeMinutesText));
        OnPropertyChanged(nameof(BlackMarketBuyOrderAlertThreshold));
        OnPropertyChanged(nameof(BlackMarketBuyOrderAlertThresholdText));
        OnPropertyChanged(nameof(BlackMarketAlertMaximumPriceAgeMinutes));
        OnPropertyChanged(nameof(BlackMarketAlertMaximumPriceAgeMinutesText));
        OnPropertyChanged(nameof(IsPriceAlertActive));
        OnPropertyChanged(nameof(IsPriceAlertInactive));
        OnPropertyChanged(nameof(IsAvailabilityAlertActive));
        OnPropertyChanged(nameof(IsAvailabilityAlertInactive));
        OnPropertyChanged(nameof(IsBlackMarketBuyOrderAlertActive));
        OnPropertyChanged(nameof(IsBlackMarketBuyOrderAlertInactive));
    }

    private void SetMonitoringActivationResult(AlertActivationResult result)
    {
        MonitoringErrorText = result switch
        {
            AlertActivationResult.InvalidPriceThreshold => LocalizationController.Translation("ENTER_VALID_PRICE_LIMIT"),
            AlertActivationResult.InvalidMaximumPriceAge => LocalizationController.Translation("ENTER_VALID_MAXIMUM_PRICE_AGE"),
            AlertActivationResult.MaximumActiveAlertsReached => LocalizationController.Translation("MAXIMUM_ACTIVE_ITEM_ALERTS_REACHED"),
            AlertActivationResult.ItemNotFound => LocalizationController.Translation("ITEM_ALERT_COULD_NOT_BE_ACTIVATED"),
            AlertActivationResult.ItemNotBlackMarketEligible => LocalizationController.Translation("ITEM_NOT_BLACK_MARKET_ELIGIBLE"),
            _ => string.Empty
        };

        MonitoringErrorVisibility = string.IsNullOrEmpty(MonitoringErrorText)
            ? Visibility.Collapsed
            : Visibility.Visible;

        OnPropertyChanged(nameof(IsPriceAlertActive));
        OnPropertyChanged(nameof(IsPriceAlertInactive));
        OnPropertyChanged(nameof(IsAvailabilityAlertActive));
        OnPropertyChanged(nameof(IsAvailabilityAlertInactive));
        OnPropertyChanged(nameof(IsBlackMarketBuyOrderAlertActive));
        OnPropertyChanged(nameof(IsBlackMarketBuyOrderAlertInactive));
    }

    #endregion

    #region Refresh

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        SaveSettings();
        IsActive = false;
    }

    public async Task RefreshOnOpeningAsync()
    {
        if (_isDisposed || _isRefreshInProgress || string.IsNullOrWhiteSpace(Item?.UniqueName))
        {
            return;
        }

        await RefreshWithCooldownAsync();
    }

    public async Task RefreshManuallyAsync()
    {
        if (!CanRefreshManually || string.IsNullOrWhiteSpace(Item?.UniqueName))
        {
            return;
        }

        await RefreshWithCooldownAsync();
    }

    private async Task RefreshWithCooldownAsync()
    {
        if (!_itemRefreshCooldownTracker.TryStart(Item.UniqueName, out var remainingCooldown))
        {
            StartRefreshCooldown(remainingCooldown);
            return;
        }

        StartRefreshCooldown(remainingCooldown);
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (_isDisposed || _isRefreshInProgress)
        {
            return;
        }

        _isRefreshInProgress = true;
        OnPropertyChanged(nameof(CanRefreshManually));
        IsTaskProgressbarIndeterminate = true;

        try
        {
            await UpdateMarketPricesAsync();

            if (_isDisposed)
            {
                return;
            }

            UpdateMainTabItemPrices();
            UpdateQualityTabItemPrices();
            UpdateHistoryChartPricesAsync();
        }
        finally
        {
            _isRefreshInProgress = false;
            OnPropertyChanged(nameof(CanRefreshManually));

            if (!_isDisposed)
            {
                IsTaskProgressbarIndeterminate = false;
            }
        }
    }

    public void ApplySelectedQualityFilter()
    {
        if (!_isInitialized || _isDisposed)
        {
            return;
        }

        UpdateMainTabItemPrices();
        UpdateHistoryChartPricesAsync();
    }

    public void ApplyHistoryTimeRangeFilter()
    {
        if (!_isInitialized || _isDisposed)
        {
            return;
        }

        UpdateHistoryChartPricesAsync();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (IsActive)
        {
            SaveSettings();
        }

        _isDisposed = true;
        _refreshCooldownTimer.Stop();
        _refreshCooldownTimer.Tick -= RefreshCooldownTimer_Tick;
        _alertController.AlertStateChanged -= AlertController_AlertStateChanged;
        RemoveLocationFiltersEvents();
    }

    #endregion

    private void StartRefreshCooldown(TimeSpan remainingCooldown)
    {
        if (remainingCooldown <= TimeSpan.Zero)
        {
            return;
        }

        _refreshCooldownTimer.Stop();
        _refreshCooldownTimer.Interval = remainingCooldown;
        _isRefreshCooldownActive = true;
        OnPropertyChanged(nameof(CanRefreshManually));
        _refreshCooldownTimer.Start();
    }

    private void RefreshCooldownTimer_Tick(object sender, EventArgs e)
    {
        _refreshCooldownTimer.Stop();

        if (_isDisposed)
        {
            return;
        }

        _isRefreshCooldownActive = false;
        OnPropertyChanged(nameof(CanRefreshManually));
        RefreshCooldownExpired?.Invoke(this, EventArgs.Empty);
    }

    #region Error methods

    private void SetErrorValues(Error error)
    {
        switch (error)
        {
            case Error.NoItemInfo:
                Icon = new BitmapImage(new Uri(@"pack://application:,,,/"
                                               + Assembly.GetExecutingAssembly().GetName().Name + ";component/"
                                               + "Resources/Trash.png", UriKind.Absolute));
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_NO_ITEM_INFO"));
                return;

            case Error.NoPrices:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED"));
                return;

            case Error.GeneralError:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_GENERAL_ERROR"));
                return;

            case Error.ToManyRequests:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("TOO_MANY_REQUESTS_CLOSE_WINDOWS_OR_WAIT"));
                return;

            default:
                SetLoadingImageToError();
                SetErrorBar(Visibility.Visible, LocalizationController.Translation("ERROR_GENERAL_ERROR"));
                return;
        }
    }

    private void ErrorBarReset()
    {
        IsTaskProgressbarIndeterminate = false;
        SetErrorBar(Visibility.Hidden, string.Empty);
    }

    private void SetLoadingImageToError()
    {
        IsTaskProgressbarIndeterminate = true;
    }

    private void SetErrorBar(Visibility visibility, string errorMessage, Exception exception = null)
    {
        ErrorBarText = errorMessage;
        ErrorBarException = exception;
        ErrorBarVisibility = visibility;
    }

    #endregion

    #region Prices

    public async Task UpdateMarketPricesAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            var localList = await trackingController.MarketController.GetResponsesForItem(Item?.UniqueName);

            if (_isDisposed)
            {
                return;
            }

            var apiList = await ApiController.GetCityItemPricesFromJsonAsync(Item?.UniqueName);

            if (_isDisposed)
            {
                return;
            }

            CurrentItemPrices = MergeMarketResponses(localList, apiList);

            LastUpdatedText = $"{LocalizationController.Translation("LAST_UPDATE")}: {DateTime.UtcNow.CurrentDateTimeFormat()}";
            ErrorBarReset();
        }
        catch (TooManyRequestsException ex)
        {
            SetErrorValues(Error.ToManyRequests);
            Log.Warning(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static List<MarketResponse> MergeMarketResponses(IEnumerable<MarketResponse> localList, IEnumerable<MarketResponse> apiList)
    {
        var result = new Dictionary<(string ItemTypeId, string City, int QualityLevel), MarketResponse>();

        void AddOrUpdate(MarketResponse response)
        {
            var key = (response.ItemTypeId, response.City, response.QualityLevel);

            if (!result.TryGetValue(key, out var existing))
            {
                result[key] = response;
                return;
            }

            var existingDate = MaxDate(existing.SellPriceMaxDate, existing.BuyPriceMaxDate);
            var responseDate = MaxDate(response.SellPriceMaxDate, response.BuyPriceMaxDate);

            if (responseDate > existingDate)
            {
                result[key] = response;
            }
        }

        foreach (var response in localList)
        {
            AddOrUpdate(response);
        }
        foreach (var response in apiList)
        {
            AddOrUpdate(response);
        }

        return result.Values.ToList();
    }

    private static DateTime MaxDate(DateTime a, DateTime b)
    {
        return a > b ? a : b;
    }

    private static void FindBestPrice(IReadOnlyCollection<ItemPricesObject> list)
    {
        if (list == null || list.Count == 0)
            return;

        for (var i = 1; i <= 5; i++)
        {
            var max = GetMaxPrice(list, i);

            var itemPricesObjectBuyPriceMax = list.Where(x => x.Visibility == Visibility.Visible && x.QualityLevel == i).FirstOrDefault(s => s.BuyPriceMax == max);
            if (itemPricesObjectBuyPriceMax != null)
            {
                itemPricesObjectBuyPriceMax.IsBestBuyMaxPrice = true;
            }

            var min = GetMinPrice(list, i);

            var itemPricesObjectSellPriceMin = list.Where(x => x.Visibility == Visibility.Visible && x.QualityLevel == i).FirstOrDefault(s => s.SellPriceMin == min);
            if (itemPricesObjectSellPriceMin != null)
            {
                itemPricesObjectSellPriceMin.IsBestSellMinPrice = true;
            }
        }
    }

    private static ulong GetMaxPrice(IEnumerable<ItemPricesObject> list, int quality)
    {
        var max = ulong.MinValue;
        foreach (var type in list.Where(x => x.QualityLevel == quality))
        {
            if (type.BuyPriceMax == 0)
                continue;

            if (type.BuyPriceMax > max)
                max = type.BuyPriceMax;
        }

        return max;
    }

    private static ulong GetMinPrice(IEnumerable<ItemPricesObject> list, int quality)
    {
        var min = ulong.MaxValue;
        foreach (var type in list.Where(x => x.QualityLevel == quality))
        {
            if (type.SellPriceMin == 0)
                continue;

            if (type.SellPriceMin < min)
                min = type.SellPriceMin;
        }

        return min;
    }

    #endregion Prices

    #region Main tab

    public void UpdateMainTabItemPrices()
    {
        if (_isDisposed)
        {
            return;
        }

        var currentItemPrices = CurrentItemPrices?.Select(x => new ItemPricesObject(x)).ToList();
        UpdateMainTabItemPricesObjects(currentItemPrices);
        FilterMainTabItemPrices(MainTabBindings.ItemPrices);
    }

    private void UpdateMainTabItemPricesObjects(List<ItemPricesObject> newPrices)
    {
        foreach (var newItemPricesObject in newPrices ?? new List<ItemPricesObject>())
        {
            if (MainTabBindings.ItemPrices == null)
            {
                break;
            }

            lock (MainTabBindings.ItemPrices)
            {
                var currentItemPricesObject = MainTabBindings.ItemPrices?.FirstOrDefault(x => x.MarketLocation == newItemPricesObject.MarketLocation && x.QualityLevel == newItemPricesObject.QualityLevel);

                if (currentItemPricesObject == null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MainTabBindings.ItemPrices?.Add(newItemPricesObject);
                    });
                }

                if (newItemPricesObject?.SellPriceMinDate > currentItemPricesObject?.SellPriceMinDate)
                {
                    currentItemPricesObject.SellPriceMin = newItemPricesObject.SellPriceMin;
                    currentItemPricesObject.SellPriceMinDate = newItemPricesObject.SellPriceMinDate;
                }

                if (newItemPricesObject?.SellPriceMaxDate > currentItemPricesObject?.SellPriceMaxDate)
                {
                    currentItemPricesObject.SellPriceMax = newItemPricesObject.SellPriceMax;
                    currentItemPricesObject.SellPriceMaxDate = newItemPricesObject.SellPriceMaxDate;
                }

                if (newItemPricesObject?.BuyPriceMinDate > currentItemPricesObject?.BuyPriceMinDate)
                {
                    currentItemPricesObject.BuyPriceMin = newItemPricesObject.BuyPriceMin;
                    currentItemPricesObject.BuyPriceMinDate = newItemPricesObject.BuyPriceMinDate;
                }

                if (newItemPricesObject?.BuyPriceMaxDate > currentItemPricesObject?.BuyPriceMaxDate)
                {
                    currentItemPricesObject.BuyPriceMax = newItemPricesObject.BuyPriceMax;
                    currentItemPricesObject.BuyPriceMaxDate = newItemPricesObject.BuyPriceMaxDate;
                }
            }
        }
    }

    private void FilterMainTabItemPrices(ObservableCollection<ItemPricesObject> prices)
    {
        var checkedLocations = GetCheckedLocations();

        foreach (var currentItemPricesObject in prices?.ToList() ?? new List<ItemPricesObject>())
        {
            if (checkedLocations.Contains(currentItemPricesObject.MarketLocation) && currentItemPricesObject.QualityLevel == MainTabBindings.QualitiesSelection.Quality)
            {
                currentItemPricesObject.Visibility = Visibility.Visible;
            }
            else
            {
                currentItemPricesObject.Visibility = Visibility.Collapsed;
            }
        }

        var itemPricesView = CollectionViewSource.GetDefaultView(prices);
        itemPricesView.Filter = item => item is ItemPricesObject itemPricesObject && itemPricesObject.Visibility == Visibility.Visible;
        itemPricesView.Refresh();

        FindBestPrice(prices?.Where(x => x.Visibility == Visibility.Visible).ToList());
    }

    private List<MarketLocation> GetCheckedLocations()
    {
        return LocationFilters?.Where(x => x?.IsChecked == true).Select(x => x.Location).ToList() ?? new List<MarketLocation>();
    }

    #endregion

    #region Quality tab / Real money tab

    public void UpdateQualityTabItemPrices()
    {
        if (_isDisposed)
        {
            return;
        }

        var marketResponse = CurrentItemPrices?.ToList();
        UpdateQualityTabItemPricesObjects(marketResponse);
        SetMarketQualityObjectVisibility(QualityTabBindings?.Prices);
    }

    private void UpdateQualityTabItemPricesObjects(List<MarketResponse> newPrices)
    {
        if (QualityTabBindings?.Prices == null)
        {
            return;
        }

        var existingPrices = QualityTabBindings.Prices.ToDictionary(x => x.MarketLocation);

        foreach (var marketResponse in newPrices ?? new List<MarketResponse>())
        {
            if (existingPrices.TryGetValue(marketResponse.MarketLocation, out var currentPriceObject))
            {
                currentPriceObject.SetValues(marketResponse);
                existingPrices[marketResponse.MarketLocation] = currentPriceObject;
            }
            else
            {
                currentPriceObject = new MarketQualityObject(marketResponse);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    QualityTabBindings?.Prices?.Add(currentPriceObject);
                });
                existingPrices.Add(marketResponse.MarketLocation, currentPriceObject);
            }
        }
    }

    private void SetMarketQualityObjectVisibility(List<MarketQualityObject> prices)
    {
        foreach (var currentItemPricesObject in prices?.ToList() ?? new List<MarketQualityObject>())
        {
            if (GetCheckedLocations().Contains(currentItemPricesObject.MarketLocation))
            {
                currentItemPricesObject.Visibility = Visibility.Visible;
            }
            else
            {
                currentItemPricesObject.Visibility = Visibility.Collapsed;
            }
        }
    }

    #endregion

    #region History

    public async void UpdateHistoryChartPricesAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        var requestVersion = ++_historyRequestVersion;
        List<MarketHistoriesResponse> historyItemPrices;

        try
        {
            var locations = GetCheckedLocations();
            var selectedTimeRangeDays = HistoryBindings.SelectedTimeRange?.Days ?? 30;
            DateTime? startDate = selectedTimeRangeDays > 0
                ? DateTime.UtcNow.AddDays(-selectedTimeRangeDays)
                : null;

            historyItemPrices = await ApiController.GetHistoryItemPricesFromJsonAsync(
                Item.UniqueName,
                locations,
                startDate,
                MainTabBindings.QualitiesSelection.Quality).ConfigureAwait(true);

            if (_isDisposed || historyItemPrices == null || requestVersion != _historyRequestVersion)
            {
                return;
            }
        }
        catch (TooManyRequestsException)
        {
            if (requestVersion != _historyRequestVersion)
            {
                return;
            }

            DebugConsole.WriteWarn(MethodBase.GetCurrentMethod()?.DeclaringType, new TooManyRequestsException());
            SetErrorValues(Error.ToManyRequests);
            return;
        }

        SetHistoryChart(historyItemPrices);
    }

    private void SetHistoryChart(List<MarketHistoriesResponse> historyItemPrices)
    {
        var date = new List<string>();
        var seriesCollectionHistory = new ObservableCollection<ISeries>();
        var xAxes = new ObservableCollection<Axis>();

        foreach (var marketHistory in historyItemPrices)
        {
            if (marketHistory == null)
            {
                continue;
            }

            var amount = new ObservableCollection<ObservablePoint>();
            var counter = 0;
            foreach (var data in marketHistory.Data?.OrderBy(x => x.Timestamp).ToList() ?? new List<MarketHistoryResponse>())
            {
                if (!date.Exists(x => x.Contains(data.Timestamp.ToString("g", CultureInfo.CurrentCulture))))
                {
                    date.Add(data.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                }

                amount.Add(new ObservablePoint(counter++, data.AveragePrice));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = WorldData.GetUniqueNameOrDefault(marketHistory.Location),
                Values = amount,
                Fill = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), true),
                Stroke = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), false),
                GeometryStroke = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), false),
                GeometryFill = Locations.GetLocationBrush(marketHistory.Location.GetMarketLocationByLocationNameOrId(), true),
                GeometrySize = 7,
                YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
            };

            seriesCollectionHistory.Add(lineSeries);
        }

        xAxes.Add(new Axis()
        {
            LabelsRotation = 15,
            Labels = date,
            Labeler = (value) => new DateTime((long) value).ToString("g", CultureInfo.CurrentCulture),
            UnitWidth = TimeSpan.FromDays(1).Ticks
        });

        HistoryBindings.XAxesHistory = xAxes.ToArray();
        HistoryBindings.SeriesHistory = seriesCollectionHistory;
    }

    #endregion

    #region Bindings

    public Item Item
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ulong PriceAlertThreshold => Item?.AlertModeMinSellPriceIsUndercutPrice ?? 0;

    public string PriceAlertThresholdText
    {
        get => PriceAlertThreshold == 0
            ? string.Empty
            : PriceAlertThreshold.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (Item == null)
            {
                return;
            }

            Item.AlertModeMinSellPriceIsUndercutPrice = ulong.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var priceThreshold)
                    ? priceThreshold
                    : 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PriceAlertThreshold));
        }
    }

    public uint PriceAlertMaximumPriceAgeMinutes => Item?.PriceAlertMaximumPriceAgeMinutes
        ?? AlertOptions.DefaultMaximumPriceAgeMinutes;

    public string PriceAlertMaximumPriceAgeMinutesText
    {
        get => PriceAlertMaximumPriceAgeMinutes == 0
            ? string.Empty
            : PriceAlertMaximumPriceAgeMinutes.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (Item == null)
            {
                return;
            }

            Item.PriceAlertMaximumPriceAgeMinutes = uint.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var maximumPriceAgeMinutes)
                    ? maximumPriceAgeMinutes
                    : 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PriceAlertMaximumPriceAgeMinutes));
        }
    }

    public bool IsPriceAlertActive => _alertController.IsPriceAlertActive(Item?.UniqueName);

    public bool IsPriceAlertInactive => !IsPriceAlertActive;

    public uint AvailabilityAlertMaximumPriceAgeMinutes => Item?.AvailabilityAlertMaximumPriceAgeMinutes
        ?? AlertOptions.DefaultMaximumPriceAgeMinutes;

    public string AvailabilityAlertMaximumPriceAgeMinutesText
    {
        get => AvailabilityAlertMaximumPriceAgeMinutes == 0
            ? string.Empty
            : AvailabilityAlertMaximumPriceAgeMinutes.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (Item == null)
            {
                return;
            }

            Item.AvailabilityAlertMaximumPriceAgeMinutes = uint.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var maximumPriceAgeMinutes)
                    ? maximumPriceAgeMinutes
                    : 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AvailabilityAlertMaximumPriceAgeMinutes));
        }
    }

    public bool IsAvailabilityAlertActive => _alertController.IsAvailabilityAlertActive(Item?.UniqueName);

    public bool IsAvailabilityAlertInactive => !IsAvailabilityAlertActive;

    public Visibility BlackMarketMonitoringVisibility => BlackMarketItemEligibility.IsEligible(Item)
        ? Visibility.Visible
        : Visibility.Collapsed;

    public ulong BlackMarketBuyOrderAlertThreshold => Item?.BlackMarketBuyOrderAlertThreshold ?? 0;

    public string BlackMarketBuyOrderAlertThresholdText
    {
        get => BlackMarketBuyOrderAlertThreshold == 0
            ? string.Empty
            : BlackMarketBuyOrderAlertThreshold.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (Item == null)
            {
                return;
            }

            Item.BlackMarketBuyOrderAlertThreshold = ulong.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var priceThreshold)
                    ? priceThreshold
                    : 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BlackMarketBuyOrderAlertThreshold));
        }
    }

    public uint BlackMarketAlertMaximumPriceAgeMinutes => Item?.BlackMarketAlertMaximumPriceAgeMinutes
        ?? AlertOptions.DefaultMaximumPriceAgeMinutes;

    public string BlackMarketAlertMaximumPriceAgeMinutesText
    {
        get => BlackMarketAlertMaximumPriceAgeMinutes == 0
            ? string.Empty
            : BlackMarketAlertMaximumPriceAgeMinutes.ToString(CultureInfo.InvariantCulture);
        set
        {
            if (Item == null)
            {
                return;
            }

            Item.BlackMarketAlertMaximumPriceAgeMinutes = uint.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var maximumPriceAgeMinutes)
                    ? maximumPriceAgeMinutes
                    : 0;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BlackMarketAlertMaximumPriceAgeMinutes));
        }
    }

    public bool IsBlackMarketBuyOrderAlertActive => _alertController
        .IsBlackMarketBuyOrderAlertActive(Item?.UniqueName);

    public bool IsBlackMarketBuyOrderAlertInactive => !IsBlackMarketBuyOrderAlertActive;

    public bool IsAlertSoundEnabled
    {
        get => Item?.IsAlertSoundEnabled ?? true;
        set
        {
            if (Item == null || Item.IsAlertSoundEnabled == value)
            {
                return;
            }

            Item.IsAlertSoundEnabled = value;
            _alertController.UpdateSoundPreference(Item, value);
            OnPropertyChanged();
        }
    }

    public string MonitoringErrorText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public Visibility MonitoringErrorVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public string TitleName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemTierLevel
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemTier
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemCategoryName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemTypeName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ItemUniqueName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public XmlLanguage ItemListViewLanguage
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = XmlLanguage.GetLanguage(CultureInfo.DefaultThreadCurrentCulture?.IetfLanguageTag ?? string.Empty);

    public ItemDetailsMainTabBindings MainTabBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ItemDetailsQualityTabBindings QualityTabBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ItemDetailsHistoryBindings HistoryBindings
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public SolidColorPaint HistoryLegendTextPaint { get; } = CreateChartTextPaint();

    public ItemDetailsRealMoneyTabBindings RealMoneyTabBindings
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
    }

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

    public List<MarketResponse> CurrentItemPrices
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

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

    public bool CanRefreshManually => !_isDisposed && !_isRefreshInProgress && !_isRefreshCooldownActive;

    public bool HasActiveRefreshCooldown => _isRefreshCooldownActive;

    public bool IsActive { get; private set; }

    public string LastUpdatedText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MainTabLocationFilterObject> LocationFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ExtraItemInformation ExtraItemInformation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ItemDetailsTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    #endregion

    #region Helper

    public ulong Sum(params ulong[] values)
    {
        return values.Aggregate(0UL, (current, t) => current + t);
    }

    public ulong Average(params ulong[] values)
    {
        if (values.Length == 0) return 0;

        var sum = Sum(values);
        var result = sum / (ulong) values.Length;
        return result;
    }

    #endregion
}
