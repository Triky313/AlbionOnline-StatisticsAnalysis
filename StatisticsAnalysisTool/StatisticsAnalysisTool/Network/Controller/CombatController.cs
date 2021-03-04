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

        public event Action<bool, bool> OnChangeCombatMode;

        public void UpdateCombatMode(bool inActiveCombat, bool inPassiveCombat)
        {
            OnChangeCombatMode?.Invoke(inActiveCombat, inPassiveCombat);
        }

        public void AddClusterStartTimer()
        {
            _clusterStarts.Add(DateTime.UtcNow);
        }

        public void AddDamage(long causerId, double healthChange)
        {
            if (!IsObjectIdInPartyOrLocalPlayer(causerId))
            {
                return;
            }

            var damageValue = (int)Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageValue <= 0)
            {
                return;
            }

            var causerName = GetUsernameByObjectId(causerId);

            AddInternalDamage(causerId, causerName, damageValue);

            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }

            if (IsUiUpdateAllowed())
            {
                var damageListByNewestCluster = _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();
                var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserId)
                    .Select(x => new DamageObject(x.First().CauserId, x.First().TargetId, x.First().CauserName, x.Sum(s => s.Damage))).ToList();

                UpdateDamageMeterUi(groupedDamageList);
            }
        }

        public void UpdateDamageMeterUi(List<DamageObject> damageList)
        {
            var highestDamage = GetHighestDamage(damageList);

            foreach (var damageObject in damageList)
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserId == damageObject.CauserId))
                {
                    _mainWindow.Dispatcher?.InvokeAsync(() =>
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
                    _mainWindow.Dispatcher?.InvokeAsync(() =>
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

            _mainWindow?.Dispatcher?.InvokeAsync(() =>
            {
                _mainWindowViewModel?.DamageMeter?.Clear();
            });
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

        private bool IsObjectIdInPartyOrLocalPlayer(long objectId)
        {
            if (_trackingController?.LocalUserData?.UserObjectId == objectId)
            {
                return true;
            }

            return false;
            // TODO: Party user auslesen und der ObjectID zuordnen
        }

        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed()
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= 2)
            {
                _lastDamageUiUpdate = currentDateTime;
                return true;
            }

            return false;
        }

        private long GetHighestDamage(List<DamageObject> damageObjectList)
        {
            return (damageObjectList.Count <= 0) ? 0 : damageObjectList.Max(x => x.Damage);
        }
    }
}