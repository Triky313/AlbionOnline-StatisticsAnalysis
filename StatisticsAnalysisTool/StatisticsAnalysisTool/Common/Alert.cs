using log4net;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common
{
    public class Alert
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly MainWindow _mainWindow;
        private int _alertModeMinSellPriceIsUndercutPrice;
        private bool _isEventActive;
        private Item _item;

        public Alert(MainWindow mainWindow, AlertController alertController, Item item, int alertModeMinSellPriceIsUndercutPrice)
        {
            _mainWindow = mainWindow;
            AlertController = alertController;
            Item = item;
            AlertModeMinSellPriceIsUndercutPrice = alertModeMinSellPriceIsUndercutPrice;
        }

        private AlertController AlertController { get; }

        public Item Item
        {
            get => _item;
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }

        public int AlertModeMinSellPriceIsUndercutPrice
        {
            get => _alertModeMinSellPriceIsUndercutPrice;
            set
            {
                _alertModeMinSellPriceIsUndercutPrice = value;
                OnPropertyChanged();
            }
        }

        public void StartEvent()
        {
            if (_isEventActive) return;

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
                try
                {
                    var cityPrices = await ApiController.GetCityItemPricesFromJsonAsync(uniqueName, null, null).ConfigureAwait(false);

                    foreach (var marketResponse in cityPrices ?? new List<MarketResponse>())
                        if (Locations.GetName(marketResponse.City) != Location.BlackMarket
                            && marketResponse.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5)
                            && marketResponse.SellPriceMin <= (ulong) AlertModeMinSellPriceIsUndercutPrice
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

                    await Task.Delay(25000);
                }
                catch (FileNotFoundException e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
                }
                catch (TooManyRequestsException e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                    Log.Warn(MethodBase.GetCurrentMethod().DeclaringType, e);
                    return;
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