using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class CombatController
    {
        private readonly TrackingController _trackingController;
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;

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

        public async void AddDamage(long causerId, double healthChange)
        {
            var gameObject = _trackingController?.EntityController?.GetEntity(causerId);
            if (gameObject == null || gameObject.Value.Value?.ObjectType != GameObjectType.Player || !_trackingController.EntityController.IsUserInParty(gameObject.Value.Value.Name))
            {
                return;
            }

            var damageValue = (int)Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageValue <= 0)
            {
                return;
            }

            AddInternalDamage(gameObject.Value.Value, damageValue);

            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }

            if (IsUiUpdateAllowed())
            {
                var damageListByNewestCluster = _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();
                var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserId)
                    .Select(x => new DamageObject(
                        x.First().CauserId, 
                        x.First().TargetId, 
                        x.First().CauserName,
                        x.FirstOrDefault(y => y?.CauserMainHand != null)?.CauserMainHand,
                        x.Sum(s => s.Damage)))
                    .ToList();

                await UpdateDamageMeterUiAsync(groupedDamageList);
            }
        }
        
        public async Task UpdateDamageMeterUiAsync(List<DamageObject> damageList)
        {
            var highestDamage = GetHighestDamage(damageList);

            foreach (var damageObject in damageList)
            {
                if (damageObject?.CauserMainHand != null && damageObject.CauserMainHand.FullItemInformation == null)
                {
                    damageObject.CauserMainHand.FullItemInformation = await ItemController.GetFullItemInformationAsync(damageObject.CauserMainHand);
                }
            }

            foreach (var damageObject in damageList.OrderBy(x => x.Damage))
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserId == damageObject.CauserId))
                {
                    _mainWindow.Dispatcher?.Invoke(() => {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserId == damageObject.CauserId);
                        if (fragment != null)
                        {
                            fragment.CauserMainHand = damageObject.CauserMainHand;

                            if (damageObject.Damage > 0)
                            {
                                fragment.DamageInPercent = ((double)damageObject.Damage / highestDamage) * 100;
                            }

                            fragment.Damage = damageObject.Damage.ToShortNumber();
                        }
                    });
                }
                else
                {
                    _mainWindow.Dispatcher?.InvokeAsync(() =>
                    {
                        _mainWindowViewModel.DamageMeter.Add(new DamageMeterFragment()
                        {
                            CauserId = damageObject.CauserId, Damage = damageObject.Damage.ToShortNumber(), DamageInPercent = ((double)damageObject.Damage / highestDamage) * 100, Name = damageObject.CauserName, CauserMainHand = damageObject.CauserMainHand
                        });
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

        private void AddInternalDamage(GameObject gameObject, int damage)
        {
            var dmgObject = _damageCollection.FirstOrDefault(x => x.CauserId == gameObject.ObjectId);
            if (dmgObject != null)
            {
                dmgObject.CauserName = gameObject.Name;

                if (gameObject.CharacterEquipment?.MainHand > 0)
                {
                    dmgObject.CauserMainHand = ItemController.GetItemByIndex(gameObject.CharacterEquipment.MainHand);
                }

                dmgObject.Damage += damage;
                return;
            }

            _damageCollection.Add(new DamageObject(gameObject.ObjectId, null, gameObject.Name, ItemController.GetItemByIndex(gameObject.CharacterEquipment?.MainHand ?? 0), damage));
        }
        
        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed()
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= 1)
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