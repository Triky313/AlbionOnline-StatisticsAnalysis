using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels
{
    using LiveCharts;
    using LiveCharts.Wpf;
    using System.Globalization;

    public class ItemWindowViewModel: INotifyPropertyChanged
    {
        private readonly ItemWindow _mainWindow;
        private Item _item;
        private ItemInformation _itemInformation;
        private bool _runUpdate = true;
        private Visibility _errorBarVisibility;
        private string _errorBarText;
        private BitmapImage _icon;
        private string _itemName;
        private bool _masterpieceQualityChecked;
        private bool _excellentQualityChecked;
        private bool _outstandingQualityChecked;
        private bool _goodQualityChecked;
        private bool _normalQualityChecked;
        private ItemWindowTranslation _translation;
        private bool _hasItemPrices;
        private bool _showBlackZoneOutpostsChecked;
        private bool _showVillagesChecked;
        private SeriesCollection _seriesCollectionHistory;
        private string[] _labelsHistory;
        private bool _refreshSpin;
        private bool _isAutoUpdateActive;
        private string _refreshIconTooltipText;
        private List<MarketQualityObject> _allQualityPricesList;
        private string _itemTier;

        public enum Error { NoPrices, NoItemInfo, GeneralError }

        public ItemWindowViewModel(ItemWindow mainWindow, Item item)
        {
            _mainWindow = mainWindow;
            InitializeItemWindow(item);
        }

        public void InitializeItemWindow(Item item)
        {
            ItemInformation = null;
            ErrorBarVisibility = Visibility.Hidden;

            Item = item;
            
            IsAutoUpdateActive = true;

            Translation = new ItemWindowTranslation();
            InitializeItemData(item);

            _mainWindow.ListViewPrices.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LanguageController.CurrentCultureInfo.ToString());
        }
        
        private async void InitializeItemData(Item item)
        {
            var getFullItemInformationTask = ItemController.GetFullItemInformation(item); ;

            await _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                _mainWindow.Icon = null;
                _mainWindow.Title = "-";
            });

            Icon = null;
            ItemName = "-";
            ItemTier = "";

            if (item == null)
            {
                SetNoDataValues(Error.NoItemInfo);
                return;
            }

            if (_mainWindow.Dispatcher == null)
            {
                SetNoDataValues(Error.GeneralError);
                return;
            }

            var localizedName = ItemController.LocalizedName(Item.LocalizedNames, null, Item.UniqueName);

            Icon = item.Icon;
            ItemName = localizedName;
            await _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                _mainWindow.Icon = item.Icon;
            });

            ItemInformation = await getFullItemInformationTask;

            ItemTier = $"(T{ItemInformation?.Tier})";
            await _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                _mainWindow.Title = $"{localizedName} (T{ItemInformation?.Tier})";
            });

            StartAutoUpdater();
            RefreshSpin = IsAutoUpdateActive;
        }
        
        private void SetNoDataValues(Error error)
        {
            switch (error)
            {
                case Error.NoItemInfo:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        Icon = new BitmapImage(new Uri(@"pack://application:,,,/"
                                                       + Assembly.GetExecutingAssembly().GetName().Name + ";component/"
                                                       + "Resources/Trash.png", UriKind.Absolute));
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_NO_ITEM_INFO"));
                    });
                    return;
                case Error.NoPrices:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        HasItemPrices = false;
                        SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED"));
                    });
                    return;
                case Error.GeneralError:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        HasItemPrices = false;
                        SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_GENERAL_ERROR"));
                    });
                    return;
                default:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        HasItemPrices = false;
                        SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_GENERAL_ERROR"));
                    });
            return;
            }
        }

        private void SetErrorBar(Visibility visibility, string errorMessage)
        {
            ErrorBarText = errorMessage;
            ErrorBarVisibility = visibility;
        }
        
        private async void StartAutoUpdater()
        {
            await Task.Run(async () => {
                while (RunUpdate)
                {
                    await Task.Delay(50);
                    if (_mainWindow.Dispatcher != null && !IsAutoUpdateActive)
                        continue;
                    
                    GetPriceStats();
                    SetQualityPriceStatsOnListView();
                    await Task.Delay(Settings.Default.RefreshRate - 500);
                }
            });
        }

        public async void SetQualityPriceStatsOnListView()
        {
            var statPricesList = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName,
                Locations.GetLocationsListByArea(new IsLocationAreaActive(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true)),new List<int>());

            if (statPricesList == null)
                return;

            var marketQualityObjectList = new List<MarketQualityObject>();
            
            foreach (var stat in statPricesList)
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

        private static void SetQualityStat(MarketResponse marketResponse, ref MarketQualityObject marketQualityObject)
        {
            if (marketQualityObject == null)
                return;

            switch (ItemController.GetQuality(marketResponse.QualityLevel))
            {
                case FrequentlyValues.ItemQuality.Normal:
                    marketQualityObject.SellPriceMinNormal = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinNormalDate = marketResponse.SellPriceMinDate;
                    return;
                case FrequentlyValues.ItemQuality.Good:
                    marketQualityObject.SellPriceMinGood = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinGoodDate = marketResponse.SellPriceMinDate;
                    return;
                case FrequentlyValues.ItemQuality.Outstanding:
                    marketQualityObject.SellPriceMinOutstanding = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinOutstandingDate = marketResponse.SellPriceMinDate;
                    return;
                case FrequentlyValues.ItemQuality.Excellent:
                    marketQualityObject.SellPriceMinExcellent = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinExcellentDate = marketResponse.SellPriceMinDate;
                    return;
                case FrequentlyValues.ItemQuality.Masterpiece:
                    marketQualityObject.SellPriceMinMasterpiece = marketResponse.SellPriceMin;
                    marketQualityObject.SellPriceMinMasterpieceDate = marketResponse.SellPriceMinDate;
                    return;
            }
        }

        public async void GetPriceStats()
        {
            if (Item.UniqueName == null)
                return;

            var statPricesList = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName,
                Locations.GetLocationsListByArea(new IsLocationAreaActive(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true)),
                GetQualities());

            if (statPricesList == null)
                return;

            var statsPricesTotalList = PriceUpdate(statPricesList);

            FindBestPrice(ref statsPricesTotalList);

            var marketCurrentPricesItemList = new List<MarketCurrentPricesItem>();
            foreach (var item in statsPricesTotalList)
                marketCurrentPricesItemList.Add(new MarketCurrentPricesItem(item));

            _mainWindow.Dispatcher?.Invoke(() =>
            {
                if (_mainWindow.FaLoadIcon.Visibility != Visibility.Hidden)
                {
                    _mainWindow.FaLoadIcon.Visibility = Visibility.Hidden;
                }
                _mainWindow.ListViewPrices.ItemsSource = marketCurrentPricesItemList;
                SetDifferenceCalculationText(statsPricesTotalList);
            });

            HasItemPrices = true;
            RefreshIconTooltipText = $"{LanguageController.Translation("LAST_UPDATE")}: {Formatting.CurrentDateTimeFormat(DateTime.Now)}";
        }

        private List<MarketResponseTotal> PriceUpdate(List<MarketResponse> newStatsPricesList)
        {
            var currentStatsPricesTotalList = new List<MarketResponseTotal>();

            foreach (var newStats in newStatsPricesList)
            {
                try
                {
                    if (currentStatsPricesTotalList.Exists(s => Locations.GetParameterName(s.City) == newStats.City))
                    {
                        var curStats = currentStatsPricesTotalList.Find(s => Locations.GetName(s.City) == newStats.City);

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
                    list.Find(s => s.BuyPriceMax == max).BestBuyMaxPrice = true;
                }
            }
            catch
            {
                // ignored
            }

            var min = GetMinPrice(list);

            try
            {
                if (list.Exists(s => s.SellPriceMin == min))
                {
                    list.First(s => s.SellPriceMin == min).BestSellMinPrice = true;
                }
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

        private void SetDifferenceCalculationText(List<MarketResponseTotal> statsPricesTotalList)
        {
            ulong? bestBuyMaxPrice = 0UL;
            ulong? bestSellMinPrice = 0UL;

            if (statsPricesTotalList?.Count > 0)
            {
                bestBuyMaxPrice = statsPricesTotalList.FirstOrDefault(s => s.BestBuyMaxPrice)?.BuyPriceMax ?? 0UL;
                bestSellMinPrice = statsPricesTotalList.FirstOrDefault(s => s.BestSellMinPrice)?.SellPriceMin ?? 0UL;
            }

            var diffPrice = (int)bestBuyMaxPrice - (int)bestSellMinPrice;

            _mainWindow.LblDifCalcText.Content = $"{LanguageController.Translation("BOUGHT_FOR")} {string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", bestSellMinPrice)} | " +
                                                 $"{LanguageController.Translation("SELL_FOR")} {string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", bestBuyMaxPrice)} | " +
                                                 $"{LanguageController.Translation("PROFIT")} {string.Format(LanguageController.CurrentCultureInfo, "{0:n0}", diffPrice)}";
        }

        private List<int> GetQualities()
        {
            var qualities = new List<int>();

            if (NormalQualityChecked)
                qualities.Add(1);

            if (GoodQualityChecked)
                qualities.Add(2);

            if (OutstandingQualityChecked)
                qualities.Add(3);

            if (ExcellentQualityChecked)
                qualities.Add(4);

            if (MasterpieceQualityChecked)
                qualities.Add(5);

            return qualities;
        }

        public async void SetHistoryChart()
        {
            var historyItemPrices = await ApiController.GetHistoryItemPricesFromJsonAsync(Item.UniqueName,
            Locations.GetLocationsListByArea(new IsLocationAreaActive(ShowBlackZoneOutpostsChecked, ShowVillagesChecked, true)), 
                DateTime.Now.AddDays(-30), GetQualities()).ConfigureAwait(true);

            var date = new List<string>();
            var seriesCollectionHistory = new SeriesCollection();

            foreach (var marketHistory in historyItemPrices)
            {
                var amount = new ChartValues<int>();
                foreach (var data in  marketHistory?.Data?.OrderBy(x => x.Timestamp).ToList() ?? new List<MarketHistoryResponse>())
                {
                    if(!date.Exists(x => x.Contains(data.Timestamp.ToString("g", CultureInfo.CurrentCulture))))
                    {
                        date.Add(data.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                    }

                    amount.Add(data.AveragePrice);
                }
                
                seriesCollectionHistory.Add(new LineSeries
                {
                    Title = Locations.GetName(Locations.GetName(marketHistory?.Location)),
                    Values = amount,
                    Fill = Locations.GetLocationBrush(Locations.GetName(marketHistory?.Location), true),
                    Stroke = Locations.GetLocationBrush(Locations.GetName(marketHistory?.Location), false)
                });
            }

            LabelsHistory = date.ToArray();
            SeriesCollectionHistory = seriesCollectionHistory;
        }
        
        public Item Item {
            get => _item;
            set {
                _item = value;
                OnPropertyChanged();
            }
        }

        public ItemInformation ItemInformation {
            get => _itemInformation;
            set {
                _itemInformation = value;
                OnPropertyChanged();
            }
        }

        public string ItemName {
            get => _itemName;
            set {
                _itemName = value;
                OnPropertyChanged();
            }
        }

        public string ItemTier {
            get => _itemTier;
            set {
                _itemTier = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage Icon {
            get => _icon;
            set {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public bool RunUpdate {
            get => _runUpdate;
            set {
                _runUpdate = value;
                OnPropertyChanged();
            }
        }

        public string ErrorBarText {
            get => _errorBarText;
            set {
                _errorBarText = value;
                OnPropertyChanged();
            }
        }

        public Visibility ErrorBarVisibility {
            get => _errorBarVisibility;
            set {
                _errorBarVisibility = value;
                OnPropertyChanged();
            }
        }

        public ItemWindowTranslation Translation {
            get => _translation;
            set {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public bool NormalQualityChecked {
            get => _normalQualityChecked;
            set
            {
                _normalQualityChecked = !_normalQualityChecked && value;
                OnPropertyChanged();
            }
        }

        public bool GoodQualityChecked {
            get => _goodQualityChecked;
            set {
                _goodQualityChecked = !_goodQualityChecked && value;
                OnPropertyChanged();
            }
        }

        public bool OutstandingQualityChecked {
            get => _outstandingQualityChecked;
            set {
                _outstandingQualityChecked = !_outstandingQualityChecked && value;
                OnPropertyChanged();
            }
        }

        public bool ExcellentQualityChecked {
            get => _excellentQualityChecked;
            set {
                _excellentQualityChecked = !_excellentQualityChecked && value;
                OnPropertyChanged();
            }
        }

        public bool MasterpieceQualityChecked {
            get => _masterpieceQualityChecked;
            set {
                _masterpieceQualityChecked = !_masterpieceQualityChecked && value;
                OnPropertyChanged();
            }
        }

        public bool ShowBlackZoneOutpostsChecked {
            get => _showBlackZoneOutpostsChecked;
            set {
                _showBlackZoneOutpostsChecked = value;
                OnPropertyChanged();
            }
        }
        
        public bool ShowVillagesChecked {
            get => _showVillagesChecked;
            set {
                _showVillagesChecked = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsAutoUpdateActive
        {
            get => _isAutoUpdateActive;
            set {
                _isAutoUpdateActive = value;
                OnPropertyChanged();
            }
        }
        
        public bool HasItemPrices {
            get => _hasItemPrices;
            set {
                _hasItemPrices = value;
                OnPropertyChanged();
            }
        }

        public SeriesCollection SeriesCollectionHistory
        {
            get => _seriesCollectionHistory;
            set
            {
                _seriesCollectionHistory = value;
                OnPropertyChanged();
            }
        }

        public string[] LabelsHistory
        {
            get => _labelsHistory;
            set
            {
                _labelsHistory = value;
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

        public List<MarketQualityObject> AllQualityPricesList {
            get => _allQualityPricesList;
            set
            {
                _allQualityPricesList = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}