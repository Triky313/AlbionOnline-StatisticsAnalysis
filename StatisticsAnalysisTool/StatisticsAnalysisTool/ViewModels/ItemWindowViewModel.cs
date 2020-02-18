using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    using System.Windows.Media;

    public class ItemWindowViewModel: INotifyPropertyChanged
    {
        private readonly ItemWindow _mainWindow;
        private ItemInformation _itemData = new ItemInformation();
        private Item _item;
        private ItemInformation _itemInformation;
        private bool _runUpdate = true;
        private Visibility _errorBarVisibility;
        private string _errorBarText;
        private bool _isAutoUpdateActive;
        private BitmapImage _icon;
        private string _itemTitle;
        private bool _masterpieceQualityChecked;
        private bool _excellentQualityChecked;
        private bool _outstandingQualityChecked;
        private bool _goodQualityChecked;
        private bool _normalQualityChecked;
        private ItemWindowTranslation _translation;
        private bool _hasItemPrices;
        private bool _showBlackZoneOutpostsChecked;
        private bool _showVillagesChecked;
        private bool _autoUpdateDataChecked;
        private SeriesCollection _seriesCollectionHistory;
        private string[] _labelsHistory;

        public enum Error { NoPrices, NoItemInfo, GeneralError }

        public ItemWindowViewModel(ItemWindow mainWindow, Item item)
        {
            _mainWindow = mainWindow;
            Item = item;

            ErrorBarVisibility = Visibility.Hidden;
            AutoUpdateDataChecked = true;

            InitializeTranslation();
            InitializeItemData(item);

            _mainWindow.ListViewPrices.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LanguageController.DefaultCultureInfo.ToString());
        }

        private void InitializeTranslation()
        {
            Translation = new ItemWindowTranslation()
            {
                Normal = LanguageController.Translation("NORMAL"),
                Good = LanguageController.Translation("GOOD"),
                Outstanding = LanguageController.Translation("OUTSTANDING"),
                Excellent = LanguageController.Translation("EXCELLENT"),
                Masterpiece = LanguageController.Translation("MASTERPIECE"),
                ShowBlackzoneOutposts = LanguageController.Translation("SHOW_BLACKZONE_OUTPOSTS"),
                ShowVillages = LanguageController.Translation("SHOW_VILLAGES"),
                AutoUpdateData = LanguageController.Translation("AUTO_UPDATE_DATA"),
                LastUpdate = LanguageController.Translation("LAST_UPDATE"),
                City = LanguageController.Translation("CITY"),
                SellPriceMin = LanguageController.Translation("SELL_PRICE_MIN"),
                SellPriceMinDate = LanguageController.Translation("SELL_PRICE_MIN_DATE"),
                SellPriceMax = LanguageController.Translation("SELL_PRICE_MAX"),
                SellPriceMaxDate = LanguageController.Translation("SELL_PRICE_MAX_DATE"),
                BuyPriceMin = LanguageController.Translation("BUY_PRICE_MIN"),
                BuyPriceMinDate = LanguageController.Translation("BUY_PRICE_MIN_DATE"),
                BuyPriceMax = LanguageController.Translation("BUY_PRICE_MAX"),
                BuyPriceMaxDate = LanguageController.Translation("BUY_PRICE_MAX_DATE"),
                DifferentCalculation = $"{LanguageController.Translation("DIFFERENT_CALCULATION")}:"
            };
        }

        private async void InitializeItemData(Item item)
        {
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
            ItemInformation = await ApiController.GetItemInfoFromJsonAsync(item).ConfigureAwait(false);

            if (ItemInformation == null)
            {
                SetNoDataValues(Error.NoItemInfo);
                return;
            }

            await _mainWindow.Dispatcher.InvokeAsync(() =>
            {
                _mainWindow.Icon = item.Icon;
                ItemTitle = $"{localizedName} (T{ItemInformation.Tier})";
                _mainWindow.Title = $"{localizedName} (T{ItemInformation.Tier})";
                Icon = item.Icon;
            });

            StartAutoUpdater();
        }

        private void SetNoDataValues(Error error, string message = null)
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
                        Debug.Print(message);
                    });
                    return;
                default:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        HasItemPrices = false;
                        SetErrorBar(Visibility.Visible, LanguageController.Translation("ERROR_GENERAL_ERROR"));
                        Debug.Print(message);
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
                if (_isAutoUpdateActive)
                    return;

                _isAutoUpdateActive = true;
                while (RunUpdate)
                {
                    await Task.Delay(500);
                    if (_mainWindow.Dispatcher != null && !AutoUpdateDataChecked)
                        continue;

                    GetPriceStats();
                    await Task.Delay(Settings.Default.RefreshRate - 500);
                }
                _isAutoUpdateActive = false;
            });
        }

        public async void GetPriceStats()
        {
            if (Item.UniqueName == null)
                return;

            await Task.Run(async () =>
            {
                var statPricesList = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName, 
                    Locations.GetLocationsListByArea(new IsLocationAreaActive()
                    {
                        Cities = true,
                        Villages = ShowVillagesChecked,
                        BlackZoneOutposts = ShowBlackZoneOutpostsChecked
                    }),
                    GetQualities());

                if (statPricesList == null)
                {
                    return;
                }

                var statsPricesTotalList = PriceUpdate(statPricesList);

                FindBestPrice(ref statsPricesTotalList);

                var marketCurrentPricesItemList = new List<MarketCurrentPricesItem>();
                foreach (var item in statsPricesTotalList)
                    marketCurrentPricesItemList.Add(new MarketCurrentPricesItem(item));

                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    if(_mainWindow.FaLoadIcon.Visibility != Visibility.Hidden)
                    {
                        _mainWindow.FaLoadIcon.Visibility = Visibility.Hidden;
                    }
                    _mainWindow.ListViewPrices.ItemsSource = marketCurrentPricesItemList;
                    HasItemPrices = true;
                    SetDifferenceCalculationText(statsPricesTotalList);
                    _mainWindow.LblLastUpdate.Content = Utilities.DateFormat(DateTime.Now, 0);
                });
            });
        }

        private List<MarketResponseTotal> PriceUpdate(List<MarketResponse> newStatsPricesList)
        {
            var currentStatsPricesTotalList = new List<MarketResponseTotal>();

            foreach (var newStats in newStatsPricesList)
            {
                if (currentStatsPricesTotalList.Exists(s => Locations.GetParameterName(s.City) == newStats.City))
                {
                    var curStats = currentStatsPricesTotalList.Find(s => Locations.GetName(s.City) == newStats.City);
                    
                    if (newStats.SellPriceMinDate < curStats.SellPriceMinDate)
                    {
                        curStats.SellPriceMin = newStats.SellPriceMin;
                        curStats.SellPriceMinDate = newStats.SellPriceMinDate;
                    }

                    if (newStats.SellPriceMaxDate < curStats.SellPriceMaxDate)
                    {
                        curStats.SellPriceMax = newStats.SellPriceMax;
                        curStats.SellPriceMaxDate = newStats.SellPriceMaxDate;
                    }

                    if (newStats.BuyPriceMinDate < curStats.BuyPriceMinDate)
                    {
                        curStats.BuyPriceMin = newStats.BuyPriceMin;
                        curStats.BuyPriceMinDate = newStats.BuyPriceMinDate;
                    }

                    if (newStats.BuyPriceMaxDate < curStats.BuyPriceMaxDate)
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

            return currentStatsPricesTotalList;
        }

        private void FindBestPrice(ref List<MarketResponseTotal> list)
        {
            if (list.Count == 0)
                return;

            var max = GetMaxPrice(list);

            try
            {
                list.Find(s => s.BuyPriceMax == max).BestBuyMaxPrice = true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }

            var min = GetMinPrice(list);

            try
            {
                list.Find(s => s.SellPriceMin == min).BestSellMinPrice = true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
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

            _mainWindow.LblDifCalcText.Content = $"{LanguageController.Translation("BOUGHT_FOR")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", bestSellMinPrice)} | " +
                                                 $"{LanguageController.Translation("SELL_FOR")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", bestBuyMaxPrice)} | " +
                                                 $"{LanguageController.Translation("PROFIT")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", diffPrice)}";
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
                Locations.GetLocationsListByArea(new IsLocationAreaActive()
            {
                Cities = true,
                Villages = ShowVillagesChecked,
                BlackZoneOutposts = ShowBlackZoneOutpostsChecked
            }),DateTime.UtcNow, GetQualities()).ConfigureAwait(true);

            var date = new List<string>();
            var seriesCollectionHistory = new SeriesCollection();

            foreach (var item in historyItemPrices)
            {
                var amount = new ChartValues<int>();
                foreach (var data in item?.Data ?? new List<MarketHistoryResponse>())
                {
                    date.Add(data.Timestamp.ToString("g", CultureInfo.CurrentCulture));
                    amount.Add(data.AveragePrice);
                }
                
                seriesCollectionHistory.Add(new LineSeries
                {
                    Title = Locations.GetName(Locations.GetName(item?.Location)),
                    Values = amount,
                    Fill = (Brush)Application.Current.Resources["Solid.Color.Gold.Fill"],
                    Stroke = (Brush)Application.Current.Resources["Solid.Color.Text.Gold"]
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

        public string ItemTitle {
            get => _itemTitle;
            set {
                _itemTitle = value;
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
        
        public bool AutoUpdateDataChecked {
            get => _autoUpdateDataChecked;
            set {
                _autoUpdateDataChecked = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}