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
        private readonly TrackingController _trackingController;
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ObservableCollection<DamageObject> _damageCollection = new ObservableCollection<DamageObject>();
        private readonly ObservableCollection<DateTime> _clusterStarts = new ObservableCollection<DateTime>();

        public CombatController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddClusterStartTimer()
        {
            _clusterStarts.Add(DateTime.UtcNow);
        }

        public void AddDamage(long objectId, long causerId, double healthChange)
        {
            // TODO: Damage unter 0 weg lassen und nicht hinzufügen
            var damageValue = (int)Math.Round(healthChange.ToPositive(), MidpointRounding.AwayFromZero);
            var causerName = GetUsernameByObjectId(causerId);

            AddInternalDamage(causerId, causerName, damageValue);

            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }
            
            var damageListByNewestCluster = _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();

            var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserId)
                .Select(x => new DamageObject(x.First().CauserId, x.First().TargetId, x.First().CauserName, x.Sum(s => s.Damage))).ToList();

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
                            fragment.MaximumDamage = highestDamage;
                            fragment.Damage = damageObject.Damage;
                        }
                    });
                }
                else
                {
                    _mainWindow.Dispatcher?.Invoke(() =>
                    {
                        _mainWindowViewModel.DamageMeter.Add(new DamageMeterFragment() { CauserId = damageObject.CauserId, Damage = damageObject.Damage, MaximumDamage = highestDamage, Name = damageObject.CauserName });
                    });
                }
            }
        }
        
        public void RemoveAll()
        {
            _damageCollection.Clear();
            _clusterStarts.Clear();
            // TODO: DamageUi reset adden
        }

        private void AddInternalDamage(long causerId, string causerName, int damage)
        {
            var dmgObject = _damageCollection.FirstOrDefault(x => x.CauserId == causerId);
            if (dmgObject != null)
            {
                dmgObject.CauserName = causerName;
                dmgObject.Damage += damage;
                return;
            }

            _damageCollection.Add(new DamageObject(causerId, null, causerName, damage));
        }

        private string GetUsernameByObjectId(long objectId)
        {
            if (_trackingController?.LocalUserData?.UserObjectId == objectId && !string.IsNullOrEmpty(_trackingController?.LocalUserData?.Username))
            {
                return _trackingController?.LocalUserData?.Username;
            }

            return "Unknown";
            // TODO: Party user auslesen und der ObjectID zuordnen
        }

        private long GetHighestDamage(List<DamageObject> damageObjectList)
        {
            return (damageObjectList.Count <= 0) ? 0 : damageObjectList.Max(x => x.Damage);
        }
    }
}