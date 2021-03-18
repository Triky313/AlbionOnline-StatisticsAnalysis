using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
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

#if DEBUG
            UpdateDamageMeterUi(SetRandomDamageValues(40));
#endif
        }

        #region Damage Meter methods

        public event Action<bool, bool> OnChangeCombatMode;

        public void UpdateCombatMode(bool inActiveCombat, bool inPassiveCombat)
        {
            OnChangeCombatMode?.Invoke(inActiveCombat, inPassiveCombat);
        }

        public DateTime AddClusterStartTimer()
        {
            var time = DateTime.UtcNow;
            _clusterStarts.Add(time);
            return time;
        }

        public void AddDamage(long causerId, double healthChange)
        {
            var gameObject = _trackingController?.EntityController?.GetEntity(causerId);
            if (gameObject == null || healthChange >= 0 || gameObject.Value.Value?.ObjectType != GameObjectType.Player || !_trackingController.EntityController.IsUserInParty(gameObject.Value.Value.Name))
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

            var damageListByNewestCluster = _damageCollection.Where(x => x.StartTime >= _clusterStarts.GetHighestDateTime()).ToList();
            UpdateDamageMeterUi(damageListByNewestCluster);
        }

        public void UpdateDamageMeterUi(List<DamageObject> damageObjectList)
        {
            if (!IsUiUpdateAllowed())
            {
                return;
            }

            var groupedDamageList = damageObjectList.GroupBy(x => x.CauserGuid)
                .Select(x => new DamageObject(
                    x.First().StartTime,
                    x.First().CauserGuid,
                    x.First().CauserName,
                    x.FirstOrDefault(y => y?.MainHandItemIndex != null && y.StartTime >= x.Max().StartTime)?.MainHandItemIndex ?? 0,
                    x.Sum(s => s.Damage),
                    Utilities.GetValuePerHourToDouble(x.Sum(s => s.Damage), DateTime.UtcNow - x.First().StartTime)))
                .OrderByDescending(x => x.Damage).ToList();

            var highestDamage = GetHighestDamage(groupedDamageList);

            foreach (var damageObject in groupedDamageList)
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserGuid == damageObject.CauserGuid))
                {
                    _mainWindow.Dispatcher?.Invoke(async () =>
                    {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserGuid == damageObject.CauserGuid);
                        if (fragment != null)
                        {
                            fragment.CauserMainHand = await SetItemInfoIfSlotTypeMainHandAsync(damageObject.MainHandItemIndex);

                            if (damageObject.Damage > 0)
                            {
                                fragment.DamageInPercent = ((double)damageObject.Damage / highestDamage) * 100;
                            }

                            fragment.Damage = damageObject.Damage.ToShortNumber();
                            fragment.Dps = damageObject.Dps.ToShortNumber();
                        }
                        _mainWindowViewModel.DamageMeter.OrderByReference(_mainWindowViewModel.DamageMeter.OrderByDescending(x => x.DamageInPercent).ToList());
                    });
                }
                else
                {
                    _mainWindow.Dispatcher?.InvokeAsync(async () =>
                    {
                        var damageMeterFragment = new DamageMeterFragment()
                        {
                            CauserGuid = damageObject.CauserGuid,
                            Damage = damageObject.Damage.ToShortNumber(),
                            Dps = damageObject.Dps.ToShortNumber(),
                            DamageInPercent = ((double) damageObject.Damage / highestDamage) * 100,
                            Name = damageObject.CauserName,
                            CauserMainHand = await SetItemInfoIfSlotTypeMainHandAsync(damageObject.MainHandItemIndex)
                        };

                        _mainWindowViewModel.DamageMeter.Add(damageMeterFragment);
                        _mainWindowViewModel.DamageMeter.OrderByReference(_mainWindowViewModel.DamageMeter.OrderByDescending(x => x.DamageInPercent).ToList());
                    });
                }
            }
        }

        public void ResetDamage(DateTime newStartTime)
        {
            foreach (var damageObject in _damageCollection)
            {
                damageObject.Damage = 0;
                damageObject.StartTime = newStartTime;
            }

            _mainWindow?.Dispatcher?.InvokeAsync(() =>
            {
                _mainWindowViewModel?.DamageMeter?.Clear();
            });
        }

        private void AddInternalDamage(GameObject gameObject, int damage)
        {
            if (gameObject?.ObjectId == null)
            {
                return;
            }

            var dmgObject = _damageCollection.FirstOrDefault(x => x.CauserGuid == gameObject.UserGuid);
            if (dmgObject != null)
            {
                dmgObject.CauserName = gameObject.Name;
                dmgObject.MainHandItemIndex = gameObject.CharacterEquipment?.MainHand ?? 0;
                _trackingController.EntityController.DetectUsedWeapon();

                dmgObject.Damage += damage;
                return;
            }

            _damageCollection.Add(new DamageObject(DateTime.UtcNow, gameObject.UserGuid, gameObject.Name, gameObject.CharacterEquipment?.MainHand ?? 0, damage, 0));
        }

        private async Task<Item> SetItemInfoIfSlotTypeMainHandAsync(int index)
        {
            if (index <= 0)
            {
                return null;
            }

            var item = ItemController.GetItemByIndex(index);
            if (item == null)
            {
                return null;
            }

            var fullItemInfo = await ItemController.GetFullItemInformationAsync(item);
            if (ItemController.IsItemSlotType(fullItemInfo, "mainhand"))
            {
                item.FullItemInformation = fullItemInfo;
                return item;
            }

            return null;
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

        #endregion

        #region Debug methods

        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        private List<DamageObject> SetRandomDamageValues(int playerAmount = 5)
        {
            var randomDamageList = new List<DamageObject>();
            
            for (var i = 0; i < playerAmount; i++)
            {
                var causerGuid = new Guid($"{_random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
                var damage = _random.Next(500, 999999);
                
                randomDamageList.Add(new DamageObject(DateTime.UtcNow, causerGuid, GenerateName(10), GetRandomWeaponIndex(), damage, _random.Next(5000, 9999999)));
            }

            return randomDamageList;
        }

        private static string GenerateName(int len)
        {
            string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
            string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
            string Name = "";
            Name += consonants[_random.Next(consonants.Length)].ToUpper();
            Name += vowels[_random.Next(vowels.Length)];
            int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
            while (b < len)
            {
                Name += consonants[_random.Next(consonants.Length)];
                b++;
                Name += vowels[_random.Next(vowels.Length)];
                b++;
            }

            return Name;
        }

        private static int GetRandomWeaponIndex()
        {
            var indexArray = new List<int> { 6211, 5926, 1176, 1171, 6553, 1087, 6413, 5181, 5080, 5705, 4998, 4777, 4696, 2075, 5472, 0 };

            var index = _random.Next(indexArray.Count);
            var itemIndex = indexArray[index];
            indexArray.RemoveAt(index);
            return itemIndex;
        }

        #endregion
    }
}