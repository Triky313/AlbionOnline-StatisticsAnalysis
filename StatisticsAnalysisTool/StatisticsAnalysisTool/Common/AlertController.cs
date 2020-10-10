using log4net;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
        }

        private void Add(string uniqueName, int alertModeMinSellPriceIsUndercutPrice)
        {
            if (IsAlertInCollection(uniqueName) || !IsSpaceInAlertsCollection())
            {
                return;
            }

            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                }
            };

            var alertController = this;
            var alert = new Alert(_mainWindow, alertController, uniqueName, alertModeMinSellPriceIsUndercutPrice);
            alert.StartEvent();
            _alerts.Add(alert);
        }

        public void Remove(string uniqueName)
        {
            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                }
            };

            var alert = GetAlertByUniqueName(uniqueName);
            if (alert != null)
            {
                alert.StopEvent();
                _alerts.Remove(alert);
                DeactivateAlert(uniqueName);
            }
        }

        public bool ToggleAlert(ref Item item)
        {
            try
            {
                if (IsAlertInCollection(item.UniqueName))
                {
                    Remove(item.UniqueName);
                    return false;
                }
                else
                {
                    Add(item.UniqueName, item.AlertModeMinSellPriceIsUndercutPrice);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(ToggleAlert), e);
                return false;
            }
        }

        private void DeactivateAlert(string uniqueName)
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

        private bool IsAlertInCollection(string uniqueName) => _alerts.Any(alert => alert.UniqueName == uniqueName);

        private Alert GetAlertByUniqueName(string uniqueName)
        {
            return _alerts.FirstOrDefault(alert => alert.UniqueName == uniqueName);
        }

        private bool IsSpaceInAlertsCollection() => _alerts.Count < _maxAlertsAtSameTime;
    }
}