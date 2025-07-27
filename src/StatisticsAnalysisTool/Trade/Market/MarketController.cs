using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
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

    #region Buy from market

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
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
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
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                AuctionEntry = tempOffer,
                InstantBuySellContent = instantBuySellContent
            };

            _ = trackingController.TradeController.AddTradeToBindingCollectionAsync(trade);
        }

        await trackingController.TradeController.SaveInFileAfterExceedingLimit(20);
    }

    public void ResetTempOffers()
    {
        _tempOffers.Clear();
    }

    public void ResetTempNumberToBuyList()
    {
        _tempNumberToBuyList.Clear();
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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        return 0;
    }

    #endregion

    #region Sell to market

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
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
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

    #region Market price tracking

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
        var marketDto = await FileController.LoadAsync<List<MarketDto>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.MarketFileName));
        _marketResponses = marketDto.ToDictionary(
            dto => (dto.UniqueName, dto.City.GetMarketLocationByLocationNameOrId(), dto.QualityLevel),
            dto => new MarketResponse
            {
                ItemTypeId = dto.UniqueName,
                City = dto.City,
                QualityLevel = dto.QualityLevel,
                SellPriceMin = dto.SellPriceMin,
                SellPriceMinDate = dto.SellPriceMinDate,
                SellPriceMax = dto.SellPriceMax,
                SellPriceMaxDate = dto.SellPriceMaxDate,
                BuyPriceMin = dto.BuyPriceMin,
                BuyPriceMinDate = dto.BuyPriceMinDate,
                BuyPriceMax = dto.BuyPriceMax,
                BuyPriceMaxDate = dto.BuyPriceMaxDate
            });
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
}