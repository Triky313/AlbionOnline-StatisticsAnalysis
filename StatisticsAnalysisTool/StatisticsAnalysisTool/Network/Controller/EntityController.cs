using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly ConcurrentDictionary<Guid, PlayerGameObject> _knownEntities = new ConcurrentDictionary<Guid, PlayerGameObject>();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new ConcurrentDictionary<Guid, string>();
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ObservableCollection<EquipmentItem> _newEquipmentItems = new ObservableCollection<EquipmentItem>();
        private readonly ObservableCollection<SpellEffect> _spellEffects = new ObservableCollection<SpellEffect>();
        private readonly ConcurrentDictionary<long, CharacterEquipmentData> _tempCharacterEquipmentData = new ConcurrentDictionary<long, CharacterEquipmentData>();
        private double _lastLocalEntityGuildTaxInPercent;
        private double _lastLocalEntityClusterTaxInPercent;

        public EntityController(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        #region Entities

        public event Action<GameObject> OnAddEntity;

        public void AddEntity(long objectId, Guid userGuid, Guid? interactGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            PlayerGameObject gameObject;
            
            if (_knownEntities.TryRemove(userGuid, out var oldEntity))
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    InteractGuid = interactGuid,
                    ObjectSubType = objectSubType,
                    CharacterEquipment = oldEntity.CharacterEquipment,
                    CombatStart = oldEntity.CombatStart,
                    CombatTime = oldEntity.CombatTime,
                    Damage = oldEntity.Damage
                };
            else
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    ObjectSubType = objectSubType
                };

            if (_tempCharacterEquipmentData.TryGetValue(objectId, out var characterEquipmentData))
            {
                ResetTempCharacterEquipment();
                gameObject.CharacterEquipment = characterEquipmentData.CharacterEquipment;
                _tempCharacterEquipmentData.TryRemove(objectId, out _);
            }

            _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveAllEntities()
        {
            foreach (var entity in _knownEntities.Where(x =>
                x.Value.ObjectSubType != GameObjectSubType.LocalPlayer && !_knownPartyEntities.ContainsKey(x.Key)))
                _knownEntities.TryRemove(entity.Key, out _);

            foreach (var entity in _knownEntities.Where(x =>
                x.Value.ObjectSubType == GameObjectSubType.LocalPlayer || _knownPartyEntities.ContainsKey(x.Key))) entity.Value.ObjectId = null;
        }

        public bool ExistLocalEntity()
        {
            return _knownEntities?.Any(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer) ?? false;
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }
        public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false)
        {
            return onlyInParty ? _knownEntities.Where(x => IsUserInParty(x.Value.Name)).ToList() : _knownEntities.ToList();
        }

        public bool IsEntityInParty(long objectId) => GetAllEntities(true).Any(x => x.Value.ObjectId == objectId);

        public KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity() => _knownEntities?.FirstOrDefault(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer);

        #endregion

        #region Party

        public void AddToParty(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid)) _knownPartyEntities.TryAdd(guid, username);

            SetPartyMemberUi();
        }

        public void RemoveFromParty(string username)
        {
            var partyMember = _knownPartyEntities.FirstOrDefault(x => x.Value == username);

            if (partyMember.Value != null) _knownPartyEntities.TryRemove(partyMember.Key, out _);

            SetPartyMemberUi();
        }

        public void ResetPartyMember()
        {
            _knownPartyEntities.Clear();

            foreach (var member in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer))
            {
                _knownPartyEntities.TryAdd(member.Key, member.Value.Name);
            }

            SetPartyMemberUi();
        }

        public void SetParty(Dictionary<Guid, string> party, bool resetPartyBefore = false)
        {
            if (resetPartyBefore)
            {
                ResetPartyMember();
            }

            foreach (var member in party)
            {
                AddToParty(member.Key, member.Value);
            }

            SetPartyMemberUi();
        }

        private void SetPartyMemberUi()
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                _mainWindowViewModel.PartyMemberCircles.Clear();
                foreach (var member in _knownPartyEntities) _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle { Name = member.Value });
                _mainWindowViewModel.PartyMemberNumber = _knownPartyEntities.Count;
            });
        }

        public bool IsUserInParty(string name)
        {
            return _knownPartyEntities.Any(x => x.Value == name);
        }

        public bool IsUserInParty(long objectId)
        {
            var entity = _knownEntities.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity.Value == null)
            {
                return false;
            }

            return _knownPartyEntities.Any(x => x.Value == entity.Value.Name);
        }

        #endregion

        #region Equipment

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity?.Value != null)
                entity.Value.Value.CharacterEquipment = equipment;
            else
                _tempCharacterEquipmentData.TryAdd(objectId, new CharacterEquipmentData
                {
                    CharacterEquipment = equipment,
                    TimeStamp = DateTime.UtcNow
                });
        }

        public void ResetTempCharacterEquipment()
        {
            foreach (var characterEquipment in _tempCharacterEquipmentData)
                if (Utilities.IsBlockingTimeExpired(characterEquipment.Value.TimeStamp, 30))
                    _tempCharacterEquipmentData.TryRemove(characterEquipment.Key, out _);
        }

        public void AddEquipmentItem(EquipmentItem item)
        {
            if (_newEquipmentItems.Any(x => x.ItemIndex.Equals(item.ItemIndex) && x.SpellDictionary.Values == item.SpellDictionary.Values) ||
                item.SpellDictionary.Count <= 0) return;

            lock (item)
            {
                _newEquipmentItems.Add(item);
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void AddSpellEffect(SpellEffect spell)
        {
            if (!IsUserInParty(spell.CauserId)) return;

            if (_spellEffects.Any(x => x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex))) return;

            lock (spell)
            {
                _spellEffects.Add(spell);
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void DetectUsedWeapon()
        {
            var playerItemList = new Dictionary<long, int>();

            foreach (var item in _newEquipmentItems.ToList())
            foreach (var spell in from itemSpell in item.SpellDictionary
                from spell in _spellEffects
                where spell.SpellIndex.Equals(itemSpell.Value)
                select spell)
            {
                if (playerItemList.Any(x => x.Key.Equals(spell.CauserId))) continue;

                playerItemList.Add(spell.CauserId, item.ItemIndex);
            }

            foreach (var playerItem in playerItemList.ToList()) SetCharacterMainHand(playerItem.Key, playerItem.Value);
        }

        private void SetCharacterMainHand(long objectId, int itemIndex)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);

            if (entity?.Value == null) return;

            if (entity.Value.Value?.CharacterEquipment == null)
                entity.Value.Value.CharacterEquipment = new CharacterEquipment
                {
                    MainHand = itemIndex
                };

            //if (entity.Value.Value != null)
            //{
            //    entity.Value.Value.CharacterEquipment.MainHand = itemIndex;
            //}
        }

        private void RemoveSpellAndEquipmentObjects()
        {
            foreach (var item in _newEquipmentItems.Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)).ToList())
                lock (item)
                {
                    _newEquipmentItems.Remove(item);
                }

            foreach (var spell in _spellEffects.Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)).ToList())
                lock (spell)
                {
                    _spellEffects.Remove(spell);
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

        #endregion

        #region Health

        public void HealthUpdate(
            long objectId,
            GameTimeStamp TimeStamp,
            double HealthChange,
            double NewHealthValue,
            EffectType EffectType,
            EffectOrigin EffectOrigin,
            long CauserId,
            int CausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                TimeStamp,
                HealthChange,
                NewHealthValue,
                EffectType,
                EffectOrigin,
                CauserId,
                CausingSpellType
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

        #endregion
    }
}