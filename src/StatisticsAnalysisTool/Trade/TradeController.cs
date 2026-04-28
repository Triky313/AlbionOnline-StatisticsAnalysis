using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using StatisticsAnalysisTool.Diagnostics;

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
        if (IsLastTradeTheSame(trade))
        {
            return;
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.TradeMonitoringBindings?.Trades.Add(trade);
        });

        if (_mainWindowViewModel?.TradeMonitoringBindings != null)
        {
            await _mainWindowViewModel.TradeMonitoringBindings.UpdateFilteredTradesAsync();
        }

        await ServiceLocator.Resolve<SatNotificationManager>().ShowTradeAsync(trade);
    }

    private Trade _lastAddedTrade;

    private bool IsLastTradeTheSame(Trade trade)
    {
        if (_lastAddedTrade is null)
        {
            _lastAddedTrade = trade;
            return false;
        }

        long ticksDifference = Math.Abs(trade.Ticks - _lastAddedTrade.Ticks);

        if (ticksDifference > 500 * TimeSpan.TicksPerMillisecond)
        {
            _lastAddedTrade = null;
            return false;
        }

        if (trade.Id == _lastAddedTrade.Id
            && trade.Guid == _lastAddedTrade.Guid
            && trade.Type == _lastAddedTrade.Type
            && trade.ItemIndex == _lastAddedTrade.ItemIndex)
        {
            _lastAddedTrade = trade;
            return true;
        }

        _lastAddedTrade = trade;
        return false;
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
        tradeBindings.EnsureTradeCollectionViewInitialized();
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

        if (_mainWindowViewModel?.TradeMonitoringBindings != null)
        {
            await _mainWindowViewModel.TradeMonitoringBindings.UpdateFilteredTradesAsync();
        }
    }

    #region Merchant buy and crafting costs 

    private long _buildingObjectId = -1;
    private Trade _upcomingTrade;
    private Trade _lastTrade;
    private readonly HashSet<CraftingBuildingInfo> _craftingBuildingInfos = new();

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
                ClusterIndex = ClusterController.CurrentCluster.SourceClusterIndex ?? ClusterController.CurrentCluster.Index,
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
                ClusterIndex = ClusterController.CurrentCluster.SourceClusterIndex ?? ClusterController.CurrentCluster.Index,
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
                                     "MailContent__TotalDistanceFee;MailContent__TaxRate;MailContent__TaxSetupRate;Amount;" +
                                     "AuctionEntry__UnitPriceSilver;AuctionEntry__TotalDistanceFee;AuctionEntry__TotalPriceSilver;AuctionEntry__Amount;AuctionEntry__Tier;AuctionEntry__IsFinished;" +
                                     "AuctionEntry__AuctionType;AuctionEntry__HasBuyerFetched;AuctionEntry__HasSellerFetched;AuctionEntry__SellerName;" +
                                     "AuctionEntry__ItemTypeId;AuctionEntry__EnchantmentLevel;AuctionEntry__QualityLevel;AuctionEntry__Expires;" +
                                     "InstantBuySellContent__UnitPrice;InstantBuySellContent__Quantity;InstantBuySellContent__TotalDistanceFee;InstantBuySellContent__TaxRate\n";

            return csvHeader + string.Join(Environment.NewLine, _mainWindowViewModel?.TradeMonitoringBindings?.Trades.Select(trade => TradeMapping.Mapping(trade).CsvOutput).ToArray() ?? Array.Empty<string>());
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }

    #endregion

    #region Save / Load data

    public async Task LoadFromFileAsync()
    {
        string filePath = AppDataPaths.UserDataFile(Settings.Default.TradesFileName);

        if (!File.Exists(filePath))
        {
            await SetTradesToBindings(new List<Trade>());
            return;
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath);

            // Migrate old trade data
            var trades = LoadAndMigrateTrades(json);
            await SetTradesToBindings(trades);
        }
        catch (JsonException e)
        {
            string backupPath = BackupCorruptedTradeFile(filePath);
            var message = backupPath is null
                ? $"Trades could not be loaded due to invalid JSON content: {filePath}."
                : $"Trades could not be loaded due to invalid JSON content: {filePath}. Corrupted file was backed up to: {backupPath}.";

            DebugConsole.WriteWarn(MethodBase.GetCurrentMethod()?.DeclaringType, new InvalidDataException(message, e));
            Log.Warning(e, "{message}", message);
            await SetTradesToBindings([]);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            await SetTradesToBindings([]);
        }
    }

    private static string BackupCorruptedTradeFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            var backupPath = $"{filePath}.corrupt.{DateTime.UtcNow:yyyyMMddHHmmss}";
            File.Move(filePath, backupPath, overwrite: false);
            return backupPath;
        }
        catch
        {
            return null;
        }
    }


    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(AppDataPaths.UserDataDirectory);
        await FileController.SaveAsync(_mainWindowViewModel.TradeMonitoringBindings?.Trades?.Select(TradeMapping.Mapping),
            AppDataPaths.UserDataFile(Settings.Default.TradesFileName));
        Log.Information("Trades saved");
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

        DirectoryController.CreateDirectoryWhenNotExists(AppDataPaths.UserDataDirectory);
        await FileController.SaveAsync(tradeDtos,
            AppDataPaths.UserDataFile(Settings.Default.TradesFileName));
        _tradeCounter = 0;
    }

    private async Task SetTradesToBindings(IEnumerable<Trade> trades)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var enumerable = trades as Trade[] ?? trades.ToArray();
            _mainWindowViewModel?.TradeMonitoringBindings?.Trades?.AddRange(enumerable.AsEnumerable());
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Refresh();
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeStatsObject?.SetTradeStats(enumerable);
        }, DispatcherPriority.Background, CancellationToken.None);

        if (_mainWindowViewModel?.TradeMonitoringBindings != null)
        {
            await _mainWindowViewModel.TradeMonitoringBindings.UpdateFilteredTradesAsync();
        }
    }

    #endregion

    #region Migrate old trade data from 02.07.2025 and older

    public static List<Trade> LoadAndMigrateTrades(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        if (!ContainsLegacyTradeProperties(json))
        {
            var currentTradeDtos = JsonSerializer.Deserialize<List<TradeDto>>(json);
            return MapTradeDtos(currentTradeDtos);
        }

        if (JsonNode.Parse(json) is not JsonArray root)
        {
            return [];
        }

        foreach (var node in root)
        {
            if (node?["MailContent"] is not JsonObject mailContent)
            {
                continue;
            }

            if (mailContent.ContainsKey("InternalTotalPrice"))
            {
                var val = mailContent["InternalTotalPrice"]?.GetValue<long>() ?? 0L;
                mailContent["InternalTotalPriceWithoutTax"] = val;
                mailContent.Remove("InternalTotalPrice");
            }

            if (mailContent.ContainsKey("InternalUnitPrice"))
            {
                var val = mailContent["InternalUnitPrice"]?.GetValue<long>() ?? 0L;
                mailContent["InternalUnitPricePaidWithOverpayment"] = val;
                mailContent.Remove("InternalUnitPrice");
            }
        }

        var tradeDtos = JsonSerializer.Deserialize<List<TradeDto>>(root.ToJsonString());
        return MapTradeDtos(tradeDtos);
    }

    private static bool ContainsLegacyTradeProperties(string json)
    {
        return json.Contains("\"InternalTotalPrice\"", StringComparison.Ordinal)
               || json.Contains("\"InternalUnitPrice\"", StringComparison.Ordinal);
    }

    private static List<Trade> MapTradeDtos(IEnumerable<TradeDto> tradeDtos)
    {
        if (tradeDtos == null)
        {
            return [];
        }

        var trades = new List<Trade>();
        foreach (var dto in tradeDtos)
        {
            var mapped = TradeMapping.Mapping(dto);
            if (mapped != null)
            {
                trades.Add(mapped);
            }
        }

        return trades;
    }

    #endregion
}
