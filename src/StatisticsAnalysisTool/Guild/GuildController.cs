using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Guild;

public class GuildController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private int _currentTabId;

    public GuildController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetTabId(int id)
    {
        _currentTabId = id;
    }

    public void AddSiphonedEnergyEntry(string username, FixPoint quantity, long timestamp, bool isManualEntry = false)
    {
        AddSiphonedEnergyEntries(new List<string> { username }, new List<FixPoint> { quantity }, new List<long> { timestamp }, isManualEntry);
    }

    public void AddSiphonedEnergyEntries(List<string> usernames, List<FixPoint> quantities, List<long> timestamps, bool isManualEntry = false)
    {
        // Siphoned Energy tab is 2
        if ((_currentTabId == 2 && (usernames.Count == quantities.Count && usernames.Count == timestamps.Count)) || isManualEntry)
        {
            for (int i = 0; i < quantities.Count; i++)
            {
                string username = usernames[i];
                FixPoint quantity = quantities[i];
                DateTime timestamp = new DateTime(timestamps[i]);

                var siphonedEnergyEntry = new SiphonedEnergyItem()
                {
                    CharacterName = username,
                    Quantity = quantity,
                    Timestamp = timestamp
                };

                if (!_mainWindowViewModel.GuildBindings.SiphonedEnergyList.ToList().Any(x =>
                        x.CharacterName == siphonedEnergyEntry.CharacterName
                        && x.Quantity.InternalValue == siphonedEnergyEntry.Quantity.InternalValue
                        && x.Timestamp == siphonedEnergyEntry.Timestamp))
                {
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        _mainWindowViewModel.GuildBindings.SiphonedEnergyList.Add(siphonedEnergyEntry);
                        UpdateSiphonedEnergyOverview();
                    });
                }
            }
        }
    }

    public void UpdateSiphonedEnergyOverview()
    {
        var siphonedEnergies = _mainWindowViewModel.GuildBindings.SiphonedEnergyList.ToList();

        var grouped = siphonedEnergies
            .Where(x => x.IsDisabled == false)
            .GroupBy(x => x.CharacterName)
            .Select(item => new SiphonedEnergyItem()
            {
                CharacterName = item.Key,
                Quantity = FixPoint.FromInternalValue(item.Sum(x => x.Quantity.InternalValue)),
                Timestamp = item.Max(x => x.Timestamp)
            })
            .OrderBy(x => x.Timestamp)
            .ToList();

        _mainWindowViewModel.GuildBindings.SiphonedEnergyOverviewList = new ObservableRangeCollection<SiphonedEnergyItem>(grouped);
    }

    public async Task RemoveTradesByIdsAsync(IEnumerable<int> hashCodes)
    {
        await Task.Run(async () =>
        {
            var itemToRemove = _mainWindowViewModel?.GuildBindings?.SiphonedEnergyList?.ToList().Where(x => hashCodes.Contains(x.GetHashCode())).ToList();
            var newList = _mainWindowViewModel?.GuildBindings?.SiphonedEnergyList?.ToList();

            if (itemToRemove != null && itemToRemove.Any())
            {
                foreach (var item in itemToRemove)
                {
                    newList?.Remove(item);
                }
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateSiphonedEnergyList(newList);
            });
        });
    }

    private void UpdateSiphonedEnergyList(IEnumerable<SiphonedEnergyItem> updatedList)
    {
        var guildBindings = _mainWindowViewModel.GuildBindings;
        guildBindings.SiphonedEnergyList.Clear();
        guildBindings.SiphonedEnergyList.AddRange(updatedList);
        guildBindings.SiphonedEnergyCollectionView = CollectionViewSource.GetDefaultView(guildBindings.SiphonedEnergyList) as ListCollectionView;

        UpdateSiphonedEnergyOverview();
    }

    #region Save / Load data

    public async Task LoadFromFileAsync(string guildId)
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GuildFileName));

        var dtos = await FileController.LoadAsync<List<GuildDto>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GuildFileName));
        var guild = dtos.Select(GuildMapping.Mapping);

        _mainWindowViewModel.GuildBindings.SiphonedEnergyList
            = new ObservableRangeCollection<SiphonedEnergyItem>(guild.FirstOrDefault(x => x.GuildId == guildId)?.SiphonedEnergy ?? new List<SiphonedEnergyItem>());
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(_mainWindowViewModel.TradeMonitoringBindings?.Trades?.Select(TradeMapping.Mapping),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.GuildFileName));
        Debug.Print("Guild data saved");
    }

    #endregion
}