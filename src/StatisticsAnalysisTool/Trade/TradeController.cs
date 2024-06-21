using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Serilog;

namespace StatisticsAnalysisTool.Trade;

public class TradeController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private int _tradeCounter;

    public TradeController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        if (_mainWindowViewModel?.TradeMonitoringBindings?.Trades != null)
        {
            _mainWindowViewModel.TradeMonitoringBindings.Trades.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _mainWindowViewModel?.TradeMonitoringBindings?.TradeStatsObject.SetTradeStats(_mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Cast<Trade>().ToList());
    }

    public async Task AddTradeToBindingCollectionAsync(Trade trade)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.TradeMonitoringBindings?.Trades.Add(trade);
        });

        await ServiceLocator.Resolve<SatNotificationManager>().ShowTradeAsync(trade);
    }

    public async Task RemoveTradesByIdsAsync(IEnumerable<long> ids)
    {
        await Task.Run(async () =>
        {
            var tradesToRemove = _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.ToList().Where(x => ids.Contains(x.Id)).ToList();
            var newList = _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.ToList();

            if (tradesToRemove != null && tradesToRemove.Any())
            {
                foreach (var trade in tradesToRemove)
                {
                    newList?.Remove(trade);
                }
            }

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                await UpdateTradesAsync(newList);
            });
        });
    }

    private async Task UpdateTradesAsync(IEnumerable<Trade> updatedList)
    {
        var tradeBindings = _mainWindowViewModel.TradeMonitoringBindings;
        tradeBindings.Trades.Clear();
        tradeBindings.Trades.AddRange(updatedList);
        tradeBindings.TradeCollectionView = CollectionViewSource.GetDefaultView(tradeBindings.Trades) as ListCollectionView;
        await tradeBindings.UpdateFilteredTradesAsync();

        tradeBindings.TradeStatsObject.SetTradeStats(tradeBindings.TradeCollectionView?.Cast<Trade>().ToList());

        tradeBindings.UpdateTotalTradesUi(null, null);
        tradeBindings.UpdateCurrentTradesUi(null, null);
    }

    public async Task RemoveTradesByDaysInSettingsAsync()
    {
        var deleteAfterDays = SettingsController.CurrentSettings?.DeleteTradesOlderThanSpecifiedDays ?? 0;
        if (deleteAfterDays <= 0)
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var mail in _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.ToList()
                         .Where(x => x?.Timestamp.AddDays(deleteAfterDays) < DateTime.UtcNow)!)
            {
                _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.Remove(mail);
            }
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeStatsObject?.SetTradeStats(_mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Cast<Trade>().ToList());

            _mainWindowViewModel?.TradeMonitoringBindings?.UpdateTotalTradesUi(null, null);
            _mainWindowViewModel?.TradeMonitoringBindings?.UpdateCurrentTradesUi(null, null);
        });
    }

    #region Merchant buy and crafting costs 

    private long _buildingObjectId = -1;
    private Trade _upcomingTrade;
    private Trade _lastTrade;
    private readonly HashSet<CraftingBuildingInfo> _craftingBuildingInfos = new ();

    public void RegisterBuilding(long buildingObjectId)
    {
        _buildingObjectId = buildingObjectId;
    }

    public void UnregisterBuilding(long buildingObjectId)
    {
        if (buildingObjectId != _buildingObjectId)
        {
            return;
        }

        _buildingObjectId = -1;
        _upcomingTrade = null;
    }

    public void AddCraftingBuildingInfo(CraftingBuildingInfo craftingBuildingInfo)
    {
        _craftingBuildingInfos.Add(craftingBuildingInfo);
    }

    public void ResetCraftingBuildingInfo()
    {
        _craftingBuildingInfos.Clear();
    }

    public void SetUpcomingTrade(long buildingObjectId, long dateTimeTicks, long internalTotalPrice, int quantity, int itemIndex)
    {
        if (_buildingObjectId != buildingObjectId || quantity <= 0 || internalTotalPrice <= 0)
        {
            return;
        }

        var craftingBuildingInfo = _craftingBuildingInfos?.FirstOrDefault(x => x.ObjectId == buildingObjectId);
        if (craftingBuildingInfo == null)
        {
            return;
        }

        var unitPrice = internalTotalPrice / quantity;

        if (CraftingBuildingData.DoesCraftingBuildingNameFit(craftingBuildingInfo.BuildingName, new List<CraftingBuildingName>
            {
                CraftingBuildingName.Forge, CraftingBuildingName.HuntersLodge,
                CraftingBuildingName.MagicItems, CraftingBuildingName.ToolMaker,
                CraftingBuildingName.Alchemist, CraftingBuildingName.Cook
            }))
        {
            _upcomingTrade = new Trade()
            {
                Ticks = dateTimeTicks,
                Type = TradeType.Crafting,
                Id = dateTimeTicks,
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                Guid = Guid.NewGuid(),
                ItemIndex = itemIndex,
                InstantBuySellContent = new InstantBuySellContent()
                {
                    InternalUnitPrice = unitPrice,
                    Quantity = quantity,
                    TaxRate = 0
                }
            };
        }

        if (CraftingBuildingData.DoesCraftingBuildingNameFit(craftingBuildingInfo.BuildingName, new List<CraftingBuildingName>
            {
                CraftingBuildingName.FarmingMerchant
            }))
        {
            _upcomingTrade = new Trade()
            {
                Ticks = dateTimeTicks,
                Type = TradeType.InstantBuy,
                Id = dateTimeTicks,
                ClusterIndex = ClusterController.CurrentCluster.MainClusterIndex ?? ClusterController.CurrentCluster.Index,
                Guid = Guid.NewGuid(),
                ItemIndex = itemIndex,
                InstantBuySellContent = new InstantBuySellContent()
                {
                    InternalUnitPrice = unitPrice,
                    Quantity = quantity,
                    TaxRate = 0
                }
            };
        }
    }

    public async Task TradeFinishedAsync(long userObjectId, long buildingObjectId)
    {
        if (_upcomingTrade == _lastTrade 
            || _trackingController.EntityController.LocalUserData.UserObjectId != userObjectId 
            || _upcomingTrade is null 
            || _buildingObjectId != buildingObjectId)
        {
            return;
        }

        await AddTradeToBindingCollectionAsync(_upcomingTrade);
        _lastTrade = _upcomingTrade;
    }

    #endregion

    #region Export

    public string GetTradesAsCsv()
    {
        try
        {
            const string csvHeader = "Ticks;ClusterIndex;Description;Type;ItemName;MailTypeText;" +
                                     "MailContent__UsedQuantity;MailContent__Quantity;MailContent__UniqueItemName;MailContent__TotalPrice;MailContent__UnitPrice;" +
                                     "MailContent__TaxRate;MailContent__TaxSetupRate;Amount;" +
                                     "AuctionEntry__UnitPriceSilver;AuctionEntry__TotalPriceSilver;AuctionEntry__Amount;AuctionEntry__Tier;AuctionEntry__IsFinished;" +
                                     "AuctionEntry__AuctionType;AuctionEntry__HasBuyerFetched;AuctionEntry__HasSellerFetched;" +
                                     "AuctionEntry__ItemTypeId;AuctionEntry__EnchantmentLevel;AuctionEntry__QualityLevel;AuctionEntry__Expires;" +
                                     "InstantBuySellContent__UnitPrice;InstantBuySellContent__Quantity;InstantBuySellContent__TaxRate\n";
            
            return csvHeader + string.Join(Environment.NewLine, _mainWindowViewModel?.TradeMonitoringBindings?.Trades.Select(trade => TradeMapping.Mapping(trade).CsvOutput).ToArray() ?? Array.Empty<string>());
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }

    #endregion

    #region Save / Load data

    public async Task LoadFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.TradesFileName));

        var tradeDtos = await FileController.LoadAsync<List<TradeDto>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.TradesFileName));
        var trades = tradeDtos.Select(TradeMapping.Mapping).ToList();

        await SetTradesToBindings(trades);
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_mainWindowViewModel.TradeMonitoringBindings?.Trades?.Select(TradeMapping.Mapping),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.TradesFileName));
        Debug.Print("Trades saved");
    }

    public async Task SaveInFileAfterExceedingLimit(int limit)
    {
        if (++_tradeCounter < limit)
        {
            return;
        }

        if (_mainWindowViewModel?.TradeMonitoringBindings?.Trades == null)
        {
            return;
        }

        var tradeMonitoringBindingsTrade = _mainWindowViewModel.TradeMonitoringBindings.Trades;
        var tradeDtos = tradeMonitoringBindingsTrade?.Select(TradeMapping.Mapping).ToList();

        if (tradeDtos == null)
        {
            return;
        }

        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(tradeDtos,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.TradesFileName));
        _tradeCounter = 0;
    }

    private async Task SetTradesToBindings(IEnumerable<Trade> trades)
    {
        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
        {
            var enumerable = trades as Trade[] ?? trades.ToArray();
            _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.AddRange(enumerable.AsEnumerable());
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Refresh();
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeStatsObject?.SetTradeStats(enumerable);
        }, DispatcherPriority.Background, CancellationToken.None);
    }

    #endregion
}