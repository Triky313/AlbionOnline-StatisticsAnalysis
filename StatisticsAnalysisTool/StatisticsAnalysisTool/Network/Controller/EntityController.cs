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

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ConcurrentDictionary<Guid, GameObject> _knownEntities = new ConcurrentDictionary<Guid, GameObject>();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new ConcurrentDictionary<Guid, string>();
        private readonly ObservableCollection<EquipmentItem> _newEquipmentItems = new ObservableCollection<EquipmentItem>();
        private readonly ObservableCollection<SpellEffect> _spellEffects = new ObservableCollection<SpellEffect>();

        public EntityController(MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddEntity(long objectId, Guid userGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            var gameObject = new GameObject(objectId)
            {
                Name = name,
                ObjectType = objectType,
                UserGuid = userGuid,
                ObjectSubType = objectSubType,
            };

            _knownEntities.TryRemove(userGuid, out _);
            _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveAllEntities()
        {
            foreach (var entity in _knownEntities.Where(x => x.Value.ObjectSubType != GameObjectSubType.LocalPlayer && !_knownPartyEntities.ContainsKey(x.Key)))
            {
                _knownEntities.TryRemove(entity.Key, out _);
            }

            foreach (var entity in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer || _knownPartyEntities.ContainsKey(x.Key)))
            {
                entity.Value.ObjectId = null;
            }
        }

        public void ResetPartyMember()
        {
            _knownPartyEntities.Clear();

            foreach (var member in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer))
            {
                _knownPartyEntities.TryAdd(member.Key, member.Value.Name);
            }
        }

        public void AddToParty(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid))
            {
                _knownPartyEntities.TryAdd(guid, username);
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
            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindowViewModel.PartyMemberCircles.Clear();
                foreach (var member in _knownPartyEntities)
                {
                    _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle() {Name = member.Value});
                }
            });
        }

        public bool IsUserInParty(string name)
        {
            return _knownPartyEntities.Any(x => x.Value == name);
        }

        public bool IsUserInParty(long objectId)
        {
            var entity = _knownEntities.FirstOrDefault(x  => x.Value.ObjectId == objectId);
            if (entity.Value == null)
            {
                return false;
            }

            return _knownPartyEntities.Any(x => x.Value == entity.Value.Name);
        }

        public KeyValuePair<Guid, GameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity?.Value != null)
            {
                entity.Value.Value.CharacterEquipment = equipment;
            }
        }

        public void SetCharacterMainHand(long objectId, int itemIndex)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);

            if (entity?.Value == null)
            {
                return;
            }

            if (entity.Value.Value?.CharacterEquipment == null)
            {
                entity.Value.Value.CharacterEquipment = new CharacterEquipment();
            }

            if (entity.Value.Value != null)
            {
                entity.Value.Value.CharacterEquipment.MainHand = itemIndex;
            }
        }

        public event Action<GameObject> OnAddEntity;

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

        public void AddEquipmentItem(EquipmentItem item)
        {
            if (_newEquipmentItems.Any(x => x.ItemIndex.Equals(item.ItemIndex) && x.SpellDictionary.Values == item.SpellDictionary.Values) || item.SpellDictionary.Count <= 0)
            {
                return;
            }

            lock (item)
            {
                _newEquipmentItems.Add(item);
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void AddSpellEffect(SpellEffect spell)
        {
            if (!IsUserInParty(spell.CauserId))
            {
                return;
            }

            if (_spellEffects.Any(x => x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex)))
            {
                return;
            }

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
            {
                foreach (var spell in from itemSpell in item.SpellDictionary from spell in _spellEffects where spell.SpellIndex.Equals(itemSpell.Value) select spell)
                {
                    if (playerItemList.Any(x => x.Key.Equals(spell.CauserId)))
                    {
                        continue;
                    }

                    playerItemList.Add(spell.CauserId, item.ItemIndex);
                }
            }

            foreach (var playerItem in playerItemList.ToList())
            {
                SetCharacterMainHand(playerItem.Key, playerItem.Value);
            }
        }

        private void RemoveSpellAndEquipmentObjects()
        {
            foreach (var item in _newEquipmentItems.Where(x => x?.TimeStamp < DateTime.UtcNow.AddMinutes(-2)).ToList())
            {
                lock (item)
                {
                    _newEquipmentItems.Remove(item);
                }
            }

            foreach (var spell in _spellEffects.Where(x => x?.TimeStamp < DateTime.UtcNow.AddMinutes(-2)).ToList())
            {
                lock (spell)
                {
                    _spellEffects.Remove(spell);
                }
            }
        }
    }
}