using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Alert;

public sealed class AlertController
{
    private const int MaximumSimultaneousAlerts = 10;
    private readonly Dictionary<string, Alert> _alerts = new(StringComparer.Ordinal);
    private readonly ICollectionView _itemsView;

    public AlertController(ICollectionView itemsView)
    {
        _itemsView = itemsView ?? throw new ArgumentNullException(nameof(itemsView));
    }

    public event EventHandler<AlertStateChangedEventArgs> AlertStateChanged;

    public bool IsPriceAlertActive(string uniqueName)
    {
        return GetAlert(uniqueName)?.IsPriceAlertActive == true;
    }

    public bool IsAvailabilityAlertActive(string uniqueName)
    {
        return GetAlert(uniqueName)?.IsAvailabilityAlertActive == true;
    }

    public bool IsBlackMarketBuyOrderAlertActive(string uniqueName)
    {
        return GetAlert(uniqueName)?.IsBlackMarketBuyOrderAlertActive == true;
    }

    public AlertActivationResult SetPriceAlert(
        Item item,
        bool isActive,
        ulong priceThreshold,
        uint maximumPriceAgeMinutes,
        bool playSound)
    {
        var trackedItem = GetTrackedItem(item?.UniqueName);
        if (trackedItem == null)
        {
            return AlertActivationResult.ItemNotFound;
        }

        if (isActive && priceThreshold == 0)
        {
            return AlertActivationResult.InvalidPriceThreshold;
        }

        if (isActive && maximumPriceAgeMinutes == 0)
        {
            return AlertActivationResult.InvalidMaximumPriceAge;
        }

        var alert = GetAlert(trackedItem.UniqueName);
        var shouldStart = isActive && alert == null;
        if (shouldStart)
        {
            if (!HasCapacity)
            {
                return AlertActivationResult.MaximumActiveAlertsReached;
            }

            alert = new Alert(this, trackedItem);
            _alerts.Add(trackedItem.UniqueName, alert);
        }

        if (alert != null)
        {
            alert.SetPriceAlert(isActive, priceThreshold, maximumPriceAgeMinutes);
            alert.SetPlaySound(playSound);
        }

        trackedItem.AlertModeMinSellPriceIsUndercutPrice = priceThreshold;
        SetMaximumPriceAge(
            maximumPriceAgeMinutes,
            value => trackedItem.PriceAlertMaximumPriceAgeMinutes = value);
        trackedItem.IsPriceAlertActive = isActive;

        if (shouldStart)
        {
            alert.Start();
        }

        CompleteAlertStateChange(trackedItem, alert);
        return AlertActivationResult.Success;
    }

    public AlertActivationResult SetAvailabilityAlert(
        Item item,
        bool isActive,
        uint maximumPriceAgeMinutes,
        bool playSound)
    {
        var trackedItem = GetTrackedItem(item?.UniqueName);
        if (trackedItem == null)
        {
            return AlertActivationResult.ItemNotFound;
        }

        if (isActive && maximumPriceAgeMinutes == 0)
        {
            return AlertActivationResult.InvalidMaximumPriceAge;
        }

        var alert = GetAlert(trackedItem.UniqueName);
        var shouldStart = isActive && alert == null;
        if (shouldStart)
        {
            if (!HasCapacity)
            {
                return AlertActivationResult.MaximumActiveAlertsReached;
            }

            alert = new Alert(this, trackedItem);
            _alerts.Add(trackedItem.UniqueName, alert);
        }

        if (alert != null)
        {
            alert.SetAvailabilityAlert(isActive, maximumPriceAgeMinutes);
            alert.SetPlaySound(playSound);
        }

        SetMaximumPriceAge(
            maximumPriceAgeMinutes,
            value => trackedItem.AvailabilityAlertMaximumPriceAgeMinutes = value);
        trackedItem.IsAvailabilityAlertActive = isActive;

        if (shouldStart)
        {
            alert.Start();
        }

        CompleteAlertStateChange(trackedItem, alert);
        return AlertActivationResult.Success;
    }

    public AlertActivationResult SetBlackMarketBuyOrderAlert(
        Item item,
        bool isActive,
        ulong minimumBuyOrderPrice,
        uint maximumPriceAgeMinutes,
        bool playSound)
    {
        var trackedItem = GetTrackedItem(item?.UniqueName);
        if (trackedItem == null)
        {
            return AlertActivationResult.ItemNotFound;
        }

        if (isActive && !BlackMarketItemEligibility.IsEligible(trackedItem))
        {
            return AlertActivationResult.ItemNotBlackMarketEligible;
        }

        if (isActive && minimumBuyOrderPrice == 0)
        {
            return AlertActivationResult.InvalidPriceThreshold;
        }

        if (isActive && maximumPriceAgeMinutes == 0)
        {
            return AlertActivationResult.InvalidMaximumPriceAge;
        }

        var alert = GetAlert(trackedItem.UniqueName);
        var shouldStart = isActive && alert == null;
        if (shouldStart)
        {
            if (!HasCapacity)
            {
                return AlertActivationResult.MaximumActiveAlertsReached;
            }

            alert = new Alert(this, trackedItem);
            _alerts.Add(trackedItem.UniqueName, alert);
        }

        if (alert != null)
        {
            alert.SetBlackMarketBuyOrderAlert(
                isActive,
                minimumBuyOrderPrice,
                maximumPriceAgeMinutes);
            alert.SetPlaySound(playSound);
        }

        trackedItem.BlackMarketBuyOrderAlertThreshold = minimumBuyOrderPrice;
        SetMaximumPriceAge(
            maximumPriceAgeMinutes,
            value => trackedItem.BlackMarketAlertMaximumPriceAgeMinutes = value);
        trackedItem.IsBlackMarketBuyOrderAlertActive = isActive;

        if (shouldStart)
        {
            alert.Start();
        }

        CompleteAlertStateChange(trackedItem, alert);
        return AlertActivationResult.Success;
    }

    public void UpdateSoundPreference(Item item, bool playSound)
    {
        var trackedItem = GetTrackedItem(item?.UniqueName);
        if (trackedItem == null)
        {
            return;
        }

        trackedItem.IsAlertSoundEnabled = playSound;

        var alert = GetAlert(trackedItem.UniqueName);
        if (alert == null)
        {
            return;
        }

        alert.SetPlaySound(playSound);
        SaveActiveAlertsToLocalFile();
        RaiseAlertStateChanged(trackedItem.UniqueName);
    }

    public void HandleTriggeredAlert(Alert alert, ItemAlertType alertType, MarketResponse marketResponse)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            _ = dispatcher.BeginInvoke(() => HandleTriggeredAlert(alert, alertType, marketResponse));
            return;
        }

        if (alert == null
            || marketResponse == null
            || !_alerts.TryGetValue(alert.Item.UniqueName, out var activeAlert)
            || !ReferenceEquals(activeAlert, alert)
            || !IsAlertTypeActive(alert, alertType))
        {
            return;
        }

        switch (alertType)
        {
            case ItemAlertType.PriceThreshold:
                alert.SetPriceAlert(
                    false,
                    alert.PriceThreshold,
                    alert.PriceMaximumAgeMinutes);
                alert.Item.IsPriceAlertActive = false;
                break;
            case ItemAlertType.MarketAvailability:
                alert.SetAvailabilityAlert(
                    false,
                    alert.AvailabilityMaximumAgeMinutes);
                alert.Item.IsAvailabilityAlertActive = false;
                break;
            case ItemAlertType.BlackMarketBuyOrder:
                alert.SetBlackMarketBuyOrderAlert(
                    false,
                    alert.BlackMarketBuyOrderThreshold,
                    alert.BlackMarketMaximumAgeMinutes);
                alert.Item.IsBlackMarketBuyOrderAlertActive = false;
                break;
            default:
                return;
        }

        if (alert.PlaySound)
        {
            SoundController.PlayAlertSound(
                SoundController.GetCurrentSoundPath(SettingsController.CurrentSettings.SelectedAlertSound));
        }

        Application.Current?.MainWindow?.FlashWindow(12);
        var itemAlertWindow = new ItemAlertWindow(new AlertInfos(alert.Item, marketResponse, alertType));
        itemAlertWindow.Show();

        CompleteAlertStateChange(alert.Item, alert);
    }

    public void StopAllAlerts()
    {
        foreach (var alert in _alerts.Values.ToArray())
        {
            alert.SetPriceAlert(
                false,
                alert.PriceThreshold,
                alert.PriceMaximumAgeMinutes);
            alert.SetAvailabilityAlert(
                false,
                alert.AvailabilityMaximumAgeMinutes);
            alert.SetBlackMarketBuyOrderAlert(
                false,
                alert.BlackMarketBuyOrderThreshold,
                alert.BlackMarketMaximumAgeMinutes);
            alert.Stop();
            alert.Item.IsPriceAlertActive = false;
            alert.Item.IsAvailabilityAlertActive = false;
            alert.Item.IsBlackMarketBuyOrderAlertActive = false;
            alert.Item.IsAlertActive = false;
            RaiseAlertStateChanged(alert.Item.UniqueName);
        }

        _alerts.Clear();
    }

    public async Task LoadFromFileAsync()
    {
        var alertSaveObjects = await FileController.LoadAsync<List<AlertSaveObject>>(
            AppDataPaths.UserDataFile(Settings.Default.ActiveAlertsFileName));

        if (alertSaveObjects == null)
        {
            return;
        }

        foreach (var savedAlert in alertSaveObjects)
        {
            var isPriceAlertActive = savedAlert.IsPriceAlertActive
                ?? savedAlert.MinSellUndercutPrice > 0;
            var isBlackMarketAlertActive = savedAlert.IsBlackMarketBuyOrderAlertActive
                && savedAlert.BlackMarketMinimumBuyOrderPrice > 0;

            if (!isPriceAlertActive
                && !savedAlert.IsAvailabilityAlertActive
                && !isBlackMarketAlertActive)
            {
                continue;
            }

            var item = GetTrackedItem(savedAlert.UniqueName);
            if (item == null || !HasCapacity)
            {
                continue;
            }

            if (isBlackMarketAlertActive && !BlackMarketItemEligibility.IsEligible(item))
            {
                isBlackMarketAlertActive = false;
            }

            if (!isPriceAlertActive
                && !savedAlert.IsAvailabilityAlertActive
                && !isBlackMarketAlertActive)
            {
                continue;
            }

            var legacyMaximumPriceAgeMinutes = GetSavedMaximumPriceAge(
                savedAlert.MaximumPriceAgeMinutes,
                AlertOptions.DefaultMaximumPriceAgeMinutes);
            var priceMaximumAgeMinutes = GetSavedMaximumPriceAge(
                savedAlert.PriceMaximumPriceAgeMinutes,
                legacyMaximumPriceAgeMinutes);
            var availabilityMaximumAgeMinutes = GetSavedMaximumPriceAge(
                savedAlert.AvailabilityMaximumPriceAgeMinutes,
                legacyMaximumPriceAgeMinutes);
            var blackMarketMaximumAgeMinutes = GetSavedMaximumPriceAge(
                savedAlert.BlackMarketMaximumPriceAgeMinutes,
                AlertOptions.DefaultMaximumPriceAgeMinutes);
            var playSound = savedAlert.PlaySound ?? true;

            item.AlertModeMinSellPriceIsUndercutPrice = savedAlert.MinSellUndercutPrice;
            item.PriceAlertMaximumPriceAgeMinutes = priceMaximumAgeMinutes;
            item.AvailabilityAlertMaximumPriceAgeMinutes = availabilityMaximumAgeMinutes;
            item.BlackMarketBuyOrderAlertThreshold = savedAlert.BlackMarketMinimumBuyOrderPrice;
            item.BlackMarketAlertMaximumPriceAgeMinutes = blackMarketMaximumAgeMinutes;
            item.IsPriceAlertActive = isPriceAlertActive;
            item.IsAvailabilityAlertActive = savedAlert.IsAvailabilityAlertActive;
            item.IsBlackMarketBuyOrderAlertActive = isBlackMarketAlertActive;
            item.IsAlertSoundEnabled = playSound;
            item.IsAlertActive = true;

            var alert = new Alert(this, item);
            alert.SetPriceAlert(
                isPriceAlertActive,
                savedAlert.MinSellUndercutPrice,
                priceMaximumAgeMinutes);
            alert.SetAvailabilityAlert(
                savedAlert.IsAvailabilityAlertActive,
                availabilityMaximumAgeMinutes);
            alert.SetBlackMarketBuyOrderAlert(
                isBlackMarketAlertActive,
                savedAlert.BlackMarketMinimumBuyOrderPrice,
                blackMarketMaximumAgeMinutes);
            alert.SetPlaySound(playSound);
            _alerts.Add(item.UniqueName, alert);
            alert.Start();
            RaiseAlertStateChanged(item.UniqueName);
        }

        Application.Current?.Dispatcher?.Invoke(_itemsView.Refresh);
    }

    private bool HasCapacity => _alerts.Count < MaximumSimultaneousAlerts;

    private Alert GetAlert(string uniqueName)
    {
        return !string.IsNullOrWhiteSpace(uniqueName)
            && _alerts.TryGetValue(uniqueName, out var alert)
                ? alert
                : null;
    }

    private Item GetTrackedItem(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName)
            || _itemsView.SourceCollection is not IEnumerable<Item> items)
        {
            return null;
        }

        return items.FirstOrDefault(item => string.Equals(item.UniqueName, uniqueName, StringComparison.Ordinal));
    }

    private static bool IsAlertTypeActive(Alert alert, ItemAlertType alertType)
    {
        return alertType switch
        {
            ItemAlertType.PriceThreshold => alert.IsPriceAlertActive,
            ItemAlertType.MarketAvailability => alert.IsAvailabilityAlertActive,
            ItemAlertType.BlackMarketBuyOrder => alert.IsBlackMarketBuyOrderAlertActive,
            _ => false
        };
    }

    private static uint GetSavedMaximumPriceAge(uint? savedValue, uint fallbackValue)
    {
        return savedValue is > 0
            ? savedValue.Value
            : fallbackValue;
    }

    private static void SetMaximumPriceAge(uint maximumPriceAgeMinutes, Action<uint> setValue)
    {
        if (maximumPriceAgeMinutes > 0)
        {
            setValue(maximumPriceAgeMinutes);
        }
    }

    private void CompleteAlertStateChange(Item item, Alert alert)
    {
        if (alert != null && !alert.HasActiveAlert)
        {
            alert.Stop();
            _alerts.Remove(item.UniqueName);
            alert = null;
        }

        item.IsAlertActive = alert?.HasActiveAlert == true;
        SaveActiveAlertsToLocalFile();
        RaiseAlertStateChanged(item.UniqueName);
        Application.Current?.MainWindow?.Dispatcher?.Invoke(_itemsView.Refresh);
    }

    private void RaiseAlertStateChanged(string uniqueName)
    {
        AlertStateChanged?.Invoke(this, new AlertStateChangedEventArgs(uniqueName));
    }

    private void SaveActiveAlertsToLocalFile()
    {
        if (!AppDataPaths.TryEnsureUserDataDirectory())
        {
            Log.Debug("Skipped active alert save because no Albion server is active.");
            return;
        }

        var activeItemAlerts = _alerts.Values.Select(alert => new AlertSaveObject
        {
            UniqueName = alert.Item.UniqueName,
            MinSellUndercutPrice = alert.PriceThreshold,
            MaximumPriceAgeMinutes = alert.PriceMaximumAgeMinutes,
            PriceMaximumPriceAgeMinutes = alert.PriceMaximumAgeMinutes,
            AvailabilityMaximumPriceAgeMinutes = alert.AvailabilityMaximumAgeMinutes,
            IsPriceAlertActive = alert.IsPriceAlertActive,
            IsAvailabilityAlertActive = alert.IsAvailabilityAlertActive,
            BlackMarketMinimumBuyOrderPrice = alert.BlackMarketBuyOrderThreshold,
            BlackMarketMaximumPriceAgeMinutes = alert.BlackMarketMaximumAgeMinutes,
            IsBlackMarketBuyOrderAlertActive = alert.IsBlackMarketBuyOrderAlertActive,
            PlaySound = alert.PlaySound
        }).ToList();

        try
        {
            var fileString = JsonSerializer.Serialize(activeItemAlerts);
            File.WriteAllText(
                AppDataPaths.UserDataFile(Settings.Default.ActiveAlertsFileName),
                fileString,
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "Active item alerts could not be saved");
        }
    }

    private struct AlertSaveObject
    {
        public string UniqueName { get; init; }

        public ulong MinSellUndercutPrice { get; init; }

        public uint? MaximumPriceAgeMinutes { get; init; }

        public uint? PriceMaximumPriceAgeMinutes { get; init; }

        public uint? AvailabilityMaximumPriceAgeMinutes { get; init; }

        public bool? IsPriceAlertActive { get; init; }

        public bool IsAvailabilityAlertActive { get; init; }

        public ulong BlackMarketMinimumBuyOrderPrice { get; init; }

        public uint? BlackMarketMaximumPriceAgeMinutes { get; init; }

        public bool IsBlackMarketBuyOrderAlertActive { get; init; }

        public bool? PlaySound { get; init; }
    }
}
