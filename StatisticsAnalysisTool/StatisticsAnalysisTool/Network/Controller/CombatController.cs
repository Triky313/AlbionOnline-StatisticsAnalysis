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
using System.Threading.Tasks;

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

            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }
            var damageListByNewestCluster = _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();

            var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserId)
                .Select(x => new DamageObject(x.First().CauserId, x.First().TargetId, x.First().Name, x.Sum(s => s.Damage))).ToList();

            await UpdateDamageMeterUiAsync(groupedDamageList);
        }

        public async Task UpdateDamageMeterUiAsync(List<DamageObject> damageList)
        {
            // Todo: Aktuallisierung nur alle 2-3 Sekunden erlauben
            await damageList.ForEachAsync(damageObject =>
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserId == damageObject.CauserId))
                {
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserId == damageObject.CauserId);
                        if (fragment != null)
                        {
                            fragment.Damage = damageObject.Damage;
                            fragment.MaximumDamage = GetHighestDamage();
                        }
                    });
                }
                else
                {
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindowViewModel.DamageMeter.Add(new DamageMeterFragment() { CauserId = damageObject.CauserId, Damage = damageObject.Damage, MaximumDamage = GetHighestDamage(), Name = damageObject.CauserId.ToString() });
                    });
                }
            });
        }
        
        public void RemoveAll()
        {
            _damageCollection.Clear();
            _clusterStarts.Clear();
        }

        private long GetHighestDamage()
        {
            return _damageCollection.Count <= 0 ? 0 : _damageCollection.Max(x =>
            {
                if (x == null)
                {
                    return 0;
                }

                return x.Damage;
            });
        }
    }
}