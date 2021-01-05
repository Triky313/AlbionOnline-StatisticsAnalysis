using log4net;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public class Alert
    {
        private readonly MainWindow _mainWindow;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AlertController AlertController { get; }
        private Item _item;
        private int _alertModeMinSellPriceIsUndercutPrice;
        private bool _isEventActive;

        public Alert(MainWindow mainWindow, AlertController alertController, Item item, int alertModeMinSellPriceIsUndercutPrice)
        {
            _mainWindow = mainWindow;
            AlertController = alertController;
            Item = item;
            AlertModeMinSellPriceIsUndercutPrice = alertModeMinSellPriceIsUndercutPrice;
        }

        public void StartEvent()
        {
            if (_isEventActive)
            {
                return;
            }
            
            _isEventActive = true;
            AlertEventAsync(_item.UniqueName);
        }

        public void StopEvent()
        {
            _isEventActive = false;
            _mainWindow.FlashWindow(12);
        }

        private async void AlertEventAsync(string uniqueName)
        {
            while (_isEventActive)
            {
                try
                {
                    var cityPrices = await ApiController.GetCityItemPricesFromJsonAsync(uniqueName, null, null).ConfigureAwait(false);

                    foreach (var marketResponse in cityPrices ?? new List<MarketResponse>())
                    {
                        if (Locations.GetName(marketResponse.City) != Location.BlackMarket  
                            && marketResponse.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5) 
                            && marketResponse.SellPriceMin <= (ulong)AlertModeMinSellPriceIsUndercutPrice 
                            && AlertModeMinSellPriceIsUndercutPrice > 0)
                        {
                            SoundController.PlayAlertSound();
                            StopEvent();
                            AlertController.DeactivateAlert(uniqueName);

                            _mainWindow.Dispatcher.Invoke(() =>
                            {
                                var itemAlertWindow = new ItemAlertWindow(new AlertInfos(_item, marketResponse));
                                itemAlertWindow.Show();
                            });

                            break;
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

        public Item Item {
            get => _item;
            set {
                _item = value;
                OnPropertyChanged();
            }
        }

        public int AlertModeMinSellPriceIsUndercutPrice {
            get => _alertModeMinSellPriceIsUndercutPrice;
            set {
                _alertModeMinSellPriceIsUndercutPrice = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}