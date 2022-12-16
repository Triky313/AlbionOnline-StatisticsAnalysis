﻿using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
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
    public class EntityController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly ConcurrentDictionary<Guid, PlayerGameObject> _knownEntities = new();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new();
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ObservableCollection<EquipmentItemInternal> _newEquipmentItems = new();
        private readonly ObservableCollection<SpellEffect> _spellEffects = new();
        private readonly ConcurrentDictionary<long, CharacterEquipmentData> _tempCharacterEquipmentData = new();
        private double _lastLocalEntityGuildTaxInPercent;
        private double _lastLocalEntityClusterTaxInPercent;

        public LocalUserData LocalUserData { get; init; } = new();

        public EntityController(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        #region Entities

        public event Action<GameObject> OnAddEntity;

        public void AddEntity(long objectId, Guid userGuid, Guid? interactGuid, string name, string guild, string alliance, 
            CharacterEquipment characterEquipment, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            PlayerGameObject gameObject;

            if (_knownEntities.TryRemove(userGuid, out var oldEntity))
            {
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    Guild = guild,
                    Alliance = alliance,
                    InteractGuid = interactGuid,
                    ObjectSubType = objectSubType,
                    CharacterEquipment = characterEquipment ?? oldEntity.CharacterEquipment,
                    CombatStart = oldEntity.CombatStart,
                    CombatTime = oldEntity.CombatTime,
                    Damage = oldEntity.Damage,
                    Heal = oldEntity.Heal
                };
            }
            else
            {
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    Guild = guild,
                    Alliance = alliance,
                    ObjectSubType = objectSubType,
                    CharacterEquipment = characterEquipment
                };
            }

            if (_tempCharacterEquipmentData.TryGetValue(objectId, out var characterEquipmentData))
            {
                ResetTempCharacterEquipment();
                gameObject.CharacterEquipment = characterEquipmentData.CharacterEquipment;
                _tempCharacterEquipmentData.TryRemove(objectId, out _);
            }

            _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveEntitiesByLastUpdate(int withoutAnUpdateForMinutes)
        {
            foreach (var entity in _knownEntities.Where(x =>
                         x.Value.ObjectSubType != GameObjectSubType.LocalPlayer
                         && x.Value.Damage <= 0
                         && !IsEntityInParty(x.Key)
                         && new DateTime(x.Value.LastUpdate).AddMinutes(withoutAnUpdateForMinutes).Ticks < DateTime.UtcNow.Ticks))
            {
                _knownEntities.TryRemove(entity.Key, out _);
            }
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetEntity(string uniqueName)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.Name == uniqueName);
        }

        public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false)
        {
            return new List<KeyValuePair<Guid, PlayerGameObject>>(onlyInParty ? _knownEntities.ToArray().Where(x => IsEntityInParty(x.Value.Name)) : _knownEntities.ToArray());
        }

        public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntitiesWithDamageOrHeal()
        {
            return new List<KeyValuePair<Guid, PlayerGameObject>>(_knownEntities.ToArray().Where(x => x.Value.Damage > 0 || x.Value.Heal > 0));
        }

        #endregion

        #region Party

        public async Task AddToPartyAsync(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid))
            {
                _knownPartyEntities.TryAdd(guid, username);
            }

            await SetPartyMemberUiAsync();
        }

        public async Task RemoveFromPartyAsync(Guid? guid)
        {
            if (guid is { } notNullGuid)
            {
                if (notNullGuid == GetLocalEntity()?.Key)
                {
                    await ResetPartyMemberAsync();
                    await AddLocalEntityToPartyAsync();
                }
                else
                {
                    _ = _knownPartyEntities.TryRemove(notNullGuid, out _);
                }

                await SetPartyMemberUiAsync();
            }
        }

        public async Task ResetPartyMemberAsync()
        {
            _knownPartyEntities.Clear();
            await SetPartyMemberUiAsync();
        }

        public async Task AddLocalEntityToPartyAsync()
        {
            foreach (var member in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer))
            {
                _knownPartyEntities.TryAdd(member.Key, member.Value.Name);
            }

            await SetPartyMemberUiAsync();
        }

        public async Task SetPartyAsync(Dictionary<Guid, string> party, bool resetPartyBefore = false)
        {
            if (resetPartyBefore)
            {
                await ResetPartyMemberAsync();
            }

            foreach (var member in party)
            {
                await AddToPartyAsync(member.Key, member.Value);
            }

            await SetPartyMemberUiAsync();
        }

        private async Task SetPartyMemberUiAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel.PartyMemberCircles.Clear();
                foreach (var member in _knownPartyEntities) _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle
                {
                    Name = member.Value,
                    UserGuid = member.Key
                });
                _mainWindowViewModel.PartyMemberNumber = _knownPartyEntities.Count;
            });
        }

        public bool IsEntityInParty(string name)
        {
            return _knownPartyEntities.Any(x => x.Value == name);
        }

        public bool IsEntityInParty(long objectId)
        {
            var entity = _knownEntities.FirstOrDefault(x => x.Value.ObjectId == objectId);
            return entity.Value != null && _knownPartyEntities.Any(x => x.Value == entity.Value.Name);
        }

        public bool IsEntityInParty(Guid guid)
        {
            return _knownPartyEntities.Any(x => x.Key == guid);
        }

        public void CopyPartyToClipboard()
        {
            var output = string.Empty;
            var partyString = _knownPartyEntities.Aggregate(output, (current, entity) => current + $"{entity.Value},");
            Clipboard.SetDataObject(partyString[..(partyString.Length > 0 ? partyString.Length - 1 : 0)]);
        }

        #endregion

        #region Equipment

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity?.Value != null)
            {
                entity.Value.Value.CharacterEquipment = equipment;
            }
            else
            {
                _tempCharacterEquipmentData.TryAdd(objectId, new CharacterEquipmentData
                {
                    CharacterEquipment = equipment,
                    TimeStamp = DateTime.UtcNow
                });
            }
        }

        public void ResetTempCharacterEquipment()
        {
            foreach (var characterEquipment in _tempCharacterEquipmentData.ToArray())
            {
                if (Utilities.IsBlockingTimeExpired(characterEquipment.Value.TimeStamp, 30))
                {
                    _tempCharacterEquipmentData.TryRemove(characterEquipment.Key, out _);
                }
            }
        }

        public void AddEquipmentItem(EquipmentItemInternal itemInternal)
        {
            lock (_newEquipmentItems)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_newEquipmentItems.ToList().Any(x => x == null || x.ItemIndex.Equals(itemInternal?.ItemIndex) && x.SpellDictionary?.Values == itemInternal.SpellDictionary?.Values))
                    {
                        return;
                    }

                    _newEquipmentItems.Add(itemInternal);
                });
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void AddSpellEffect(SpellEffect spell)
        {
            if (!IsEntityInParty(spell.CauserId))
            {
                return;
            }

            lock (_spellEffects)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_spellEffects.Any(x => x == null || (x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex))))
                    {
                        return;
                    }

                    _spellEffects.Add(spell);
                });
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void DetectUsedWeapon()
        {
            var playerItemList = new Dictionary<long, int>();

            try
            {
                lock (_newEquipmentItems)
                {
                    foreach (var item in _newEquipmentItems.ToList())
                    {
                        foreach (var spell in
                                 (from itemSpell in item.SpellDictionary.ToArray()
                                  from spell in _spellEffects.ToArray()
                                  where spell != null && spell.SpellIndex.Equals(itemSpell.Value)
                                  select spell).ToArray())
                        {
                            if (playerItemList.Any(x => x.Key.Equals(spell.CauserId)))
                            {
                                continue;
                            }

                            playerItemList.Add(spell.CauserId, item.ItemIndex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }

            foreach (var (key, value) in playerItemList.ToList())
            {
                SetCharacterMainHand(key, value);
            }
        }

        private void SetCharacterMainHand(long objectId, int itemIndex)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            var entityValue = entity?.Value;

            if (entityValue == null)
            {
                return;
            }

            entityValue.CharacterEquipment ??= new CharacterEquipment
            {
                MainHand = itemIndex
            };
        }

        private void RemoveSpellAndEquipmentObjects()
        {
            lock (_newEquipmentItems)
            {
                foreach (var item in _newEquipmentItems.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _newEquipmentItems.Remove(item);
                    });
                }
            }

            lock (_spellEffects)
            {
                foreach (var spell in _spellEffects.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _spellEffects.Remove(spell);
                    });
                }
            }
        }

        public class CharacterEquipmentData
        {
            public DateTime TimeStamp { get; set; }
            public CharacterEquipment CharacterEquipment { get; set; }
        }

        #endregion

        #region Damage

        public void ResetEntitiesDamageStartTime()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.CombatStart = null;
            }
        }

        public void ResetEntitiesDamageTimes()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.ResetCombatTimes();
            }
        }

        public void ResetEntitiesDamage()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.Damage = 0;
            }
        }

        public void ResetEntitiesHeal()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.Heal = 0;
            }
        }

        #endregion

        #region Health

        public void HealthUpdate(
            long objectId,
            GameTimeStamp timeStamp,
            double healthChange,
            double newHealthValue,
            EffectType effectType,
            EffectOrigin effectOrigin,
            long causerId,
            int causingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                timeStamp,
                healthChange,
                newHealthValue,
                effectType,
                effectOrigin,
                causerId,
                causingSpellType
            );
        }

        public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;

        #endregion

        #region Local Entity

        public FixPoint GetLastLocalEntityClusterTax(FixPoint yieldPreClusterTax) => FixPoint.FromFloatingPointValue(yieldPreClusterTax.DoubleValue / 100 * _lastLocalEntityClusterTaxInPercent);

        public void SetLastLocalEntityClusterTax(FixPoint yieldPreTax, FixPoint clusterTax)
        {
            _lastLocalEntityClusterTaxInPercent = (100 / yieldPreTax.DoubleValue) * clusterTax.DoubleValue;
        }

        public void SetLastLocalEntityGuildTax(FixPoint yieldPreTax, FixPoint guildTax)
        {
            _lastLocalEntityGuildTaxInPercent = (100 / yieldPreTax.DoubleValue) * guildTax.DoubleValue;
        }

        public FixPoint GetLastLocalEntityGuildTax(FixPoint yieldPreTax) => FixPoint.FromFloatingPointValue(yieldPreTax.DoubleValue / 100 * _lastLocalEntityGuildTaxInPercent);

        public bool ExistLocalEntity()
        {
            return _knownEntities?.Any(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer) ?? false;
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity() => _knownEntities?.ToArray().FirstOrDefault(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer);

        #endregion
    }
}