using FontAwesome.WPF;
using log4net;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common
{
    public class Alert
    {
        public Alert(ref AlertController alertController, ref MainWindow mainWindow, ref ImageAwesome imageAwesome, ref Item item)
        {
            AlertController = alertController;
            MainWindow = mainWindow;
            ImageAwesome = imageAwesome;
            Item = item;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AlertController AlertController { get; }
        private MainWindow MainWindow { get; }
        public ImageAwesome ImageAwesome { get; set; }
        public Item Item { get; set; }

        private bool _isEventActive;

        public void StartEvent()
        {
            if (_isEventActive)
            {
                return;
            }

            MainWindow.Dispatcher?.Invoke(() =>
            {
                ImageAwesome.Icon = FontAwesomeIcon.ToggleOn;
                ImageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Blue.2"]);
            });

            _isEventActive = true;
            AlertEventAsync();
        }

        public void StopEvent()
        {
            MainWindow.Dispatcher?.Invoke(() =>
            {
                ImageAwesome.Icon = FontAwesomeIcon.ToggleOff;
                ImageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
            });
            _isEventActive = false;
        }

        private async void AlertEventAsync()
        {
            while (_isEventActive)
            {
                try
                {
                    var cityPrices = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName, null, null).ConfigureAwait(false);

                    foreach (var price in cityPrices ?? new List<MarketResponse>())
                    {
                        if (price.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5000) && price.SellPriceMin <= (ulong)Item.AlertModeMinSellPriceIsUndercutPrice)
                        {
                            SoundController.PlayAlertSound();
                            StopEvent();
                            AlertController.Remove(Item);
                        }
                    }

                    await Task.Delay(25000);
                }
                catch (FileNotFoundException e)
                {
                    Log.Error(nameof(AlertEventAsync), e);
                }
                catch (TooManyRequestsException e)
                {
                    Log.Warn(nameof(AlertEventAsync), e);
                    return;
                }
            }
        }
    }
}