using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Alert;

public sealed class AlertController
{
    private readonly ObservableCollection<Alert> _alerts = new();
    private readonly ICollectionView _itemsView;

    private const int TicksMaxAlertsAtSameTime = 10;

    public AlertController(ICollectionView itemsView)
    {
        _itemsView = itemsView;

        _ = LoadFromFileAsync();
    }

    private void Add(Item item, int alertModeMinSellPriceIsUndercutPrice)
    {
        if (IsAlertInCollection(item.UniqueName) || !IsSpaceInAlertsCollection())
        {
            return;
        }

        _alerts.CollectionChanged += delegate (object _, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SaveActiveAlertsToLocalFile();
            }
        };

        var alertController = this;
        var alert = new Alert(alertController, item, alertModeMinSellPriceIsUndercutPrice);
        alert.StartEvent();
        _alerts.Add(alert);
    }

    private void Remove(string uniqueName)
    {
        _alerts.CollectionChanged += delegate (object _, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                SaveActiveAlertsToLocalFile();
            }
        };

        var alert = GetAlertByUniqueName(uniqueName);
        if (alert != null)
        {
            alert.StopEvent();
            _alerts.Remove(alert);
        }
    }

    public bool ToggleAlert(ref Item item)
    {
        try
        {
            if (!IsAlertInCollection(item.UniqueName) && !IsSpaceInAlertsCollection())
            {
                return false;
            }

            if (IsAlertInCollection(item.UniqueName))
            {
                DeactivateAlert(item.UniqueName);
                return false;
            }

            ActivateAlert(item.UniqueName, item.AlertModeMinSellPriceIsUndercutPrice);
            return true;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }

    public void DeactivateAlert(string uniqueName)
    {
        try
        {
            var itemCollection = (ObservableCollection<Item>) _itemsView.SourceCollection;
            var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

            if (item == null) return;

            item.IsAlertActive = false;
            Remove(item.UniqueName);

            Application.Current.MainWindow?.Dispatcher?.Invoke(_itemsView.Refresh);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private void ActivateAlert(string uniqueName, int minSellUndercutPrice)
    {
        try
        {
            var itemCollection = (ObservableCollection<Item>) _itemsView.SourceCollection;
            var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

            if (item == null) return;

            item.IsAlertActive = true;
            item.AlertModeMinSellPriceIsUndercutPrice = minSellUndercutPrice;
            Add(item, item.AlertModeMinSellPriceIsUndercutPrice);

            Application.Current.MainWindow?.Dispatcher?.Invoke(_itemsView.Refresh);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private bool IsAlertInCollection(string uniqueName)
    {
        return _alerts.Any(alert => alert.Item.UniqueName == uniqueName);
    }

    private Alert GetAlertByUniqueName(string uniqueName)
    {
        return _alerts.FirstOrDefault(alert => alert.Item.UniqueName == uniqueName);
    }

    public bool IsSpaceInAlertsCollection()
    {
        return _alerts.Count < TicksMaxAlertsAtSameTime;
    }

    #region Load / Save local file data

    private async Task LoadFromFileAsync()
    {
        var alertSaveObjectList = await FileController.LoadAsync<List<AlertSaveObject>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.ActiveAlertsFileName));

        if (alertSaveObjectList != null)
        {
            foreach (var alert in alertSaveObjectList)
            {
                ActivateAlert(alert.UniqueName, alert.MinSellUndercutPrice);
            }
        }
    }

    private void SaveActiveAlertsToLocalFile()
    {
        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.ActiveAlertsFileName);
        var activeItemAlerts = _alerts.Select(alert => new AlertSaveObject
        { UniqueName = alert.Item.UniqueName, MinSellUndercutPrice = alert.AlertModeMinSellPriceIsUndercutPrice }).ToList();
        var fileString = JsonSerializer.Serialize(activeItemAlerts);

        try
        {
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private struct AlertSaveObject
    {
        public string UniqueName { get; init; }

        public int MinSellUndercutPrice { get; init; }
    }

    #endregion
}