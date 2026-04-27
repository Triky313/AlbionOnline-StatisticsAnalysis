using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Trade.Market;

public class MarketController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
{
    private ObservableCollection<AuctionEntry> _tempOffers = [];
    private ObservableCollection<AuctionEntry> _tempBuyOrders = [];
    private ObservableCollection<int> _tempNumberToBuyList = [];
    private bool _hasEncryptionInfoBeenShownYet;

    private Dictionary<(string UniqueName, MarketLocation Location, int QualityLevel), MarketResponse> _marketResponses = new();

    #region Buy tracking

    public void AddOffers(IEnumerable<AuctionEntry> auctionOffers)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempOffers = new ObservableCollection<AuctionEntry>(auctionOffers);
    }

    public void AddOffers(IEnumerable<AuctionEntry> auctionOffers, IEnumerable<int> numberToBuyList)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempOffers = new ObservableCollection<AuctionEntry>(auctionOffers);
        _tempNumberToBuyList = new ObservableCollection<int>(numberToBuyList);
    }

    public async Task AddBuyAsync(Purchase purchase)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        var tempOffer = _tempOffers.FirstOrDefault(x => x.Id == purchase.AuctionId);
        if (tempOffer != null)
        {
            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = purchase.Amount,
                InternalUnitPrice = tempOffer.UnitPriceSilver,
                InternalDistanceFee = tempOffer.TotalDistanceFee,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantBuy,
                Id = purchase.AuctionId,
                ClusterIndex = ClusterController.CurrentCluster.SourceClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempOffer,
                InstantBuySellContent = instantBuySellContent
            };

            _ = trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
            await trackingController.TradeController.SaveInFileAfterExceedingLimit(20);
        }
        else if (!_hasEncryptionInfoBeenShownYet)
        {
            mainWindowViewModel.InformationBarText = LocalizationController.Translation("DIRECT_PURCHASE_AND_SALE_DATA_IS_ENCRYPTED");
            mainWindowViewModel.InformationBarVisibility = Visibility.Visible;

            _hasEncryptionInfoBeenShownYet = true;
        }
    }

    public async Task AddBuyAsync(List<long> purchaseIds)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        foreach (long purchaseId in purchaseIds)
        {
            if (_tempOffers.Count <= 0)
            {
                if (!_hasEncryptionInfoBeenShownYet)
                {
                    mainWindowViewModel.InformationBarText = LocalizationController.Translation("DIRECT_PURCHASE_AND_SALE_DATA_IS_ENCRYPTED");
                    mainWindowViewModel.InformationBarVisibility = Visibility.Visible;

                    _hasEncryptionInfoBeenShownYet = true;
                }

                break;
            }

            var tempOffer = _tempOffers.FirstOrDefault(x => x.Id == purchaseId);
            if (tempOffer == null)
            {
                continue;
            }

            var quantity = GetQuantityOfTempNumberToBuyList(purchaseIds, purchaseId);

            if (quantity <= 0)
            {
                continue;
            }

            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = quantity,
                InternalUnitPrice = tempOffer.UnitPriceSilver,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantBuy,
                Id = purchaseId,
                ClusterIndex = ClusterController.CurrentCluster.SourceClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempOffer,
                InstantBuySellContent = instantBuySellContent
            };

            _ = trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
        }

        await trackingController.TradeController.SaveInFileAfterExceedingLimit(20);
    }

    private int GetQuantityOfTempNumberToBuyList(IList<long> purchaseIds, long currentPurchaseId)
    {
        try
        {
            var index = purchaseIds.IndexOf(currentPurchaseId);
            return _tempNumberToBuyList[index];
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        return 0;
    }

    public void ResetTempOffers()
    {
        _tempOffers.Clear();
    }

    public void ResetTempNumberToBuyList()
    {
        _tempNumberToBuyList.Clear();
    }

    #endregion

    #region Sell tracking

    public void AddBuyOrders(IEnumerable<AuctionEntry> auctionOrders)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        _tempBuyOrders = new ObservableCollection<AuctionEntry>(auctionOrders);
    }

    public async Task AddSaleAsync(Sale sale)
    {
        if (!SettingsController.CurrentSettings.IsTradeMonitoringActive)
        {
            return;
        }

        var tempBuyOrder = _tempBuyOrders.FirstOrDefault(x => x.Id == sale.AuctionId);
        if (tempBuyOrder != null)
        {
            var instantBuySellContent = new InstantBuySellContent()
            {
                Quantity = sale.Amount,
                InternalUnitPrice = tempBuyOrder.UnitPriceSilver,
                TaxRate = SettingsController.CurrentSettings.TradeMonitoringMarketTaxRate
            };

            var trade = new Trade()
            {
                Ticks = DateTime.UtcNow.Ticks,
                Type = TradeType.InstantSell,
                Id = sale.AuctionId,
                ClusterIndex = ClusterController.CurrentCluster.SourceClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempBuyOrder,
                InstantBuySellContent = instantBuySellContent
            };

            _ = trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
            await trackingController.TradeController.SaveInFileAfterExceedingLimit(10);
        }
    }

    public void ResetTempBuyOrders()
    {
        _tempBuyOrders.Clear();
    }

    #endregion

    #region Market data

    public void UpdateSellOrderMarketData(IEnumerable<AuctionEntry> auctionOffers)
    {
        foreach (var offer in auctionOffers)
        {
            string locationIndex = ClusterController.CurrentCluster.Index;
            MarketLocation marketLocation = locationIndex.GetMarketLocationByLocationNameOrId();

            if (marketLocation == MarketLocation.Unknown)
            {
                continue;
            }

            var key = (offer.ItemTypeId, marketLocation, offer.QualityLevel);

            ulong sellPrice = (ulong) FixPoint.FromInternalValue(offer.UnitPriceSilver).IntegerValue;
            DateTime expires = offer.Expires;
            DateTime created = expires - TimeSpan.FromDays(30);

            if (!_marketResponses.TryGetValue(key, out var response))
            {
                response = new MarketResponse
                {
                    ItemTypeId = offer.ItemTypeId,
                    City = locationIndex,
                    QualityLevel = offer.QualityLevel,
                    SellPriceMin = sellPrice,
                    SellPriceMax = sellPrice,
                    SellPriceMinDate = created,
                    SellPriceMaxDate = created,
                };
                _marketResponses[key] = response;
            }
            else
            {
                UpdateSellPriceRange(response, sellPrice, created);
            }
        }
    }

    public void UpdateBuyOrderMarketData(IEnumerable<AuctionEntry> auctionOrders)
    {
        foreach (var offer in auctionOrders)
        {
            string locationIndex = ClusterController.CurrentCluster.Index;
            MarketLocation marketLocation = locationIndex.GetMarketLocationByLocationNameOrId();

            if (marketLocation == MarketLocation.Unknown)
            {
                continue;
            }

            var key = (offer.ItemTypeId, marketLocation, offer.QualityLevel);

            ulong buyPrice = (ulong) FixPoint.FromInternalValue(offer.UnitPriceSilver).IntegerValue;
            DateTime expires = offer.Expires;
            DateTime created = expires - TimeSpan.FromDays(30);

            if (!_marketResponses.TryGetValue(key, out var response))
            {
                response = new MarketResponse
                {
                    ItemTypeId = offer.ItemTypeId,
                    City = marketLocation.ToString(),
                    QualityLevel = offer.QualityLevel,
                    BuyPriceMin = buyPrice,
                    BuyPriceMax = buyPrice,
                    BuyPriceMinDate = created,
                    BuyPriceMaxDate = created,
                };
                _marketResponses[key] = response;
            }
            else
            {
                UpdateBuyPriceRange(response, buyPrice, created);
            }
        }
    }

    public async Task<List<MarketResponse>> GetResponsesForItem(string uniqueName)
    {
        var result = _marketResponses
            .Where(x => x.Key.UniqueName == uniqueName)
            .Select(x => x.Value)
            .ToList();

        return await Task.FromResult(result);
    }

    #endregion

    #region Save / Load data

    public async Task LoadFromFileAsync()
    {
        var marketDtos = await FileController.LoadAsync<List<MarketDto>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.MarketFileName));

        _marketResponses = new Dictionary<(string UniqueName, MarketLocation Location, int QualityLevel), MarketResponse>();

        int ignoredDuplicateCount = 0;
        foreach (var marketDto in marketDtos)
        {
            var marketResponse = MarketMapping.Mapping(marketDto);
            if (!TryAddLoadedMarketResponse(marketResponse))
            {
                ignoredDuplicateCount++;
            }
        }

        if (ignoredDuplicateCount > 0)
        {
            Log.Warning("{count} duplicate market entries were ignored while loading {file}.", ignoredDuplicateCount, Settings.Default.MarketFileName);
        }
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));

        DateTime now = DateTime.UtcNow;

        var marketData = _marketResponses.Values
            .Where(x =>
                (x.SellPriceMinDate.AddDays(30) > now) ||
                (x.SellPriceMaxDate.AddDays(30) > now) ||
                (x.BuyPriceMinDate.AddDays(30) > now) ||
                (x.BuyPriceMaxDate.AddDays(30) > now)
            )
            .Select(MarketMapping.Mapping)
            .ToList();

        await FileController.SaveAsync(marketData, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.MarketFileName));
        Log.Information("Market data saved");
    }

    #endregion

    #region Persistence helpers

    private bool TryAddLoadedMarketResponse(MarketResponse marketResponse)
    {
        var key = CreateMarketResponseKey(marketResponse.ItemTypeId, marketResponse.City, marketResponse.QualityLevel);
        if (!_marketResponses.TryGetValue(key, out var existingResponse))
        {
            _marketResponses[key] = marketResponse;
            return true;
        }

        if (AreEquivalentExceptCity(existingResponse, marketResponse))
        {
            return false;
        }

        MergeMarketResponse(existingResponse, marketResponse);
        return true;
    }

    private static (string UniqueName, MarketLocation Location, int QualityLevel) CreateMarketResponseKey(string uniqueName, string city, int qualityLevel)
    {
        return (uniqueName, city.GetMarketLocationByLocationNameOrId(), qualityLevel);
    }

    private static bool AreEquivalentExceptCity(MarketResponse left, MarketResponse right)
    {
        return string.Equals(left.ItemTypeId, right.ItemTypeId, StringComparison.Ordinal)
               && left.QualityLevel == right.QualityLevel
               && left.SellPriceMin == right.SellPriceMin
               && left.SellPriceMinDate == right.SellPriceMinDate
               && left.SellPriceMax == right.SellPriceMax
               && left.SellPriceMaxDate == right.SellPriceMaxDate
               && left.BuyPriceMin == right.BuyPriceMin
               && left.BuyPriceMinDate == right.BuyPriceMinDate
               && left.BuyPriceMax == right.BuyPriceMax
               && left.BuyPriceMaxDate == right.BuyPriceMaxDate;
    }

    private static void MergeMarketResponse(MarketResponse target, MarketResponse source)
    {
        UpdateSellPriceRange(target, source.SellPriceMin, source.SellPriceMinDate);
        UpdateSellPriceRange(target, source.SellPriceMax, source.SellPriceMaxDate);
        UpdateBuyPriceRange(target, source.BuyPriceMin, source.BuyPriceMinDate);
        UpdateBuyPriceRange(target, source.BuyPriceMax, source.BuyPriceMaxDate);
    }

    #endregion

    #region Market response helpers

    private static void UpdateSellPriceRange(MarketResponse response, ulong sellPrice, DateTime created)
    {
        if (sellPrice <= 0)
        {
            return;
        }

        if (response.SellPriceMin == 0 || sellPrice < response.SellPriceMin || created > response.SellPriceMinDate)
        {
            response.SellPriceMin = sellPrice;
            response.SellPriceMinDate = created;
        }

        if (sellPrice > response.SellPriceMax || created > response.SellPriceMaxDate)
        {
            response.SellPriceMax = sellPrice;
            response.SellPriceMaxDate = created;
        }
    }

    private static void UpdateBuyPriceRange(MarketResponse response, ulong buyPrice, DateTime created)
    {
        if (buyPrice <= 0)
        {
            return;
        }

        if (response.BuyPriceMin == 0 || buyPrice < response.BuyPriceMin || created > response.BuyPriceMinDate)
        {
            response.BuyPriceMin = buyPrice;
            response.BuyPriceMinDate = created;
        }

        if (buyPrice > response.BuyPriceMax || created > response.BuyPriceMaxDate)
        {
            response.BuyPriceMax = buyPrice;
            response.BuyPriceMaxDate = created;
        }
    }

    #endregion
}