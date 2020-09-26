using FontAwesome.WPF;
using log4net;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common
{
    public class Alert
    {
        public Alert(ref ImageAwesome imageAwesome, ref Item item)
        {
            ImageAwesome = imageAwesome;
            Item = item;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ImageAwesome ImageAwesome { get; set; }
        public Item Item { get; set; }

        private bool _isEventActive;

        public void StartEvent()
        {
            if (_isEventActive)
            {
                return;
            }

            ImageAwesome.Icon = FontAwesomeIcon.ToggleOn;
            ImageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Blue.2"]);
            _isEventActive = true;
            AlertEventAsync();
        }

        public void StopEvent()
        {
            ImageAwesome.Icon = FontAwesomeIcon.ToggleOff;
            ImageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
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
                        if (price.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5) && price.SellPriceMin <= (ulong)Item.AlertModeMinSellPriceIsUndercutPrice)
                        {
                            Debug.Print($"{Item.UniqueName} Preis unterboten!");
                            StopEvent();
                        }
                    }

                    await Task.Delay(25000);
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