using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private List<MarketResponseTotal> _statsPricesTotalList = new List<MarketResponseTotal>();
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
        }

        private void Translation()
        {
            ChbShowVillages.Content = LanguageController.Translation("SHOW_VILLAGES");
            ChbAutoUpdateData.Content = LanguageController.Translation("AUTO_UPDATE_DATA");
            LblLastUpdate.ToolTip = LanguageController.Translation("LAST_UPDATE");
            LblCityTitel.Content = LanguageController.Translation("CITY");
            LblSellPriceMin.Content = LanguageController.Translation("SELL_PRICE_MIN");
            LblSellPriceMinDate.Content = LanguageController.Translation("SELL_PRICE_MIN_DATE");
            LblSellPriceMax.Content = LanguageController.Translation("SELL_PRICE_MAX");
            LblSellPriceMaxDate.Content = LanguageController.Translation("SELL_PRICE_MAX_DATE");
            LblBuyPriceMin.Content = LanguageController.Translation("BUY_PRICE_MIN");
            LblBuyPriceMinDate.Content = LanguageController.Translation("BUY_PRICE_MIN_DATE");
            LblBuyPriceMax.Content = LanguageController.Translation("BUY_PRICE_MAX");
            LblBuyPriceMaxDate.Content = LanguageController.Translation("BUY_PRICE_MAX_DATE");
            LblDifCalcName.Content = $"{LanguageController.Translation("DIFFERENT_CALCULATION")}:";
        }

        private async void InitializeItemData(Item item)
        {
            if (item == null)
                return;

            _uniqueName = item.UniqueName;

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
                Dispatcher.Invoke(() =>
                {
                    FaLoadIcon.Visibility = Visibility.Hidden;
                });
                return;
            }

            _itemData = itemDataTaskResult;
            
            await Dispatcher.InvokeAsync(() =>
            {
                Title = $"{_itemData.LocalizedName} (T{_itemData.Tier})";
                LblItemName.Content = $"{_itemData.LocalizedName} (T{_itemData.Tier})";
                LblItemId.Content = _itemData.UniqueName;
                ImgItemImage.Source = item.Icon;

                FaLoadIcon.Visibility = Visibility.Hidden;
            });
        }

        private void StartAutoUpdater()
        {
            Task.Run(async () => {
                if (_isAutoUpdateActive)
                    return;

                _isAutoUpdateActive = true;
                while (_runUpdate)
                {
                    await Task.Delay(500);
                    if (Dispatcher.Invoke(() => !ChbAutoUpdateData.IsChecked ?? false))
                        continue;
                    GetPriceStats(_uniqueName, Dispatcher.Invoke(() => ChbShowVillages.IsChecked ?? false));
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
                _statsPricesTotalList.Clear();
                var statPricesList = await StatisticsAnalysisManager.GetItemPricesFromJsonAsync(uniqueName, showVillages);

                if (statPricesList == null)
                    return;

                Dispatcher.Invoke(() =>
                {
                    foreach (var stats in statPricesList)
                    {
                        if (_statsPricesTotalList.Exists(s => Locations.GetName(s.City) == stats.City))
                        {
                            var spt = _statsPricesTotalList.Find(s => Locations.GetName(s.City) == stats.City);
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

                            _statsPricesTotalList.Add(newSpt);
                        }
                    }

                    FindBestPrice(ref _statsPricesTotalList);

                    SpStats.Children.Clear();
                    foreach (var spt in _statsPricesTotalList)
                    {
                        CreateGridElement(spt);
                    }

                    SetDifferenceCalculationText(_statsPricesTotalList);

                    LblLastUpdate.Content = Utility.DateFormat(DateTime.Now, 0);
                });

            });
        }

        private void FindBestPrice(ref List<MarketResponseTotal> list)
        {
            if (list.Count == 0)
                return;

            var max = ulong.MinValue;
            foreach (var type in list)
            {
                if (type.BuyPriceMax == 0) continue;
                if (type.BuyPriceMax > max)
                {
                    max = type.BuyPriceMax;
                }
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
                if (type.SellPriceMin == 0) continue;
                if (type.SellPriceMin < min)
                {
                    min = type.SellPriceMin;
                }
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

        private void CreateGridElement(MarketResponseTotal stats)
        {
            var textColor = new SolidColorBrush(Colors.Gainsboro);
            
            var bestPriceLabelStyle = FindResource("BestPriceLabel") as Style;

            var grid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Top, Height = 30, Margin = new Thickness(0, 0, 0, 0)
            };

            var lblCity = new Label
            {
                Content = stats.City,
                Foreground = textColor,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 150,
                Height = 28,
                Margin = new Thickness(10, 0, 0, 0)
            };

            switch(stats.City)
            {
                case Location.Caerleon:
                    lblCity.Style = FindResource("CaerleonStyle") as Style;
                    break;
                case Location.Thetford:
                    lblCity.Style = FindResource("ThetfordStyle") as Style;
                    break;
                case Location.Bridgewatch:
                    lblCity.Style = FindResource("BridgewatchStyle") as Style;
                    break;
                case Location.Martlock:
                    lblCity.Style = FindResource("MartlockStyle") as Style;
                    break;
                case Location.Lymhurst:
                    lblCity.Style = FindResource("LymhurstStyle") as Style;
                    break;
                case Location.FortSterling:
                    lblCity.Style = FindResource("FortSterlingStyle") as Style;
                    break;
                default:
                    lblCity.Style = FindResource("DefaultCityStyle") as Style;
                    break;
            }

            List<Image> silverImages = new List<Image>();

            var lblSellPriceMin = new Label
            {
                Content = string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", stats.SellPriceMin),
                Foreground = DateTimeToOld(stats.SellPriceMinDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Width = 85,
                Height = 28,
                Margin = new Thickness(194, 0, 0, 0)
            };
            lblSellPriceMin.Style = (stats.BestSellMinPrice && stats.SellPriceMin != 0) ? bestPriceLabelStyle : lblSellPriceMin.Style;
            silverImages.Add(new Image
            {
                Margin = new Thickness(168, 0, 0, 0),
                Style = FindResource("Image.Price.Silver") as Style
            });

            var lblSellPriceMax = new Label
            {
                Content = string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", stats.SellPriceMax),
                Foreground = DateTimeToOld(stats.SellPriceMaxDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Width = 85,
                Height = 28,
                Margin = new Thickness(444, 0, 0, 0)
            };
            silverImages.Add(new Image
            {
                Margin = new Thickness(418, 0, 0, 0),
                Style = FindResource("Image.Price.Silver") as Style
            });

            var lblBuyPriceMin = new Label
            {
                Content = string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", stats.BuyPriceMin),
                Foreground = DateTimeToOld(stats.BuyPriceMinDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Width = 85,
                Height = 28,
                Margin = new Thickness(694, 0, 0, 0)
            };
            silverImages.Add(new Image
            {
                Margin = new Thickness(668, 0, 0, 0),
                Style = FindResource("Image.Price.Silver") as Style
            });

            var lblBuyPriceMax = new Label
            {
                Content = string.Format(LanguageController.DefaultCultureInfo, "{0:n0}", stats.BuyPriceMax),
                Foreground = DateTimeToOld(stats.BuyPriceMaxDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Width = 85,
                Height = 28,
                Margin = new Thickness(944, 0, 0, 0)
            };
            lblBuyPriceMax.Style = (stats.BestBuyMaxPrice && stats.BuyPriceMax != 0) ? bestPriceLabelStyle : lblBuyPriceMax.Style;
            silverImages.Add(new Image
            {
                Margin = new Thickness(918, 0, 0, 0),
                Style = FindResource("Image.Price.Silver") as Style
            });

            var lblSellPriceMinDate = new Label
            {
                Content = Utility.DateFormat(stats.SellPriceMinDate, 2),
                Foreground = DateTimeToOld(stats.SellPriceMinDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 120,
                Height = 28,
                Margin = new Thickness(293, 0, 0, 0)
            };

            var lblSellPriceMaxDate = new Label
            {
                Content = Utility.DateFormat(stats.SellPriceMaxDate, 2),
                Foreground = DateTimeToOld(stats.SellPriceMaxDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 120,
                Height = 28,
                Margin = new Thickness(543, 0, 0, 0)
            };

            var lblBuyPriceMinDate = new Label
            {
                Content = Utility.DateFormat(stats.BuyPriceMinDate, 2),
                Foreground = DateTimeToOld(stats.BuyPriceMinDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 120,
                Height = 28,
                Margin = new Thickness(793, 0, 0, 0)
            };

            var lblBuyPriceMaxDate = new Label
            {
                Content = Utility.DateFormat(stats.BuyPriceMaxDate, 2),
                Foreground = DateTimeToOld(stats.BuyPriceMaxDate),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 120,
                Height = 28,
                Margin = new Thickness(1043, 0, 0, 0)
            };

            SpStats.Children.Add(grid);
            grid.Children.Add(lblCity);
            grid.Children.Add(lblSellPriceMin);
            grid.Children.Add(lblSellPriceMax);
            grid.Children.Add(lblBuyPriceMin);
            grid.Children.Add(lblBuyPriceMax);
            grid.Children.Add(lblSellPriceMinDate);
            grid.Children.Add(lblSellPriceMaxDate);
            grid.Children.Add(lblBuyPriceMinDate);
            grid.Children.Add(lblBuyPriceMaxDate);
            foreach (var image in silverImages)
                grid.Children.Add(image);

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
        
        private SolidColorBrush DateTimeToOld(DateTime dateTime)
        {
            // ReSharper disable once PossibleNullReferenceException
            var textColorNormal = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDCDCDC"));
            // ReSharper disable once PossibleNullReferenceException
            var textColorToOldFirst = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9898A5"));
            // ReSharper disable once PossibleNullReferenceException
            var textColorToOldSecond = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF81818C"));
            // ReSharper disable once PossibleNullReferenceException
            var textColorToOldThird = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF696972"));
            // ReSharper disable once PossibleNullReferenceException
            var textColorNoValue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF525259"));

            if (dateTime.Date == DateTime.MinValue.Date)
                return textColorNoValue;

            var currentDateTime = DateTime.Now.ToUniversalTime();

            if (dateTime.AddHours(6) < currentDateTime.AddHours(-1))
            {
                return textColorToOldThird;
            } else if (dateTime.AddHours(4) < currentDateTime.AddHours(-1))
            {
                return textColorToOldSecond;
            } else if (dateTime.AddHours(2) < currentDateTime.AddHours(-1))
            {
                return textColorToOldFirst;
            }
            else
            {
                return textColorNormal;
            }
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