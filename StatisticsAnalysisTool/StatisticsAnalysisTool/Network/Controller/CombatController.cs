using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Controller
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
            UpdateDamageMeterUi(SetRandomDamageValues(40));
#endif
        }

        #region Damage Meter methods

        public void AddDamage(long causerId, double healthChange)
        {
            var gameObject = _trackingController?.EntityController?.GetEntity(causerId);
            if (gameObject == null || healthChange >= 0 || gameObject.Value.Value?.ObjectType != GameObjectType.Player ||
                !_trackingController.EntityController.IsUserInParty(gameObject.Value.Value.Name)) return;

            var damageValue = (int) Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
            if (damageValue <= 0) return;

            if (gameObject.Value.Value?.CombatStart == null) gameObject.Value.Value.CombatStart = DateTime.UtcNow;

            gameObject.Value.Value.Damage += damageValue;

            UpdateDamageMeterUi(_trackingController.EntityController.GetAllEntities(true));
        }

        public void UpdateDamageMeterUi(List<KeyValuePair<Guid, PlayerGameObject>> entities)
        {
            if (!IsUiUpdateAllowed()) return;

            var highestDamage = GetHighestDamage(entities);
            _trackingController.EntityController.DetectUsedWeapon();

            foreach (var damageObject in entities)
                if (_mainWindowViewModel.DamageMeter.Any(x => x.CauserGuid == damageObject.Value.UserGuid))
                    _mainWindow.Dispatcher?.Invoke(async () =>
                    {
                        var fragment = _mainWindowViewModel.DamageMeter.FirstOrDefault(x => x.CauserGuid == damageObject.Value.UserGuid);
                        if (fragment != null)
                        {
                            fragment.CauserMainHand = await SetItemInfoIfSlotTypeMainHandAsync(fragment.CauserMainHand,
                                damageObject.Value?.CharacterEquipment?.MainHand);

                            if (damageObject.Value?.Damage > 0) fragment.DamageInPercent = (double) damageObject.Value.Damage / highestDamage * 100;

                            fragment.Damage = damageObject.Value?.Damage.ToShortNumberString();
                            if (damageObject.Value?.Dps != null) fragment.Dps = damageObject.Value.Dps;

                            if (damageObject.Value != null) fragment.DamagePercentage = GetDamagePercentage(entities, damageObject.Value.Damage);
                        }

                        _mainWindowViewModel.SetDamageMeterSort();
                    });
                else
                    _mainWindow.Dispatcher?.InvokeAsync(async () =>
                    {
                        var mainHandItem = ItemController.GetItemByIndex(damageObject.Value?.CharacterEquipment?.MainHand ?? 0);

                        if (damageObject.Value != null)
                        {
                            var damageMeterFragment = new DamageMeterFragment
                            {
                                CauserGuid = damageObject.Value.UserGuid,
                                Damage = damageObject.Value.Damage.ToShortNumberString(),
                                Dps = damageObject.Value.Dps,
                                DamageInPercent = (double) damageObject.Value.Damage / highestDamage * 100,
                                DamagePercentage = GetDamagePercentage(entities, damageObject.Value.Damage),
                                Name = damageObject.Value.Name,
                                CauserMainHand =
                                    await SetItemInfoIfSlotTypeMainHandAsync(mainHandItem, damageObject.Value?.CharacterEquipment?.MainHand)
                            };

                            _mainWindowViewModel.DamageMeter.Add(damageMeterFragment);
                        }

                        _mainWindowViewModel.SetDamageMeterSort();
                    });
        }

        public void ResetDamageMeter()
        {
            _trackingController.EntityController.ResetEntitiesDamageTimes();
            _trackingController.EntityController.ResetEntitiesDamage();
            _trackingController.EntityController.ResetEntitiesDamageStartTime();

            _mainWindow?.Dispatcher?.InvokeAsync(() => { _mainWindowViewModel?.DamageMeter?.Clear(); });
        }

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
            if (difference.Seconds >= 1)
            {
                _lastDamageUiUpdate = currentDateTime;
                return true;
            }

            return false;
        }

        private long GetHighestDamage(List<KeyValuePair<Guid, PlayerGameObject>> playerObjects)
        {
            return playerObjects.Count <= 0 ? 0 : playerObjects.Max(x => x.Value.Damage);
        }

        private double GetDamagePercentage(List<KeyValuePair<Guid, PlayerGameObject>> playerObjects, double playerDamage)
        {
            var totalDamage = playerObjects.Sum(x => x.Value.Damage);
            return 100.00 / totalDamage * playerDamage;
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
            if (!_trackingController.EntityController.IsUserInParty(objectId)) return;

            var playerObject = _trackingController.EntityController.GetEntity(objectId);

            if (playerObject?.Value == null) return;

            if ((inActiveCombat || inPassiveCombat) && playerObject.Value.Value.CombatTimes.Any(x => x?.EndTime == null)) return;

            if (inActiveCombat || inPassiveCombat) playerObject.Value.Value.AddCombatTime(new CombatTime(DateTime.UtcNow));

            if (!inActiveCombat && !inPassiveCombat)
            {
                var combatTime = playerObject.Value.Value.CombatTimes.FirstOrDefault(x => x.EndTime == null);
                if (combatTime != null) combatTime.EndTime = DateTime.UtcNow;
            }
        }

        #endregion

        #region Debug methods

        private static readonly Random _random = new Random(DateTime.Now.Millisecond);

        private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount = 5)
        {
            var randomPlayerList = new List<KeyValuePair<Guid, PlayerGameObject>>();

            for (var i = 0; i < playerAmount; i++)
            {
                var causerGuid = new Guid($"{_random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
                var damage = _random.Next(500, 9999);
                var objectId = _random.Next(20, 9999);
                var len = _random.Next(3, 10);
                var randomTime = _random.Next(1, 1000);

                randomPlayerList.Add(new KeyValuePair<Guid, PlayerGameObject>(causerGuid, new PlayerGameObject(objectId)
                {
                    CharacterEquipment = new CharacterEquipment
                    {
                        MainHand = GetRandomWeaponIndex()
                    },
                    CombatTime = new TimeSpan(0, 0, 0, randomTime),
                    Damage = damage,
                    Name = GenerateName(len),
                    ObjectSubType = GameObjectSubType.Player,
                    ObjectType = GameObjectType.Player,
                    UserGuid = causerGuid
                }));
            }

            return randomPlayerList;
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