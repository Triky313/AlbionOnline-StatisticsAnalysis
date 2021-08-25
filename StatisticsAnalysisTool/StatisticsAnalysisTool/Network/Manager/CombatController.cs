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
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class CombatController
    {
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public CombatController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;

            OnChangeCombatMode += AddCombatTime;

#if DEBUG
            RunDamageMeterDebugAsync(10);
#endif
        }

        #region Damage Meter methods

        public async void AddDamageAsync(long objectId, long causerId, double healthChange, double newHealthValue)
        {
            var gameObject = _trackingController?.EntityController?.GetEntity(causerId);

            if (gameObject == null || gameObject.Value.Value?.ObjectType != GameObjectType.Player || !_trackingController.EntityController.IsUserInParty(gameObject.Value.Value.Name))
            {
                return;
            }

            if (GetHealthChangeType(healthChange) == HealthChangeType.Damage)
            {
                var damageChangeValue = (int)Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
                if (damageChangeValue <= 0)
                {
                    return;
                }

                gameObject.Value.Value.Damage += damageChangeValue;
            }

            if (GetHealthChangeType(healthChange) == HealthChangeType.Heal)
            {
                var healChangeValue = healthChange;
                if (healChangeValue <= 0)
                {
                    return;
                }

                if (IsMaxHealthReached(objectId, newHealthValue))
                {
                    return;
                }

                gameObject.Value.Value.Heal += (int)Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            }

            if (gameObject.Value.Value?.CombatStart == null)
            {
                gameObject.Value.Value.CombatStart = DateTime.UtcNow;
            }

            if (IsUiUpdateAllowed())
            {
                await UpdateDamageMeterUiAsync(_mainWindowViewModel.DamageMeter, _trackingController.EntityController.GetAllEntities(true));
            }
        }

        private static bool IsUiUpdateActive;

        public async Task UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
        {
            IsUiUpdateActive = true;

            var highestDamage = entities.GetHighestDamage();
            var highestHeal = entities.GetHighestHeal();
            _trackingController.EntityController.DetectUsedWeapon();

            foreach (var healthChangeObject in entities)
            {
                if (_mainWindow?.Dispatcher == null || healthChangeObject.Value?.UserGuid == null)
                {
                    continue;
                }
                
                var fragment = damageMeter.FirstOrDefault(x => x.CauserGuid == healthChangeObject.Value.UserGuid);

                if (fragment != null)
                {
                    await Application.Current.Dispatcher.Invoke(async delegate
                    {
                        await UpdateDamageMeterFragmentAsync(fragment, healthChangeObject, entities, highestDamage, highestHeal);
                    });
                }
                else
                {
                    await Application.Current.Dispatcher.Invoke(async delegate
                    {
                        await AddDamageMeterFragmentAsync(damageMeter, healthChangeObject, entities, highestDamage, highestHeal);
                    });
                }

                Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.SetDamageMeterSort());
            }

            IsUiUpdateActive = false;
        }

        private async Task UpdateDamageMeterFragmentAsync(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject, List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            fragment.CauserMainHand = await SetItemInfoIfSlotTypeMainHandAsync(fragment.CauserMainHand, healthChangeObject.Value?.CharacterEquipment?.MainHand);

            // Damage
            if (healthChangeObject.Value?.Damage > 0)
            {
                fragment.DamageInPercent = (double)healthChangeObject.Value.Damage / highestDamage * 100;
            }

            fragment.Damage = healthChangeObject.Value?.Damage.ToShortNumberString();
            if (healthChangeObject.Value?.Dps != null)
            {
                fragment.Dps = healthChangeObject.Value.Dps;
            }

            if (healthChangeObject.Value != null)
            {
                fragment.DamagePercentage = entities.GetDamagePercentage(healthChangeObject.Value.Damage);
            }

            // Heal
            if (healthChangeObject.Value?.Heal > 0)
            {
                fragment.HealInPercent = (double)healthChangeObject.Value.Heal / highestHeal * 100;
            }

            fragment.Heal = healthChangeObject.Value?.Heal.ToShortNumberString();
            if (healthChangeObject.Value?.Hps != null)
            {
                fragment.Hps = healthChangeObject.Value.Hps;
            }

            if (healthChangeObject.Value != null)
            {
                fragment.HealPercentage = entities.GetDamagePercentage(healthChangeObject.Value.Heal);
            }
        }

        private async Task AddDamageMeterFragmentAsync(ICollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
            List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            if (healthChangeObject.Value == null 
                || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal)) 
                || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0))
            {
                return;
            }

            var mainHandItem = ItemController.GetItemByIndex(healthChangeObject.Value?.CharacterEquipment?.MainHand ?? 0);

            var damageMeterFragment = new DamageMeterFragment
            {
                CauserGuid = healthChangeObject.Value.UserGuid,
                Damage = healthChangeObject.Value.Damage.ToShortNumberString(),
                Dps = healthChangeObject.Value.Dps,
                DamageInPercent = (double)healthChangeObject.Value.Damage / highestDamage * 100,
                DamagePercentage = entities.GetDamagePercentage(healthChangeObject.Value.Damage),

                Heal = healthChangeObject.Value.Heal.ToShortNumberString(),
                Hps = healthChangeObject.Value.Hps,
                HealInPercent = (double)healthChangeObject.Value.Heal / highestHeal * 100,
                HealPercentage = entities.GetHealPercentage(healthChangeObject.Value.Heal),

                Name = healthChangeObject.Value.Name,
                CauserMainHand = await SetItemInfoIfSlotTypeMainHandAsync(mainHandItem, healthChangeObject.Value?.CharacterEquipment?.MainHand)
            };

            damageMeter.Add(damageMeterFragment);
        }

        public void ResetDamageMeter()
        {
            _trackingController.EntityController.ResetEntitiesDamageTimes();
            _trackingController.EntityController.ResetEntitiesDamage();
            _trackingController.EntityController.ResetEntitiesDamageStartTime();

            _mainWindow?.Dispatcher?.InvokeAsync(() => { _mainWindowViewModel?.DamageMeter?.Clear(); });
        }

        public Dictionary<long, double> LastPlayersHealth = new();

        public bool IsMaxHealthReached(long objectId, double newHealthValue)
        {
            var playerHealth = LastPlayersHealth?.FirstOrDefault(x => x.Key == objectId);
            if (playerHealth?.Value.CompareTo(newHealthValue) == 0)
            {
                return true;
            }

            SetLastPlayersHealth(objectId, newHealthValue);
            return false;
        }

        private void SetLastPlayersHealth(long key, double value)
        {
            if (LastPlayersHealth.ContainsKey(key))
            {
                LastPlayersHealth[key] = value;
            }
            else
            {
                LastPlayersHealth.Add(key, value);
            }
        }

        private HealthChangeType GetHealthChangeType(double healthChange) => healthChange <= 0 ? HealthChangeType.Damage : HealthChangeType.Heal;

        private async Task<Item> SetItemInfoIfSlotTypeMainHandAsync(Item currentItem, int? newIndex)
        {
            if (newIndex == null || newIndex <= 0) return currentItem;

            var item = ItemController.GetItemByIndex((int) newIndex);
            if (item == null) return currentItem;

            var fullItemInfo = await ItemController.GetFullItemInformationAsync(item);
            if (ItemController.IsItemSlotType(fullItemInfo, "mainhand"))
            {
                item.FullItemInformation = fullItemInfo;
                return item;
            }

            return currentItem;
        }

        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed()
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= 1 && !IsUiUpdateActive)
            {
                _lastDamageUiUpdate = currentDateTime;
                return true;
            }

            return false;
        }
        
        #endregion

        #region Combat Mode / Combat Timer

        public event Action<long, bool, bool> OnChangeCombatMode;

        public void UpdateCombatMode(long objectId, bool inActiveCombat, bool inPassiveCombat)
        {
            OnChangeCombatMode?.Invoke(objectId, inActiveCombat, inPassiveCombat);
        }

        private void AddCombatTime(long objectId, bool inActiveCombat, bool inPassiveCombat)
        {
            if (!_trackingController.EntityController.IsUserInParty(objectId))
            {
                return;
            }

            var playerObject = _trackingController.EntityController.GetEntity(objectId);

            if (playerObject?.Value == null)
            {
                return;
            }

            if ((inActiveCombat || inPassiveCombat) && playerObject.Value.Value.CombatTimes.Any(x => x?.EndTime == null))
            {
                return;
            }

            if (inActiveCombat || inPassiveCombat) playerObject.Value.Value.AddCombatTime(new TimeCollectObject(DateTime.UtcNow));

            if (!inActiveCombat && !inPassiveCombat)
            {
                var combatTime = playerObject.Value.Value.CombatTimes.FirstOrDefault(x => x.EndTime == null);
                if (combatTime != null)
                {
                    combatTime.EndTime = DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region Debug methods

        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        private async void RunDamageMeterDebugAsync(int runs = 30)
        {
            var entities = SetRandomDamageValues(20);

            for (var i = 0; i < runs; i++)
            {
                await UpdateDamageMeterUiAsync(_mainWindowViewModel.DamageMeter, entities);
                await Task.Delay(1080);

                foreach (var entity in entities)
                {
                    if (_random.Next(0, 10) < 8)
                    {
                        continue;
                    }

                    entity.Value.Damage += _random.Next(10, 500);
                    entity.Value.Heal += _random.Next(10, 750);
                    entity.Value.AddCombatTime(new TimeCollectObject(DateTime.UtcNow)
                    {
                        EndTime = DateTime.UtcNow.AddSeconds(_random.Next(1, 35))
                    });
                }
            }
        }

        private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount = 5)
        {
            var randomPlayerList = new List<KeyValuePair<Guid, PlayerGameObject>>();
            
            for (var i = 0; i < playerAmount; i++)
            {
                var randomPlayer = GetRandomPlayerDebug();
                randomPlayerList.Add(new KeyValuePair<Guid, PlayerGameObject>(randomPlayer.CauserGuid, new PlayerGameObject(randomPlayer.ObjectId)
                {
                    CharacterEquipment = new CharacterEquipment
                    {
                        MainHand = GetRandomWeaponIndex()
                    },
                    CombatTime = new TimeSpan(0, 0, 0, randomPlayer.RandomTime),
                    Damage = randomPlayer.Damage,
                    Heal = randomPlayer.Heal,
                    Name = GenerateName(randomPlayer.Name),
                    ObjectSubType = GameObjectSubType.Player,
                    ObjectType = GameObjectType.Player,
                    UserGuid = randomPlayer.CauserGuid
                }));
            }

            return randomPlayerList;
        }

        private RandomPlayerDebugStruct GetRandomPlayerDebug()
        {
            return new RandomPlayerDebugStruct()
            {
                CauserGuid = new Guid($"{_random.Next(1000, 9999)}0000-0000-0000-0000-000000000000"),
                Damage = _random.Next(500, 9999),
                Heal = _random.Next(500, 9999),
                ObjectId = _random.Next(20, 9999),
                Name = _random.Next(3, 10),
                RandomTime = _random.Next(1, 1000)
            };
        }

        struct RandomPlayerDebugStruct
        {
            public Guid CauserGuid { get; set; }
            public int Damage { get; set; }
            public int Heal { get; set; }
            public int ObjectId { get; set; }
            public int Name { get; set; }
            public int RandomTime { get; set; }
        }

        private static string GenerateName(int len)
        {
            string[] consonants = {"b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x"};
            string[] vowels = {"a", "e", "i", "o", "u", "ae", "y"};
            var Name = "";
            Name += consonants[_random.Next(consonants.Length)].ToUpper();
            Name += vowels[_random.Next(vowels.Length)];
            var b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
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
            var indexArray = new List<int> {6180, 5900, 6326, 5614, 6600, 5602, 6467, 5181, 5080, 5705, 4998, 4777, 4696, 6045, 0};

            var index = _random.Next(indexArray.Count);
            var itemIndex = indexArray[index];
            indexArray.RemoveAt(index);
            return itemIndex;
        }

        #endregion
    }
}