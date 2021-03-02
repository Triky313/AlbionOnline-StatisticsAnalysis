using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class CombatController
    {
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ObservableCollection<DamageObject> _damageCollection = new ObservableCollection<DamageObject>();
        private readonly ObservableCollection<DateTime> _clusterStarts = new ObservableCollection<DateTime>();

        public CombatController(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddClusterStartTimer()
        {
            _clusterStarts.Add(DateTime.UtcNow);
        }

        public async void AddDamage(long objectId, long causerId, double healthChange)
        {
            var damageValue = (int)Math.Round(healthChange.ToPositive(), MidpointRounding.AwayFromZero);
            // TODO: ObjectId dem Namen zuweisen, anhand von PartyController
            _damageCollection.Add(new DamageObject(causerId, objectId, causerId.ToString(), damageValue));
            // TODO: Einbauen, dass vorhandene Werte addiert werden und nicht immer wieder jeder damage neu hinzugefügt wird

            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }
            
            var damageListByNewestCluster = _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();

            var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserId)
                .Select(x => new DamageObject(x.First().CauserId, x.First().TargetId, x.First().Name, x.Sum(s => s.Damage))).ToList();

            UpdateDamageMeterUi(groupedDamageList);
        }

        public void UpdateDamageMeterUi(List<DamageObject> damageList)
        {
            // Todo: Aktuallisierung nur alle 2-3 Sekunden erlauben
            var highestDamage = GetHighestDamage(damageList);

            foreach (var damageObject in damageList)
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserId == damageObject.CauserId))
                {
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserId == damageObject.CauserId);
                        if (fragment != null)
                        {
                            fragment.Damage = damageObject.Damage;
                            if (highestDamage > fragment.MaximumDamage)
                            {
                                fragment.MaximumDamage = highestDamage;
                            }
                        }
                    });
                }
                else
                {
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindowViewModel.DamageMeter.Add(new DamageMeterFragment() { CauserId = damageObject.CauserId, Damage = damageObject.Damage, MaximumDamage = highestDamage, Name = damageObject.CauserId.ToString() });
                    });
                }
            }
        }
        
        public void RemoveAll()
        {
            _damageCollection.Clear();
            _clusterStarts.Clear();
        }

        private long GetHighestDamage(List<DamageObject> damageObjectList)
        {
            return (damageObjectList.Count <= 0) ? 0 : damageObjectList.Max(x => x.Damage);
        }
    }
}