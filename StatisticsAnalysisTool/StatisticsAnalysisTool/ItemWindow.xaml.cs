using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Utilities;

namespace StatisticsAnalysisTool
{
    /// <summary>
    ///     Interaktionslogik für ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow
    {
        private ItemData _itemData =  new ItemData();
        private string _uniqueName;
        private bool _runUpdate = true;
        private bool _isAutoUpdateActive;

        public ItemWindow(Item item)
        {
            InitializeComponent();
            
            LblItemName.Content = "";
            LblItemId.Content = "";
            LblLastUpdate.Content = "";

            Translation();
            InitializeItemData(item);

            ListViewPrices.Language = System.Windows.Markup.XmlLanguage.GetLanguage(LanguageController.DefaultCultureInfo.ToString());
        }
        
        private void Translation()
        {
            ChbShowVillages.Content = LanguageController.Translation("SHOW_VILLAGES");
            ChbAutoUpdateData.Content = LanguageController.Translation("AUTO_UPDATE_DATA");
            LblLastUpdate.ToolTip = LanguageController.Translation("LAST_UPDATE");
            GvcCityTitel.Header = LanguageController.Translation("CITY");
            GvcSellPriceMin.Header = LanguageController.Translation("SELL_PRICE_MIN");
            GvcSellPriceMinDate.Header = LanguageController.Translation("SELL_PRICE_MIN_DATE");
            GvcSellPriceMax.Header = LanguageController.Translation("SELL_PRICE_MAX");
            GvcSellPriceMaxDate.Header = LanguageController.Translation("SELL_PRICE_MAX_DATE");
            GvcBuyPriceMin.Header = LanguageController.Translation("BUY_PRICE_MIN");
            GvcBuyPriceMinDate.Header = LanguageController.Translation("BUY_PRICE_MIN_DATE");
            GvcBuyPriceMax.Header = LanguageController.Translation("BUY_PRICE_MAX");
            GvcBuyPriceMaxDate.Header = LanguageController.Translation("BUY_PRICE_MAX_DATE");
            LblDifCalcName.Content = $"{LanguageController.Translation("DIFFERENT_CALCULATION")}:";
        }

        private async void InitializeItemData(Item item)
        {
            if (item == null)
                return;

            _uniqueName = item.UniqueName;

            if (Dispatcher == null)
                return;

            await Dispatcher.InvokeAsync(() =>
            {
                FaLoadIcon.Visibility = Visibility.Visible;
                Icon = item.Icon;
            });

            StartAutoUpdater();

            var itemDataTaskResult = await StatisticsAnalysisManager.GetItemDataFromJsonAsync(item);

            if (itemDataTaskResult == null)
            {
                LblItemName.Content = LanguageController.Translation("ERROR_PRICES_CAN_NOT_BE_LOADED");
                Dispatcher?.Invoke(() => { FaLoadIcon.Visibility = Visibility.Hidden; });
                return;
            }

            _itemData = itemDataTaskResult;

            if (Dispatcher == null)
                return;

            await Dispatcher.InvokeAsync(() =>
                {
                    Title = $"{_itemData.LocalizedName} (T{_itemData.Tier})";
                    LblItemName.Content = $"{_itemData.LocalizedName} (T{_itemData.Tier})";
                    LblItemId.Content = _itemData.UniqueName;
                    ImgItemImage.Source = item.Icon;

                    FaLoadIcon.Visibility = Visibility.Hidden;
                });
        
        }

        private async void StartAutoUpdater()
        {
            await Task.Run(async () => {
                if (_isAutoUpdateActive)
                    return;

                _isAutoUpdateActive = true;
                while (_runUpdate)
                {
                    await Task.Delay(500);
                    if (Dispatcher != null && Dispatcher.Invoke(() => !ChbAutoUpdateData.IsChecked ?? false))
                        continue;
                    GetPriceStats(_uniqueName, Dispatcher != null && Dispatcher.Invoke(() => ChbShowVillages.IsChecked ?? false));
                    await Task.Delay(StatisticsAnalysisManager.RefreshRate - 500);
                }
                _isAutoUpdateActive = false;
            });
        }

        private async void GetPriceStats(string uniqueName, bool showVillages = false)
        {
            if (uniqueName == null)
                return;

            await Task.Run(async () =>
            {
                var statPricesList = await StatisticsAnalysisManager.GetItemPricesFromJsonAsync(uniqueName, showVillages);

                if (statPricesList == null)
                    return;

                var statsPricesTotalList = PriceUpdate(statPricesList);

                FindBestPrice(ref statsPricesTotalList);

                var marketCurrentPricesItemList = new List<MarketCurrentPricesItem>();
                foreach (var item in statsPricesTotalList)
                    marketCurrentPricesItemList.Add(new MarketCurrentPricesItem(item));

                Dispatcher?.Invoke(() =>
                {
                    ListViewPrices.ItemsSource = marketCurrentPricesItemList;
                    SetDifferenceCalculationText(statsPricesTotalList);
                    LblLastUpdate.Content = Utility.DateFormat(DateTime.Now, 0);
                });
            });
        }

        private List<MarketResponseTotal> PriceUpdate(List<MarketResponse> statPricesList)
        {
            var statsPricesTotalList = new List<MarketResponseTotal>();

            foreach (var stats in statPricesList)
            {
                if (statsPricesTotalList.Exists(s => Locations.GetName(s.City) == stats.City))
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
                    var newSpt = new MarketResponseTotal()
                    {
                        City = Locations.GetName(stats.City),
                        SellPriceMin = stats.SellPriceMin,
                        SellPriceMax = stats.SellPriceMax,
                        BuyPriceMin = stats.BuyPriceMin,
                        BuyPriceMax = stats.BuyPriceMax,
                        SellPriceMinDate = stats.SellPriceMinDate,
                        SellPriceMaxDate = stats.SellPriceMaxDate,
                        BuyPriceMinDate = stats.BuyPriceMinDate,
                        BuyPriceMaxDate = stats.BuyPriceMaxDate,
                    };

                    statsPricesTotalList.Add(newSpt);
                }
            }

            return statsPricesTotalList;
        }

        private void FindBestPrice(ref List<MarketResponseTotal> list)
        {
            if (list.Count == 0)
                return;

            var max = ulong.MinValue;
            foreach (var type in list)
            {
                if (type.BuyPriceMax == 0) 
                    continue;

                if (type.BuyPriceMax > max)
                    max = type.BuyPriceMax;
            }

            try
            {
                list.Find(s => s.BuyPriceMax == max).BestBuyMaxPrice = true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
            

            var min = ulong.MaxValue;
            foreach (var type in list)
            {
                if (type.SellPriceMin == 0) 
                    continue;

                if (type.SellPriceMin < min)
                    min = type.SellPriceMin;
            }

            try
            {
                list.Find(s => s.SellPriceMin == min).BestSellMinPrice = true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }

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

            LblDifCalcText.Content = $"{LanguageController.Translation("BOUGHT_FOR")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", bestSellMinPrice)} | " +
                                     $"{LanguageController.Translation("SELL_FOR")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", bestBuyMaxPrice)} | " +
                                     $"{LanguageController.Translation("PROFIT")} {string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", diffPrice)}";
        }
        
        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _runUpdate = false;
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ShowVillagesPrices_Click(object sender, RoutedEventArgs e)
        {
            var chb = e.Source as CheckBox;
            if (chb?.IsChecked ?? false)
            {
                Height = 515;
                GetPriceStats(_uniqueName, true);
            }
            else
            {
                Height = 335;
                GetPriceStats(_uniqueName);
            }

        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
        
    }
}