using FontAwesome.WPF;
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
            AlertEvent();
        }

        public void StopEvent()
        {
            ImageAwesome.Icon = FontAwesomeIcon.ToggleOff;
            ImageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
            _isEventActive = false;
        }

        private async void AlertEvent()
        {
            while (_isEventActive)
            {
                var cityPrices = await ApiController.GetCityItemPricesFromJsonAsync(Item.UniqueName, null, null);

                foreach (var price in cityPrices ?? new List<MarketResponse>())
                {
                    if (price.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5) && price.SellPriceMin <= (ulong)Item.AlertModeMinSellPriceIsUndercutPrice)
                    {
                        Debug.Print($"{Item.UniqueName} Preis unterboten!");
                        StopEvent();
                    }
                }

                await Task.Delay(20000);
            }
        }
    }
}