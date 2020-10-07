using log4net;
using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
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
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AlertController AlertController { get; }
        private string _uniqueName;
        private int _alertModeMinSellPriceIsUndercutPrice;
        private bool _isEventActive;

        public Alert(AlertController alertController, string uniqueName, int alertModeMinSellPriceIsUndercutPrice)
        {
            AlertController = alertController;
            UniqueName = uniqueName;
            AlertModeMinSellPriceIsUndercutPrice = alertModeMinSellPriceIsUndercutPrice;
        }

        public void StartEvent()
        {
            if (_isEventActive)
            {
                return;
            }
            
            _isEventActive = true;
            AlertEventAsync(UniqueName);
        }

        public void StopEvent()
        {
            _isEventActive = false;
        }

        private async void AlertEventAsync(string uniqueName)
        {
            while (_isEventActive)
            {
                try
                {
                    var cityPrices = await ApiController.GetCityItemPricesFromJsonAsync(uniqueName, null, null).ConfigureAwait(false);

                    foreach (var price in cityPrices ?? new List<MarketResponse>())
                    {
                        if (price.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-500) && price.SellPriceMin <= (ulong)AlertModeMinSellPriceIsUndercutPrice && AlertModeMinSellPriceIsUndercutPrice > 0)
                        {
                            SoundController.PlayAlertSound();
                            StopEvent();
                            AlertController.Remove(uniqueName);
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

        public string UniqueName {
            get => _uniqueName;
            set {
                _uniqueName = value;
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