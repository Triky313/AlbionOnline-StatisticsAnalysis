using log4net;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StatisticsAnalysisTool.Common
{
    public class AlertController
    {
        private readonly MainWindow _mainWindow;
        private readonly ICollectionView _itemsView;
        private readonly ObservableCollection<Alert> _alerts = new ObservableCollection<Alert>();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _maxAlertsAtSameTime = 10;

        public AlertController(MainWindow mainWindow, ICollectionView itemsView)
        {
            _mainWindow = mainWindow;
            _itemsView = itemsView;

            SetActiveAlertsFromLocalFile();
        }

        private void Add(Item item, int alertModeMinSellPriceIsUndercutPrice)
        {
            if (IsAlertInCollection(item.UniqueName) || !IsSpaceInAlertsCollection())
            {
                return;
            }

            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    SaveActiveAlertsToLocalFile();
                }
            };

            var alertController = this;
            var alert = new Alert(_mainWindow, alertController, item, alertModeMinSellPriceIsUndercutPrice);
            alert.StartEvent();
            _alerts.Add(alert);
        }

        private void Remove(string uniqueName)
        {
            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
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
                Log.Error(nameof(ToggleAlert), e);
                return false;
            }
        }

        public void DeactivateAlert(string uniqueName)
        {
            try
            {
                var itemCollection = (ObservableCollection<Item>)_itemsView.SourceCollection;
                var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

                if (item == null)
                {
                    return;
                }

                item.IsAlertActive = false;
                Remove(item.UniqueName);

                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    _itemsView.Refresh();
                });
            }
            catch (Exception e)
            {
                Log.Error(nameof(DeactivateAlert), e);
            }
        }

        private void ActivateAlert(string uniqueName, int minSellUndercutPrice)
        {
            try
            {
                var itemCollection = (ObservableCollection<Item>)_itemsView.SourceCollection;
                var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

                if (item == null)
                {
                    return;
                }

                item.IsAlertActive = true;
                item.AlertModeMinSellPriceIsUndercutPrice = minSellUndercutPrice;
                Add(item, item.AlertModeMinSellPriceIsUndercutPrice);

                _mainWindow.Dispatcher?.Invoke(() =>
                {
                    _itemsView.Refresh();
                });
            }
            catch (Exception e)
            {
                Log.Error(nameof(DeactivateAlert), e);
            }
        }

        private bool IsAlertInCollection(string uniqueName) => _alerts.Any(alert => alert.Item.UniqueName == uniqueName);

        private Alert GetAlertByUniqueName(string uniqueName)
        {
            return _alerts.FirstOrDefault(alert => alert.Item.UniqueName == uniqueName);
        }

        public bool IsSpaceInAlertsCollection() => _alerts.Count < _maxAlertsAtSameTime;

        #region Alert file controls

        private void SetActiveAlertsFromLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ActiveAlertsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);

                    foreach (var alert in JsonConvert.DeserializeObject<List<AlertSaveObject>>(localItemString))
                    {
                        ActivateAlert(alert.UniqueName, alert.MinSellUndercutPrice);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(nameof(SetActiveAlertsFromLocalFile), e);
                }
            } 
            else
            {
                Log.Info($"{nameof(SetActiveAlertsFromLocalFile)}: No active alerts.");
            }
        }

        private void SaveActiveAlertsToLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ActiveAlertsFileName}";
            var activeItemAlerts = _alerts.Select(alert => new AlertSaveObject() { UniqueName = alert.Item.UniqueName, MinSellUndercutPrice = alert.AlertModeMinSellPriceIsUndercutPrice }).ToList();
            var fileString = JsonConvert.SerializeObject(activeItemAlerts);

            try
            {
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.Error(nameof(SaveActiveAlertsToLocalFile), e);
            }
        }

        private struct AlertSaveObject
        {
            public string UniqueName { get; set; }

            public int MinSellUndercutPrice { get; set; }
        }

        #endregion
    }
}