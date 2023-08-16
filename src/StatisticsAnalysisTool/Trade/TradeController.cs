using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Trade;

public class TradeController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private int _tradeCounter;

    public TradeController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        if (_mainWindowViewModel?.TradeMonitoringBindings?.Trades != null)
        {
            _mainWindowViewModel.TradeMonitoringBindings.Trades.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        _mainWindowViewModel?.TradeMonitoringBindings?.TradeStatsObject.SetTradeStats(_mainWindowViewModel?.TradeMonitoringBindings?.Trades);
    }

    public async Task AddTradeToBindingCollectionAsync(Trade trade)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel?.TradeMonitoringBindings?.Trades.Add(trade);
            _mainWindowViewModel?.TradeMonitoringBindings?.TradeCollectionView?.Refresh();
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