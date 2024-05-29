using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;

namespace StatisticsAnalysisTool.Alert;

public class Alert
{
    private int _alertModeMinSellPriceIsUndercutPrice;
    private bool _isEventActive;
    private Item _item;

    public Alert(AlertController alertController, Item item, int alertModeMinSellPriceIsUndercutPrice)
    {
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
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.MainWindow.FlashWindow(12);
        });
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
                    if (marketResponse.City.GetMarketLocationByLocationNameOrId() != MarketLocation.BlackMarket
                        && marketResponse.SellPriceMinDate >= DateTime.UtcNow.AddMinutes(-5)
                        && marketResponse.SellPriceMin <= (ulong) AlertModeMinSellPriceIsUndercutPrice
                        && AlertModeMinSellPriceIsUndercutPrice > 0)
                    {
                        SoundController.PlayAlertSound(SoundController.GetCurrentSoundPath(SettingsController.CurrentSettings.SelectedAlertSound));
                        StopEvent();
                        AlertController.DeactivateAlert(uniqueName);

                        Application.Current.Dispatcher.Invoke(() =>
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
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
            catch (TooManyRequestsException e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
                return;
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}