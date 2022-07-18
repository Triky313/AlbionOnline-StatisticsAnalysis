using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class CombatController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public bool IsDamageMeterActive { get; set; } = false;

        public CombatController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;

            OnChangeCombatMode += AddCombatTime;
            OnDamageUpdate += UpdateDamageMeterUiAsync;

#if DEBUG
            RunDamageMeterDebugAsync(5, 10);
#endif
        }

        #region Damage Meter methods

        public event Action<ObservableCollection<DamageMeterFragment>, List<KeyValuePair<Guid, PlayerGameObject>>> OnDamageUpdate;

        public Task AddDamage(long objectId, long causerId, double healthChange, double newHealthValue)
        {
            if (!IsDamageMeterActive || objectId == causerId)
            {
                return Task.CompletedTask;
            }

            var gameObject = _trackingController?.EntityController?.GetEntity(causerId);
            var gameObjectValue = gameObject?.Value;

            if (gameObject?.Value == null
                || gameObject.Value.Value?.ObjectType != GameObjectType.Player
                || !_trackingController.EntityController.IsEntityInParty(gameObject.Value.Value.Name)
                )
            {
                return Task.CompletedTask;
            }

            if (GetHealthChangeType(healthChange) == HealthChangeType.Damage)
            {
                var damageChangeValue = (int)Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
                if (damageChangeValue <= 0)
                {
                    return Task.CompletedTask;
                }

                gameObject.Value.Value.Damage += damageChangeValue;
            }

            if (GetHealthChangeType(healthChange) == HealthChangeType.Heal)
            {
                var healChangeValue = healthChange;
                if (healChangeValue <= 0)
                {
                    return Task.CompletedTask;
                }

                if (IsMaxHealthReached(objectId, newHealthValue))
                {
                    return Task.CompletedTask;
                }

                gameObject.Value.Value.Heal += (int)Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            }

            gameObjectValue.CombatStart ??= DateTime.UtcNow;

            OnDamageUpdate?.Invoke(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter, _trackingController.EntityController.GetAllEntitiesWithDamageOrHeal());
            return Task.CompletedTask;
        }

        private static bool _isUiUpdateActive;

        public async void UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
        {
            if (!IsUiUpdateAllowed())
            {
                return;
            }

            _isUiUpdateActive = true;

            var highestDamage = entities.GetHighestDamage();
            var highestHeal = entities.GetHighestHeal();

            _trackingController.EntityController.DetectUsedWeapon();

            foreach (var healthChangeObject in entities)
            {
                if (_mainWindow?.Dispatcher == null || healthChangeObject.Value?.UserGuid == null)
                {
                    continue;
                }

                var fragment = damageMeter.ToList().FirstOrDefault(x => x.CauserGuid == healthChangeObject.Value.UserGuid);
                if (fragment != null)
                {
                    UpdateDamageMeterFragment(fragment, healthChangeObject, entities, highestDamage, highestHeal);
                }
                else
                {
                    await AddDamageMeterFragmentAsync(damageMeter, healthChangeObject, entities, highestDamage, highestHeal).ConfigureAwait(true);
                }

                Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.DamageMeterBindings?.SetDamageMeterSort());
            }

            if (HasDamageMeterDupes(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter))
            {
                await RemoveDuplicatesAsync(_mainWindowViewModel?.DamageMeterBindings?.DamageMeter);
            }

            _isUiUpdateActive = false;
        }

        private static void UpdateDamageMeterFragment(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
            List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            var healthChangeObjectValue = healthChangeObject.Value;

            if (healthChangeObjectValue?.CharacterEquipment?.MainHand != null)
            {
                var item = ItemController.GetItemByIndex(healthChangeObjectValue.CharacterEquipment?.MainHand);
                if (item != null)
                {
                    fragment.CauserMainHand = item;
                }
            }

            // Damage
            if (healthChangeObjectValue?.Damage > 0)
            {
                fragment.DamageInPercent = (double)healthChangeObjectValue.Damage / highestDamage * 100;
                fragment.Damage = healthChangeObjectValue.Damage;
            }

            if (healthChangeObjectValue?.Dps != null)
            {
                fragment.Dps = healthChangeObjectValue.Dps;
            }

            // Heal
            if (healthChangeObjectValue?.Heal > 0)
            {
                fragment.HealInPercent = (double)healthChangeObjectValue.Heal / highestHeal * 100;
                fragment.Heal = healthChangeObjectValue.Heal;
            }

            if (healthChangeObjectValue?.Hps != null)
            {
                fragment.Hps = healthChangeObjectValue.Hps;
            }

            // Generally
            if (healthChangeObjectValue != null)
            {
                fragment.DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage);
                fragment.HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal);
            }
        }

        private static async Task AddDamageMeterFragmentAsync(ICollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
            List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            if (healthChangeObject.Value == null
                || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal))
                || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0))
            {
                return;
            }

            var healthChangeObjectValue = healthChangeObject.Value;
            var item = ItemController.GetItemByIndex(healthChangeObject.Value?.CharacterEquipment?.MainHand ?? 0);

            var damageMeterFragment = new DamageMeterFragment
            {
                CauserGuid = healthChangeObjectValue.UserGuid,
                Damage = healthChangeObjectValue.Damage,
                Dps = healthChangeObjectValue.Dps,
                DamageInPercent = (double)healthChangeObjectValue.Damage / highestDamage * 100,
                DamagePercentage = entities.GetDamagePercentage(healthChangeObjectValue.Damage),

                Heal = healthChangeObjectValue.Heal,
                Hps = healthChangeObjectValue.Hps,
                HealInPercent = (double)healthChangeObjectValue.Heal / highestHeal * 100,
                HealPercentage = entities.GetHealPercentage(healthChangeObjectValue.Heal),

                Name = healthChangeObjectValue.Name,
                CauserMainHand = item
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                damageMeter.Add(damageMeterFragment);
            });
        }

        private static bool HasDamageMeterDupes(IEnumerable<DamageMeterFragment> damageMeter)
        {
            return damageMeter.ToList().GroupBy(x => x.Name).Any(g => g.Count() > 1);
        }

        private static async Task RemoveDuplicatesAsync(ObservableCollection<DamageMeterFragment> damageMeter)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var damageMeterWithoutDupes = (from dmf in damageMeter.ToList()
                                               group dmf by dmf.Name into x
                                               select new DamageMeterFragment(x.FirstOrDefault())).ToList();

                if (damageMeterWithoutDupes.Count <= 0)
                {
                    return;
                }

                foreach (var damageMeterFragment in damageMeter.ToList())
                {
                    if (damageMeterWithoutDupes.Any(x => x.Equals(damageMeterFragment)))
                    {
                        damageMeter.Remove(damageMeterFragment);
                    }
                }
            });
        }

        public void ResetDamageMeterByClusterChange()
        {
            if (!_mainWindowViewModel.IsDamageMeterResetByMapChangeActive)
            {
                return;
            }

            _trackingController.CombatController.ResetDamageMeter();
            _trackingController.CombatController.LastPlayersHealth.Clear();
        }

        public void ResetDamageMeter()
        {
            _trackingController.EntityController.ResetEntitiesDamageTimes();
            _trackingController.EntityController.ResetEntitiesDamage();
            _trackingController.EntityController.ResetEntitiesHeal();
            _trackingController.EntityController.ResetEntitiesDamageStartTime();

            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                _mainWindowViewModel?.DamageMeterBindings?.DamageMeter?.Clear();
            });
        }

        public ConcurrentDictionary<long, double> LastPlayersHealth = new();

        public bool IsMaxHealthReached(long objectId, double newHealthValue)
        {
            var playerHealth = LastPlayersHealth?.ToArray().FirstOrDefault(x => x.Key == objectId);
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
                try
                {
                    LastPlayersHealth.TryAdd(key, value);
                }
                catch (Exception e)
                {
                    Log.Warn(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }

        private static HealthChangeType GetHealthChangeType(double healthChange) => healthChange <= 0 ? HealthChangeType.Damage : HealthChangeType.Heal;

        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed(int waitTimeInSeconds = 1)
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= waitTimeInSeconds && !_isUiUpdateActive)
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
            if (!_trackingController.EntityController.IsEntityInParty(objectId))
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

        private static readonly Random Random = new(DateTime.Now.Millisecond);

        private async void RunDamageMeterDebugAsync(int player = 20, int damageRuns = 100)
        {
            var entities = SetRandomDamageValues(player);
            var tasks = new List<Task>();

            foreach (var entity in entities)
            {
                tasks.Add(AddDamageAsync(entity.Value, damageRuns));
            }

            await Task.WhenAll(tasks);
        }

        private async Task AddDamageAsync(PlayerGameObject entity, int runs)
        {
            for (var i = 0; i < runs; i++)
            {
                var damage = Random.Next(-100, 100);
                await AddDamage(9999, entity.ObjectId ?? -1, damage, Random.Next(2000, 3000));
                //Debug.Print($"--- AddDamage - {entity.Name}: {damage}");

                await Task.Delay(Random.Next(1, 1000));
            }
        }

        private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount)
        {
            for (var i = 0; i < playerAmount; i++)
            {
                var guid = new Guid($"{Random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
                var interactGuid = Guid.NewGuid();
                var name = TestMethods.GenerateName(Random.Next(3, 10));

                _trackingController?.EntityController?.AddEntity(i, guid, interactGuid, name, GameObjectType.Player, GameObjectSubType.Mob);

                // Only if SetCharacterMainHand is public
                //_trackingController?.EntityController?.SetCharacterMainHand(i, TestMethods.GetRandomWeaponIndex());
                _trackingController?.EntityController?.AddToPartyAsync(guid, name);
            }

            return _trackingController?.EntityController?.GetAllEntities();
        }

        #endregion
    }
}