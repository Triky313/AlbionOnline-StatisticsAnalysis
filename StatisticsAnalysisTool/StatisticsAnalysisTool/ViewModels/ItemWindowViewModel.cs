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
using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;

namespace StatisticsAnalysisTool.ViewModels
{
    public class ItemWindowViewModel: INotifyPropertyChanged
    {
        private readonly ItemWindow _mainWindow;
        private ItemInformation _itemData = new ItemInformation();
        private Item _item;
        private ItemInformation _itemInformation;
        private bool _runUpdate = true;
        private bool _isAutoUpdateActive;
        private BitmapImage _icon;
        private string _itemTitle;
        private string _itemId;
        
        public enum Error { NoPrices, NoItemInfo, GeneralError }

        public ItemWindowViewModel(ItemWindow mainWindow, Item item)
        {
            _mainWindow = mainWindow;
            Item = item;

            Translation();
            InitializeItemData(item);

            _mainWindow.ListViewPrices.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LanguageController.DefaultCultureInfo.ToString());
        }

        private void Translation()
        {
            _mainWindow.ChbShowVillages.Content = LanguageController.Translation("SHOW_VILLAGES");
            _mainWindow.ChbShowBlackZoneOutposts.Content = LanguageController.Translation("SHOW_BLACKZONE_OUTPOSTS");
            _mainWindow.ChbAutoUpdateData.Content = LanguageController.Translation("AUTO_UPDATE_DATA");
            _mainWindow.LblLastUpdate.ToolTip = LanguageController.Translation("LAST_UPDATE");
            _mainWindow.GvcCityTitel.Header = LanguageController.Translation("CITY");
            _mainWindow.GvcSellPriceMin.Header = LanguageController.Translation("SELL_PRICE_MIN");
            _mainWindow.GvcSellPriceMinDate.Header = LanguageController.Translation("SELL_PRICE_MIN_DATE");
            _mainWindow.GvcSellPriceMax.Header = LanguageController.Translation("SELL_PRICE_MAX");
            _mainWindow.GvcSellPriceMaxDate.Header = LanguageController.Translation("SELL_PRICE_MAX_DATE");
            _mainWindow.GvcBuyPriceMin.Header = LanguageController.Translation("BUY_PRICE_MIN");
            _mainWindow.GvcBuyPriceMinDate.Header = LanguageController.Translation("BUY_PRICE_MIN_DATE");
            _mainWindow.GvcBuyPriceMax.Header = LanguageController.Translation("BUY_PRICE_MAX");
            _mainWindow.GvcBuyPriceMaxDate.Header = LanguageController.Translation("BUY_PRICE_MAX_DATE");
            _mainWindow.LblDifCalcName.Content = $"{LanguageController.Translation("DIFFERENT_CALCULATION")}:";
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
                ItemId = ItemInformation.UniqueName;
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
                        ItemTitle = LanguageController.Translation("ERROR_NO_ITEM_INFO");
                        Icon = new BitmapImage(new Uri(@"pack://application:,,,/"
                                                       + Assembly.GetExecutingAssembly().GetName().Name + ";component/"
                                                       + "Resources/Trash.png", UriKind.Absolute));
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                    });
                    return;
                case Error.NoPrices:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        ItemTitle = LanguageController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED");
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                    });
                    return;
                case Error.GeneralError:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        ItemTitle = LanguageController.Translation("ERROR_GENERAL_ERROR");
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        Debug.Print(message);
                    });
                    return;
                default:
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        ItemTitle = LanguageController.Translation("ERROR_GENERAL_ERROR");
                        _mainWindow.FaLoadIcon.Icon = FontAwesomeIcon.Times;
                        _mainWindow.FaLoadIcon.Spin = false;
                        Debug.Print(message);
                    });
            return;
        }
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
                    if (_mainWindow.Dispatcher != null && _mainWindow.Dispatcher.Invoke(() => !_mainWindow.ChbAutoUpdateData.IsChecked ?? false))
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
                var showVillagesIsChecked = _mainWindow.Dispatcher != null && _mainWindow.Dispatcher.Invoke(() => _mainWindow.ChbShowVillages.IsChecked ?? false);
                var showBlackZoneOutpostsIsChecked = _mainWindow.Dispatcher != null && _mainWindow.Dispatcher.Invoke(() => _mainWindow.ChbShowBlackZoneOutposts.IsChecked ?? false);

                var statPricesList = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName, Locations.GetLocationsListByArea(new IsLocationAreaActive()
                {
                    Cities = true,
                    Villages = showVillagesIsChecked,
                    BlackZoneOutposts = showBlackZoneOutpostsIsChecked
                }));

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
                    SetDifferenceCalculationText(statsPricesTotalList);
                    _mainWindow.LblLastUpdate.Content = Utilities.DateFormat(DateTime.Now, 0);
                });
            });
        }

        private List<MarketResponseTotal> PriceUpdate(List<MarketResponse> statPricesList)
        {
            var statsPricesTotalList = new List<MarketResponseTotal>();

            foreach (var stats in statPricesList)
            {
                if (statsPricesTotalList.Exists(s => Locations.GetParameterName(s.City) == stats.City))
                {
                    var spt = statsPricesTotalList.Find(s => Locations.GetName(s.City) == stats.City);
                    if (stats.SellPriceMin < spt.SellPriceMin)
                        spt.SellPriceMin = stats.SellPriceMin;

                    if (stats.SellPriceMax > spt.SellPriceMax)
                        spt.SellPriceMax = stats.SellPriceMax;

                    if (stats.BuyPriceMin < spt.BuyPriceMin)
                        spt.BuyPriceMin = stats.BuyPriceMin;

                    if (stats.BuyPriceMax > spt.BuyPriceMax)
                        spt.BuyPriceMax = stats.BuyPriceMax;
                }
                else
                {
                    statsPricesTotalList.Add(new MarketResponseTotal(stats));
                }
            }

            return statsPricesTotalList;
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

        public string ItemId {
            get => _itemId;
            set {
                _itemId = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}