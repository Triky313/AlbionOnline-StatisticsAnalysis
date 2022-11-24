using FontAwesome5;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using StatisticsAnalysisTool.Models.ItemWindowModel;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels
{
    public class ItemWindowViewModelOld : INotifyPropertyChanged
    {
        public enum Error
        {
            NoPrices,
            NoItemInfo,
            GeneralError,
            ToManyRequests
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly ItemWindowOld _itemWindow;
        private List<MarketQualityObject> _allQualityPricesList;
        private string _averagePrices;
        private List<MarketResponse> _currentCityPrices;
        private GoldResponseModel _currentGoldPrice;
        private string _errorBarText;
        private Visibility _errorBarVisibility;
        private bool _excellentQualityChecked;
        private bool _goodQualityChecked;
        private BitmapImage _icon;
        private Visibility _informationLoadingImageVisibility;
        private bool _isAutoUpdateActive;
        private Item _item;
        private XmlLanguage _itemListViewLanguage;
        private string _itemName;
        private string _itemTierLevel;
        private Axis[] _xAxesHistory;
        private EFontAwesomeIcon _loadingImageIcon;
        private bool _loadingImageSpin;
        private Visibility _loadingImageVisibility;
        private List<MarketCurrentPricesItem> _marketCurrentPricesItemList;
        private bool _masterpieceQualityChecked;
        private bool _normalQualityChecked;
        private bool _outstandingQualityChecked;
        private List<MarketQualityObject> _realMoneyPriceList;
        private string _refreshIconTooltipText;
        private bool _refreshSpin;
        private bool _runUpdate = true;
        private ObservableCollection<ISeries> _seriesHistory = new();
        private bool _showBlackZoneOutpostsChecked;
        private bool _showVillagesChecked;
        private ItemWindowTranslation _translation;
        private ObservableCollection<RequiredResource> _requiredResources = new();
        private RequiredJournal _requiredJournal;
        private Visibility _requiredJournalVisibility = Visibility.Collapsed;
        private Visibility _craftingTabVisibility = Visibility.Collapsed;
        private EssentialCraftingValuesTemplate _essentialCraftingValues;
        private ExtraItemInformation _extraItemInformation = new();
        private string _craftingNotes;
        private readonly Timer _timer = new();
        private double _refreshRateInMilliseconds = 10;
        private Visibility _craftingInfoPopupVisibility = Visibility.Hidden;

        private CraftingCalculation _craftingCalculation = new()
        {
            AuctionsHouseTax = 0.0d,
            CraftingTax = 0.0d,
            PossibleItemCrafting = 0.0d,
            SetupFee = 0.0d,
            TotalCosts = 0.0,
            TotalJournalCosts = 0.0d,
            TotalItemSells = 0.0d,
            TotalJournalSells = 0.0d,
            TotalResourceCosts = 0.0d,
            GrandTotal = 0.0d
        };

        public ItemWindowViewModelOld(ItemWindowOld itemWindow, Item item)
        {
            _itemWindow = itemWindow;
            InitializeItemWindow(item);
        }

        #region Init

        public void InitializeItemWindow(Item item)
        {
            ErrorBarVisibility = Visibility.Hidden;
            SetDefaultQualityIfNoOneChecked();

            Item = item;

            Translation = new ItemWindowTranslation();
            InitializeItemData(item);

            ItemListViewLanguage = XmlLanguage.GetLanguage(LanguageController.CurrentCultureInfo.ToString());
        }

        private async void InitializeItemData(Item item)
        {
            InformationLoadingImageVisibility = Visibility.Visible;

            Icon = null;
            ItemName = "-";
            ItemTierLevel = string.Empty;

            if (item == null)
            {
                SetErrorValues(Error.NoItemInfo);
                return;
            }

            ItemTierLevel = Item?.Tier != -1 && Item?.Level != -1 ? $"T{Item?.Tier}.{Item?.Level}" : string.Empty;
            InitExtraItemInformation();
            await InitCraftingTabAsync();

            await _itemWindow.Dispatcher.InvokeAsync(() =>
            {
                _itemWindow.Icon = null;
                _itemWindow.Title = "-";
            });

            if (Application.Current.Dispatcher == null)
            {
                SetErrorValues(Error.GeneralError);
                return;
            }

            var localizedName = ItemController.LocalizedName(Item?.LocalizedNames, null, Item?.UniqueName);

            Icon = item.Icon;
            ItemName = localizedName;

            await _itemWindow.Dispatcher.InvokeAsync(() =>
            {
                _itemWindow.Icon = item.Icon;
                _itemWindow.Title = $"{localizedName} (T{item.Tier})";
            });

            InitTimer();
            IsAutoUpdateActive = true;
            UpdateValues(null, null);

            InformationLoadingImageVisibility = Visibility.Hidden;
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
            }
        }

        #endregion

        #region Timer

        private void InitTimer()
        {
            _timer.Interval = SettingsController.CurrentSettings.RefreshRate;
            _timer.Elapsed += UpdateInterval;
            _timer.Elapsed += UpdateValues;
        }

        private void UpdateInterval(object sender, EventArgs e)
        {
            if (Math.Abs(_refreshRateInMilliseconds - SettingsController.CurrentSettings.RefreshRate) <= 0)
            {
                return;
            }

            _refreshRateInMilliseconds = SettingsController.CurrentSettings.RefreshRate;
            _timer.Interval = _refreshRateInMilliseconds;
        }

        private async void UpdateValues(object sender, EventArgs e)
        {
            if (Item.UniqueName != null)
            {
                await GetCityItemPricesAsync();
                await GetItemPricesInRealMoneyAsync();
            }

            GetMainPriceStats();
            SetQualityPriceStatsOnListView();
        }

        #endregion

        #region Crafting tab

        private async Task InitCraftingTabAsync()
        {
            var areResourcesAvailable = false;

            switch (Item?.FullItemInformation)
            {
                case Weapon weapon when weapon.CraftingRequirements?.FirstOrDefault()?.CraftResource?.Count > 0:
                case EquipmentItem equipmentItem when equipmentItem.CraftingRequirements?.FirstOrDefault()?.CraftResource?.Count > 0:
                case Mount mount when mount.CraftingRequirements?.FirstOrDefault()?.CraftResource?.Count > 0:
                case ConsumableItem consumableItem when consumableItem.CraftingRequirements?.FirstOrDefault()?.CraftResource?.Count > 0:
                    areResourcesAvailable = true;
                    break;
            }

            if (areResourcesAvailable)
            {
                CraftingTabVisibility = Visibility.Visible;

                EssentialCraftingValues = new EssentialCraftingValuesTemplate(this, CurrentCityPrices, Item.UniqueName);
                SetJournalInfo();
                await SetRequiredResourcesAsync();
                CraftingNotes = CraftingTabController.GetNote(Item.UniqueName);
            }
        }

        private void SetJournalInfo()
        {
            var craftingJournalType = Item?.FullItemInformation switch
            {
                Weapon weapon => CraftingController.GetCraftingJournalItem(Item.Tier, weapon.CraftingJournalType),
                EquipmentItem equipmentItem => CraftingController.GetCraftingJournalItem(Item.Tier, equipmentItem.CraftingJournalType),
                _ => null
            };

            if (craftingJournalType == null)
            {
                return;
            }

            RequiredJournalVisibility = Visibility.Visible;

            var fullItemInformation = ItemController.GetItemByUniqueName(ItemController.GetGeneralJournalName(craftingJournalType.UniqueName))?.FullItemInformation;

            RequiredJournal = new RequiredJournal(this)
            {
                UniqueName = craftingJournalType.UniqueName,
                CostsPerJournal = 0,
                CraftingResourceName = craftingJournalType.LocalizedName,
                Icon = craftingJournalType.Icon,
                Weight = ItemController.GetWeight(fullItemInformation),
                RequiredJournalAmount = CraftingController.GetRequiredJournalAmount(Item, CraftingCalculation.PossibleItemCrafting),
                SellPricePerJournal = 0
            };
        }

        private async Task SetRequiredResourcesAsync()
        {
            var currentItemEnchantmentLevel = Item.Level;
            List<CraftingRequirements> craftingRequirements = null;

            var enchantments = Item?.FullItemInformation switch
            {
                EquipmentItem equipmentItem => equipmentItem.Enchantments,
                ConsumableItem consumableItem => consumableItem.Enchantments,
                _ => null
            };

            var enchantment = enchantments?.Enchantment?.FirstOrDefault(x => x.EnchantmentLevelInteger == currentItemEnchantmentLevel);

            if (enchantment != null)
            {
                craftingRequirements = enchantment.CraftingRequirements;
            }

            if (craftingRequirements == null)
            {
                craftingRequirements = Item?.FullItemInformation switch
                {
                    Weapon weapon => weapon.CraftingRequirements,
                    EquipmentItem equipmentItem => equipmentItem.CraftingRequirements,
                    Mount mount => mount.CraftingRequirements,
                    ConsumableItem consumableItem => consumableItem.CraftingRequirements,
                    _ => null
                };
            }

            if (craftingRequirements?.FirstOrDefault()?.CraftResource == null)
            {
                return;
            }

            if (int.TryParse(craftingRequirements.FirstOrDefault()?.AmountCrafted, out var amountCrafted))
            {
                EssentialCraftingValues.AmountCrafted = amountCrafted;
            }

            await foreach (var craftResource in craftingRequirements
                               .SelectMany(x => x.CraftResource).ToList().GroupBy(x => x.UniqueName).Select(grp => grp.FirstOrDefault()).ToAsyncEnumerable().ConfigureAwait(false))
            {
                var item = GetSuitableResourceItem(craftResource.UniqueName);
                var craftingQuantity = (long)Math.Round(item?.UniqueName?.ToUpper().Contains("ARTEFACT") ?? false
                    ? CraftingCalculation.PossibleItemCrafting
                    : EssentialCraftingValues.CraftingItemQuantity, MidpointRounding.ToPositiveInfinity);

                RequiredResources.Add(new RequiredResource(this)
                {
                    CraftingResourceName = item?.LocalizedName,
                    UniqueName = item?.UniqueName,
                    OneProductionAmount = craftResource.Count,
                    Icon = item?.Icon,
                    ResourceCost = 0,
                    Weight = ItemController.GetWeight(item?.FullItemInformation),
                    CraftingQuantity = craftingQuantity,
                    IsArtifactResource = item?.UniqueName?.ToUpper().Contains("ARTEFACT") ?? false,
                    IsTomeOfInsightResource = item?.UniqueName?.ToUpper().Contains("SKILLBOOK_STANDARD") ?? false
                });
            }
        }

        private Item GetSuitableResourceItem(string uniqueName)
        {
            var suitableUniqueName = $"{uniqueName}_LEVEL{Item.Level}@{Item.Level}";
            return ItemController.GetItemByUniqueName(suitableUniqueName) ?? ItemController.GetItemByUniqueName(uniqueName);
        }

        public void UpdateCraftingCalculationTab()
        {
            if (EssentialCraftingValues == null || CraftingCalculation == null)
            {
                return;
            }

            // PossibleItem crafting
            var possibleItemCrafting = EssentialCraftingValues.CraftingItemQuantity / 100d * EssentialCraftingValues.CraftingBonus * ((EssentialCraftingValues.IsCraftingWithFocus)
                ? ((23.1d / 100d) + 1d) : 1d);

            // Crafting quantity
            if (RequiredResources?.Count > 0)
            {
                foreach (var requiredResource in RequiredResources.ToList())
                {
                    if (requiredResource.IsArtifactResource || requiredResource.IsTomeOfInsightResource)
                    {
                        requiredResource.CraftingQuantity = (long) Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity);
                        continue;
                    }
                    
                    requiredResource.CraftingQuantity = EssentialCraftingValues.CraftingItemQuantity;
                }
            }

            CraftingCalculation.PossibleItemCrafting = Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity);

            // Crafting (Usage) tax
            CraftingCalculation.CraftingTax = CraftingController.GetCraftingTax(EssentialCraftingValues.UsageFeePerHundredFood, Item, CraftingCalculation.PossibleItemCrafting);

            // Setup fee
            CraftingCalculation.SetupFee = CraftingController.GetSetupFeeCalculation(EssentialCraftingValues.CraftingItemQuantity, EssentialCraftingValues.SetupFee, EssentialCraftingValues.SellPricePerItem);

            // Auctions house tax
            CraftingCalculation.AuctionsHouseTax =
                EssentialCraftingValues.SellPricePerItem * Convert.ToInt64(EssentialCraftingValues.CraftingItemQuantity) / 100 * Convert.ToInt64(EssentialCraftingValues.AuctionHouseTax);

            // Total resource costs
            CraftingCalculation.TotalResourceCosts = RequiredResources?.Sum(x => x.TotalCost) ?? 0;

            // Other costs
            CraftingCalculation.OtherCosts = EssentialCraftingValues.OtherCosts;

            // Total item sells
            CraftingCalculation.TotalItemSells = EssentialCraftingValues.SellPricePerItem * (CraftingCalculation.PossibleItemCrafting * EssentialCraftingValues.AmountCrafted);

            if (RequiredJournal != null)
            {
                // Required journal amount
                RequiredJournal.RequiredJournalAmount = CraftingController.GetRequiredJournalAmount(Item, Math.Round(possibleItemCrafting, MidpointRounding.ToNegativeInfinity));

                // Total journal costs
                CraftingCalculation.TotalJournalCosts = RequiredJournal.CostsPerJournal * RequiredJournal.RequiredJournalAmount;

                // Total journal sells
                CraftingCalculation.TotalJournalSells = RequiredJournal.RequiredJournalAmount * RequiredJournal.SellPricePerJournal;
            }

            // Amount crafted
            CraftingCalculation.AmountCrafted = EssentialCraftingValues.AmountCrafted;

            // Weight
            var requiredResourcesWeights = RequiredResources?.Sum(x => x.TotalWeight) ?? 0;
            var possibleItemCraftingWeights = CraftingCalculation?.PossibleItemCrafting * ItemController.GetWeight(Item?.FullItemInformation) ?? 0;

            if (CraftingCalculation != null)
            {
                CraftingCalculation.TotalResourcesWeight = requiredResourcesWeights;
                CraftingCalculation.TotalRequiredJournalWeight = RequiredJournal?.TotalWeight ?? 0;
                CraftingCalculation.TotalUnfinishedCraftingWeight = CraftingCalculation.TotalResourcesWeight + CraftingCalculation.TotalRequiredJournalWeight;

                CraftingCalculation.TotalCraftedItemWeight = possibleItemCraftingWeights;
                CraftingCalculation.TotalFinishedCraftingWeight = CraftingCalculation.TotalCraftedItemWeight;
            }
        }

        #endregion Crafting tab

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
                    SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_NO_ITEM_INFO"));
                    return;

                case Error.NoPrices:
                    SetLoadingImageToError();
                    SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED"));
                    return;

                case Error.GeneralError:
                    SetLoadingImageToError();
                    SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_GENERAL_ERROR"));
                    return;

                case Error.ToManyRequests:
                    SetLoadingImageToError();
                    SetErrorBar(Visibility.Visible, LanguageController.Translation("TOO_MANY_REQUESTS_CLOSE_WINDOWS_OR_WAIT"));
                    return;

                default:
                    SetLoadingImageToError();
                    SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_GENERAL_ERROR"));
                    return;
            }
        }

        private void ErrorBarReset()
        {
            LoadingImageSpin = true;
            SetErrorBar(Visibility.Hidden, string.Empty);
        }

        private void SetLoadingImageToError()
        {
            LoadingImageIcon = EFontAwesomeIcon.Solid_Times;
            LoadingImageSpin = false;
        }

        private void SetErrorBar(Visibility visibility, string errorMessage)
        {
            ErrorBarText = errorMessage;
            ErrorBarVisibility = visibility;
        }

        #endregion

        #region History

        public async Task SetHistoryChartPricesAsync()
        {
            List<MarketHistoriesResponse> historyItemPrices;

            try
            {
                var locations = Locations.GetLocationsListByArea(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true, true, true);
                historyItemPrices = await ApiController.GetHistoryItemPricesFromJsonAsync(Item.UniqueName, locations, DateTime.Now.AddDays(-30), GetQualities()).ConfigureAwait(true);

                if (historyItemPrices == null)
                {
                    return;
                }
            }
            catch (TooManyRequestsException)
            {
                ConsoleManager.WriteLineForWarning(MethodBase.GetCurrentMethod()?.DeclaringType, new TooManyRequestsException());
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
                var amount = new ObservableCollection<ObservablePoint>();

                var counter = 0;
                foreach (var data in marketHistory?.Data?.OrderBy(x => x.Timestamp).ToList() ?? new List<MarketHistoryResponse>())
                {
                    if (!date.Exists(x => x.Contains(data.Timestamp.ToString("g", CultureInfo.CurrentCulture))))
                    {
                        date.Add(data.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                    }

                    amount.Add(new ObservablePoint(counter++, data.AveragePrice));
                }

                var lineSeries = new LineSeries<ObservablePoint>
                {
                    Name = WorldData.GetUniqueNameOrDefault(marketHistory?.Location),
                    Values = amount,
                    Fill = Locations.GetLocationBrush(Locations.GetLocationByLocationNameOrId(marketHistory?.Location), true),
                    Stroke = Locations.GetLocationBrush(Locations.GetLocationByLocationNameOrId(marketHistory?.Location), false),
                    GeometryStroke = Locations.GetLocationBrush(Locations.GetLocationByLocationNameOrId(marketHistory?.Location), false),
                    GeometryFill = Locations.GetLocationBrush(Locations.GetLocationByLocationNameOrId(marketHistory?.Location), true),
                    GeometrySize = 5,
                    TooltipLabelFormatter = p => $"{p.Context.Series.Name}: {p.PrimaryValue:N0}"
                };

                seriesCollectionHistory.Add(lineSeries);
            }

            xAxes.Add(new Axis()
            {
                LabelsRotation = 15,
                Labels = date,
                Labeler = value => new DateTime((long)value).ToString(CultureInfo.CurrentCulture),
                UnitWidth = TimeSpan.FromDays(1).Ticks
            });

            XAxesHistory = xAxes.ToArray();
            SeriesHistory = seriesCollectionHistory;
        }

        #endregion

        #region Prices

        public async Task GetCityItemPricesAsync()
        {
            try
            {
                CurrentCityPrices = await ApiController.GetCityItemPricesFromJsonAsync(Item?.UniqueName).ConfigureAwait(false);
                ErrorBarReset();
            }
            catch (TooManyRequestsException e)
            {
                CurrentCityPrices = null;
                SetErrorValues(Error.ToManyRequests);
                Log.Warn(nameof(GetCityItemPricesAsync), e);
            }
        }

        public async Task GetItemPricesInRealMoneyAsync()
        {
            if (CurrentCityPrices == null)
            {
                return;
            }

            var realMoneyMarketObject = new List<MarketQualityObject>();

            var filteredCityPrices = GetFilteredCityPrices(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true, true, true);
            foreach (var stat in filteredCityPrices)
            {
                if (realMoneyMarketObject.Exists(x => x.Location == stat.City))
                {
                    var marketQualityObject = realMoneyMarketObject.Find(x => x.LocationName == stat.City);
                    await SetRealMoneyQualityStat(stat, marketQualityObject);
                }
                else
                {
                    var marketQualityObject = new MarketQualityObject { Location = stat.City };
                    await SetRealMoneyQualityStat(stat, marketQualityObject);
                    realMoneyMarketObject.Add(marketQualityObject);
                }
            }
            RealMoneyPriceList = realMoneyMarketObject;
        }

        public void SetQualityPriceStatsOnListView()
        {
            if (CurrentCityPrices == null)
            {
                return;
            }

            var filteredCityPrices = GetFilteredCityPrices(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true, true, true);
            var marketQualityObjectList = new List<MarketQualityObject>();

            foreach (var stat in filteredCityPrices)
            {
                if (marketQualityObjectList.Exists(x => x.Location == stat.City))
                {
                    var marketQualityObject = marketQualityObjectList.Find(x => x.LocationName == stat.City);
                    SetQualityStat(stat, ref marketQualityObject);
                }
                else
                {
                    var marketQualityObject = new MarketQualityObject { Location = stat.City };
                    SetQualityStat(stat, ref marketQualityObject);
                    marketQualityObjectList.Add(marketQualityObject);
                }
            }

            AllQualityPricesList = marketQualityObjectList;
        }

        private async Task SetRealMoneyQualityStat(MarketResponse marketResponse, MarketQualityObject marketQualityObject)
        {
            if (marketQualityObject == null)
                return;

            if (_currentGoldPrice == null)
            {
                var getGoldPricesObjectList = await ApiController.GetGoldPricesFromJsonAsync(null, 1);
                _currentGoldPrice = getGoldPricesObjectList?.FirstOrDefault();
            }

            switch (ItemController.GetQuality(marketResponse.QualityLevel))
            {
                case ItemQuality.Normal:
                    marketQualityObject.SellPriceMinNormalStringInRalMoney =
                        Converter.GoldToDollar(marketResponse.SellPriceMin, _currentGoldPrice?.Price ?? 0);
                    marketQualityObject.SellPriceMinNormalDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Good:
                    marketQualityObject.SellPriceMinGoodStringInRalMoney =
                        Converter.GoldToDollar(marketResponse.SellPriceMin, _currentGoldPrice?.Price ?? 0);
                    marketQualityObject.SellPriceMinGoodDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Outstanding:
                    marketQualityObject.SellPriceMinOutstandingStringInRalMoney =
                        Converter.GoldToDollar(marketResponse.SellPriceMin, _currentGoldPrice?.Price ?? 0);
                    marketQualityObject.SellPriceMinOutstandingDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Excellent:
                    marketQualityObject.SellPriceMinExcellentStringInRalMoney =
                        Converter.GoldToDollar(marketResponse.SellPriceMin, _currentGoldPrice?.Price ?? 0);
                    marketQualityObject.SellPriceMinExcellentDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Masterpiece:
                    marketQualityObject.SellPriceMinMasterpieceStringInRalMoney =
                        Converter.GoldToDollar(marketResponse.SellPriceMin, _currentGoldPrice?.Price ?? 0);
                    marketQualityObject.SellPriceMinMasterpieceDate = marketResponse.SellPriceMinDate;
                    return;
            }
        }

        private static void SetQualityStat(MarketResponse marketResponse, ref MarketQualityObject marketQualityObject)
        {
            if (marketQualityObject == null)
                return;

            switch (ItemController.GetQuality(marketResponse.QualityLevel))
            {
                case ItemQuality.Normal:
                    marketQualityObject.SellPriceMinNormal = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinNormalDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Good:
                    marketQualityObject.SellPriceMinGood = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinGoodDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Outstanding:
                    marketQualityObject.SellPriceMinOutstanding = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinOutstandingDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Excellent:
                    marketQualityObject.SellPriceMinExcellent = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinExcellentDate = marketResponse.SellPriceMinDate;
                    return;

                case ItemQuality.Masterpiece:
                    marketQualityObject.SellPriceMinMasterpiece = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinMasterpieceDate = marketResponse.SellPriceMinDate;
                    return;
            }
        }

        private List<MarketResponse> GetFilteredCityPrices(bool blackZoneOutposts, bool villages, bool cities, bool blackMarket, bool getAllQualities = false)
        {
            return CurrentCityPrices?.Where(x =>
                Locations.GetLocationsListByArea(blackZoneOutposts, villages, cities, blackMarket, true).Contains(x.CityEnum)
                && (GetQualities().Contains(x.QualityLevel) || getAllQualities)).ToList();
        }

        public void GetMainPriceStats()
        {
            if (CurrentCityPrices is not { Count: > 0 })
            {
                return;
            }

            var filteredCityPrices = GetFilteredCityPrices(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true, true);
            var statsPricesTotalList = PriceUpdate(filteredCityPrices);

            FindBestPrice(ref statsPricesTotalList);

            var marketCurrentPricesItemList = statsPricesTotalList.Select(item => new MarketCurrentPricesItem(item)).ToList();

            if (LoadingImageVisibility != Visibility.Hidden)
            {
                LoadingImageVisibility = Visibility.Hidden;
            }

            MarketCurrentPricesItemList = marketCurrentPricesItemList;
            SetAveragePricesString();

            RefreshIconTooltipText = $"{LanguageController.Translation("LAST_UPDATE")}: {DateTime.Now.CurrentDateTimeFormat()}";
        }

        private static List<MarketResponseTotal> PriceUpdate(List<MarketResponse> newStatsPricesList)
        {
            var currentStatsPricesTotalList = new List<MarketResponseTotal>();

            foreach (var newStats in newStatsPricesList ?? new List<MarketResponse>())
                try
                {
                    if (currentStatsPricesTotalList.Exists(s => Locations.GetParameterName(s.City) == newStats.City))
                    {
                        var curStats = currentStatsPricesTotalList.Find(s => WorldData.GetUniqueNameOrDefault((int)s.City) == newStats.City);

                        if (newStats?.SellPriceMinDate < curStats?.SellPriceMinDate)
                        {
                            curStats.SellPriceMin = newStats.SellPriceMin;
                            curStats.SellPriceMinDate = newStats.SellPriceMinDate;
                        }

                        if (newStats?.SellPriceMaxDate < curStats?.SellPriceMaxDate)
                        {
                            curStats.SellPriceMax = newStats.SellPriceMax;
                            curStats.SellPriceMaxDate = newStats.SellPriceMaxDate;
                        }

                        if (newStats?.BuyPriceMinDate < curStats?.BuyPriceMinDate)
                        {
                            curStats.BuyPriceMin = newStats.BuyPriceMin;
                            curStats.BuyPriceMinDate = newStats.BuyPriceMinDate;
                        }

                        if (newStats?.BuyPriceMaxDate < curStats?.BuyPriceMaxDate)
                        {
                            curStats.BuyPriceMax = newStats.BuyPriceMax;
                            curStats.BuyPriceMaxDate = newStats.BuyPriceMaxDate;
                        }
                    }
                    else
                    {
                        currentStatsPricesTotalList.Add(new MarketResponseTotal(newStats));
                    }
                }
                catch
                {
                    return currentStatsPricesTotalList;
                }

            return currentStatsPricesTotalList;
        }

        private void FindBestPrice(ref List<MarketResponseTotal> list)
        {
            if (list.Count == 0)
                return;

            var max = GetMaxPrice(list);

            try
            {
                if (list.Exists(s => s.BuyPriceMax == max))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    list.Find(s => s?.BuyPriceMax == max).BestBuyMaxPrice = true;
                }
            }
            catch
            {
                // ignored
            }

            var min = GetMinPrice(list);

            try
            {
                if (list.Exists(s => s.SellPriceMin == min)) list.First(s => s.SellPriceMin == min).BestSellMinPrice = true;
            }
            catch
            {
                // ignored
            }
        }

        private static ulong GetMaxPrice(List<MarketResponseTotal> list)
        {
            var max = ulong.MinValue;
            foreach (var type in list)
            {
                if (type.BuyPriceMax == 0)
                    continue;

                if (type.BuyPriceMax > max)
                    max = type.BuyPriceMax;
            }

            return max;
        }

        private static ulong GetMinPrice(List<MarketResponseTotal> list)
        {
            var min = ulong.MaxValue;
            foreach (var type in list)
            {
                if (type.SellPriceMin == 0)
                    continue;

                if (type.SellPriceMin < min)
                    min = type.SellPriceMin;
            }

            return min;
        }

        private void SetAveragePricesString()
        {
            var cityPrices = GetFilteredCityPrices(false, false, true, false);

            var sellPriceMin = new List<ulong>();
            var sellPriceMax = new List<ulong>();
            var buyPriceMin = new List<ulong>();
            var buyPriceMax = new List<ulong>();

            foreach (var price in cityPrices ?? new List<MarketResponse>())
            {
                if (price.SellPriceMin != 0) sellPriceMin.Add(price.SellPriceMin);

                if (price.SellPriceMax != 0) sellPriceMax.Add(price.SellPriceMax);

                if (price.BuyPriceMin != 0) buyPriceMin.Add(price.BuyPriceMin);

                if (price.BuyPriceMax != 0) buyPriceMax.Add(price.BuyPriceMax);
            }

            var sellPriceMinAverage = Average(sellPriceMin.ToArray());
            var sellPriceMaxAverage = Average(sellPriceMax.ToArray());
            var buyPriceMinAverage = Average(buyPriceMin.ToArray());
            var buyPriceMaxAverage = Average(buyPriceMax.ToArray());

            AveragePrices = $"{string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", sellPriceMinAverage)}  |  " +
                            $"{string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", sellPriceMaxAverage)}  |  " +
                            $"{string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", buyPriceMinAverage)}  |  " +
                            $"{string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", buyPriceMaxAverage)}";
        }

        #endregion Prices

        #region Bindings

        public Item Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }

        public string ItemTierLevel
        {
            get => _itemTierLevel;
            set
            {
                _itemTierLevel = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public List<MarketResponse> CurrentCityPrices
        {
            get => _currentCityPrices;
            set
            {
                _currentCityPrices = value;
                if (EssentialCraftingValues != null)
                {
                    EssentialCraftingValues.CurrentCityPrices = value;
                }

                OnPropertyChanged();
            }
        }

        public string AveragePrices
        {
            get => _averagePrices;
            set
            {
                _averagePrices = value;
                OnPropertyChanged();
            }
        }

        public Visibility LoadingImageVisibility
        {
            get => _loadingImageVisibility;
            set
            {
                _loadingImageVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility InformationLoadingImageVisibility
        {
            get => _informationLoadingImageVisibility;
            set
            {
                _informationLoadingImageVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CraftingInfoPopupVisibility
        {
            get => _craftingInfoPopupVisibility;
            set
            {
                _craftingInfoPopupVisibility = value;
                OnPropertyChanged();
            }
        }

        public EFontAwesomeIcon LoadingImageIcon
        {
            get => _loadingImageIcon;
            set
            {
                _loadingImageIcon = value;
                OnPropertyChanged();
            }
        }

        public bool LoadingImageSpin
        {
            get => _loadingImageSpin;
            set
            {
                _loadingImageSpin = value;
                OnPropertyChanged();
            }
        }

        public List<MarketCurrentPricesItem> MarketCurrentPricesItemList
        {
            get => _marketCurrentPricesItemList;
            set
            {
                _marketCurrentPricesItemList = value;
                OnPropertyChanged();
            }
        }

        public XmlLanguage ItemListViewLanguage
        {
            get => _itemListViewLanguage;
            set
            {
                _itemListViewLanguage = value;
                OnPropertyChanged();
            }
        }

        public bool RunUpdate
        {
            get => _runUpdate;
            set
            {
                _runUpdate = value;
                OnPropertyChanged();
            }
        }

        public string ErrorBarText
        {
            get => _errorBarText;
            set
            {
                _errorBarText = value;
                OnPropertyChanged();
            }
        }

        public Visibility ErrorBarVisibility
        {
            get => _errorBarVisibility;
            set
            {
                _errorBarVisibility = value;
                OnPropertyChanged();
            }
        }

        public bool NormalQualityChecked
        {
            get => _normalQualityChecked;
            set
            {
                _normalQualityChecked = !_normalQualityChecked && value;
                SetQualityPriceStatsOnListView();
                _ = GetCityItemPricesAsync();
                _ = GetItemPricesInRealMoneyAsync();
                OnPropertyChanged();
            }
        }

        public bool GoodQualityChecked
        {
            get => _goodQualityChecked;
            set
            {
                _goodQualityChecked = !_goodQualityChecked && value;
                SetQualityPriceStatsOnListView();
                _ = GetCityItemPricesAsync();
                _ = GetItemPricesInRealMoneyAsync();
                OnPropertyChanged();
            }
        }

        public bool OutstandingQualityChecked
        {
            get => _outstandingQualityChecked;
            set
            {
                _outstandingQualityChecked = !_outstandingQualityChecked && value;
                SetQualityPriceStatsOnListView();
                _ = GetCityItemPricesAsync();
                _ = GetItemPricesInRealMoneyAsync();
                OnPropertyChanged();
            }
        }

        public bool ExcellentQualityChecked
        {
            get => _excellentQualityChecked;
            set
            {
                _excellentQualityChecked = !_excellentQualityChecked && value;
                SetQualityPriceStatsOnListView();
                _ = GetCityItemPricesAsync();
                _ = GetItemPricesInRealMoneyAsync();
                OnPropertyChanged();
            }
        }

        public bool MasterpieceQualityChecked
        {
            get => _masterpieceQualityChecked;
            set
            {
                _masterpieceQualityChecked = !_masterpieceQualityChecked && value;
                SetQualityPriceStatsOnListView();
                _ = GetCityItemPricesAsync();
                _ = GetItemPricesInRealMoneyAsync();
                OnPropertyChanged();
            }
        }

        public bool ShowBlackZoneOutpostsChecked
        {
            get => _showBlackZoneOutpostsChecked;
            set
            {
                _showBlackZoneOutpostsChecked = value;
                OnPropertyChanged();
            }
        }

        public bool ShowVillagesChecked
        {
            get => _showVillagesChecked;
            set
            {
                _showVillagesChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsAutoUpdateActive
        {
            get => _isAutoUpdateActive;
            set
            {
                _isAutoUpdateActive = value;

                _timer.Enabled = _isAutoUpdateActive;
                RefreshSpin = IsAutoUpdateActive;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ISeries> SeriesHistory
        {
            get => _seriesHistory;
            set
            {
                _seriesHistory = value;
                OnPropertyChanged();
            }
        }

        public Axis[] XAxesHistory
        {
            get => _xAxesHistory;
            set
            {
                _xAxesHistory = value;
                OnPropertyChanged();
            }
        }

        public bool RefreshSpin
        {
            get => _refreshSpin;
            set
            {
                _refreshSpin = value;
                OnPropertyChanged();
            }
        }

        public string RefreshIconTooltipText
        {
            get => _refreshIconTooltipText;
            set
            {
                _refreshIconTooltipText = value;
                OnPropertyChanged();
            }
        }

        public List<MarketQualityObject> AllQualityPricesList
        {
            get => _allQualityPricesList;
            set
            {
                _allQualityPricesList = value;
                OnPropertyChanged();
            }
        }

        public List<MarketQualityObject> RealMoneyPriceList
        {
            get => _realMoneyPriceList;
            set
            {
                _realMoneyPriceList = value;
                OnPropertyChanged();
            }
        }

        public RequiredJournal RequiredJournal
        {
            get => _requiredJournal;
            set
            {
                _requiredJournal = value;
                OnPropertyChanged();
            }
        }

        public CraftingCalculation CraftingCalculation
        {
            get => _craftingCalculation;
            set
            {
                _craftingCalculation = value;
                OnPropertyChanged();
            }
        }

        public EssentialCraftingValuesTemplate EssentialCraftingValues
        {
            get => _essentialCraftingValues;
            set
            {
                _essentialCraftingValues = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RequiredResource> RequiredResources
        {
            get => _requiredResources;
            set
            {
                _requiredResources = value;
                OnPropertyChanged();
            }
        }

        public Visibility RequiredJournalVisibility
        {
            get => _requiredJournalVisibility;
            set
            {
                _requiredJournalVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility CraftingTabVisibility
        {
            get => _craftingTabVisibility;
            set
            {
                _craftingTabVisibility = value;
                OnPropertyChanged();
            }
        }

        public string CraftingNotes
        {
            get => _craftingNotes;
            set
            {
                _craftingNotes = value;
                OnPropertyChanged();
            }
        }

        public ExtraItemInformation ExtraItemInformation
        {
            get => _extraItemInformation;
            set
            {
                _extraItemInformation = value;
                OnPropertyChanged();
            }
        }

        public ItemWindowTranslation Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Bindings

        #region Helper

        public ulong Sum(params ulong[] values)
        {
            return values.Aggregate(0UL, (current, t) => current + t);
        }

        public ulong Average(params ulong[] values)
        {
            if (values.Length == 0) return 0;

            var sum = Sum(values);
            var result = sum / (ulong)values.Length;
            return result;
        }

        private List<int> GetQualities()
        {
            var qualities = new List<int>();

            if (NormalQualityChecked)
            {
                qualities.Add(1);
            }

            if (GoodQualityChecked)
            {
                qualities.Add(2);
            }

            if (OutstandingQualityChecked)
            {
                qualities.Add(3);
            }

            if (ExcellentQualityChecked)
            {
                qualities.Add(4);
            }

            if (MasterpieceQualityChecked)
            {
                qualities.Add(5);
            }

            return qualities;
        }

        private void SetDefaultQualityIfNoOneChecked()
        {
            if (!NormalQualityChecked && !GoodQualityChecked && !OutstandingQualityChecked && !ExcellentQualityChecked && !MasterpieceQualityChecked)
            {
                NormalQualityChecked = true;
            }
        }

        #endregion
    }
}