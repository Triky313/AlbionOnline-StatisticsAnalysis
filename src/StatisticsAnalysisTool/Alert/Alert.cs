using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Alert;

public sealed class Alert
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(25);
    private static readonly TimeSpan RateLimitRetryDelay = TimeSpan.FromMinutes(1);
    private readonly AlertController _alertController;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _isStarted;

    public Alert(AlertController alertController, Item item)
    {
        _alertController = alertController ?? throw new ArgumentNullException(nameof(alertController));
        Item = item ?? throw new ArgumentNullException(nameof(item));
        PriceMaximumAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
        AvailabilityMaximumAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
        BlackMarketMaximumAgeMinutes = AlertOptions.DefaultMaximumPriceAgeMinutes;
        PlaySound = true;
    }

    public Item Item { get; }

    public ulong PriceThreshold { get; private set; }

    public uint PriceMaximumAgeMinutes { get; private set; }

    public bool IsPriceAlertActive { get; private set; }

    public uint AvailabilityMaximumAgeMinutes { get; private set; }

    public bool IsAvailabilityAlertActive { get; private set; }

    public ulong BlackMarketBuyOrderThreshold { get; private set; }

    public uint BlackMarketMaximumAgeMinutes { get; private set; }

    public bool IsBlackMarketBuyOrderAlertActive { get; private set; }

    public bool PlaySound { get; private set; }

    public bool HasActiveAlert => IsPriceAlertActive
        || IsAvailabilityAlertActive
        || IsBlackMarketBuyOrderAlertActive;

    public void Start()
    {
        if (_isStarted || !HasActiveAlert)
        {
            return;
        }

        _isStarted = true;
        _ = MonitorAsync(_cancellationTokenSource.Token);
    }

    public void Stop()
    {
        if (!_isStarted)
        {
            return;
        }

        _isStarted = false;
        _cancellationTokenSource.Cancel();
    }

    public void SetPriceAlert(bool isActive, ulong priceThreshold, uint maximumPriceAgeMinutes)
    {
        IsPriceAlertActive = isActive;
        PriceThreshold = priceThreshold;
        PriceMaximumAgeMinutes = GetMaximumPriceAgeMinutes(
            isActive,
            maximumPriceAgeMinutes,
            PriceMaximumAgeMinutes);
    }

    public void SetAvailabilityAlert(bool isActive, uint maximumPriceAgeMinutes)
    {
        IsAvailabilityAlertActive = isActive;
        AvailabilityMaximumAgeMinutes = GetMaximumPriceAgeMinutes(
            isActive,
            maximumPriceAgeMinutes,
            AvailabilityMaximumAgeMinutes);
    }

    public void SetBlackMarketBuyOrderAlert(
        bool isActive,
        ulong minimumBuyOrderPrice,
        uint maximumPriceAgeMinutes)
    {
        IsBlackMarketBuyOrderAlertActive = isActive;
        BlackMarketBuyOrderThreshold = minimumBuyOrderPrice;
        BlackMarketMaximumAgeMinutes = GetMaximumPriceAgeMinutes(
            isActive,
            maximumPriceAgeMinutes,
            BlackMarketMaximumAgeMinutes);
    }

    public void SetPlaySound(bool playSound)
    {
        PlaySound = playSound;
    }

    private async Task MonitorAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && HasActiveAlert)
        {
            try
            {
                var cityPrices = await ApiController
                    .GetCityItemPricesFromJsonAsync(Item.UniqueName)
                    .ConfigureAwait(false);

                ProcessResponses(cityPrices ?? []);

                if (!await WaitForNextPollAsync(PollInterval, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }
            }
            catch (TooManyRequestsException ex)
            {
                Log.Warning(ex, "Item alert rate limited. Item={ItemUniqueName}", Item.UniqueName);

                if (!await WaitForNextPollAsync(RateLimitRetryDelay, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Item alert monitoring failed. Item={ItemUniqueName}", Item.UniqueName);

                if (!await WaitForNextPollAsync(PollInterval, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }
            }
        }
    }

    private void ProcessResponses(IReadOnlyCollection<MarketResponse> marketResponses)
    {
        var referenceTime = DateTime.UtcNow;

        if (IsPriceAlertActive && PriceThreshold > 0)
        {
            var priceResponse = FindPriceThresholdResponse(
                marketResponses,
                PriceThreshold,
                PriceMaximumAgeMinutes,
                referenceTime);

            if (priceResponse != null)
            {
                _alertController.HandleTriggeredAlert(this, ItemAlertType.PriceThreshold, priceResponse);
            }
        }

        if (IsAvailabilityAlertActive)
        {
            var availabilityResponse = FindAvailabilityResponse(
                marketResponses,
                AvailabilityMaximumAgeMinutes,
                referenceTime);

            if (availabilityResponse != null)
            {
                _alertController.HandleTriggeredAlert(this, ItemAlertType.MarketAvailability, availabilityResponse);
            }
        }

        if (IsBlackMarketBuyOrderAlertActive && BlackMarketBuyOrderThreshold > 0)
        {
            var blackMarketResponse = FindBlackMarketBuyOrderResponse(
                marketResponses,
                BlackMarketBuyOrderThreshold,
                BlackMarketMaximumAgeMinutes,
                referenceTime);

            if (blackMarketResponse != null)
            {
                _alertController.HandleTriggeredAlert(this, ItemAlertType.BlackMarketBuyOrder, blackMarketResponse);
            }
        }
    }

    internal static MarketResponse FindPriceThresholdResponse(
        IReadOnlyCollection<MarketResponse> marketResponses,
        ulong priceThreshold,
        uint maximumPriceAgeMinutes,
        DateTime referenceTime)
    {
        var oldestAcceptedPriceDate = GetOldestAcceptedPriceDate(maximumPriceAgeMinutes, referenceTime);

        return marketResponses
            .Where(response => IsValidSellOffer(response, oldestAcceptedPriceDate))
            .Where(response => response.SellPriceMin <= priceThreshold)
            .OrderBy(response => response.SellPriceMin)
            .FirstOrDefault();
    }

    internal static MarketResponse FindAvailabilityResponse(
        IReadOnlyCollection<MarketResponse> marketResponses,
        uint maximumPriceAgeMinutes,
        DateTime referenceTime)
    {
        var oldestAcceptedPriceDate = GetOldestAcceptedPriceDate(maximumPriceAgeMinutes, referenceTime);

        return marketResponses
            .Where(response => IsValidSellOffer(response, oldestAcceptedPriceDate))
            .OrderByDescending(response => response.SellPriceMinDate)
            .FirstOrDefault();
    }

    internal static MarketResponse FindBlackMarketBuyOrderResponse(
        IReadOnlyCollection<MarketResponse> marketResponses,
        ulong minimumBuyOrderPrice,
        uint maximumPriceAgeMinutes,
        DateTime referenceTime)
    {
        var oldestAcceptedPriceDate = GetOldestAcceptedPriceDate(maximumPriceAgeMinutes, referenceTime);

        return marketResponses
            .Where(response => IsValidBlackMarketBuyOrder(response, oldestAcceptedPriceDate))
            .Where(response => response.BuyPriceMax >= minimumBuyOrderPrice)
            .OrderByDescending(response => response.BuyPriceMax)
            .FirstOrDefault();
    }

    internal static async Task<bool> WaitForNextPollAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        await Task.Delay(delay, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        return !cancellationToken.IsCancellationRequested;
    }

    private static uint GetMaximumPriceAgeMinutes(
        bool isActive,
        uint maximumPriceAgeMinutes,
        uint currentMaximumPriceAgeMinutes)
    {
        if (isActive && maximumPriceAgeMinutes == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPriceAgeMinutes));
        }

        return maximumPriceAgeMinutes > 0
            ? maximumPriceAgeMinutes
            : currentMaximumPriceAgeMinutes;
    }

    private static DateTime GetOldestAcceptedPriceDate(uint maximumPriceAgeMinutes, DateTime referenceTime)
    {
        if (maximumPriceAgeMinutes == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumPriceAgeMinutes));
        }

        var maximumPriceAge = TimeSpan.FromMinutes(maximumPriceAgeMinutes);
        return maximumPriceAge >= referenceTime - DateTime.MinValue
            ? DateTime.MinValue
            : referenceTime.Subtract(maximumPriceAge);
    }

    private static bool IsValidSellOffer(MarketResponse marketResponse, DateTime oldestAcceptedPriceDate)
    {
        return marketResponse != null
            && marketResponse.City.GetMarketLocationByLocationNameOrId() != MarketLocation.BlackMarket
            && marketResponse.SellPriceMin > 0
            && marketResponse.SellPriceMinDate > DateTime.MinValue
            && marketResponse.SellPriceMinDate >= oldestAcceptedPriceDate;
    }

    private static bool IsValidBlackMarketBuyOrder(
        MarketResponse marketResponse,
        DateTime oldestAcceptedPriceDate)
    {
        return marketResponse != null
            && marketResponse.City.GetMarketLocationByLocationNameOrId() == MarketLocation.BlackMarket
            && marketResponse.BuyPriceMax > 0
            && marketResponse.BuyPriceMaxDate > DateTime.MinValue
            && marketResponse.BuyPriceMaxDate >= oldestAcceptedPriceDate;
    }
}
