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
            _ = UpdateDamageMeterUiAsync(SetRandomDamageValues(40));
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

        public async void AddDamage(long causerId, double healthChange)
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

            if (IsUiUpdateAllowed())
            {
                var damageListByNewestCluster = _damageCollection.Where(x => x.StartTime >= _clusterStarts.GetHighestDateTime()).ToList();
                var groupedDamageList = damageListByNewestCluster.GroupBy(x => x.CauserGuid)
                    .Select(x => new DamageObject(
                        x.First().CauserGuid,
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

            foreach (var damageObject in damageList.OrderByDescending(x => x.Damage))
            {
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserGuid == damageObject.CauserGuid))
                {
                    _mainWindow.Dispatcher?.Invoke(() => {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserGuid == damageObject.CauserGuid);
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
                            CauserGuid = damageObject.CauserGuid,
                            Damage = damageObject.Damage.ToShortNumber(),
                            DamageInPercent = ((double)damageObject.Damage / highestDamage) * 100,
                            Name = damageObject.CauserName,
                            CauserMainHand = damageObject.CauserMainHand
                        });
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

                _trackingController.EntityController.DetectUsedWeapon();

                if (gameObject.CharacterEquipment?.MainHand > 0)
                {
                    dmgObject.CauserMainHand = ItemController.GetItemByIndex(gameObject.CharacterEquipment.MainHand);
                }

                dmgObject.Damage += damage;
                return;
            }

            _damageCollection.Add(new DamageObject(gameObject.UserGuid, gameObject.Name, ItemController.GetItemByIndex(gameObject.CharacterEquipment?.MainHand ?? 0), damage));
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
                
                randomDamageList.Add(new DamageObject(causerGuid, GenerateName(10), new Item()
                {
                    UniqueName = "T8_2H_CURSEDSTAFF@3",
                    FullItemInformation = new ItemInformation()
                    {
                        UniqueName = "T8_2H_CURSEDSTAFF@3",
                        CategoryId = GetRandomWeaponCategoryId()
                    }
                }, damage));
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

        private static string GetRandomWeaponCategoryId()
        {
            var names = new List<string> { "sword", "spear", "quarterstaff", "naturestaff", "mace", "holystaff", "hammer", "froststaff", "firestaff", "dagger", "cursestaff", "crossbow", "bow", "axe", "arcanestaff", "Unknown" };

            var index = _random.Next(names.Count);
            var name = names[index];
            names.RemoveAt(index);
            return name;
        }

        #endregion
    }
}