using FontAwesome.WPF;
using log4net;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Common
{
    public class AlertController
    {
        private readonly ObservableCollection<Alert> _alerts = new ObservableCollection<Alert>();
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private void Add(Item item)
        {
            if (IsAlertInCollection(item.UniqueName))
            {
                return;
            }

            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                }
            };

            var alert = new Alert(item.UniqueName);
            alert.StartEvent();

            _alerts.Add(alert);
            Debug.Print($"Added alert for {item.UniqueName}");
        }

        private void Remove(Item item)
        {
            _alerts.CollectionChanged += delegate (object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                }
            };

            var alert = GetAlertByUniqueName(item.UniqueName);
            if (alert != null)
            {
                alert.StopEvent();

                _alerts.Remove(alert);
            }
            Debug.Print($"Remove alert for {item.UniqueName}");
        }

        public bool ToggleAlert(ref ImageAwesome imageAwesome, ref Item item)
        {
            try
            {
                if (IsAlertInCollection(item.UniqueName))
                {
                    Remove(item);
                    imageAwesome.Icon = FontAwesomeIcon.ToggleOff;
                    imageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Text.Normal"]);
                    return false;
                }
                else
                {
                    Add(item);
                    imageAwesome.Icon = FontAwesomeIcon.ToggleOn;
                    imageAwesome.Foreground = new SolidColorBrush((Color)Application.Current.Resources["Color.Blue.2"]);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(ToggleAlert), e);
                return false;
            }
        }
        
        private bool IsAlertInCollection(string uniqueName) => _alerts.Any(alert => alert.UniqueName == uniqueName);

        private Alert GetAlertByUniqueName(string uniqueName)
        {
            return _alerts.FirstOrDefault(alert => alert.UniqueName == uniqueName);
        }
    }
}